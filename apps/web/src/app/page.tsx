import { Suspense } from 'react';
import TechnologyList from '@/components/TechnologyList';

export default function Home() {
  return (
    <main className="mx-auto max-w-5xl px-6 py-12">
      <header className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">TechVerse X</h1>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">Explore. Learn. Build. Innovate.</p>
      </header>

      <div className="mb-8 rounded-lg border border-neutral-300 bg-white p-4 text-sm dark:border-neutral-700 dark:bg-neutral-900">
        <p className="font-medium">Tahap: Fase 1 — kerangka kode.</p>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">
          Navigasi utama sengaja belum dibuat. Tulang punggungnya — bidang teknologi atau fungsi
          aplikasi — masih keputusan terbuka (Issue #1), dan menebaknya sekarang berarti membangun
          layar yang harus dibongkar lagi.
        </p>
      </div>

      {/* fetch di Next.js 16 menahan render; Suspense membuat kerangka halaman
          tetap tampil sementara daftarnya menyusul. */}
      <Suspense fallback={<p className="text-sm text-neutral-500">Memuat daftar teknologi…</p>}>
        <TechnologyList />
      </Suspense>
    </main>
  );
}
