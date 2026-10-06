import Link from 'next/link';
import { connection } from 'next/server';
import type { Metadata } from 'next';
import { FieldIcon } from '@/components/Icons';
import KotakCari from '@/components/KotakCari';
import MaturityBadge from '@/components/MaturityBadge';
import { cari, type Field, type TechnologySummary } from '@/lib/api';
import { mesinPengembang } from '@/lib/lingkungan';

/**
 * `/cari?q=…` — sasaran Bulan 3 di docs/RENCANA-V1.md.
 *
 * Kata kuncinya di URL, bukan di state komponen. Hasil pencarian karena itu bisa
 * ditautkan, di-bookmark, dan dibuka lagi oleh tombol kembali peramban.
 */

type Params = { searchParams: Promise<{ q?: string | string[] }> };

/** `?q=a&q=b` sah menurut HTML; ambil yang pertama, jangan biarkan jadi array. */
function kataKunci(q: string | string[] | undefined): string {
  return (Array.isArray(q) ? q[0] : q)?.trim() ?? '';
}

export async function generateMetadata({ searchParams }: Params): Promise<Metadata> {
  const q = kataKunci((await searchParams).q);

  return {
    title: q ? `Cari: ${q} — TechVerse X` : 'Cari — TechVerse X',

    // Halaman hasil pencarian tidak punya isi sendiri, dan setiap kata kunci
    // melahirkan URL baru. Membiarkannya diindeks berarti menyuruh mesin pencari
    // memetakan ruang kata kunci yang tak terbatas — dan yang dipetakan bukan
    // halaman TechVerse X, melainkan bayangannya.
    robots: { index: false, follow: true },
  };
}

export default async function CariPage({ searchParams }: Params) {
  // Sama seperti FieldGrid dan TechnologyList: tanpa ini Next memanggang
  // halaman saat build, dan yang tersimpan adalah jawaban API pada saat build.
  await connection();

  const q = kataKunci((await searchParams).q);

  return (
    <main className="mx-auto max-w-4xl px-5 py-12 sm:px-8 sm:py-16">
      <p className="eyebrow">Pencarian</p>
      <h1 className="mt-2 text-4xl font-semibold tracking-tight sm:text-5xl">Cari</h1>
      <p className="mt-3 text-fg-soft">
        Bidang dan topik dicari sekaligus. Keduanya punya halaman sendiri.
      </p>

      <div className="mt-6 max-w-2xl">
        {/* id sendiri: kotak di kepala halaman (layout) sudah memakai id "q". */}
        <KotakCari id="q-halaman" defaultValue={q} autoFocus besar />
      </div>

      <div className="mt-10">
        <Hasil q={q} />
      </div>
    </main>
  );
}

async function Hasil({ q }: { q: string }) {
  if (q.length === 0) {
    return (
      <p className="text-sm text-fg-soft">
        Ketik kata kuncinya di atas. Coba nama bidang (<em>cybersecurity</em>, <em>quantum</em>) atau nama
        teknologi.
      </p>
    );
  }

  const result = await cari(q);

  if (!result.ok) {
    return (
      <div className="glass border-amber/40 p-5 text-sm">
        <p className="font-medium text-amber">Pencarian belum bisa dijalankan.</p>
        <p className="mt-1 text-fg-soft">
          {/* Sebabnya dicatat ke log server oleh lib/api.ts, bukan dicetak ke
              pembaca — lihat lib/lingkungan.ts. */}
          {mesinPengembang ? result.reason : 'Coba muat ulang beberapa saat lagi.'}
        </p>
      </div>
    );
  }

  // `query`, bukan `q`: yang disebut ke pembaca harus kata kunci yang BENAR-BENAR
  // dipakai server. Aturan itu tertulis dua kali — di `SearchResponse` dan di
  // `SearchResult` — dan sampai 2026-09-24 halaman ini mengabaikan keduanya.
  //
  // 📏 Bukan kehati-hatian teoretis; diukur lewat API sungguhan: kata kunci 230
  // karakter dijawab `query` 200 karakter (`PencarianTeks.Bersihkan` memotongnya),
  // sementara halaman ini mencetak 230 karakter penuh. Pembaca diberi tahu bahwa
  // yang dicari adalah sesuatu yang tidak pernah dicari.
  const { query: dicari, fields, technologies, isEmpty } = result.data;

  // Sengaja TANPA menyebut angka batasnya. Angka itu milik server
  // (`PencarianTeks.MaksPanjangKataKunci`), dan menuliskannya di sini akan jadi
  // sumber kedua yang bisa hanyut tanpa ada yang merah — cacat yang sama dengan
  // kembaran `seed.sh` dulu.
  const dipendekkan = dicari !== q;

  if (isEmpty) {
    return (
      <div className="glass p-5 text-sm">
        <p className="font-medium text-fg">Tidak ada yang cocok dengan “{dicari}”.</p>
        {dipendekkan ? (
          <p className="mt-1 text-fg-soft">
            Kata kuncinya dipendekkan sebelum dicari.
          </p>
        ) : null}
        <p className="mt-1 text-fg-soft">
          {/*
            🔴 Kalimat ini menyebut BATAS pencarian hari ini, dan #54 wajib
            mengubahnya di PR yang sama begitu isi halaman ikut dicari.

            Kalimat sebelumnya ("Isi TechVerse X masih dibangun bidang per bidang")
            mengundang pembaca percaya isi halaman ikut tercari - justru yang #54
            larang dijanjikan - dan ia janji tanpa tanggal (ADR-024).
          */}
          Pencarian mencocokkan nama dan ringkasan bidang serta topik, belum isi halamannya. Semua bidang
          bisa ditelusuri dari{' '}
          <Link href="/" className="text-cyan underline decoration-cyan/40 underline-offset-2 hover:decoration-cyan">
            halaman muka
          </Link>
          .
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-10">
      {/* Juga di jalan yang BERHASIL, bukan cuma saat nol hasil: kalau yang dicari
          bukan yang diketik, hasil yang tampil pun bukan jawaban atas yang diketik. */}
      {dipendekkan ? (
        <p className="text-sm text-fg-soft">
          Hasil untuk “{dicari}” — kata kuncinya dipendekkan sebelum dicari.
        </p>
      ) : null}
      {fields.length > 0 ? <BidangCocok fields={fields} /> : null}
      {technologies.items.length > 0 ? (
        <TopikCocok items={technologies.items} total={technologies.totalItems} />
      ) : null}
    </div>
  );
}

function BidangCocok({ fields }: { fields: Field[] }) {
  return (
    <section>
      <h2 className="mb-4 text-2xl font-semibold tracking-tight">{fields.length} bidang</h2>
      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {fields.map((field) => (
          <li key={field.id} className="glass glass-hover group relative flex flex-col p-5">
            <div className="flex items-start gap-4">
              <span className="grid size-11 shrink-0 place-items-center rounded-xl border border-cyan/40 bg-gradient-to-br from-cyan/20 to-violet/20 text-cyan">
                <FieldIcon slug={field.slug} className="size-5" />
              </span>
              <div className="min-w-0">
                <h3 className="text-lg font-semibold tracking-tight">
                  <Link href={`/teknologi/${field.slug}`} className="stretched-link">
                    {field.name}
                  </Link>
                </h3>
                <p className="mt-1 line-clamp-3 text-sm leading-relaxed text-fg-soft">{field.summary}</p>
              </div>
            </div>
            <p className="mt-4 font-mono text-xs text-fg-mute">
              {field.topicCount} topik · {field.reviewedTopicCount} diperiksa manusia
            </p>
          </li>
        ))}
      </ul>
    </section>
  );
}

function TopikCocok({ items, total }: { items: TechnologySummary[]; total: number }) {
  return (
    <section>
      <h2 className="mb-4 text-2xl font-semibold tracking-tight">
        {total} topik
        {total > items.length ? (
          <span className="ml-3 align-middle text-sm font-normal text-fg-mute">
            menampilkan {items.length} teratas
          </span>
        ) : null}
      </h2>
      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {items.map((topic) => (
          <li key={topic.id} className="glass glass-hover group relative flex flex-col p-5">
            <p className="eyebrow !text-violet">{topic.fieldName}</p>
            <h3 className="mt-2 text-xl font-semibold tracking-tight">
              <Link href={`/teknologi/${topic.slug}`} className="stretched-link">
                {topic.name}
              </Link>
            </h3>
            <p className="mt-2 line-clamp-3 text-sm leading-relaxed text-fg-soft">{topic.summary}</p>

            {/* Aturan keras ADR-012: kematangan isi ikut tampil di mana pun
                judulnya tampil — termasuk di hasil pencarian. */}
            <p className="mt-auto pt-5">
              <MaturityBadge maturity={topic.maturity} />
            </p>
          </li>
        ))}
      </ul>
    </section>
  );
}
