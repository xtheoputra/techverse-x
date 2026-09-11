/**
 * Kotak pencarian — sasaran Bulan 3 di docs/RENCANA-V1.md, yang ukuran
 * selesainya berbunyi *"orang bisa menemukan halaman tanpa menebak URL"*.
 *
 * 🔑 **`<form>` biasa, bukan komponen klien.** Tidak ada `useState`, tidak ada
 * `onSubmit`, tidak ada `'use client'`: peramban sendiri yang menyusun
 * `/cari?q=…` dari `method="get"`. Konsekuensinya bukan sekadar bundel yang
 * lebih kecil — kotak ini **bekerja sebelum satu baris JavaScript pun dimuat**,
 * dan itu keadaan yang sering terjadi di tier gratis (ADR-019).
 *
 * Dan karena hasilnya sebuah URL, hasil pencarian bisa ditautkan, di-bookmark,
 * dan dibuka kembali oleh tombol "kembali" peramban. Kotak berbasis state tidak
 * memberi satu pun dari ketiganya tanpa kerja tambahan.
 */
export default function KotakCari({
  defaultValue = '',
  autoFocus = false,
}: {
  defaultValue?: string;
  autoFocus?: boolean;
}) {
  return (
    <form action="/cari" method="get" role="search" className="flex gap-2">
      <label htmlFor="q" className="sr-only">
        Cari bidang atau topik
      </label>
      <input
        id="q"
        name="q"
        type="search"
        defaultValue={defaultValue}
        autoFocus={autoFocus}
        placeholder="Cari bidang atau topik…"
        className="min-w-0 flex-1 rounded-md border border-neutral-300 bg-white px-3 py-1.5 text-sm outline-none placeholder:text-neutral-400 focus:border-neutral-500 dark:border-neutral-700 dark:bg-neutral-900 dark:focus:border-neutral-400"
      />
      <button
        type="submit"
        className="shrink-0 rounded-md border border-neutral-300 bg-white px-3 py-1.5 text-sm font-medium hover:bg-neutral-100 dark:border-neutral-700 dark:bg-neutral-900 dark:hover:bg-neutral-800"
      >
        Cari
      </button>
    </form>
  );
}
