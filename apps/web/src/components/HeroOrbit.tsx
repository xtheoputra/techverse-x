/**
 * Grafik hero: simpul-simpul yang mengorbit satu inti — gambaran "banyak bidang,
 * satu peta". SVG inline, tanpa gambar dan tanpa JS; geraknya animasi CSS yang
 * mati sendiri bila pembaca meminta gerak dikurangi (globals.css).
 *
 * Murni hiasan, jadi `aria-hidden`. Tidak membawa makna yang tak ada di teks.
 */
export default function HeroOrbit({ className }: { className?: string }) {
  // Simpul: [cx, cy, jari-jari, warna, tunda denyut]
  const cincinLuar: [number, number, number, string, string][] = [
    [260, 36, 5, '#22d3ee', '0s'],
    [458, 150, 4, '#8b5cf6', '0.6s'],
    [420, 392, 5, '#e879f9', '1.2s'],
    [120, 410, 4, '#22d3ee', '1.8s'],
    [48, 190, 5, '#8b5cf6', '2.4s'],
  ];
  const cincinTengah: [number, number, number, string, string][] = [
    [260, 96, 4, '#a5f3fc', '0.3s'],
    [394, 226, 4, '#c4b5fd', '0.9s'],
    [300, 392, 3.5, '#f0abfc', '1.5s'],
    [128, 300, 4, '#a5f3fc', '2.1s'],
  ];

  return (
    <svg viewBox="0 0 520 520" fill="none" aria-hidden="true" className={className}>
      <defs>
        <radialGradient id="hero-inti" cx="50%" cy="50%" r="50%">
          <stop offset="0" stopColor="#ffffff" stopOpacity="0.95" />
          <stop offset="0.35" stopColor="#67e8f9" stopOpacity="0.55" />
          <stop offset="1" stopColor="#8b5cf6" stopOpacity="0" />
        </radialGradient>
        <linearGradient id="hero-cincin" x1="0" y1="0" x2="520" y2="520" gradientUnits="userSpaceOnUse">
          <stop stopColor="#22d3ee" stopOpacity="0.7" />
          <stop offset="1" stopColor="#8b5cf6" stopOpacity="0.7" />
        </linearGradient>
        <filter id="hero-cahaya" x="-50%" y="-50%" width="200%" height="200%">
          <feGaussianBlur stdDeviation="3.5" />
        </filter>
      </defs>

      {/* Cahaya inti */}
      <circle cx="260" cy="260" r="150" fill="url(#hero-inti)" opacity="0.5" />

      {/* Cincin statis tipis */}
      <circle cx="260" cy="260" r="224" stroke="url(#hero-cincin)" strokeOpacity="0.22" strokeWidth="1" />
      <circle cx="260" cy="260" r="164" stroke="url(#hero-cincin)" strokeOpacity="0.3" strokeWidth="1" strokeDasharray="2 7" />
      <circle cx="260" cy="260" r="104" stroke="url(#hero-cincin)" strokeOpacity="0.38" strokeWidth="1" />

      {/* Cincin luar berputar + simpulnya */}
      <g className="orbit-ring">
        <circle cx="260" cy="260" r="224" stroke="#22d3ee" strokeOpacity="0.28" strokeWidth="1.2" strokeDasharray="1 18" strokeLinecap="round" />
        {cincinLuar.map(([cx, cy, r, warna, tunda]) => (
          <g key={`${cx}-${cy}`} className="node-pulse" style={{ '--d': tunda } as React.CSSProperties}>
            <circle cx={cx} cy={cy} r={r * 2.6} fill={warna} opacity="0.35" filter="url(#hero-cahaya)" />
            <circle cx={cx} cy={cy} r={r} fill={warna} />
          </g>
        ))}
      </g>

      {/* Cincin tengah berputar berlawanan + simpul + garis ke inti */}
      <g className="orbit-ring orbit-ring--rev">
        {cincinTengah.map(([cx, cy, r, warna, tunda]) => (
          <g key={`${cx}-${cy}`}>
            <line x1="260" y1="260" x2={cx} y2={cy} stroke={warna} strokeOpacity="0.2" strokeWidth="1" />
            <g className="node-pulse" style={{ '--d': tunda } as React.CSSProperties}>
              <circle cx={cx} cy={cy} r={r * 2.6} fill={warna} opacity="0.3" filter="url(#hero-cahaya)" />
              <circle cx={cx} cy={cy} r={r} fill={warna} />
            </g>
          </g>
        ))}
      </g>

      {/* Inti: segi enam bercahaya */}
      <path
        d="M260 206 307 233v54l-47 27-47-27v-54z"
        fill="#04050b"
        stroke="url(#hero-cincin)"
        strokeWidth="2"
        strokeLinejoin="round"
      />
      <path d="m238 241 44 38M282 241l-44 38" stroke="#fff" strokeOpacity="0.9" strokeWidth="3" strokeLinecap="round" />
      <circle cx="260" cy="260" r="5" fill="#fff" />
    </svg>
  );
}
