import Link from 'next/link';
import type { Metadata } from 'next';
import { GeistMono } from 'geist/font/mono';
import { GeistSans } from 'geist/font/sans';
import { GeistPixelSquare } from 'geist/font/pixel';
import KotakCari from '@/components/KotakCari';
import { LogoMark } from '@/components/Icons';
import './globals.css';

export const metadata: Metadata = {
  title: 'TechVerse X',
  description: 'Explore. Learn. Build. Innovate.',
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="id" className={`${GeistSans.variable} ${GeistMono.variable} ${GeistPixelSquare.variable}`}>
      <body className="min-h-screen font-sans text-fg antialiased">
        {/* Latar: satu elemen tetap di belakang segalanya (lihat globals.css). */}
        <div className="aurora" aria-hidden="true">
          <div className="aurora-blob aurora-blob--a" />
          <div className="aurora-blob aurora-blob--b" />
        </div>

        <a
          href="#isi"
          className="sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded-lg focus:bg-bg-raised focus:px-4 focus:py-2 focus:text-sm focus:text-fg focus:ring-2 focus:ring-cyan"
        >
          Lompat ke isi halaman
        </a>

        {/*
          Kotak cari duduk di layout, bukan di halaman muka.

          🔑 Ukuran selesai Bulan 3 (docs/RENCANA-V1.md) berbunyi "orang bisa
          menemukan halaman tanpa menebak URL" — dan orang tidak selalu MASUK
          lewat halaman muka. Mesin pencari, tautan yang dibagikan, dan bookmark
          semuanya mendarat langsung di halaman topik. Kotak yang cuma ada di
          beranda menuntut pengunjung kembali ke sana lebih dulu, yaitu persis
          menebak-nebak yang mau dihilangkan.

          ⚠️ Kepala halaman SENGAJA cuma merek + kotak cari (ADR-024). Tautan ke
          salah satu bagian ADR-009 (/explore, /learn, /labs, ...) masuk ke sini
          HANYA di PR yang membuktikan halamannya punya isi sungguhan dalam bentuk
          produksi - docker-compose.prod.yml, tulis tertutup. Menu yang lengkap
          tapi kosong tidak bisa dipakai siapa pun (RENCANA-V1). typedRoutes di
          next.config.ts menjaga separuhnya: tautan ke rute yang tidak ada gagal
          `next build`. Separuh "halamannya berisi" tidak dijaga kode apa pun.
          Desain baru (ADR-028) tidak mengubah aturan ini: ia hanya mengganti
          kulitnya.
        */}
        <header className="sticky top-0 z-40 border-b border-line bg-bg/55 backdrop-blur-xl">
          <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-x-6 gap-y-3 px-5 py-3 sm:px-8">
            <Link href="/" className="group flex items-center gap-2.5">
              <LogoMark className="size-8 transition-transform duration-500 group-hover:rotate-[60deg]" />
              <span className="text-[1.05rem] font-semibold tracking-tight">
                TechVerse <span className="gradient-text">X</span>
              </span>
            </Link>
            <div className="ml-auto w-full sm:w-auto sm:min-w-80">
              <KotakCari />
            </div>
          </div>
        </header>

        <div id="isi">{children}</div>

        <footer className="mt-24 border-t border-line">
          <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-4 px-5 py-8 sm:px-8">
            <div className="flex items-center gap-2.5 text-fg-soft">
              <LogoMark className="size-6 opacity-80" />
              <span className="text-sm">TechVerse X</span>
            </div>
            <p className="font-mono text-xs uppercase tracking-[0.2em] text-fg-mute">
              Explore · Learn · Build · Innovate
            </p>
          </div>
        </footer>
      </body>
    </html>
  );
}
