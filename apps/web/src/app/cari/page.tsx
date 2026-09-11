import Link from 'next/link';
import { connection } from 'next/server';
import type { Metadata } from 'next';
import KotakCari from '@/components/KotakCari';
import MaturityBadge from '@/components/MaturityBadge';
import { cari, type Field, type TechnologySummary } from '@/lib/api';
import { mesinPengembang } from '@/lib/lingkungan';

/**
 * `/cari?q=…` — sasaran Bulan 3 di docs/RENCANA-V1.md.
 *
 * Kata kuncinya di URL, bukan di state komponen. Hasil pencarian karena itu bisa
 * ditautkan, di-bookmark, dan dibuka lagi oleh tombol kembali peramban.
 */

type Params = { searchParams: Promise<{ q?: string | string[] }> };

/** `?q=a&q=b` sah menurut HTML; ambil yang pertama, jangan biarkan jadi array. */
function kataKunci(q: string | string[] | undefined): string {
  return (Array.isArray(q) ? q[0] : q)?.trim() ?? '';
}

export async function generateMetadata({ searchParams }: Params): Promise<Metadata> {
  const q = kataKunci((await searchParams).q);

  return {
    title: q ? `Cari: ${q} — TechVerse X` : 'Cari — TechVerse X',

    // Halaman hasil pencarian tidak punya isi sendiri, dan setiap kata kunci
    // melahirkan URL baru. Membiarkannya diindeks berarti menyuruh mesin pencari
    // memetakan ruang kata kunci yang tak terbatas — dan yang dipetakan bukan
    // halaman TechVerse X, melainkan bayangannya.
    robots: { index: false, follow: true },
  };
}

export default async function CariPage({ searchParams }: Params) {
  // Sama seperti FieldGrid dan TechnologyList: tanpa ini Next memanggang
  // halaman saat build, dan yang tersimpan adalah jawaban API pada saat build.
  await connection();

  const q = kataKunci((await searchParams).q);

  return (
    <main className="mx-auto max-w-3xl px-6 py-12">
      <h1 className="text-2xl font-bold tracking-tight">Cari</h1>
      <p className="mt-1 text-sm text-neutral-600 dark:text-neutral-400">
        Bidang dan topik dicari sekaligus. Keduanya punya halaman sendiri.
      </p>

      <div className="mt-4">
        <KotakCari defaultValue={q} autoFocus />
      </div>

      <div className="mt-8">
        <Hasil q={q} />
      </div>
    </main>
  );
}

async function Hasil({ q }: { q: string }) {
  if (q.length === 0) {
    return (
      <p className="text-sm text-neutral-600 dark:text-neutral-400">
        Ketik kata kuncinya di atas. Coba nama bidang (<em>cybersecurity</em>, <em>quantum</em>) atau nama
        teknologi.
      </p>
    );
  }

  const result = await cari(q);

  if (!result.ok) {
    return (
      <div className="rounded-lg border border-amber-300 bg-amber-50 p-4 text-sm dark:border-amber-800 dark:bg-amber-950/40">
        <p className="font-medium">Pencarian belum bisa dijalankan.</p>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">
          {/* Sebabnya dicatat ke log server oleh lib/api.ts, bukan dicetak ke
              pembaca — lihat lib/lingkungan.ts. */}
          {mesinPengembang ? result.reason : 'Coba muat ulang beberapa saat lagi.'}
        </p>
      </div>
    );
  }

  const { fields, technologies, isEmpty } = result.data;

  if (isEmpty) {
    return (
      <div className="rounded-lg border border-neutral-300 bg-white p-4 text-sm dark:border-neutral-700 dark:bg-neutral-900">
        <p className="font-medium">Tidak ada yang cocok dengan “{q}”.</p>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">
          Isi TechVerse X masih dibangun bidang per bidang. Kalau kata kuncinya bidang yang sudah ada
          tapi belum punya topik, halaman bidangnya tetap bisa dibuka dari{' '}
          <Link href="/" className="underline underline-offset-2">
            halaman muka
          </Link>
          .
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-10">
      {fields.length > 0 ? <BidangCocok fields={fields} /> : null}
      {technologies.items.length > 0 ? (
        <TopikCocok items={technologies.items} total={technologies.totalItems} />
      ) : null}
    </div>
  );
}

function BidangCocok({ fields }: { fields: Field[] }) {
  return (
    <section>
      <h2 className="mb-3 text-lg font-semibold">
        {fields.length} bidang
      </h2>
      <ul className="space-y-3">
        {fields.map((field) => (
          <li
            key={field.id}
            className="rounded-lg border border-neutral-200 bg-white p-4 dark:border-neutral-800 dark:bg-neutral-900"
          >
            <h3 className="font-semibold">
              <Link href={`/teknologi/${field.slug}`} className="underline underline-offset-2">
                {field.name}
              </Link>
            </h3>
            <p className="mt-1 text-sm text-neutral-600 dark:text-neutral-400">{field.summary}</p>
            <p className="mt-2 text-xs text-neutral-500">
              {field.topicCount} topik · {field.reviewedTopicCount} diperiksa manusia
            </p>
          </li>
        ))}
      </ul>
    </section>
  );
}

function TopikCocok({ items, total }: { items: TechnologySummary[]; total: number }) {
  return (
    <section>
      <h2 className="mb-3 text-lg font-semibold">
        {total} topik
        {total > items.length ? (
          <span className="ml-2 text-sm font-normal text-neutral-500">
            menampilkan {items.length} teratas
          </span>
        ) : null}
      </h2>
      <ul className="space-y-3">
        {items.map((topic) => (
          <li
            key={topic.id}
            className="rounded-lg border border-neutral-200 bg-white p-4 dark:border-neutral-800 dark:bg-neutral-900"
          >
            <p className="text-xs uppercase tracking-wide text-neutral-500">{topic.fieldName}</p>
            <div className="mt-1 flex flex-wrap items-baseline justify-between gap-2">
              <h3 className="font-semibold">
                <Link href={`/teknologi/${topic.slug}`} className="underline underline-offset-2">
                  {topic.name}
                </Link>
              </h3>

              {/* Aturan keras ADR-012: kematangan isi ikut tampil di mana pun
                  judulnya tampil — termasuk di hasil pencarian. */}
              <MaturityBadge maturity={topic.maturity} />
            </div>
            <p className="mt-1 text-sm text-neutral-600 dark:text-neutral-400">{topic.summary}</p>
          </li>
        ))}
      </ul>
    </section>
  );
}
