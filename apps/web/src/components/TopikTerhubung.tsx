import Link from 'next/link';
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
 */
export default function TopikTerhubung({ topic }: { topic: TechnologyDetail }) {
  if (topic.requires.length === 0 && topic.requiredBy.length === 0) {
    return null;
  }

  return (
    <section aria-labelledby="topik-terhubung" className="mt-8">
      <h2 id="topik-terhubung" className="mb-2 text-lg font-semibold">
        Topik terhubung
      </h2>

      {topic.requires.length > 0 ? (
        <Daftar judul="Pelajari lebih dulu" topik={topic.requires} bidangHalaman={topic.fieldSlug} />
      ) : null}

      {topic.requiredBy.length > 0 ? (
        <Daftar judul="Dibutuhkan oleh" topik={topic.requiredBy} bidangHalaman={topic.fieldSlug} />
      ) : null}
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
    <div className="mt-3">
      <h3 className="text-sm font-medium text-neutral-600 dark:text-neutral-400">{judul}</h3>
      <ul className="mt-2 space-y-2">
        {topik.map((t) => (
          <li key={t.id} className="flex flex-wrap items-baseline gap-x-2 gap-y-1">
            <Link href={`/teknologi/${t.slug}`} className="font-medium underline underline-offset-2">
              {t.name}
            </Link>

            {/* Nama bidang hanya kalau BERBEDA dari bidang halaman ini: di bidang
                yang sama ia cuma mengulang remah roti di atas. */}
            {t.fieldSlug !== bidangHalaman ? (
              <span className="text-sm text-neutral-500">· {t.fieldName}</span>
            ) : null}

            {/* ADR-012: judul yang ditautkan tidak pernah tampil tanpa label
                kematangannya SENDIRI - bukan label halaman yang menautkannya. */}
            <MaturityBadge maturity={t.maturity} />
          </li>
        ))}
      </ul>
    </div>
  );
}
