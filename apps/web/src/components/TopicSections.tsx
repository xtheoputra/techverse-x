import type { TechnologyDetail } from '@/lib/api';

/**
 * Kelima bagian template ADR-012 pada satu halaman topik.
 *
 * 🔑 **Bagian yang kosong tetap ditampilkan, dan dikatakan kosong.** Godaannya
 * menyembunyikan bagian yang belum diisi supaya halamannya terlihat rapi — dan
 * itu persis kesalahan yang sudah dua kali ditemukan di repo ini: `MarkDrafted()`
 * yang mengaku "kelima bagian terisi" sambil meluluskan halaman kosong, dan kartu
 * bidang yang mengaku "ditulis manusia" di atas "0 diperiksa manusia". Halaman
 * setengah jadi yang TERLIHAT utuh adalah cara paling cepat kehilangan jejak
 * berapa banyak pekerjaan yang tersisa — dan `RENCANA-V1.md` menyebut 40 halaman
 * setengah jadi sebagai sebab kematian proyek ini.
 *
 * `missingSections` datang dari server apa adanya; halaman ini tidak
 * menghitungnya sendiri.
 *
 * ⚠️ Relasi antar-topik BUKAN salah satu bagian ini (ADR-023). Ia dirender
 * `TopikTerhubung` sesudah komponen ini, disembunyikan kalau kosong, dan tidak
 * pernah masuk `missingSections` — aturan "bagian kosong tetap ditampilkan" di
 * atas berlaku untuk kelima bagian template saja.
 */
export default function TopicSections({ topic }: { topic: TechnologyDetail }) {
  const missing = new Set(topic.missingSections);

  return (
    <div className="space-y-8">
      {missing.size > 0 ? (
        <p className="rounded-lg border border-amber-300 bg-amber-50 p-3 text-sm dark:border-amber-800 dark:bg-amber-950/40">
          Halaman ini belum lengkap. Bagian yang masih kosong:{' '}
          <strong>{topic.missingSections.join(', ')}</strong>.
        </p>
      ) : null}

      <Section title="Overview" empty={missing.has('Overview')}>
        <p className="text-neutral-700 dark:text-neutral-300">{topic.summary}</p>
      </Section>

      <Section title="Learning Roadmap" empty={missing.has('Learning Roadmap')}>
        <ol className="space-y-3">
          {topic.roadmap.map((step) => (
            <li key={step.order} className="flex gap-3">
              <span className="mt-0.5 shrink-0 rounded bg-neutral-200 px-2 py-0.5 font-mono text-xs dark:bg-neutral-800">
                {step.isPrerequisite ? 'pra' : step.order}
              </span>
              <span>
                <span className="font-medium">{step.title}</span>
                {step.description ? (
                  <span className="block text-sm text-neutral-600 dark:text-neutral-400">{step.description}</span>
                ) : null}
              </span>
            </li>
          ))}
        </ol>
      </Section>

      <Section title="Tools" empty={missing.has('Tools')}>
        <ul className="space-y-2">
          {topic.tools.map((tool) => (
            <li key={tool.slug}>
              <span className="font-medium">
                {tool.homepage ? (
                  <a href={tool.homepage} rel="noreferrer noopener" target="_blank" className="underline underline-offset-2">
                    {tool.name}
                  </a>
                ) : (
                  tool.name
                )}
              </span>
              {tool.note ? <span className="text-sm text-neutral-600 dark:text-neutral-400"> — {tool.note}</span> : null}
            </li>
          ))}
        </ul>
      </Section>

      <Section title="Mini Project" empty={missing.has('Mini Project')}>
        <ul className="space-y-3">
          {topic.projects.map((project) => (
            <li key={project.id}>
              <span className="font-medium">{project.title}</span>
              <span className="block text-sm text-neutral-600 dark:text-neutral-400">{project.brief}</span>
            </li>
          ))}
        </ul>
      </Section>

      <Section title="Resources" empty={missing.has('Resources')}>
        <ul className="space-y-2">
          {topic.resources.map((resource) => (
            <li key={resource.id} className="text-sm">
              <span className="mr-2 rounded border border-neutral-300 px-1.5 py-0.5 font-mono text-xs text-neutral-500 dark:border-neutral-700">
                {RESOURCE_LABEL[resource.type] ?? resource.type}
              </span>
              <a href={resource.url} rel="noreferrer noopener" target="_blank" className="underline underline-offset-2">
                {resource.title}
              </a>
            </li>
          ))}
        </ul>
      </Section>
    </div>
  );
}

const RESOURCE_LABEL: Record<string, string> = {
  OfficialDocs: 'dokumentasi',
  Video: 'video',
  Paper: 'paper',
  Repository: 'repositori',
};

function Section({ title, empty, children }: { title: string; empty: boolean; children: React.ReactNode }) {
  return (
    <section>
      <h2 className="mb-2 text-lg font-semibold">{title}</h2>
      {empty ? (
        <p className="text-sm text-neutral-500">Belum diisi.</p>
      ) : (
        children
      )}
    </section>
  );
}
