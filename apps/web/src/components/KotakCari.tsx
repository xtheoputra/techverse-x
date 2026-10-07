import { SearchIcon } from '@/components/Icons';

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
 *
 * ⚠️ **`id` dan `name` sengaja dua hal yang berbeda.** `name` selalu `q`, sebab
 * itu nama parameter URL-nya. `id` harus unik per halaman: `/cari` memasang dua
 * kotak — satu di kepala halaman, satu di badannya — dan sampai 2026-09-17
 * keduanya ber-`id="q"`, jadi `<label htmlFor>` milik kotak kedua menunjuk input
 * PERTAMA. Diukur di HTML jadinya (dua `id="q"`), bukan ditebak.
 *
 * Tampilan: kapsul kaca dengan ikon kaca pembesar. Tombolnya tetap ada (tidak
 * hanya ikon) supaya pengiriman lewat papan ketik dan pembaca layar jelas.
 */
export default function KotakCari({
  id = 'q',
  defaultValue = '',
  autoFocus = false,
  besar = false,
}: {
  id?: string;
  defaultValue?: string;
  autoFocus?: boolean;
  /** Ukuran hero: lebih tinggi, huruf lebih besar. */
  besar?: boolean;
}) {
  return (
    <form
      action="/cari"
      method="get"
      role="search"
      className={`group flex items-center gap-2 rounded-full border border-line bg-surface pl-4 pr-1.5 transition focus-within:border-cyan/60 focus-within:shadow-[0_0_0_4px_rgb(34_211_238/0.12),0_0_40px_-8px_rgb(34_211_238/0.5)] ${
        besar ? 'py-2' : 'py-1'
      }`}
    >
      <label htmlFor={id} className="sr-only">
        Cari bidang atau topik
      </label>
      <SearchIcon className={`shrink-0 text-fg-mute transition group-focus-within:text-cyan ${besar ? 'size-6' : 'size-[18px]'}`} />
      <input
        id={id}
        name="q"
        type="search"
        defaultValue={defaultValue}
        autoFocus={autoFocus}
        placeholder="Cari bidang atau topik…"
        className={`min-w-0 flex-1 bg-transparent text-fg outline-none placeholder:text-fg-mute ${
          besar ? 'px-1 py-1.5 text-lg' : 'px-1 py-1 text-sm'
        }`}
      />
      <button
        type="submit"
        className={`shrink-0 rounded-full bg-gradient-to-r from-cyan to-violet font-medium text-[#04050b] transition hover:brightness-110 active:scale-95 ${
          besar ? 'px-6 py-2.5 text-base' : 'px-4 py-1.5 text-sm'
        }`}
      >
        Cari
      </button>
    </form>
  );
}
