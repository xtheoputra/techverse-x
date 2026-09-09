import Link from 'next/link';
import { connection } from 'next/server';
import MaturityBadge from '@/components/MaturityBadge';
import { searchTechnologies } from '@/lib/api';

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
        <p className="font-medium">API belum bisa dihubungi.</p>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">{result.reason}</p>
        <p className="mt-2 text-neutral-600 dark:text-neutral-400">
          Jalankan <code className="rounded bg-neutral-200 px-1 py-0.5 dark:bg-neutral-800">run.ps1 up</code> lalu{' '}
          <code className="rounded bg-neutral-200 px-1 py-0.5 dark:bg-neutral-800">run.ps1 api</code>.
        </p>
      </div>
    );
  }

  const { items, totalItems } = result.data;

  if (items.length === 0) {
    return (
      <div className="rounded-lg border border-neutral-300 bg-white p-4 text-sm dark:border-neutral-700 dark:bg-neutral-900">
        <p className="font-medium">Belum ada satu topik pun.</p>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">
          Isi contoh: <code className="rounded bg-neutral-200 px-1 py-0.5 dark:bg-neutral-800">run.ps1 seed</code>
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
