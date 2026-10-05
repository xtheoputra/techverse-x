import Link from 'next/link';
import { connection } from 'next/server';
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
      <div className="rounded-lg border border-amber-300 bg-amber-50 p-4 text-sm dark:border-amber-800 dark:bg-amber-950/40">
        {mesinPengembang ? (
          <>
            {/* Frasa ini dikutip sebagai gejala di issue #40 - jangan diubah
                tanpa ikut menyapu issue itu. */}
            <p className="font-medium">API belum bisa dihubungi.</p>
            <p className="mt-1 text-neutral-600 dark:text-neutral-400">{result.reason}</p>
            <p className="mt-2 text-neutral-600 dark:text-neutral-400">
              Jalankan <code className="rounded bg-neutral-200 px-1 py-0.5 dark:bg-neutral-800">run.ps1 up</code> lalu{' '}
              <code className="rounded bg-neutral-200 px-1 py-0.5 dark:bg-neutral-800">run.ps1 api</code>.
            </p>
          </>
        ) : (
          <>
            <p className="font-medium">Daftar topik belum bisa dimuat.</p>
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
            <p className="mt-1 text-neutral-600 dark:text-neutral-400">
              Coba muat ulang beberapa saat lagi.
            </p>
          </>
        )}
      </div>
    );
  }

  const { items, totalItems } = result.data;

  if (items.length === 0) {
    return (
      <div className="rounded-lg border border-neutral-300 bg-white p-4 text-sm dark:border-neutral-700 dark:bg-neutral-900">
        <p className="font-medium">Belum ada satu topik pun.</p>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">
          {mesinPengembang ? (
            <>
              Isi contoh: <code className="rounded bg-neutral-200 px-1 py-0.5 dark:bg-neutral-800">run.ps1 seed</code>
            </>
          ) : (
            <>Itu keadaan yang jujur, bukan galat — Bulan 1 mengejar taksonominya lebih dulu.</>
          )}
        </p>
      </div>
    );
  }

  return (
    <section>
      <h2 className="mb-3 text-lg font-semibold">{totalItems} topik tercatat</h2>
      <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {items.map((technology) => (
          <li
            key={technology.id}
            className="rounded-lg border border-neutral-200 bg-white p-4 dark:border-neutral-800 dark:bg-neutral-900"
          >
            <p className="text-xs uppercase tracking-wide text-neutral-500">{technology.fieldName}</p>
            <h3 className="mt-1 font-semibold">
              <Link href={`/teknologi/${technology.slug}`} className="underline underline-offset-2">
                {technology.name}
              </Link>
            </h3>
            <p className="mt-1 text-sm text-neutral-600 dark:text-neutral-400">{technology.summary}</p>

            {/* Aturan keras ADR-012: kematangan isi selalu ikut tampil. Kontrak
                API-nya sendiri yang menjamin datanya ada — lihat
                TechnologySummaryResponse. */}
            <p className="mt-2">
              <MaturityBadge maturity={technology.maturity} />
            </p>

            <p className="mt-2 font-mono text-xs text-neutral-400">/teknologi/{technology.slug}</p>
          </li>
        ))}
      </ul>
    </section>
  );
}
