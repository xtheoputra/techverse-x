import Link from 'next/link';
import { notFound } from 'next/navigation';
import { connection } from 'next/server';
import type { Metadata } from 'next';
import MaturityBadge from '@/components/MaturityBadge';
import TopicSections from '@/components/TopicSections';
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
    <main className="mx-auto max-w-3xl px-6 py-12">
      <Breadcrumb />
      <div className="rounded-lg border border-amber-300 bg-amber-50 p-4 text-sm dark:border-amber-800 dark:bg-amber-950/40">
        <p className="font-medium">Halaman ini belum bisa diambil.</p>
        {mesinPengembang ? (
          <p className="mt-1 text-neutral-600 dark:text-neutral-400">{resolved.reason}</p>
        ) : null}
        <p className="mt-2 text-neutral-600 dark:text-neutral-400">
          Ini bukan berarti alamatnya salah — muat ulang beberapa detik lagi.
        </p>
      </div>
    </main>
  );
}

function Breadcrumb({ trail }: { trail?: string }) {
  return (
    <nav className="mb-6 text-sm text-neutral-500">
      <Link href="/" className="underline underline-offset-2 hover:text-neutral-800 dark:hover:text-neutral-200">
        TechVerse X
      </Link>
      {trail ? <span> · {trail}</span> : null}
    </nav>
  );
}

async function FieldView({ field }: { field: Field }) {
  const topics = await searchTechnologies({ field: field.slug, pageSize: 50 });

  return (
    <main className="mx-auto max-w-3xl px-6 py-12">
      <Breadcrumb trail="Bidang" />

      <header className="mb-6">
        <h1 className="text-3xl font-bold tracking-tight">{field.name}</h1>
        <p className="mt-2 text-neutral-600 dark:text-neutral-400">{field.summary}</p>
        <p className="mt-3 text-sm text-neutral-500">
          {field.topicCount} topik · <strong>{field.reviewedTopicCount}</strong> sudah diperiksa manusia
        </p>
      </header>

      {!topics.ok ? (
        <p className="text-sm text-neutral-500">
          Daftar topik belum bisa diambil.{mesinPengembang ? ` ${topics.reason}` : ''}
        </p>
      ) : topics.data.items.length === 0 ? (
        <p className="rounded-lg border border-neutral-200 bg-white p-4 text-sm text-neutral-600 dark:border-neutral-800 dark:bg-neutral-900 dark:text-neutral-400">
          {/* Teks pembaca ADR-024: kalimat ini dulu ditutup "Bulan 1 mengejar
              taksonominya lebih dulu" - nama bulan rencana yang basi sendiri. */}
          Bidang ini belum punya satu topik pun. Itu keadaan yang jujur, bukan galat.
        </p>
      ) : (
        <>
          {/* pageSize 50 memotong DIAM-DIAM tanpa kalimat ini - dan penelusuran di
              skala pengembangan tidak akan pernah melihatnya. */}
          {topics.data.totalItems > topics.data.items.length ? (
            <p className="mb-3 text-sm text-neutral-500">
              Menampilkan {topics.data.items.length} dari {topics.data.totalItems} topik.
            </p>
          ) : null}

          <ul className="space-y-3">
            {topics.data.items.map((topic: TechnologySummary) => (
              <li
                key={topic.id}
                className="rounded-lg border border-neutral-200 bg-white p-4 dark:border-neutral-800 dark:bg-neutral-900"
              >
                <div className="flex flex-wrap items-baseline justify-between gap-2">
                  <h2 className="font-semibold">
                    <Link href={`/teknologi/${topic.slug}`} className="underline underline-offset-2">
                      {topic.name}
                    </Link>
                  </h2>
                  <MaturityBadge maturity={topic.maturity} />
                </div>
                <p className="mt-1 text-sm text-neutral-600 dark:text-neutral-400">{topic.summary}</p>
              </li>
            ))}
          </ul>
        </>
      )}
    </main>
  );
}

function TopicView({ topic }: { topic: TechnologyDetail }) {
  return (
    <main className="mx-auto max-w-3xl px-6 py-12">
      <Breadcrumb trail={topic.fieldName} />

      <header className="mb-6">
        <div className="flex flex-wrap items-baseline justify-between gap-2">
          <h1 className="text-3xl font-bold tracking-tight">{topic.name}</h1>
          {/* ADR-012: label kematangan tidak pernah opsional. */}
          <MaturityBadge maturity={topic.maturity} />
        </div>
        <p className="mt-2 text-sm text-neutral-500">
          Bidang:{' '}
          <Link href={`/teknologi/${topic.fieldSlug}`} className="underline underline-offset-2">
            {topic.fieldName}
          </Link>
        </p>
      </header>

      <TopicSections topic={topic} />

      {/* SESUDAH template, bukan di dalamnya: relasi bukan bagian ADR-012. */}
      <TopikTerhubung topic={topic} />
    </main>
  );
}
