import Link from 'next/link';
import { notFound } from 'next/navigation';
import { connection } from 'next/server';
import type { Metadata } from 'next';
import { ArrowRight, FieldIcon } from '@/components/Icons';
import MaturityBadge from '@/components/MaturityBadge';
import SectionNav from '@/components/SectionNav';
import TopicSections, { bagianNav } from '@/components/TopicSections';
import TopikTerhubung from '@/components/TopikTerhubung';
import {
  getTechnology,
  listFields,
  searchTechnologies,
  type Field,
  type TechnologyDetail,
  type TechnologySummary,
} from '@/lib/api';
import { mesinPengembang } from '@/lib/lingkungan';

/**
 * `/teknologi/<slug>` — satu-satunya URL yang ADR-009 janjikan stabil.
 *
 * 🔴 **Ia melayani DUA jenis entitas.** ADR-009 menulisnya apa adanya:
 * *"URL KANONIK tiap bidang dan tiap topik"*. Jadi rute ini harus memutuskan
 * lebih dulu slug ini milik siapa — dan itulah kenapa tabrakan slug bidang↔topik
 * ditolak di sisi API (`CreateTechnologyHandler`). Tanpa penolakan itu, halaman
 * ini terpaksa memilih diam-diam salah satu, dan entitas yang kalah kehilangan
 * alamat yang dijanjikan stabil untuknya.
 *
 * Urutannya bidang dulu, dan itu bukan sekadar preferensi: daftar bidang
 * tertutup, cuma 14 baris, dan sudah di-cache untuk halaman muka — jadi cabang
 * ini praktis gratis.
 */

type Params = { params: Promise<{ slug: string }> };

async function resolve(slug: string) {
  const fields = await listFields();

  if (fields.ok) {
    const field = fields.data.find((f) => f.slug === slug);
    if (field) {
      return { kind: 'field' as const, field };
    }
  }

  const topic = await getTechnology(slug);

  if (topic.ok) {
    return { kind: 'topic' as const, topic: topic.data };
  }

  // ⚠️ 404 HANYA kalau API benar-benar menjawab "tidak ada". Kalau ia tidak bisa
  // dihubungi - instance Koyeb yang sedang bangun, misalnya - halaman ini tidak
  // boleh mengaku hilang. Lihat catatan `notFound` di lib/api.ts.
  return { kind: 'unavailable' as const, reason: topic.reason, gone: topic.notFound && fields.ok };
}

export async function generateMetadata({ params }: Params): Promise<Metadata> {
  const { slug } = await params;
  const resolved = await resolve(slug);

  if (resolved.kind === 'field') {
    return { title: `${resolved.field.name} — TechVerse X`, description: resolved.field.summary };
  }

  if (resolved.kind === 'topic') {
    return { title: `${resolved.topic.name} — TechVerse X`, description: resolved.topic.summary };
  }

  return { title: 'TechVerse X' };
}

export default async function TeknologiPage({ params }: Params) {
  // Sama seperti FieldGrid: tanpa ini Next memanggang halaman saat build, dan
  // yang tersimpan adalah jawaban API pada saat build.
  await connection();

  const { slug } = await params;
  const resolved = await resolve(slug);

  if (resolved.kind === 'field') {
    return <FieldView field={resolved.field} />;
  }

  if (resolved.kind === 'topic') {
    return <TopicView topic={resolved.topic} />;
  }

  if (resolved.gone) {
    notFound();
  }

  return (
    <main className="mx-auto max-w-3xl px-5 py-12 sm:px-8">
      <Breadcrumb />
      <div className="glass border-amber/40 p-5 text-sm">
        <p className="font-medium text-amber">Halaman ini belum bisa diambil.</p>
        {mesinPengembang ? <p className="mt-1 text-fg-soft">{resolved.reason}</p> : null}
        <p className="mt-2 text-fg-soft">Ini bukan berarti alamatnya salah — muat ulang beberapa detik lagi.</p>
      </div>
    </main>
  );
}

function Breadcrumb({ trail, trailHref }: { trail?: string; trailHref?: `/teknologi/${string}` }) {
  return (
    <nav aria-label="Remah roti" className="mb-8 flex flex-wrap items-center gap-2 text-sm text-fg-mute">
      <Link href="/" className="transition hover:text-cyan">
        TechVerse X
      </Link>
      {trail ? (
        <>
          <span aria-hidden="true">/</span>
          {trailHref ? (
            <Link href={trailHref} className="transition hover:text-cyan">
              {trail}
            </Link>
          ) : (
            <span className="text-fg-soft">{trail}</span>
          )}
        </>
      ) : null}
    </nav>
  );
}

async function FieldView({ field }: { field: Field }) {
  const topics = await searchTechnologies({ field: field.slug, pageSize: 50 });

  return (
    <main className="mx-auto max-w-5xl px-5 pb-8 pt-10 sm:px-8 sm:pt-14">
      <Breadcrumb trail="Bidang" />

      <header className="glass relative overflow-hidden p-6 sm:p-10">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -right-16 -top-16 size-72 rounded-full bg-gradient-to-br from-cyan/25 to-violet/25 blur-3xl"
        />
        <div className="relative flex flex-col gap-6 sm:flex-row sm:items-start">
          <span className="grid size-16 shrink-0 place-items-center rounded-2xl border border-cyan/40 bg-gradient-to-br from-cyan/20 to-violet/20 text-cyan">
            <FieldIcon slug={field.slug} className="size-8" />
          </span>
          <div>
            <p className="eyebrow">Bidang</p>
            <h1 className="mt-2 text-4xl font-semibold tracking-tight sm:text-5xl">{field.name}</h1>
            <p className="prose-tv mt-4 max-w-[60ch] text-lg">{field.summary}</p>
            <p className="mt-5 font-mono text-sm text-fg-mute">
              {field.topicCount} topik · <strong className="text-mint">{field.reviewedTopicCount}</strong> sudah
              diperiksa manusia
            </p>
          </div>
        </div>
      </header>

      <div className="mt-10">
        {!topics.ok ? (
          <p className="text-sm text-fg-mute">
            Daftar topik belum bisa diambil.{mesinPengembang ? ` ${topics.reason}` : ''}
          </p>
        ) : topics.data.items.length === 0 ? (
          <p className="glass p-5 text-sm text-fg-soft">
            {/* Teks pembaca ADR-024: kalimat ini dulu ditutup "Bulan 1 mengejar
                taksonominya lebih dulu" - nama bulan rencana yang basi sendiri. */}
            Bidang ini belum punya satu topik pun. Itu keadaan yang jujur, bukan galat.
          </p>
        ) : (
          <>
            {/* pageSize 50 memotong DIAM-DIAM tanpa kalimat ini - dan penelusuran di
                skala pengembangan tidak akan pernah melihatnya. */}
            {topics.data.totalItems > topics.data.items.length ? (
              <p className="mb-3 text-sm text-fg-mute">
                Menampilkan {topics.data.items.length} dari {topics.data.totalItems} topik.
              </p>
            ) : null}

            <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              {topics.data.items.map((topic: TechnologySummary) => (
                <li key={topic.id} className="glass glass-hover reveal group relative flex flex-col p-5">
                  <h2 className="text-xl font-semibold tracking-tight">
                    <Link href={`/teknologi/${topic.slug}`} className="stretched-link">
                      {topic.name}
                    </Link>
                  </h2>
                  <p className="mt-2 line-clamp-4 text-sm leading-relaxed text-fg-soft">{topic.summary}</p>
                  <div className="mt-auto flex items-center justify-between gap-3 pt-5">
                    <MaturityBadge maturity={topic.maturity} />
                    <ArrowRight className="size-5 shrink-0 text-fg-mute transition group-hover:translate-x-1 group-hover:text-cyan" />
                  </div>
                </li>
              ))}
            </ul>
          </>
        )}
      </div>
    </main>
  );
}

const tanggal = new Intl.DateTimeFormat('id-ID', { dateStyle: 'long', timeZone: 'UTC' });

function TopicView({ topic }: { topic: TechnologyDetail }) {
  const stat: { nilai: number; label: string }[] = [
    { nilai: topic.roadmap.length, label: 'Langkah belajar' },
    { nilai: topic.tools.length, label: 'Alat' },
    { nilai: topic.projects.length, label: 'Proyek mini' },
    { nilai: topic.resources.length, label: 'Sumber' },
  ];

  return (
    <main className="mx-auto max-w-6xl px-5 pb-8 pt-10 sm:px-8 sm:pt-14">
      <Breadcrumb trail={topic.fieldName} trailHref={`/teknologi/${topic.fieldSlug}`} />

      <header className="glass relative overflow-hidden p-6 sm:p-10">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute -right-20 -top-24 size-80 rounded-full bg-gradient-to-br from-cyan/25 to-violet/30 blur-3xl"
        />
        <div className="relative">
          <div className="flex flex-wrap items-center gap-x-4 gap-y-2">
            <Link href={`/teknologi/${topic.fieldSlug}`} className="eyebrow transition hover:brightness-125">
              {topic.fieldName}
            </Link>
            {/* ADR-012: label kematangan tidak pernah opsional. */}
            <MaturityBadge maturity={topic.maturity} />
          </div>
          <h1 className="gradient-text mt-4 max-w-[22ch] text-4xl font-semibold leading-[1.08] tracking-tight sm:text-6xl">
            {topic.name}
          </h1>

          <dl className="mt-8 grid grid-cols-2 gap-3 sm:grid-cols-4">
            {stat.map((s) => (
              <div key={s.label} className="rounded-xl border border-line bg-bg/40 px-4 py-3">
                <dt className="text-xs text-fg-mute">{s.label}</dt>
                <dd className="mt-1 font-pixel text-3xl leading-none text-fg">{s.nilai}</dd>
              </div>
            ))}
          </dl>

          <p className="mt-6 font-mono text-xs text-fg-mute">
            Diperbarui {tanggal.format(new Date(topic.updatedAt))}
          </p>
        </div>
      </header>

      <div className="mt-10 grid grid-cols-1 gap-8 lg:grid-cols-[13rem_minmax(0,1fr)] lg:gap-12">
        <SectionNav items={bagianNav(topic)} />

        <div className="min-w-0">
          <TopicSections topic={topic} />

          {/* SESUDAH template, bukan di dalamnya: relasi bukan bagian ADR-012. */}
          <TopikTerhubung topic={topic} />
        </div>
      </div>
    </main>
  );
}
