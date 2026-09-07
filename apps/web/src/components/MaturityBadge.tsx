import type { ContentMaturity } from '@/lib/api';

/**
 * Label kematangan isi. Ini penegakan aturan keras ADR-012 di lapisan tampilan:
 * halaman yang belum diperiksa manusia TIDAK PERNAH tampil tanpa mengatakannya.
 *
 * Komponennya sengaja tidak menerima varian "tanpa label". Kalau suatu saat ada
 * yang butuh menampilkan judul tanpa kematangan, ia harus mengubah komponen ini
 * dan mengubahnya akan terlihat di diff — bukan lolos karena satu pemanggil lupa
 * memasangnya.
 */
const LABELS: Record<ContentMaturity, { text: string; className: string }> = {
  Curated: {
    text: 'Kurasi tautan',
    className: 'border-sky-300 bg-sky-50 text-sky-900 dark:border-sky-800 dark:bg-sky-950/50 dark:text-sky-200',
  },
  MachineDrafted: {
    text: 'Draf — belum diperiksa manusia',
    className:
      'border-amber-300 bg-amber-50 text-amber-900 dark:border-amber-800 dark:bg-amber-950/50 dark:text-amber-200',
  },
  HumanReviewed: {
    text: 'Sudah diperiksa manusia',
    className:
      'border-emerald-300 bg-emerald-50 text-emerald-900 dark:border-emerald-800 dark:bg-emerald-950/50 dark:text-emerald-200',
  },
};

export default function MaturityBadge({ maturity }: { maturity: ContentMaturity }) {
  const label = LABELS[maturity];

  return (
    <span className={`inline-block rounded border px-1.5 py-0.5 text-xs ${label.className}`}>{label.text}</span>
  );
}
