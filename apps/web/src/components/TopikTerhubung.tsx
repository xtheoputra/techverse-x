import Link from 'next/link';
import { ArrowRight } from '@/components/Icons';
import MaturityBadge from '@/components/MaturityBadge';
import type { TechnologyDetail, TechnologySummary } from '@/lib/api';

/**
 * Relasi antar-topik pada satu halaman topik — Knowledge Graph V1 (ADR-023).
 *
 * 🔑 **BUKAN bagian template ADR-012.** Karena itu ia tidak tinggal di
 * `TopicSections`, tidak pernah masuk `missingSections`, dan dirender SESUDAH
 * kelima bagian itu. Langkah 0 roadmap tetap satu-satunya prasyarat PROSA —
 * keterampilan yang belum tentu punya halaman di situs ini — sedangkan daftar di
 * sini navigasi ke halaman yang ada. Topik yang sudah jadi tujuan sisi tidak
 * diulang di langkah 0 (Pembaruan ADR-012).
 *
 * ⚠️ **Disembunyikan seluruhnya kalau kosong — pengecualian yang disengaja** dari
 * aturan "bagian kosong tetap ditampilkan, dan dikatakan kosong" di
 * `TopicSections`. Aturan itu ada supaya halaman setengah jadi tidak terlihat
 * utuh. Relasi bukan sesuatu yang WAJIB diisi, jadi baris "belum ada relasi"
 * akan menandai setiap halaman tidak lengkap secara palsu — dan di produksi, yang
 * nol topik, justru blok ini yang tidak pernah boleh terlihat.
 *
 * 🔴 Strukturnya dijaga pemindai halaman jadi (periksa-halaman-web.mjs): sebuah
 * `<section aria-labelledby="topik-terhubung">`, sub-judul `<h3>` "Pelajari lebih
 * dulu" / "Dibutuhkan oleh", dan setiap sub-daftar berisi `<li>`. Mengubah tampilan
 * boleh; mengubah kerangka itu memerahkan CI.
 */
export default function TopikTerhubung({ topic }: { topic: TechnologyDetail }) {
  if (topic.requires.length === 0 && topic.requiredBy.length === 0) {
    return null;
  }

  return (
    <section id="terhubung" aria-labelledby="topik-terhubung" className="glass reveal mt-8 p-6 sm:p-8">
      <header className="mb-6 flex items-center gap-4">
        <span aria-hidden="true" className="font-pixel text-3xl leading-none text-cyan/80">
          06
        </span>
        <h2 id="topik-terhubung" className="text-2xl font-semibold tracking-tight">
          Topik terhubung
        </h2>
        <span aria-hidden="true" className="hidden h-px flex-1 bg-gradient-to-r from-cyan/40 to-transparent sm:block" />
      </header>

      <div className="space-y-6">
        {topic.requires.length > 0 ? (
          <Daftar judul="Pelajari lebih dulu" topik={topic.requires} bidangHalaman={topic.fieldSlug} />
        ) : null}

        {topic.requiredBy.length > 0 ? (
          <Daftar judul="Dibutuhkan oleh" topik={topic.requiredBy} bidangHalaman={topic.fieldSlug} />
        ) : null}
      </div>
    </section>
  );
}

function Daftar({
  judul,
  topik,
  bidangHalaman,
}: {
  judul: string;
  topik: TechnologySummary[];
  bidangHalaman: string;
}) {
  return (
    <div>
      <h3 className="text-sm font-medium text-fg-mute">{judul}</h3>
      <ul className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
        {topik.map((t) => (
          <li
            key={t.id}
            className="group relative rounded-xl border border-line bg-surface p-4 transition hover:border-cyan/45 hover:bg-surface-strong"
          >
            <div className="flex items-start justify-between gap-3">
              <Link href={`/teknologi/${t.slug}`} className="stretched-link font-semibold leading-snug">
                {t.name}
              </Link>
              <ArrowRight className="mt-0.5 size-4 shrink-0 text-fg-mute transition group-hover:translate-x-1 group-hover:text-cyan" />
            </div>

            {/* Nama bidang hanya kalau BERBEDA dari bidang halaman ini: di bidang
                yang sama ia cuma mengulang remah roti di atas. */}
            {t.fieldSlug !== bidangHalaman ? (
              <p className="mt-1 text-sm text-fg-mute">{t.fieldName}</p>
            ) : null}

            {/* ADR-012: judul yang ditautkan tidak pernah tampil tanpa label
                kematangannya SENDIRI - bukan label halaman yang menautkannya. */}
            <p className="mt-3">
              <MaturityBadge maturity={t.maturity} />
            </p>
          </li>
        ))}
      </ul>
    </div>
  );
}
