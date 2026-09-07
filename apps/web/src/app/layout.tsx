import type { Metadata } from 'next';
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
        {children}
      </body>
    </html>
  );
}
