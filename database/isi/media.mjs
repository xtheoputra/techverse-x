// Aturan media untuk berkas isi — ADR-028 Tahap 3b. Nol dependensi, supaya `pasang.mjs --cek`
// tetap jalan sebelum `npm ci`.
//
// Berkas ini MENCERMINKAN aturan domain (`TechnologyMedia.cs`) dan `CHECK` basis data, dan itu
// satu-satunya cermin di jalur ini — sengaja, karena pemeriksa berkas berjalan tanpa API dan
// gagal di PR, bukan di tengah pemasangan yang sudah menulis separuh. Server tetap yang
// berwenang: gerbang CI "isi dipasang dari nol" memasang berkas sungguhan ke API sungguhan, jadi
// cermin yang menyimpang ketahuan di sana.
//
// 🔴 Tambahan yang TIDAK ada di server dan hanya mungkin di sini, karena hanya sini yang melihat
// repo: (1) berkas gambar benar-benar ada dan tidak raksasa; (2) SVG tak membawa skrip — SVG yang
// dibuka langsung di alamatnya menjalankan skrip di origin situs, jadi `<script>` di dalamnya
// harus ditolak SEBELUM masuk repo; (3) gambar hidup di folder topiknya sendiri
// (`/media/<slug-topik>/…`); (4) setiap `::media[kunci]` di teks punya definisi, dan setiap
// definisi dipakai.

import { statSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

export const KARYA_SENDIRI = 'Karya sendiri';

export const KUNCI_MEDIA = ['key', 'kind', 'url', 'videoId', 'alt', 'caption', 'sourceName', 'sourceUrl', 'license'];

export const BATAS_MEDIA = {
  perTopik: 40,
  key: 80,
  url: 300,
  alt: 500,
  caption: 1000,
  sourceName: 200,
  sourceUrl: 1000,
  license: 200,
  // Tiap gambar dimuat pembaca; angka ini menahan satu berkas raksasa masuk repo tanpa disadari.
  bytSvg: 300 * 1024,
  bytRaster: 800 * 1024,
};

const POLA_SLUG = /^[a-z0-9]+(-[a-z0-9]+)*$/;
const POLA_BERKAS = /^\/media\/[A-Za-z0-9._/-]+\.(svg|png|jpe?g|webp|avif)$/i;
const POLA_VIDEO = /^[A-Za-z0-9_-]{11}$/;

const adalahTeks = (v) => typeof v === 'string' && v.trim().length > 0;

function periksaTeksMedia(galat, tempat, nilai, maks, { wajib }) {
  if (nilai === undefined || nilai === null) {
    if (wajib) galat.push(`${tempat}: wajib teks tak kosong.`);
    return false;
  }
  if (!adalahTeks(nilai)) {
    galat.push(`${tempat}: ${wajib ? 'wajib teks tak kosong' : 'bila ada, harus teks tak kosong (hapus kuncinya kalau tak dipakai)'}.`);
    return false;
  }
  if (nilai !== nilai.trim()) {
    galat.push(`${tempat}: ada spasi di awal atau akhir.`);
  }
  if (nilai.length > maks) {
    galat.push(`${tempat}: ${nilai.length} karakter, batasnya ${maks}.`);
  }
  return true;
}

function urlHttp(nilai) {
  try {
    const u = new URL(nilai);
    return u.protocol === 'http:' || u.protocol === 'https:';
  } catch {
    return false;
  }
}

/**
 * Masalah keamanan di sebuah berkas SVG, sebagai daftar kalimat (kosong = aman).
 *
 * SVG hanya disajikan lewat `<img>` di situs (skrip tak dijalankan di sana), tetapi ia juga
 * punya alamat sendiri, dan membukanya langsung menjalankan skrip di origin yang sama dengan
 * situs. Berkas ini buatan sendiri, jadi pemeriksaan ini penjaga kecelakaan (hasil salin dari
 * editor yang menyisipkan skrip, atau SVG pihak ketiga yang kelak ditempel) — bukan model ancaman
 * pihak luar.
 */
export function periksaSvg(teks) {
  const masalah = [];

  // Buang komentar dan prolog XML sebelum mencari akar — keduanya sah di depan <svg>.
  const tanpaKomentar = teks.replace(/<!--[\s\S]*?-->/g, '');
  if (!/^\s*(<\?xml[^>]*\?>\s*)?<svg[\s>]/i.test(tanpaKomentar)) {
    masalah.push('bukan SVG: akarnya harus <svg> (boleh didahului prolog XML dan komentar).');
  }

  if (!/<svg\b[^>]*\bviewBox\s*=/i.test(tanpaKomentar)) {
    masalah.push('<svg> wajib punya viewBox, supaya gambar menyusut mengikuti lebar kolom (tanpa itu ia meluber di ponsel).');
  }

  const larangan = [
    [/<script\b/i, '<script>'],
    [/<foreignObject\b/i, '<foreignObject>'],
    [/<(iframe|embed|object|audio|video)\b/i, 'elemen tersemat (iframe, embed, object, audio, video)'],
    [/\son[a-z]+\s*=/i, 'atribut penangan peristiwa (onclick, onload, …)'],
    [/javascript\s*:/i, 'URL javascript:'],
    [/data\s*:\s*text\/html/i, 'URL data:text/html'],
    [/<!DOCTYPE|<!ENTITY/i, 'DOCTYPE atau ENTITY (perluasan entitas)'],
    [/@import\b/i, '@import'],
    [/(?:xlink:)?href\s*=\s*["']\s*(?:https?:)?\/\//i, 'rujukan ke alamat luar (href berawalan http, https, atau //)'],
    [/url\(\s*["']?\s*(?:https?:)?\/\//i, 'url() ke alamat luar'],
  ];

  for (const [pola, nama] of larangan) {
    if (pola.test(tanpaKomentar)) {
      masalah.push(`memuat ${nama} — SVG di sini harus berdiri sendiri tanpa skrip dan tanpa sumber luar.`);
    }
  }

  return masalah;
}

/**
 * Memeriksa daftar `media` sebuah berkas isi. Mengembalikan kunci-kunci yang DIDEFINISIKAN
 * (untuk pencocokan dengan `::media[kunci]` di teks) dan menambahkan masalahnya ke `galat`.
 *
 * @param {string[]} galat
 * @param {string} tempat awalan pesan, mis. `isi/ai-agents/mcp.json media`
 * @param {unknown} daftar nilai `media` dari berkas (boleh tak ada)
 * @param {{ slugTopik: string, folderPublik: string }} konteks
 */
export function periksaDaftarMedia(galat, tempat, daftar, { slugTopik, folderPublik }) {
  const kunci = new Set();

  if (daftar === undefined) {
    return kunci;
  }
  if (!Array.isArray(daftar)) {
    galat.push(`${tempat}: harus berupa daftar (boleh dihilangkan).`);
    return kunci;
  }
  if (daftar.length > BATAS_MEDIA.perTopik) {
    galat.push(`${tempat}: ${daftar.length} media, batas per topik ${BATAS_MEDIA.perTopik}. Pecah topiknya atau buang yang tak terpakai.`);
  }

  const lihat = new Set();

  daftar.forEach((m, i) => {
    const di = `${tempat}[${i}]`;

    if (m === null || typeof m !== 'object' || Array.isArray(m)) {
      galat.push(`${di}: harus berupa objek.`);
      return;
    }
    for (const k of Object.keys(m)) {
      if (!KUNCI_MEDIA.includes(k)) {
        galat.push(`${di}: kunci '${k}' tidak dikenal. Yang sah: ${KUNCI_MEDIA.join(', ')}.`);
      }
    }

    // ---- kunci: alamat dari teks
    if (periksaTeksMedia(galat, `${di}.key`, m.key, BATAS_MEDIA.key, { wajib: true })) {
      if (!POLA_SLUG.test(m.key)) {
        galat.push(`${di}.key: '${m.key}' bukan slug (huruf kecil, angka, tanda hubung tunggal).`);
      }
      if (lihat.has(m.key)) {
        galat.push(`${di}.key: '${m.key}' muncul dua kali — kunci harus unik per topik.`);
      }
      lihat.add(m.key);
      kunci.add(m.key);
    }

    // ---- bentuk menurut jenis
    if (m.kind !== 'Image' && m.kind !== 'Video') {
      galat.push(`${di}.kind: harus 'Image' atau 'Video' persis, bukan ${JSON.stringify(m.kind)}.`);
    } else if (m.kind === 'Image') {
      periksaGambar(galat, di, m, { slugTopik, folderPublik });
    } else {
      if (m.url !== undefined) {
        galat.push(`${di}.url: video tak punya url; pakai videoId.`);
      }
      if (!adalahTeks(m.videoId) || !POLA_VIDEO.test(m.videoId)) {
        galat.push(`${di}.videoId: harus ID YouTube 11 karakter (huruf, angka, _ dan -), bukan ${JSON.stringify(m.videoId)}.`);
      }
    }

    // ---- teks alternatif (video: judul bingkai), keterangan, lisensi, dan sumber
    periksaTeksMedia(galat, `${di}.alt`, m.alt, BATAS_MEDIA.alt, { wajib: true });
    periksaTeksMedia(galat, `${di}.caption`, m.caption, BATAS_MEDIA.caption, { wajib: false });

    const adaLisensi = periksaTeksMedia(galat, `${di}.license`, m.license, BATAS_MEDIA.license, { wajib: true });
    const karyaSendiri = adaLisensi && m.license === KARYA_SENDIRI;

    if (adaLisensi && !karyaSendiri && m.license.toLowerCase() === KARYA_SENDIRI.toLowerCase()) {
      // Server membakukan huruf besar-kecilnya; berkas yang berbeda akan membuat "rencana kosong"
      // tak pernah tercapai (berkas ≠ server selamanya).
      galat.push(`${di}.license: tulis persis '${KARYA_SENDIRI}' (server membakukannya; huruf lain membuat pemasangan tak pernah sama dengan berkas).`);
    }

    const adaNama = periksaTeksMedia(galat, `${di}.sourceName`, m.sourceName, BATAS_MEDIA.sourceName, { wajib: false });
    const adaUrl = periksaTeksMedia(galat, `${di}.sourceUrl`, m.sourceUrl, BATAS_MEDIA.sourceUrl, { wajib: false });

    if (adaUrl && !urlHttp(m.sourceUrl)) {
      galat.push(`${di}.sourceUrl: harus http/https absolut, bukan '${m.sourceUrl}'.`);
    }
    if (adaLisensi && !karyaSendiri && !(adaNama && adaUrl)) {
      galat.push(`${di}: selain '${KARYA_SENDIRI}', sumber wajib — sourceName DAN sourceUrl. Media tanpa asal-usul yang bisa diperiksa tak boleh tayang.`);
    }
  });

  return kunci;
}

function periksaGambar(galat, di, m, { slugTopik, folderPublik }) {
  if (m.videoId !== undefined) {
    galat.push(`${di}.videoId: gambar tak punya videoId.`);
  }
  if (!adalahTeks(m.url)) {
    galat.push(`${di}.url: gambar wajib punya url (berkas sendiri di /media/${slugTopik}/…).`);
    return;
  }
  if (m.url.length > BATAS_MEDIA.url) {
    galat.push(`${di}.url: ${m.url.length} karakter, batasnya ${BATAS_MEDIA.url}.`);
  }
  if (!POLA_BERKAS.test(m.url) || m.url.includes('..') || m.url.includes('//')) {
    galat.push(`${di}.url: harus jalur berkas sendiri di /media/ berekstensi svg, png, jpg, jpeg, webp, atau avif — bukan '${m.url}'. Gambar hotlink tidak diterima.`);
    return;
  }
  if (!m.url.startsWith(`/media/${slugTopik}/`)) {
    galat.push(`${di}.url: gambar topik ini hidup di /media/${slugTopik}/…, bukan '${m.url}'.`);
    return;
  }

  const jalur = join(folderPublik, m.url);
  let info;
  try {
    info = statSync(jalur);
  } catch {
    galat.push(`${di}.url: berkas tak ada. Taruh di apps/web/public${m.url}.`);
    return;
  }
  if (!info.isFile()) {
    galat.push(`${di}.url: ${m.url} bukan berkas.`);
    return;
  }

  const svg = /\.svg$/i.test(m.url);
  const batas = svg ? BATAS_MEDIA.bytSvg : BATAS_MEDIA.bytRaster;
  if (info.size > batas) {
    galat.push(`${di}.url: berkas ${Math.round(info.size / 1024)} KB, batasnya ${Math.round(batas / 1024)} KB — tiap gambar dimuat pembaca; kecilkan.`);
  }
  if (svg) {
    for (const masalah of periksaSvg(readFileSync(jalur, 'utf8'))) {
      galat.push(`${di}.url (${m.url}): ${masalah}`);
    }
  }
}

/**
 * Mencocokkan kunci yang didefinisikan di `media` dengan yang dirujuk `::media[kunci]` di teks.
 * Dua arah, dan keduanya galat: rujukan tanpa definisi tampil kosong di situs; definisi tanpa
 * rujukan tak pernah tampil sama sekali (penampil hanya merender yang dirujuk) — biasanya salah
 * ketik di salah satu sisi, dan kalau dibiarkan ia menjadi sisa yang tetap dibawa API dan dibaca
 * peninjau tanpa guna.
 */
export function cocokkanRujukan(galat, tempat, didefinisikan, dirujuk) {
  for (const k of dirujuk) {
    if (!didefinisikan.has(k)) {
      galat.push(`${tempat}: ::media[${k}] tak punya definisi di daftar media.`);
    }
  }
  const dipakai = new Set(dirujuk);
  for (const k of didefinisikan) {
    if (!dipakai.has(k)) {
      galat.push(`${tempat}: media '${k}' didefinisikan tetapi tak dirujuk satu ::media[${k}] pun di teks — buang, atau pakai.`);
    }
  }
}
