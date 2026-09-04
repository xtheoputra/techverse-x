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

      <div className="mb-8 rounded-lg border border-neutral-300 bg-white p-4 text-sm dark:border-neutral-700 dark:bg-neutral-900">
        <p className="font-medium">Tahap: Bulan 1 — taksonomi mendarat.</p>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">
          Empat belas bidang sudah punya rumah dan alamat kanoniknya sendiri (ADR-009, ADR-010).
          Navigasi tujuh bagian aplikasi menyusul; halaman ini masih beranda sementara.
        </p>
        <p className="mt-2 text-neutral-600 dark:text-neutral-400">
          Setiap topik selalu membawa label kematangan isinya. Itu aturan keras ADR-012: halaman yang
          belum diperiksa manusia tidak pernah tampil tanpa mengatakannya.
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
