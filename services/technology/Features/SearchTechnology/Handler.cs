using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using TechVerseX.Contracts.Common;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Domain;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.SearchTechnology;

public sealed record SearchTechnologyQuery(string? Term, string? Field, int Page = 1, int PageSize = 20)
{
    public const int MaxPageSize = 100;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 20,
        > MaxPageSize => MaxPageSize,
        _ => PageSize,
    };
}

/// <summary>
/// Pencarian V1 — <b>PostgreSQL FTS</b>, sesuai tangga KERANGKA.md 4.6
/// (V1 PostgreSQL FTS → V2 OpenSearch → V3 hybrid) dan ADR-015 bagian 5.
/// </summary>
/// <remarks>
/// Sampai 2026-09-11 ini masih <c>ILIKE '%kata%'</c> saja, dan batasnya bukan
/// teoretis: <c>ILIKE</c> mencocokkan <b>potongan huruf</b>, bukan kata. Diukur
/// terhadap isi sungguhan — dari sepuluh kata kunci percobaan, <b>empat</b> yang
/// jelas ada di halaman menjawab nol semata-mata karena <b>urutan katanya
/// dibalik</b>: <c>protokol model</c>, <c>kuantum sirkuit</c>,
/// <c>digital kembaran</c>, <c>model bahasa alat</c>.
/// <para>
/// 🔑 <b>Pencocokan sebagian TIDAK dibuang, dan itu keputusan yang diukur juga.</b>
/// FTS mencocokkan kata utuh, jadi orang yang baru mengetik separuh kata
/// (<c>kubern</c>) mendapat nol hasil — terukur, dan itu keadaan normal sebuah
/// kotak pencarian. Keduanya dipakai bersama: FTS menyumbang pencocokan lintas
/// urutan kata <b>berikut peringkatnya</b>, <see cref="PencarianTeks.PolaAwalKata"/>
/// menjaga pengetikan sebagian tetap bekerja. Konsekuensinya jujur: baris yang
/// hanya cocok lewat pola itu tidak bisa memakai indeks GIN dan berperingkat 0,
/// jadi ia selalu muncul <b>sesudah</b> kecocokan FTS. Lihat ADR-022.
/// </para>
/// <para>
/// 🔴 <b>Cadangannya AWAL KATA, bukan potongan huruf di mana saja.</b> Bentuk
/// pertamanya <c>ILIKE '%kata%'</c>, dan menjalankannya langsung memperlihatkan
/// kenapa itu salah: <c>iot</c> memulangkan bidang <b>Biotechnology</b>
/// (<c>b-iot-echnology</c>). Lihat <see cref="PencarianTeks.PolaAwalKata"/>.
/// </para>
/// <para>
/// ⚠️ <b><c>websearch_to_tsquery</c>, bukan <c>to_tsquery</c>.</b> Yang kedua
/// <b>melempar</b> pada masukan yang tidak berbentuk kueri —
/// <c>to_tsquery('english', 'quantum &amp;')</c> menjawab
/// <c>ERROR: no operand in tsquery</c>. Di kotak pencarian publik itu berarti
/// HTTP 500 yang dipicu satu karakter yang diketik pengunjung.
/// <c>websearch_to_tsquery</c> tidak pernah melempar, dan sekalian memberi tanda
/// kutip untuk frasa serta <c>-</c> untuk pengecualian.
/// </para>
/// </remarks>
public sealed class SearchTechnologyHandler(TechnologyDbContext db)
{
    public async Task<PagedResponse<TechnologySummaryResponse>> HandleAsync(
        SearchTechnologyQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var source = db.Technologies
            .AsNoTracking()
            .Join(
                db.Fields.AsNoTracking(),
                t => t.FieldId,
                f => f.Id,
                (t, f) => new { Technology = t, Field = f });

        var berkataKunci = PencarianTeks.Bersihkan(query.Term, out var term);

        if (berkataKunci)
        {
            var pola = PencarianTeks.PolaAwalKata(term);

            // Bidangnya ikut dicocokkan, dan itu disengaja: taksonomi ADR-010
            // adalah cara utama isi situs ini ditemukan. Mencari "cybersecurity"
            // memang seharusnya memunculkan topik-topik di bawah bidang itu,
            // bukan hanya topik yang kebetulan menuliskan kata itu lagi.
            //
            // Keenam baris ini menutupi TEKS YANG SAMA PERSIS dari dua arah -
            // nama dan ringkasan, topik dan bidang. Kalau kedua jalur menutupi
            // kolom yang berbeda, hasil pencarian jadi mustahil dijelaskan dalam
            // satu kalimat, dan yang begitu selalu berakhir jadi dua jawaban
            // untuk satu pertanyaan.
            source = source.Where(x =>
                EF.Property<NpgsqlTsVector>(x.Technology, PencarianTeks.KolomVektor)
                    .Matches(EF.Functions.WebSearchToTsQuery(PencarianTeks.Konfigurasi, term))
                || EF.Property<NpgsqlTsVector>(x.Field, PencarianTeks.KolomVektor)
                    .Matches(EF.Functions.WebSearchToTsQuery(PencarianTeks.Konfigurasi, term))
                || Regex.IsMatch(x.Technology.Name, pola, RegexOptions.IgnoreCase)
                || Regex.IsMatch(x.Technology.Summary, pola, RegexOptions.IgnoreCase)
                || Regex.IsMatch(x.Field.Name, pola, RegexOptions.IgnoreCase)
                || Regex.IsMatch(x.Field.Summary, pola, RegexOptions.IgnoreCase));
        }

        if (Slugs.TryFrom(query.Field, out var fieldSlug))
        {
            source = source.Where(x => x.Field.Slug == fieldSlug);
        }

        var total = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        // Dua urutan yang berbeda untuk dua pertanyaan yang berbeda.
        //
        // Tanpa kata kunci ini DAFTAR: urutan tampil bidang dulu, baru nama,
        // sebab daftar yang dikelompokkan menurut taksonomi lebih berguna
        // daripada daftar abjad murni. Dengan kata kunci ini HASIL PENCARIAN,
        // dan yang paling cocok harus di atas — peringkat taksonomi turun jadi
        // pemutus seri, bukan sumbu utama.
        //
        // Peringkat bidang dibagi empat: ia menjelaskan kenapa sebuah baris ikut
        // terjaring, tapi topik yang kata kuncinya ada di NAMANYA sendiri harus
        // tetap menang atas tetangga sebidang yang tidak menyebutnya sama sekali.
        var diurut = berkataKunci
            ? source
                .OrderByDescending(x =>
                    EF.Property<NpgsqlTsVector>(x.Technology, PencarianTeks.KolomVektor)
                        .Rank(EF.Functions.WebSearchToTsQuery(PencarianTeks.Konfigurasi, term))
                    + (EF.Property<NpgsqlTsVector>(x.Field, PencarianTeks.KolomVektor)
                        .Rank(EF.Functions.WebSearchToTsQuery(PencarianTeks.Konfigurasi, term)) / 4))
                .ThenBy(x => x.Field.DisplayOrder)
                .ThenBy(x => x.Technology.Name)
            : source
                .OrderBy(x => x.Field.DisplayOrder)
                .ThenBy(x => x.Technology.Name);

        var items = await diurut
            .Skip((query.NormalizedPage - 1) * query.NormalizedPageSize)
            .Take(query.NormalizedPageSize)
            .Select(x => new TechnologySummaryResponse(
                x.Technology.Id,
                x.Technology.Slug,
                x.Technology.Name,
                x.Technology.Summary,
                x.Field.Slug,
                x.Field.Name,
                x.Technology.Maturity.ToString()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResponse<TechnologySummaryResponse>(items, query.NormalizedPage, query.NormalizedPageSize, total);
    }
}
