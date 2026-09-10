import Link from 'next/link';
import { connection } from 'next/server';
import { listFields } from '@/lib/api';
import { mesinPengembang } from '@/lib/lingkungan';

/**
 * Empat belas bidang ADR-010 — sasaran Bulan 1 di docs/RENCANA-V1.md.
 *
 * Tiap kartu membawa DUA angka, dan itu disengaja: berapa topik yang ADA, dan
 * berapa yang sudah DIPERIKSA MANUSIA. Angka pertama bisa dinaikkan mesin dalam
 * semenit; angka kedua adalah ukuran kemajuan proyek yang sebenarnya.
 */

/**
 * Label prioritas ADR-010 — dan perhatikan KATA "target"-nya.
 *
 * 🔴 Tanpa kata itu, kartunya BERBOHONG. `FieldPriority` menyatakan kematangan
 * TERTINGGI YANG DIJANJIKAN untuk topik di bawah sebuah bidang, bukan kematangan
 * yang sudah dicapai. Ketika labelnya berbunyi "Ditulis manusia", enam kartu Core
 * memasang klaim itu tepat di atas barisnya sendiri yang berbunyi
 * "0 topik · 0 diperiksa manusia" — dua kalimat yang saling membantah di satu
 * kartu, dan klaim yang persis dilarang ADR-012.
 *
 * Ini ketahuan dari MENJALANKAN halamannya, bukan dari uji: tidak ada satu pun
 * uji yang membaca teks kartu ini. Selama semua bidang masih nol topik, itulah
 * yang akan dibaca pengunjung pertama di URL publik (#40).
 */
const PRIORITY_LABEL = {
  Core: 'Target: ditulis manusia',
  Supporting: 'Target: draf',
  Peripheral: 'Target: kurasi tautan',
} as const;

export default async function FieldGrid() {
  // Sama seperti TechnologyList: tanpa ini Next.js 16 memanggang halaman saat
  // build, dan yang tersimpan adalah jawaban API pada saat build.
  await connection();

  const result = await listFields();

  if (!result.ok) {
    return (
      <div className="rounded-lg border border-amber-300 bg-amber-50 p-4 text-sm dark:border-amber-800 dark:bg-amber-950/40">
        <p className="font-medium">Daftar bidang belum bisa diambil.</p>
        <p className="mt-1 text-neutral-600 dark:text-neutral-400">
          {/* `result.reason` menyebut alamat API internal - bahan diagnosis, bukan
              kalimat untuk pembaca. Lihat lib/lingkungan.ts. */}
          {mesinPengembang ? result.reason : 'Coba muat ulang beberapa saat lagi.'}
        </p>
      </div>
    );
  }

  const fields = result.data;
  const reviewed = fields.reduce((total, field) => total + field.reviewedTopicCount, 0);

  return (
    <section>
      <div className="mb-3 flex flex-wrap items-baseline justify-between gap-2">
        <h2 className="text-lg font-semibold">{fields.length} bidang teknologi</h2>
        <p className="text-sm text-neutral-600 dark:text-neutral-400">
          <strong>{reviewed}</strong> topik sudah diperiksa manusia
        </p>
      </div>

      <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {fields.map((field) => (
          <li
            key={field.id}
            className="rounded-lg border border-neutral-200 bg-white p-4 dark:border-neutral-800 dark:bg-neutral-900"
          >
            <div className="flex items-baseline justify-between gap-2">
              <h3 className="font-semibold">
                <Link href={`/teknologi/${field.slug}`} className="underline underline-offset-2">
                  {field.name}
                </Link>
              </h3>
              <span className="shrink-0 text-xs text-neutral-500">{PRIORITY_LABEL[field.priority]}</span>
            </div>
            <p className="mt-1 text-sm text-neutral-600 dark:text-neutral-400">{field.summary}</p>
            <p className="mt-2 text-xs text-neutral-500">
              {field.topicCount} topik · {field.reviewedTopicCount} diperiksa manusia
            </p>
            {/* Alamatnya berhenti jadi teks mati: rute /teknologi/<slug> kini ada. */}
            <p className="mt-1 font-mono text-xs text-neutral-400">/teknologi/{field.slug}</p>
          </li>
        ))}
      </ul>
    </section>
  );
}
