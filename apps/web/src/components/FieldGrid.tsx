import Link from 'next/link';
import { connection } from 'next/server';
import { ArrowRight, FieldIcon } from '@/components/Icons';
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

/** Warna ikon mengikuti prioritas: cyan–violet untuk inti, violet untuk pendukung, netral untuk pinggiran. */
const PRIORITY_ICON = {
  Core: 'border-cyan/40 bg-gradient-to-br from-cyan/20 to-violet/20 text-cyan',
  Supporting: 'border-violet/35 bg-violet/10 text-violet',
  Peripheral: 'border-line bg-surface text-fg-soft',
} as const;

export default async function FieldGrid() {
  // Sama seperti TechnologyList: tanpa ini Next.js 16 memanggang halaman saat
  // build, dan yang tersimpan adalah jawaban API pada saat build.
  await connection();

  const result = await listFields();

  if (!result.ok) {
    return (
      <div className="glass border-amber/40 p-5 text-sm">
        <p className="font-medium text-amber">Daftar bidang belum bisa diambil.</p>
        <p className="mt-1 text-fg-soft">
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
    <section id="bidang" aria-labelledby="judul-bidang">
      <div className="mb-8 flex flex-wrap items-end justify-between gap-x-6 gap-y-2">
        <div>
          <p className="eyebrow">Jelajahi</p>
          <h2 id="judul-bidang" className="mt-2 text-3xl font-semibold tracking-tight sm:text-4xl">
            {fields.length} bidang teknologi
          </h2>
        </div>
        <p className="text-sm text-fg-soft">
          <strong className="font-mono text-base text-mint">{reviewed}</strong> topik sudah diperiksa manusia
        </p>
      </div>

      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {fields.map((field, i) => (
          <li
            key={field.id}
            style={{ '--d': `${Math.min(i, 8) * 0.04}s` } as React.CSSProperties}
            className="glass glass-hover reveal group relative flex flex-col p-5"
          >
            <div className="flex items-start justify-between gap-3">
              <span className={`grid size-12 place-items-center rounded-xl border ${PRIORITY_ICON[field.priority]}`}>
                <FieldIcon slug={field.slug} className="size-6" />
              </span>
              <span className="rounded-full border border-line px-2.5 py-0.5 text-[0.7rem] text-fg-mute">
                {PRIORITY_LABEL[field.priority]}
              </span>
            </div>

            <h3 className="mt-4 text-lg font-semibold tracking-tight">
              {/* Alamatnya berhenti jadi teks mati: rute /teknologi/<slug> kini ada.
                  Tautannya membentang di seluruh kartu (stretched-link), jadi satu
                  tautan tetap satu sasaran klik besar. */}
              <Link href={`/teknologi/${field.slug}`} className="stretched-link">
                {field.name}
              </Link>
            </h3>
            <p className="mt-1.5 line-clamp-3 text-sm leading-relaxed text-fg-soft">{field.summary}</p>

            <div className="mt-auto flex items-center justify-between pt-5">
              <p className="font-mono text-xs text-fg-mute">
                {field.topicCount} topik · {field.reviewedTopicCount} diperiksa manusia
              </p>
              <ArrowRight className="size-5 text-fg-mute transition group-hover:translate-x-1 group-hover:text-cyan" />
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
