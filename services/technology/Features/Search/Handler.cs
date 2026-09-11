using TechVerseX.Contracts.Common;
using TechVerseX.Contracts.Technology;
using TechVerseX.TechnologyService.Features.ListFields;
using TechVerseX.TechnologyService.Features.SearchTechnology;
using TechVerseX.TechnologyService.Infrastructure.Persistence;

namespace TechVerseX.TechnologyService.Features.Search;

public sealed record SearchQuery(string? Term, int Page = 1, int PageSize = 20);

/// <summary>
/// Satu pertanyaan — <em>apa di situs ini yang cocok dengan kata-kata ini?</em> —
/// dijawab untuk kedua jenis entitas yang punya halaman.
/// </summary>
/// <remarks>
/// 🔑 <b>Handler ini tidak punya kueri sendiri.</b> Ia memanggil
/// <see cref="ListFieldsHandler"/> dan <see cref="SearchTechnologyHandler"/>,
/// yang keduanya sudah dipakai halaman lain. Itu disengaja dan bukan kemalasan:
/// begitu ada kueri kedua yang menjawab "apakah baris ini cocok?", pertanyaan
/// yang sama punya dua jawaban, dan yang satu akan menyimpang dari yang lain
/// pada perubahan berikutnya tanpa ada yang merah.
/// <para>
/// Bagian <see cref="SearchResponse.Query"/> dibersihkan di sini juga, bukan
/// sekadar diteruskan: klien menampilkan kata yang BENAR-BENAR dicari server,
/// dan server memangkas spasi serta memotong di
/// <see cref="PencarianTeks.MaksPanjangKataKunci"/>.
/// </para>
/// <para>
/// ⚠️ Kata kunci kosong menghasilkan <b>dua daftar kosong</b>, bukan seluruh
/// isi situs. Halaman pencarian tanpa kata kunci adalah halaman yang belum
/// ditanyai apa-apa; menjawabnya dengan "semuanya" membuat tombol cari yang
/// tidak diisi terlihat seperti sudah bekerja.
/// </para>
/// </remarks>
public sealed class SearchHandler(ListFieldsHandler fields, SearchTechnologyHandler technologies)
{
    public async Task<SearchResponse> HandleAsync(SearchQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Amplop kosongnya memakai angka yang SUDAH dinormalkan
        // SearchTechnologyQuery — bukan angka mentah dari URL. Kalau tidak,
        // `?pageSize=99999` tanpa kata kunci membalas amplop yang mengaku
        // berukuran 99999, sementara `?pageSize=99999&q=apa` membalas 100.
        // Dua jawaban berbeda untuk satu parameter yang sama.
        var halaman = new SearchTechnologyQuery(query.Term, Field: null, query.Page, query.PageSize);

        if (!PencarianTeks.Bersihkan(query.Term, out var term))
        {
            return new SearchResponse(
                string.Empty,
                [],
                new PagedResponse<TechnologySummaryResponse>(
                    [],
                    halaman.NormalizedPage,
                    halaman.NormalizedPageSize,
                    0));
        }

        // Berurutan, bukan bersamaan: keduanya memakai DbContext yang sama, dan
        // DbContext EF Core tidak aman dipakai dua operasi sekaligus. Dua kueri
        // indeks terhadap tabel berisi belasan dan puluhan baris tidak menuntut
        // paralelisme; yang dituntut paralelisme adalah dua scope, dan itu harga
        // yang jauh lebih besar daripada yang dibelinya.
        var bidang = await fields.HandleAsync(term, cancellationToken).ConfigureAwait(false);

        var topik = await technologies
            .HandleAsync(halaman with { Term = term }, cancellationToken)
            .ConfigureAwait(false);

        return new SearchResponse(term, bidang, topik);
    }
}
