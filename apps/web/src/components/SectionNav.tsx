'use client';

import { useEffect, useState } from 'react';

export type NavItem = { id: string; label: string };

/**
 * Daftar isi halaman topik: lengket di sisi kiri pada layar lebar, deretan
 * pil yang bisa digeser pada layar sempit.
 *
 * 🔑 **Peningkatan bertahap, bukan syarat.** Yang dirender server adalah daftar
 * tautan jangkar biasa (`<a href="#id">`) — berfungsi tanpa JavaScript, dan itu
 * yang diindeks dan dibaca pembaca layar. Komponen klien ini HANYA menambahkan
 * satu hal: menandai bagian yang sedang dibaca. Kalau JS gagal dimuat, daftar
 * tetap berguna; yang hilang cuma sorotannya.
 */
export default function SectionNav({ items }: { items: NavItem[] }) {
  const [aktif, setAktif] = useState<string | null>(null);
  // Kunci stabil: `items` dibuat ulang di tiap render induk, id-nya tidak.
  const kunci = items.map((i) => i.id).join('|');

  useEffect(() => {
    const ids = kunci.split('|');
    let bingkai = 0;

    // Bagian yang "sedang dibaca" = bagian TERAKHIR yang tepinya sudah melewati garis
    // sepertiga atas layar. Dihitung dari posisi, bukan dari laporan
    // IntersectionObserver: versi pertama memakai pengamat itu dan terukur salah —
    // setelah gulir jauh beberapa bagian melapor sekaligus dan yang paling ATAS
    // menang, jadi daftar menyorot "Mini Project" padahal "Resources" sudah di layar.
    const hitung = () => {
      bingkai = 0;
      const garis = window.innerHeight * 0.32;
      const diDasar = window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 4;

      let terpilih: string | null = null;
      for (const id of ids) {
        const el = document.getElementById(id);
        if (el && el.getBoundingClientRect().top <= garis) terpilih = id;
      }
      // Bagian terakhir bisa lebih pendek daripada layar, jadi tepinya tak pernah
      // melewati garis; di dasar halaman ia tetap dianggap yang dibaca.
      setAktif(diDasar ? (ids[ids.length - 1] ?? terpilih) : terpilih);
    };

    // requestAnimationFrame: satu perhitungan per bingkai, dan setState tidak pernah
    // dipanggil serempak di badan efek.
    const jadwalkan = () => {
      if (bingkai === 0) bingkai = requestAnimationFrame(hitung);
    };

    jadwalkan();
    window.addEventListener('scroll', jadwalkan, { passive: true });
    window.addEventListener('resize', jadwalkan);

    return () => {
      window.removeEventListener('scroll', jadwalkan);
      window.removeEventListener('resize', jadwalkan);
      if (bingkai !== 0) cancelAnimationFrame(bingkai);
    };
  }, [kunci]);

  return (
    <nav aria-label="Di halaman ini" className="min-w-0 lg:sticky lg:top-24 lg:self-start">
      <p className="eyebrow mb-3 hidden lg:block">Di halaman ini</p>
      <ol className="-mx-5 flex gap-2 overflow-x-auto px-5 pb-2 sm:-mx-8 sm:px-8 lg:mx-0 lg:flex-col lg:gap-0 lg:overflow-visible lg:border-l lg:border-line lg:px-0 lg:pb-0">
        {items.map((item, i) => {
          const sedangDibaca = aktif === item.id;
          return (
            <li key={item.id} className="shrink-0">
              <a
                href={`#${item.id}`}
                aria-current={sedangDibaca ? 'location' : undefined}
                className={`flex items-center gap-2.5 rounded-full border px-3.5 py-1.5 text-sm transition lg:-ml-px lg:rounded-none lg:border-0 lg:border-l-2 lg:py-2 ${
                  sedangDibaca
                    ? 'border-cyan/60 bg-cyan/10 text-cyan lg:border-cyan lg:bg-transparent'
                    : 'border-line text-fg-soft hover:text-fg lg:border-transparent lg:hover:border-line-strong'
                }`}
              >
                <span className="font-mono text-[0.7rem] opacity-70">{String(i + 1).padStart(2, '0')}</span>
                {item.label}
              </a>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
