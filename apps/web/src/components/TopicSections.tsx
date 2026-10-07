import type { ResourceType, TechnologyDetail } from '@/lib/api';
import { ArrowUpRight, FlagIcon, ResourceIcon, ToolIcon } from '@/components/Icons';
import Markdown from '@/components/Markdown';
import type { NavItem } from '@/components/SectionNav';

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
 *
 * Tampilan (ADR-028): tiap bagian sebuah panel kaca bernomor; roadmap sebuah garis
 * waktu; alat kartu; sumber dikelompokkan menurut jenis. Yang berubah hanya
 * kulitnya — bagian, urutan, dan aturan di atas sama.
 */

/** Daftar isi sisi kiri; urutan dan id-nya sama dengan panel di bawah. */
export function bagianNav(topic: TechnologyDetail): NavItem[] {
  const daftar: NavItem[] = [
    { id: 'overview', label: 'Overview' },
    { id: 'roadmap', label: 'Learning Roadmap' },
    { id: 'tools', label: 'Tools' },
    { id: 'proyek', label: 'Mini Project' },
    { id: 'sumber', label: 'Resources' },
  ];

  if (topic.requires.length > 0 || topic.requiredBy.length > 0) {
    daftar.push({ id: 'terhubung', label: 'Topik terhubung' });
  }

  return daftar;
}

export default function TopicSections({ topic }: { topic: TechnologyDetail }) {
  const missing = new Set(topic.missingSections);

  return (
    <div className="space-y-8">
      {missing.size > 0 ? (
        <p className="glass border-amber/40 p-4 text-sm text-fg-soft">
          Halaman ini belum lengkap. Bagian yang masih kosong:{' '}
          <strong className="text-amber">{topic.missingSections.join(', ')}</strong>.
        </p>
      ) : null}

      <Panel id="overview" nomor={1} title="Overview" empty={missing.has('Overview')}>
        {/* Ringkasan polos sebagai pembuka (ia juga keterangan kartu dan hasil cari), lalu
            pendalaman berformat kalau ada (ADR-028 Tahap 3b). Dipecah per paragraf supaya
            ringkasan yang panjang tetap terbaca sebagai paragraf, bukan satu dinding teks. */}
        <div className="prose-tv max-w-[68ch] text-[1.125rem]">
          {paragraf(topic.summary).map((p, i) => (
            <p key={i}>{p}</p>
          ))}
        </div>
        {topic.overview ? (
          <Markdown source={topic.overview} media={topic.media} headingBase={3} className="mt-6 text-base" />
        ) : null}
      </Panel>

      <Panel id="roadmap" nomor={2} title="Learning Roadmap" empty={missing.has('Learning Roadmap')}>
        <ol className="relative">
          {/* Garis penghubung simpul: gradasi cyan → violet yang memudar. */}
          <span
            aria-hidden="true"
            className="absolute bottom-3 left-[1.125rem] top-3 w-px bg-gradient-to-b from-cyan/70 via-violet/50 to-transparent"
          />
          {topic.roadmap.map((step) => (
            <li key={step.order} className="relative pb-9 pl-16 last:pb-0">
              <span
                className={`absolute left-0 top-0 grid size-9 place-items-center rounded-full border bg-bg-raised font-mono text-sm ${
                  step.isPrerequisite
                    ? 'border-violet/60 text-violet shadow-[0_0_20px_-2px_rgb(139_92_246/0.7)]'
                    : 'border-cyan/50 text-cyan shadow-[0_0_20px_-2px_rgb(34_211_238/0.6)]'
                }`}
              >
                {step.isPrerequisite ? <FlagIcon className="size-4" /> : step.order}
              </span>
              {step.isPrerequisite ? <p className="eyebrow !text-violet">Prasyarat</p> : null}
              <h3 className="text-lg font-semibold leading-snug tracking-tight">{step.title}</h3>
              {/* Markdown terbatas (ADR-028 Tahap 3a). Judul langkah sudah <h3>, jadi
                  sub-judul di dalam teks mulai dari <h4>. */}
              {step.description ? <Markdown source={step.description} media={topic.media} headingBase={4} className="mt-2 text-base" /> : null}
            </li>
          ))}
        </ol>
      </Panel>

      <Panel id="tools" nomor={3} title="Tools" empty={missing.has('Tools')}>
        <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {topic.tools.map((tool) => (
            <li key={tool.slug} className="rounded-xl border border-line bg-surface p-4">
              <div className="flex items-start gap-3">
                <span className="grid size-9 shrink-0 place-items-center rounded-lg border border-cyan/30 bg-cyan/10 text-cyan">
                  <ToolIcon className="size-[18px]" />
                </span>
                <div className="min-w-0">
                  <p className="font-semibold">
                    {tool.homepage ? (
                      <a
                        href={tool.homepage}
                        rel="noreferrer noopener"
                        target="_blank"
                        className="inline-flex items-center gap-1 hover:text-cyan"
                      >
                        {tool.name}
                        <ArrowUpRight className="size-4 opacity-60" />
                      </a>
                    ) : (
                      tool.name
                    )}
                  </p>
                  {/* Ringkasan menjawab APA alat ini dan tinggal SEKALI di katalog,
                      dipakai bersama semua topik - itu alasan Tool jadi agregat
                      sendiri. Catatan menjawab KENAPA ia dipakai di topik INI. Sampai
                      2026-09-28 hanya catatannya yang tergambar. */}
                  {tool.summary ? <p className="mt-1 text-sm leading-relaxed text-fg-soft">{tool.summary}</p> : null}
                </div>
              </div>
              {tool.note ? (
                <p className="mt-3 border-l-2 border-violet/60 pl-3 text-sm leading-relaxed text-fg-soft">
                  {tool.note}
                </p>
              ) : null}
            </li>
          ))}
        </ul>
      </Panel>

      <Panel id="proyek" nomor={4} title="Mini Project" empty={missing.has('Mini Project')}>
        <ul className="space-y-5">
          {topic.projects.map((project) => (
            <li
              key={project.id}
              className="rounded-xl border border-cyan/25 bg-gradient-to-br from-cyan/[0.07] to-violet/[0.07] p-5 sm:p-6"
            >
              <p className="eyebrow">Misi</p>
              <h3 className="mt-2 text-xl font-semibold tracking-tight">{project.title}</h3>
              <Markdown source={project.brief} media={topic.media} headingBase={4} className="mt-3 text-base" />
            </li>
          ))}
        </ul>
      </Panel>

      <Panel id="sumber" nomor={5} title="Resources" empty={missing.has('Resources')}>
        <div className="space-y-6">
          {JENIS_SUMBER.map(({ type, judul }) => {
            const daftar = topic.resources.filter((r) => r.type === type);
            if (daftar.length === 0) return null;

            return (
              <div key={type}>
                <h3 className="mb-2 flex items-center gap-2 text-sm font-medium text-fg-mute">
                  <ResourceIcon type={type} className="size-4" />
                  {judul}
                </h3>
                <ul className="space-y-2">
                  {daftar.map((resource) => (
                    <li key={resource.id}>
                      <a
                        href={resource.url}
                        rel="noreferrer noopener"
                        target="_blank"
                        className="group flex items-center justify-between gap-4 rounded-xl border border-line bg-surface px-4 py-3 transition hover:border-cyan/45 hover:bg-surface-strong"
                      >
                        <span className="min-w-0">
                          <span className="block font-medium leading-snug group-hover:text-cyan">
                            {resource.title}
                          </span>
                          <span className="mt-0.5 block truncate font-mono text-xs text-fg-mute">
                            {inang(resource.url)}
                          </span>
                        </span>
                        <ArrowUpRight className="size-[18px] shrink-0 text-fg-mute transition group-hover:-translate-y-0.5 group-hover:translate-x-0.5 group-hover:text-cyan" />
                      </a>
                    </li>
                  ))}
                </ul>
              </div>
            );
          })}
        </div>
      </Panel>
    </div>
  );
}

const JENIS_SUMBER: { type: ResourceType; judul: string }[] = [
  { type: 'OfficialDocs', judul: 'Dokumentasi resmi' },
  { type: 'Video', judul: 'Video' },
  { type: 'Paper', judul: 'Paper' },
  { type: 'Repository', judul: 'Repositori' },
];

/** Satu paragraf per blok yang dipisah baris kosong; baris tunggal tetap satu paragraf. */
function paragraf(teks: string): string[] {
  return teks
    .split(/\n{2,}/)
    .map((p) => p.trim())
    .filter((p) => p.length > 0);
}

/** Nama inang tanpa `www.` untuk keterangan tautan; alamat yang tak bisa diurai ditampilkan apa adanya. */
function inang(url: string): string {
  try {
    return new URL(url).hostname.replace(/^www\./, '');
  } catch {
    return url;
  }
}

export function Panel({
  id,
  nomor,
  title,
  empty = false,
  children,
}: {
  id: string;
  nomor: number;
  title: string;
  empty?: boolean;
  children: React.ReactNode;
}) {
  return (
    <section id={id} aria-labelledby={`${id}-judul`} className="glass reveal p-6 sm:p-8">
      <header className="mb-6 flex items-center gap-4">
        <span aria-hidden="true" className="font-pixel text-3xl leading-none text-cyan/80">
          {String(nomor).padStart(2, '0')}
        </span>
        <h2 id={`${id}-judul`} className="text-2xl font-semibold tracking-tight">
          {title}
        </h2>
        <span aria-hidden="true" className="hidden h-px flex-1 bg-gradient-to-r from-cyan/40 to-transparent sm:block" />
      </header>
      {empty ? <p className="text-sm text-fg-mute">Belum diisi.</p> : children}
    </section>
  );
}
