import Link from 'next/link';
import type { Metadata } from 'next';
import KotakCari from '@/components/KotakCari';
import './globals.css';

export const metadata: Metadata = {
  title: 'TechVerse X',
  description: 'Explore. Learn. Build. Innovate.',
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="id">
      <body className="min-h-screen bg-neutral-50 text-neutral-900 antialiased dark:bg-neutral-950 dark:text-neutral-100">
        {/*
          Kotak cari duduk di layout, bukan di halaman muka.

          🔑 Ukuran selesai Bulan 3 (docs/RENCANA-V1.md) berbunyi "orang bisa
          menemukan halaman tanpa menebak URL" — dan orang tidak selalu MASUK
          lewat halaman muka. Mesin pencari, tautan yang dibagikan, dan bookmark
          semuanya mendarat langsung di halaman topik. Kotak yang cuma ada di
          beranda menuntut pengunjung kembali ke sana lebih dulu, yaitu persis
          menebak-nebak yang mau dihilangkan.
        */}
        <header className="border-b border-neutral-200 bg-white/80 backdrop-blur dark:border-neutral-800 dark:bg-neutral-950/80">
          <div className="mx-auto flex max-w-5xl flex-wrap items-center gap-x-6 gap-y-3 px-6 py-3">
            <Link href="/" className="font-semibold tracking-tight">
              TechVerse X
            </Link>
            <div className="ml-auto w-full sm:w-auto sm:min-w-72">
              <KotakCari />
            </div>
          </div>
        </header>

        {children}
      </body>
    </html>
  );
}
