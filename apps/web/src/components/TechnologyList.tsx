import Link from 'next/link';
import { connection } from 'next/server';
import { ArrowRight } from '@/components/Icons';
import MaturityBadge from '@/components/MaturityBadge';
import { searchTechnologies } from '@/lib/api';
import { mesinPengembang } from '@/lib/lingkungan';

/**
 * Irisan vertikal pertama yang benar-benar menyentuh backend:
 * Technology → API → Database → Web UI (KERANGKA.md 4.17 Phase 2).
 */
export default async function TechnologyList() {
  // Tanpa ini Next.js 16 memanggang halaman ini saat build, dan yang tersimpan
  // adalah jawaban API pada saat build — yaitu "API tidak bisa dihubungi",
  // karena saat build API memang tidak jalan. `connection()` menyatakan bahwa
  // komponen ini harus dirender per-permintaan.
  //
  // ⚠️ JANGAN dibuang demi "supaya bisa di-cache". Yang di-cache bukan HALAMANnya
  // melainkan PANGGILAN API-nya (lihat REVALIDATE_SECONDS di lib/api.ts), dan
  // keduanya tidak bertabrakan: halaman tetap dirender per-permintaan, tapi
  // rendernya dilayani cache data, bukan API. Dibuktikan dengan MENJALANKAN —
  // dengan peti kemas API dimatikan, halaman ini tetap menampilkan isinya.
  await connection();

  const result = await searchTechnologies({ pageSize: 24 });

  if (!result.ok) {
    return (
      <div className="glass border-amber/40 p-5 text-sm">
        {mesinPengembang ? (
          <>
            {/* Frasa ini dikutip sebagai gejala di issue #40 - jangan diubah
                tanpa ikut menyapu issue itu. */}
            <p className="font-medium text-amber">API belum bisa dihubungi.</p>
            <p className="mt-1 text-fg-soft">{result.reason}</p>
            <p className="mt-2 text-fg-soft">
              Jalankan <code className="rounded bg-surface-strong px-1 py-0.5 font-mono">run.ps1 up</code> lalu{' '}
              <code className="rounded bg-surface-strong px-1 py-0.5 font-mono">run.ps1 api</code>.
            </p>
          </>
        ) : (
          <>
            <p className="font-medium text-amber">Daftar topik belum bisa dimuat.</p>
            {/*
              🔴 Kalimat kedua di sini dulu berbunyi "Keempat belas bidang DI BAWAH
              tetap bisa dibuka", dan itu keliru dua kali sekaligus — ketahuan dari
              MENJALANKAN halaman ini dengan API dimatikan, bukan dari uji:

                1. Bidangnya ada DI ATAS, bukan di bawah. FieldGrid dirender lebih
                   dulu di page.tsx; diukur di HTML jadinya, bukan ditebak.
                2. Dan janjinya sendiri tidak bisa ditepati. Daftar bidang datang
                   dari API yang sama; kalau ia tak terjangkau, kartu bidang di
                   atas JUGA sedang memberi pesan galat, dan halaman
                   /teknologi/<slug> ikut gagal karena ia pun memanggil listFields.

              Kalimat itu lahir di PR #50 — yaitu di perbaikan yang justru dibuat
              untuk membuang bahan pengembang dari halaman ini. Pola yang sama
              persis dengan PR #51: PERBAIKANNYA SENDIRI menanam klaim yang
              berhenti benar. Jangan menjanjikan apa pun yang datang dari API yang
              sama dengan yang barusan gagal.
            */}
            <p className="mt-1 text-fg-soft">Coba muat ulang beberapa saat lagi.</p>
          </>
        )}
      </div>
    );
  }

  const { items, totalItems } = result.data;

  if (items.length === 0) {
    return (
      <div className="glass p-5 text-sm">
        <p className="font-medium text-fg">Belum ada satu topik pun.</p>
        <p className="mt-1 text-fg-soft">
          {mesinPengembang ? (
            <>
              Isi contoh: <code className="rounded bg-surface-strong px-1 py-0.5 font-mono">run.ps1 seed</code>
            </>
          ) : (
            // Teks pembaca ADR-024: kalimat ini dulu ditutup "Bulan 1 mengejar
            // taksonominya lebih dulu" - nama bulan rencana, di keadaan yang
            // justru NORMAL untuk produksi hari ini.
            <>Itu keadaan yang jujur, bukan galat.</>
          )}
        </p>
      </div>
    );
  }

  return (
    <section id="topik" aria-labelledby="judul-topik">
      <div className="mb-8">
        <p className="eyebrow">Topik</p>
        <h2 id="judul-topik" className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl">
          {totalItems} topik tercatat
          {/* pageSize 24 memotong DIAM-DIAM tanpa keterangan ini. Markah yang sama
              dengan TopikCocok di cari/page.tsx. */}
          {totalItems > items.length ? (
            <span className="ml-3 align-middle text-sm font-normal text-fg-mute">
              menampilkan {items.length} teratas
            </span>
          ) : null}
        </h2>
      </div>
      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {items.map((technology) => (
          <li key={technology.id} className="glass glass-hover reveal group relative flex flex-col p-5">
            <p className="eyebrow !text-violet">{technology.fieldName}</p>
            <h3 className="mt-2 text-xl font-semibold tracking-tight">
              <Link href={`/teknologi/${technology.slug}`} className="stretched-link">
                {technology.name}
              </Link>
            </h3>
            <p className="mt-2 line-clamp-4 text-sm leading-relaxed text-fg-soft">{technology.summary}</p>

            {/* Aturan keras ADR-012: kematangan isi selalu ikut tampil. Kontrak
                API-nya sendiri yang menjamin datanya ada — lihat
                TechnologySummaryResponse. */}
            <div className="mt-auto flex items-center justify-between gap-3 pt-5">
              <MaturityBadge maturity={technology.maturity} />
              <ArrowRight className="size-5 shrink-0 text-fg-mute transition group-hover:translate-x-1 group-hover:text-cyan" />
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
