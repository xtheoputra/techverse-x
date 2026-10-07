import type { TechnologyMedia } from '@/lib/api';

/**
 * Satu gambar, diagram, atau video di dalam isi (ADR-028 Tahap 3b), dengan keterangan, sumber,
 * dan lisensinya selalu menyertainya.
 *
 * 🔑 **Data dari API diperiksa lagi di sini, bukan dipercaya.** Domain dan basis data sudah menolak
 * gambar hotlink dan ID video yang cacat, tetapi komponen ini adalah lapis terakhir sebelum URL
 * dicetak ke halaman: kalau yang sampai ke sini tak sesuai bentuknya (API lama, baris yang
 * diubah di luar domain), ia merender NOL — bukan `<img src>` ke alamat sembarang dan bukan
 * `<iframe>` ke host sembarang. Pola yang sama dengan penjagaan skema URL di pengurai Markdown.
 *
 * Gambar disajikan lewat `<img>`, bukan disisipkan inline: SVG di dalam `<img>` tak menjalankan
 * skrip, dan pemeriksa berkas isi sudah menolak SVG yang membawanya.
 *
 * Video disematkan lewat `youtube-nocookie.com` dengan `loading="lazy"` (ditunda sampai hampir
 * terlihat) dan `sandbox` — tanpa izin menavigasi halaman induk. Tautan "Tonton di YouTube" selalu
 * ada di bawahnya, untuk pembaca yang memblokir bingkai atau memakai pembaca layar.
 */

const POLA_BERKAS = /^\/media\/[A-Za-z0-9._/-]+\.(svg|png|jpe?g|webp|avif)$/i;
const POLA_VIDEO = /^[A-Za-z0-9_-]{11}$/;

function tautanAman(url: string | null): string | null {
  if (!url) return null;
  try {
    const u = new URL(url);
    return u.protocol === 'http:' || u.protocol === 'https:' ? u.href : null;
  } catch {
    return null;
  }
}

export default function MediaFigure({ media }: { media: TechnologyMedia }) {
  const isi = media.kind === 'Image' ? gambar(media) : video(media);

  if (isi === null) {
    return null;
  }

  const sumber = tautanAman(media.sourceUrl);

  return (
    <figure className="md-figure">
      {isi}
      <figcaption>
        {media.caption ? <span className="md-figure-caption">{media.caption}</span> : null}
        <span className="md-figure-source">
          {media.sourceName ? (
            <>
              Sumber:{' '}
              {sumber ? (
                <a href={sumber} rel="noreferrer noopener" target="_blank">
                  {media.sourceName}
                </a>
              ) : (
                media.sourceName
              )}
              {' · '}
            </>
          ) : null}
          Lisensi: {media.license}
        </span>
      </figcaption>
    </figure>
  );
}

function gambar(media: TechnologyMedia) {
  const url = media.url;

  if (!url || !POLA_BERKAS.test(url) || url.includes('..') || url.includes('//')) {
    return null;
  }

  return (
    // <img> biasa, bukan next/image: ia butuh dangerouslyAllowSVG untuk SVG, dan diagram buatan
    // sendiri sudah disajikan apa adanya dari /public.
    // eslint-disable-next-line @next/next/no-img-element
    <img src={url} alt={media.alt} loading="lazy" decoding="async" className="md-image" />
  );
}

function video(media: TechnologyMedia) {
  const id = media.videoId;

  if (!id || !POLA_VIDEO.test(id)) {
    return null;
  }

  return (
    <>
      <div className="md-video">
        <iframe
          src={`https://www.youtube-nocookie.com/embed/${id}`}
          title={media.alt}
          loading="lazy"
          allow="fullscreen; picture-in-picture; encrypted-media"
          allowFullScreen
          referrerPolicy="strict-origin-when-cross-origin"
          sandbox="allow-scripts allow-same-origin allow-presentation allow-popups"
        />
      </div>
      <a
        className="md-video-tautan"
        href={`https://www.youtube.com/watch?v=${id}`}
        rel="noreferrer noopener"
        target="_blank"
      >
        Tonton di YouTube
      </a>
    </>
  );
}
