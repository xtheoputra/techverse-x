import { Suspense } from 'react';
import FieldGrid from '@/components/FieldGrid';
import HeroOrbit from '@/components/HeroOrbit';
import KotakCari from '@/components/KotakCari';
import TechnologyList from '@/components/TechnologyList';

export default function Home() {
  return (
    <main>
      {/* ------------------------------------------------------------- hero */}
      <section className="relative mx-auto grid max-w-6xl grid-cols-1 items-center gap-10 px-5 pb-16 pt-14 sm:px-8 lg:grid-cols-[1.15fr_0.85fr] lg:pb-24 lg:pt-24">
        <div>
          <p className="eyebrow rise">Explore · Learn · Build · Innovate</p>
          <h1 className="rise mt-5 text-[2.6rem] font-semibold leading-[1.04] tracking-tight [--d:0.08s] sm:text-6xl lg:text-[4.25rem]">
            Peta belajar <span className="gradient-text">teknologi masa depan</span>
          </h1>
          <p className="rise prose-tv mt-6 max-w-[34rem] text-lg [--d:0.16s]">
            Setiap topik disusun dalam lima bagian — gambaran, roadmap belajar, alat, proyek mini, dan sumber
            resmi — dan selalu berlabel seberapa matang isinya.
          </p>

          <div className="rise mt-9 max-w-xl [--d:0.24s]">
            {/* id sendiri: kotak di kepala halaman (layout) sudah memakai id "q". */}
            <KotakCari id="q-beranda" besar />
            <p className="mt-3 text-sm text-fg-mute">
              Coba <em className="not-italic text-fg-soft">cybersecurity</em>,{' '}
              <em className="not-italic text-fg-soft">quantum</em>, atau nama sebuah teknologi.
            </p>
          </div>
        </div>

        <div className="rise relative mx-auto w-full max-w-[28rem] [--d:0.2s] lg:max-w-none">
          <HeroOrbit className="h-auto w-full drop-shadow-[0_0_60px_rgb(34_211_238/0.18)]" />
        </div>
      </section>

      {/*
        🔴 Aturan teks pembaca ADR-024: teks yang tercetak di produksi tidak memuat
        bulan rencana, janji tanpa tanggal, nomor ADR, perintah pengembang, atau
        alamat internal. Spanduk ini dulu berbunyi "Tahap: Bulan 1 — taksonomi
        mendarat" dan "Navigasi tujuh bagian aplikasi menyusul; halaman ini masih
        beranda sementara". Yang pertama basi dalam kurang dari dua minggu; yang
        kedua janji tanpa tanggal yang tampil di keadaan NORMAL produksi. Nomor ADR
        menunjuk pembaca ke catatan keputusan pengembang, bukan ke isi - dan sampai
        2026-09-28 ke repositori privat yang bahkan tidak bisa mereka buka.

        Tidak ada uji JS yang menjaganya. Penjaganya pemindai teks yang TERLIHAT di
        build produksi (.github/scripts/periksa-halaman-web.mjs): ditulis ke job
        `citra` CI 2026-09-24, dan benar-benar berjalan di sana sejak #57 terjawab
        2026-09-28 - bukan lagi dijalankan tangan (ADR-024).
      */}
      <div className="mx-auto max-w-6xl px-5 sm:px-8">
        <div className="glass reveal flex items-start gap-4 p-5 sm:items-center sm:p-6">
          <span
            className="mt-1 size-2.5 shrink-0 rounded-full bg-mint shadow-[0_0_14px_3px_rgb(52_211_153/0.7)] sm:mt-0"
            aria-hidden="true"
          />
          <p className="text-[0.95rem] leading-relaxed text-fg-soft">
            <strong className="font-semibold text-fg">Jujur soal kematangan.</strong> Setiap topik selalu membawa
            label kematangan isinya: halaman yang belum diperiksa manusia tidak pernah tampil tanpa mengatakannya.
          </p>
        </div>
      </div>

      {/* fetch di Next.js 16 menahan render; Suspense membuat kerangka halaman
          tetap tampil sementara isinya menyusul. Dua Suspense terpisah supaya
          daftar bidang tidak menunggu daftar topik, dan sebaliknya. */}
      <div className="mx-auto mt-16 max-w-6xl space-y-20 px-5 sm:px-8">
        <Suspense fallback={<p className="text-sm text-fg-mute">Memuat bidang…</p>}>
          <FieldGrid />
        </Suspense>

        <Suspense fallback={<p className="text-sm text-fg-mute">Memuat daftar topik…</p>}>
          <TechnologyList />
        </Suspense>
      </div>
    </main>
  );
}
