import type { ContentMaturity } from '@/lib/api';

/**
 * Label kematangan isi. Ini penegakan aturan keras ADR-012 di lapisan tampilan:
 * halaman yang belum diperiksa manusia TIDAK PERNAH tampil tanpa mengatakannya.
 *
 * Komponennya sengaja tidak menerima varian "tanpa label". Kalau suatu saat ada
 * yang butuh menampilkan judul tanpa kematangan, ia harus mengubah komponen ini
 * dan mengubahnya akan terlihat di diff — bukan lolos karena satu pemanggil lupa
 * memasangnya.
 *
 * Makna warna, bukan hiasan: biru = kurasi tautan, kuning = draf mesin, hijau =
 * sudah diperiksa manusia. Titiknya bercahaya hanya pada yang sudah diperiksa —
 * satu-satunya klaim kepercayaan produk ini — supaya yang belum, tidak pernah
 * terlihat lebih meyakinkan daripada yang sudah.
 */
const LABELS: Record<ContentMaturity, { text: string; className: string; dot: string }> = {
  Curated: {
    text: 'Kurasi tautan',
    className: 'border-sky/40 bg-sky/10 text-sky',
    dot: 'bg-sky',
  },
  MachineDrafted: {
    text: 'Draf — belum diperiksa manusia',
    className: 'border-amber/40 bg-amber/10 text-amber',
    dot: 'bg-amber',
  },
  HumanReviewed: {
    text: 'Sudah diperiksa manusia',
    className: 'border-mint/50 bg-mint/10 text-mint shadow-[0_0_24px_-6px_rgb(52_211_153/0.7)]',
    dot: 'bg-mint shadow-[0_0_8px_2px_rgb(52_211_153/0.8)]',
  },
};

export default function MaturityBadge({ maturity }: { maturity: ContentMaturity }) {
  const label = LABELS[maturity];

  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-medium ${label.className}`}
    >
      <span className={`size-1.5 rounded-full ${label.dot}`} aria-hidden="true" />
      {label.text}
    </span>
  );
}
