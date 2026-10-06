import Link from 'next/link';

/**
 * 404 untuk seluruh situs: setiap URL yang tidak cocok dengan rute mana pun —
 * termasuk alamat bagian ADR-009 yang belum dibangun, yang pasti akan ditebak
 * orang (`/learn`, `/explore`) — DAN `notFound()` dari `/teknologi/[slug]`.
 * Sampai 2026-09-17 keduanya jatuh ke halaman bawaan Next berbahasa Inggris.
 *
 * ⚠️ **Kedua jalan itu tidak dirender sama, dan itu diukur, bukan diduga.** URL
 * yang tidak cocok (`/learn`) dirender server utuh: 404, kalimatnya ada di HTML.
 * `notFound()` dari `/teknologi/[slug]` juga 404 — tapi HTML-nya kerangka galat
 * Next (`<html id="__next_error__">`) dengan badan KOSONG, dan halaman ini baru
 * tergambar dari payload RSC sesudah JavaScript jalan. Tanpa JavaScript yang
 * terlihat hanya judul tab. Bentuk itu sudah sama persis sebelum berkas ini ada
 * (dulu dengan teks bawaan Inggris), dan harganya disengaja Next: status 404
 * sungguhan hanya bisa dikirim SEBELUM badan mulai dialirkan. `noindex` ada di
 * keduanya.
 *
 * 🔑 **Statis: tanpa fetch, tanpa kotak cari sendiri** (kepala halaman di layout
 * sudah punya). Halaman ini dirender justru saat ada yang salah; kalau ia ikut
 * memanggil API, 404 yang sah bisa berubah jadi galat setiap kali instance API
 * sedang tidur (ADR-019).
 *
 * ⚠️ Tidak menyebut bagian yang belum ada, dan tidak menjanjikan kapan ada —
 * aturan teks pembaca ADR-024.
 */
export default function NotFound() {
  return (
    <main className="mx-auto max-w-3xl px-6 py-12">
      <h1 className="text-2xl font-bold tracking-tight">Halaman ini tidak ada.</h1>
      <p className="mt-2 text-neutral-600 dark:text-neutral-400">
        Periksa alamatnya, atau mulai dari{' '}
        <Link href="/" className="underline underline-offset-2">
          halaman muka
        </Link>{' '}
        atau{' '}
        <Link href="/cari" className="underline underline-offset-2">
          pencarian
        </Link>
        .
      </p>
    </main>
  );
}
