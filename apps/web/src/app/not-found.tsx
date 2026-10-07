import Link from 'next/link';

export default function NotFound() {
  return (
    <main className="mx-auto flex min-h-[60vh] max-w-3xl flex-col items-start justify-center px-5 py-16 sm:px-8">
      <p className="font-pixel text-7xl leading-none text-cyan/80 sm:text-8xl">404</p>
      <h1 className="mt-6 text-3xl font-semibold tracking-tight sm:text-4xl">Halaman ini tidak ada.</h1>
      <p className="prose-tv mt-4 max-w-[52ch]">
        Periksa alamatnya, atau mulai dari{' '}
        <Link href="/">halaman muka</Link> atau <Link href="/cari">pencarian</Link>.
      </p>
    </main>
  );
}
