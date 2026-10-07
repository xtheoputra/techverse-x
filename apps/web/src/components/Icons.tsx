import { useId } from 'react';
import type { ResourceType } from '@/lib/api';

/**
 * Ikon dan logo — semuanya SVG buatan sendiri, bukan paket ikon pihak ketiga.
 *
 * Sengaja begitu: satu-satunya aset visual situs ini yang punya riwayat lisensi
 * adalah yang kita gambar sendiri, dan ikon yang disematkan inline ikut tema
 * (`currentColor`) serta tidak menambah satu permintaan jaringan pun. Gayanya satu:
 * garis 1.6px berujung bulat pada kisi 24×24.
 */

type IconProps = { className?: string };

function Svg({ className, children }: IconProps & { children: React.ReactNode }) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      className={className}
    >
      {children}
    </svg>
  );
}

/** Lambang merek: segi enam bergradasi dengan silang X dan satu titik pusat. */
export function LogoMark({ className }: IconProps) {
  // Dua LogoMark di satu halaman (kepala + kaki) tidak boleh berbagi id gradasi.
  const id = useId();

  return (
    <svg viewBox="0 0 32 32" fill="none" aria-hidden="true" className={className}>
      <defs>
        <linearGradient id={id} x1="4" y1="3" x2="28" y2="29" gradientUnits="userSpaceOnUse">
          <stop stopColor="#22d3ee" />
          <stop offset="1" stopColor="#8b5cf6" />
        </linearGradient>
      </defs>
      <path
        d="M16 2.5 27.7 9.25v13.5L16 29.5 4.3 22.75V9.25z"
        stroke={`url(#${id})`}
        strokeWidth="1.7"
        strokeLinejoin="round"
      />
      <path d="m10.8 11.2 10.4 9.6M21.2 11.2l-10.4 9.6" stroke={`url(#${id})`} strokeWidth="2.1" strokeLinecap="round" />
      <circle cx="16" cy="16" r="2.1" fill="#fff" />
    </svg>
  );
}

export function ArrowUpRight({ className }: IconProps) {
  return (
    <Svg className={className}>
      <path d="M7 17 17 7M8 7h9v9" />
    </Svg>
  );
}

export function ArrowRight({ className }: IconProps) {
  return (
    <Svg className={className}>
      <path d="M5 12h14M13 6l6 6-6 6" />
    </Svg>
  );
}

export function SearchIcon({ className }: IconProps) {
  return (
    <Svg className={className}>
      <circle cx="11" cy="11" r="6.5" />
      <path d="m16 16 4.5 4.5" />
    </Svg>
  );
}

// ------------------------------------------------------------------- bidang

/**
 * Ikon per bidang, dipilih dari `slug` — slug itu kunci stabil ADR-009/010,
 * sedangkan nama tampilannya boleh berubah. Bidang yang slugnya belum punya ikon
 * (bidang berikutnya yang ditambahkan lewat ADR) jatuh ke lambang umum, bukan ke ruang kosong.
 */
const FIELD_ICONS: Record<string, (props: IconProps) => React.JSX.Element> = {
  'ai-machine-learning': (p) => (
    <Svg {...p}>
      <circle cx="5" cy="6" r="1.8" />
      <circle cx="5" cy="12" r="1.8" />
      <circle cx="5" cy="18" r="1.8" />
      <circle cx="12" cy="9" r="1.8" />
      <circle cx="12" cy="15" r="1.8" />
      <circle cx="19" cy="12" r="1.8" />
      <path d="m6.7 6.6 3.6 1.7M6.8 11.4l3.5-1.7M6.8 12.6l3.5 1.7M6.7 17.4l3.6-1.7M13.7 9.7l3.6 1.7M13.7 14.3l3.6-1.7" />
    </Svg>
  ),
  'ai-agents': (p) => (
    <Svg {...p}>
      <rect x="5" y="8.5" width="14" height="10" rx="3" />
      <path d="M12 8.5V5.5" />
      <circle cx="12" cy="4.2" r="1.2" />
      <circle cx="9.5" cy="13.5" r="1" fill="currentColor" stroke="none" />
      <circle cx="14.5" cy="13.5" r="1" fill="currentColor" stroke="none" />
      <path d="M3 12.5v2M21 12.5v2M10 16.5h4" />
    </Svg>
  ),
  cybersecurity: (p) => (
    <Svg {...p}>
      <path d="M12 3 19 6v5.2c0 4.6-3 8.1-7 9.8-4-1.7-7-5.2-7-9.8V6z" />
      <path d="m9 12 2.1 2.1L15.2 10" />
    </Svg>
  ),
  'cloud-infrastructure': (p) => (
    <Svg {...p}>
      <path d="M6.5 18.5a4 4 0 0 1-.4-7.98 5.5 5.5 0 0 1 10.7-1.1 4.5 4.5 0 0 1 .7 9.08z" />
    </Svg>
  ),
  'data-engineering': (p) => (
    <Svg {...p}>
      <ellipse cx="12" cy="6" rx="7" ry="3" />
      <path d="M5 6v6c0 1.66 3.13 3 7 3s7-1.34 7-3V6M5 12v6c0 1.66 3.13 3 7 3s7-1.34 7-3v-6" />
    </Svg>
  ),
  iot: (p) => (
    <Svg {...p}>
      <circle cx="12" cy="12" r="2.2" />
      <circle cx="5" cy="5" r="1.6" />
      <circle cx="19" cy="5" r="1.6" />
      <circle cx="5" cy="19" r="1.6" />
      <circle cx="19" cy="19" r="1.6" />
      <path d="m6.2 6.2 3.8 3.8M17.8 6.2 14 10M6.2 17.8 10 14M17.8 17.8 14 14" />
    </Svg>
  ),
  'edge-ai': (p) => (
    <Svg {...p}>
      <rect x="7" y="7" width="10" height="10" rx="2" />
      <rect x="10" y="10" width="4" height="4" rx="0.8" />
      <path d="M10 3v4M14 3v4M10 17v4M14 17v4M3 10h4M3 14h4M17 10h4M17 14h4" />
    </Svg>
  ),
  robotics: (p) => (
    <Svg {...p}>
      <circle cx="12" cy="12" r="5.5" />
      <circle cx="12" cy="12" r="2" />
      <path d="M12 3v3M12 18v3M3 12h3M18 12h3M5.6 5.6l2 2M16.4 16.4l2 2M18.4 5.6l-2 2M7.6 16.4l-2 2" />
    </Svg>
  ),
  'quantum-computing': (p) => (
    <Svg {...p}>
      <ellipse cx="12" cy="12" rx="10" ry="4" />
      <ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(60 12 12)" />
      <ellipse cx="12" cy="12" rx="10" ry="4" transform="rotate(120 12 12)" />
      <circle cx="12" cy="12" r="1.4" fill="currentColor" stroke="none" />
    </Svg>
  ),
  biotechnology: (p) => (
    <Svg {...p}>
      <path d="M9 3h6M10 3v6l-5 8.6A2 2 0 0 0 6.7 21h10.6a2 2 0 0 0 1.7-3.4L14 9V3" />
      <path d="M7.6 15h8.8" />
    </Svg>
  ),
  blockchain: (p) => (
    <Svg {...p}>
      <rect x="2.5" y="8.5" width="9" height="7" rx="3.5" />
      <rect x="12.5" y="8.5" width="9" height="7" rx="3.5" />
      <path d="M9 12h6" />
    </Svg>
  ),
  'renewable-energy': (p) => (
    <Svg {...p}>
      <path d="M13 2.5 5.5 13.5H11l-1 8 8.5-12H13z" />
    </Svg>
  ),
  'space-technology': (p) => (
    <Svg {...p}>
      <path d="M12 2.5c3 2.4 4.5 6 4.5 10l-2.6 3h-3.8l-2.6-3c0-4 1.5-7.6 4.5-10z" />
      <circle cx="12" cy="9.3" r="1.6" />
      <path d="m9.4 18.5-1.4 3M14.6 18.5l1.4 3M7.5 12 4.5 16l3 .6M16.5 12l3 4-3 .6" />
    </Svg>
  ),
  xr: (p) => (
    <Svg {...p}>
      <rect x="3" y="7.5" width="18" height="9.5" rx="3.5" />
      <circle cx="8.5" cy="12.2" r="1.7" />
      <circle cx="15.5" cy="12.2" r="1.7" />
      <path d="M3 11H1.5M21 11h1.5" />
    </Svg>
  ),
  // Tiga bidang Deep Tech (ADR-010 Pembaruan 2026-10-07).
  'advanced-computing-hardware': (p) => (
    <Svg {...p}>
      <rect x="6.5" y="6.5" width="11" height="11" rx="2" />
      <rect x="9.5" y="9.5" width="5" height="5" rx="1" />
      <path d="M9.5 3.5v3M14.5 3.5v3M9.5 17.5v3M14.5 17.5v3M3.5 9.5h3M3.5 14.5h3M17.5 9.5h3M17.5 14.5h3" />
    </Svg>
  ),
  'neurotechnology-bci': (p) => (
    <Svg {...p}>
      <circle cx="12" cy="12" r="8.5" />
      <path d="M5.5 12.5h3l1.6-4 2.6 8 2-6 1.2 2h2.6" />
    </Svg>
  ),
  'advanced-materials-nanotech': (p) => (
    <Svg {...p}>
      <path d="M12 3.2 19.4 7.6v8.8L12 20.8l-7.4-4.4V7.6z" />
      <path d="M12 12v8.8M12 12 4.6 7.6M12 12l7.4-4.4" />
      <circle cx="12" cy="12" r="1.2" fill="currentColor" stroke="none" />
    </Svg>
  ),
};

function GenericField({ className }: IconProps) {
  return (
    <Svg className={className}>
      <path d="M12 3 20 7.5v9L12 21l-8-4.5v-9z" />
      <circle cx="12" cy="12" r="2" />
    </Svg>
  );
}

export function FieldIcon({ slug, className }: { slug: string } & IconProps) {
  const Ikon = FIELD_ICONS[slug] ?? GenericField;
  return <Ikon className={className} />;
}

// ------------------------------------------------------------------ sumber

const RESOURCE_ICONS: Record<ResourceType, (props: IconProps) => React.JSX.Element> = {
  OfficialDocs: (p) => (
    <Svg {...p}>
      <path d="M5 4.5A1.5 1.5 0 0 1 6.5 3H19v15.5H6.5A1.5 1.5 0 0 0 5 20z" />
      <path d="M5 20a1.5 1.5 0 0 0 1.5 1.5H19V18.5M9 8h6M9 11.5h4" />
    </Svg>
  ),
  Video: (p) => (
    <Svg {...p}>
      <rect x="3" y="5.5" width="18" height="13" rx="3" />
      <path d="m10.2 9.4 4.6 2.6-4.6 2.6z" fill="currentColor" />
    </Svg>
  ),
  Paper: (p) => (
    <Svg {...p}>
      <path d="M6.5 3h8L19 7.5V21H6.5z" />
      <path d="M14 3v5h5M9.5 12.5h6M9.5 16h6" />
    </Svg>
  ),
  Repository: (p) => (
    <Svg {...p}>
      <circle cx="6.5" cy="5.5" r="2" />
      <circle cx="6.5" cy="18.5" r="2" />
      <circle cx="17.5" cy="9" r="2" />
      <path d="M6.5 7.5v9M17.5 11c0 3-3 3.5-6 4.3-2.2.6-4 1.2-5 1.9" />
    </Svg>
  ),
};

export function ResourceIcon({ type, className }: { type: ResourceType } & IconProps) {
  const Ikon = RESOURCE_ICONS[type] ?? RESOURCE_ICONS.OfficialDocs;
  return <Ikon className={className} />;
}

export function ToolIcon({ className }: IconProps) {
  return (
    <Svg className={className}>
      <path d="m14.5 6.5 3 3M4 20l7.2-7.2M13 4.5a4.5 4.5 0 0 0 5.6 5.6l1.9 1.9-8.5 8.5L10 18.5l-1.5-1.5 4.5-4.5-1.5-1.5-4.5 4.5-1-1z" />
    </Svg>
  );
}

export function FlagIcon({ className }: IconProps) {
  return (
    <Svg className={className}>
      <path d="M5 21V4M5 5h11l-2 3.5 2 3.5H5" />
    </Svg>
  );
}
