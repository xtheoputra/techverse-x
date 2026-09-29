import { Suspense } from 'react';
import FieldGrid from '@/components/FieldGrid';
import TechnologyList from '@/components/TechnologyList';

export default function Home() {
  return (
    <main className="mx-auto max-w-5xl px-6 py-12">
      <header className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">TechVerse X</h1>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">Explore. Learn. Build. Innovate.</p>
      </header>

      {/*
        🔴 Aturan teks pembaca ADR-024: teks yang tercetak di produksi tidak memuat
        bulan rencana, janji tanpa tanggal, nomor ADR, perintah pengembang, atau
        alamat internal. Spanduk ini dulu berbunyi "Tahap: Bulan 1 — taksonomi
        mendarat" dan "Navigasi tujuh bagian aplikasi menyusul; halaman ini masih
        beranda sementara". Yang pertama basi dalam kurang dari dua minggu; yang
        kedua janji tanpa tanggal yang tampil di keadaan NORMAL produksi. Nomor ADR
        menunjuk pembaca ke catatan keputusan pengembang, bukan ke isi - dan sampai
        2026-09-28 ke repositori privat yang bahkan tidak bisa mereka buka.

        Tidak ada uji JS yang menjaganya. Penjaganya pemindai teks yang TERLIHAT di
        build produksi (.github/scripts/periksa-halaman-web.mjs): ditulis ke job
        `citra` CI 2026-09-24, dan benar-benar berjalan di sana sejak #57 terjawab
        2026-09-28 - bukan lagi dijalankan tangan (ADR-024).
      */}
      <div className="mb-8 rounded-lg border border-neutral-300 bg-white p-4 text-sm dark:border-neutral-700 dark:bg-neutral-900">
        <p className="text-neutral-600 dark:text-neutral-400">
          Setiap topik selalu membawa label kematangan isinya: halaman yang belum diperiksa manusia tidak
          pernah tampil tanpa mengatakannya.
        </p>
      </div>

      {/* fetch di Next.js 16 menahan render; Suspense membuat kerangka halaman
          tetap tampil sementara isinya menyusul. Dua Suspense terpisah supaya
          daftar bidang tidak menunggu daftar topik, dan sebaliknya. */}
      <div className="space-y-10">
        <Suspense fallback={<p className="text-sm text-neutral-500">Memuat bidang…</p>}>
          <FieldGrid />
        </Suspense>

        <Suspense fallback={<p className="text-sm text-neutral-500">Memuat daftar topik…</p>}>
          <TechnologyList />
        </Suspense>
      </div>
    </main>
  );
}
