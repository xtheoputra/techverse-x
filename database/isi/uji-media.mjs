// Gerbang ADR-028 Tahap 3b: `overview` dan `media` di berkas isi dipasang ke API SUNGGUHAN, dan
// `--ganti` menyamakannya kembali.
//
//   GERBANG_BASIS_DATA_SEKALI_PAKAI=ya node database/isi/uji-media.mjs
//
// Pilot MCP belum punya overview maupun media, jadi gerbang ini membangun TOPIK FIXTURE sendiri di
// folder sementara (pasang.mjs membacanya lewat ISI_DIR — hanya untuk gerbang ini) dan memasangnya:
//
//   A  pasang dari nol         -> overview dan ketiga media tiba; berkas == server
//   B  pasang ulang            -> nol tulisan
//   C  server disimpangkan     -> tanpa --ganti: BEDA, kode 1, server TAK berubah
//   D  dengan --ganti          -> overview kembali, media diganti, dibuang, dan dipulihkan; idempoten
//   E  berkas yang cacat       -> --cek menolak rujukan tanpa definisi DAN definisi tanpa rujukan,
//                                 dan SVG yang membawa skrip
//
// ⚠️ Ia MENULIS ke API di $API_BASE_URL dan meninggalkan satu topik fixture. Hanya untuk basis data
// SEKALI PAKAI (gerbang CI memakai `techversex_isi`); karena itu ia menolak jalan tanpa
// GERBANG_BASIS_DATA_SEKALI_PAKAI=ya. Satu berkas SVG sementara ditaruh di apps/web/public/media/ dan
// SELALU dihapus lagi (try/finally).

import { spawnSync } from 'node:child_process';
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const API = process.env.API_BASE_URL ?? 'http://localhost:5080';
const FOLDER = dirname(fileURLToPath(import.meta.url));
const AKAR = resolve(FOLDER, '..', '..');
const PASANG = join(FOLDER, 'pasang.mjs');

if (process.env.GERBANG_BASIS_DATA_SEKALI_PAKAI !== 'ya') {
  console.log('Gerbang ini menulis topik uji ke API. Jalankan hanya di basis data sekali pakai:');
  console.log('  GERBANG_BASIS_DATA_SEKALI_PAKAI=ya node database/isi/uji-media.mjs');
  process.exit(2);
}

const kegagalan = [];
const harus = (kondisi, pesan) => {
  if (!kondisi) {
    kegagalan.push(pesan);
    console.log(`  GAGAL  ${pesan}`);
  } else {
    console.log(`  ok     ${pesan}`);
  }
};

async function api(method, path, body) {
  const respons = await fetch(`${API}${path}`, {
    method,
    headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  const teks = await respons.text();
  let json = null;
  try {
    json = teks ? JSON.parse(teks) : null;
  } catch {
    json = null;
  }
  return { status: respons.status, json, teks };
}

async function simpangkan(method, path, body) {
  const r = await api(method, path, body);
  if (r.status < 200 || r.status > 299) {
    console.log(`GERBANG RUSAK: ${method} ${path} -> ${r.status} ${r.teks}`);
    process.exit(2);
  }
  return r.json;
}

function jalankan(isiDir, ...argumen) {
  const hasil = spawnSync(process.execPath, [PASANG, ...argumen], {
    env: { ...process.env, API_BASE_URL: API, ISI_DIR: isiDir, NO_COLOR: '1' },
    encoding: 'utf8',
  });
  return { kode: hasil.status, keluaran: `${hasil.stdout}${hasil.stderr}` };
}

// Baris yang berarti "pemasang MENULIS sesuatu": awalan lalu dua spasi atau lebih (baris BEDA
// memakai awalan yang sama dengan satu spasi dan tak terhitung).
const BARIS_TULIS = /^ {2}(dibuat|langkah( 0)?|proyek|sumber|sisi|draf|ganti|buang|lepas|media) {2,}/m;

const unik = Date.now().toString(36);
const slug = `uji-media-${unik}`;
const slugAlat = `alat-uji-media-${unik}`;
const topik = `/api/v1/technologies/${slug}`;
const SVG_SAH = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10" fill="#22d3ee"/></svg>';

const folderGambar = join(AKAR, 'apps', 'web', 'public', 'media', slug);
const isiDir = mkdtempSync(join(tmpdir(), 'uji-media-'));
mkdirSync(join(isiDir, 'ai-agents'), { recursive: true });
const berkasTopik = join(isiDir, 'ai-agents', `${slug}.json`);

const media = [
  { key: 'diagram', kind: 'Image', url: `/media/${slug}/diagram.svg`, alt: 'Diagram uji', caption: 'Persegi biru.', license: 'Karya sendiri' },
  { key: 'demo', kind: 'Video', videoId: 'dQw4w9WgXcQ', alt: 'Demo uji', caption: 'Video uji.', sourceName: 'Kanal uji', sourceUrl: 'https://www.youtube.com/@uji', license: 'Hak cipta pemilik kanal' },
  { key: 'ceramah', kind: 'Video', videoId: 'aqz-KE-bpKQ', alt: 'Ceramah uji', sourceName: 'Kanal lain', sourceUrl: 'https://www.youtube.com/@lain', license: 'CC BY 3.0' },
];

const overview = '## Gambaran\n\nPendalaman **berformat** untuk uji.\n\n::media[diagram]\n\n### Dua video\n\n::media[demo]\n\n::media[ceramah]';

function fixture(ubah = {}) {
  return {
    slug,
    name: 'Topik Uji Media',
    fieldSlug: 'ai-agents',
    summary: 'Ringkasan polos untuk gerbang uji media.',
    overview,
    prerequisite: { title: 'Prasyarat uji', description: 'Tak ada yang istimewa.' },
    roadmap: [{ title: 'Langkah uji', description: 'Satu langkah.' }],
    tools: [{ slug: slugAlat, name: 'Alat uji media', summary: 'Alat untuk gerbang uji.', homepage: 'https://example.com/alat', note: 'Dipakai di uji.' }],
    projects: [{ title: 'Proyek uji', brief: 'Satu proyek.' }],
    resources: [{ type: 'OfficialDocs', title: 'Dokumentasi uji', url: 'https://example.com/dokumen' }],
    requires: [],
    media,
    ...ubah,
  };
}

const tulisFixture = (isi) => writeFileSync(berkasTopik, JSON.stringify(isi, null, 2));

try {
  mkdirSync(folderGambar, { recursive: true });
  writeFileSync(join(folderGambar, 'diagram.svg'), SVG_SAH);
  tulisFixture(fixture());

  console.log(`Gerbang media terhadap ${API}, topik ${slug}`);

  // ---- A: pasang dari nol ---------------------------------------------------------
  console.log('\n== A. Pasang dari nol: overview dan ketiga media tiba');
  const a = jalankan(isiDir, slug);
  if (a.kode !== 0) console.log(a.keluaran);
  harus(a.kode === 0, 'kode keluar 0');
  harus(a.keluaran.includes('selesai'), 'berakhir dengan "selesai ... sama dengan berkas"');
  harus(/media {6}diagram/.test(a.keluaran) && /media {6}demo/.test(a.keluaran) && /media {6}ceramah/.test(a.keluaran), 'mencetak ketiga media yang ditulis');
  harus(a.keluaran.includes('nama/ringkasan/overview'), 'overview topik baru ditetapkan sesudah pembuatan (POST tak membawanya)');

  const awal = (await api('GET', topik)).json;
  harus(awal.overview === overview, 'overview di server persis berkas');
  harus(awal.media.length === 3, 'tiga media di server');
  harus(awal.media.find((m) => m.key === 'diagram')?.url === `/media/${slug}/diagram.svg`, 'gambar membawa url berkas sendiri');
  harus(awal.media.find((m) => m.key === 'demo')?.videoId === 'dQw4w9WgXcQ', 'video membawa videoId');
  harus(awal.maturity === 'MachineDrafted', 'topik berstatus draf (tak pernah tinjau)');

  // ---- B: idempoten ---------------------------------------------------------------
  console.log('\n== B. Pasang ulang: nol tulisan');
  const b = jalankan(isiDir, slug);
  harus(b.kode === 0 && b.keluaran.includes('sudah sama dengan berkas'), 'sudah sama dengan berkas, kode 0');
  harus(!BARIS_TULIS.test(b.keluaran), 'tak menulis apa pun');

  // ---- C: server disimpangkan -----------------------------------------------------
  console.log('\n== C. Server disimpangkan; tanpa --ganti: BEDA, server tak berubah');
  await simpangkan('PUT', topik, { name: 'Topik Uji Media', summary: 'Ringkasan polos untuk gerbang uji media.', overview: `${overview}\n\nParagraf yang menyimpang.` });
  await simpangkan('PUT', `${topik}/media/demo`, { kind: 'Video', videoId: 'dQw4w9WgXcQ', alt: 'Judul yang menyimpang', caption: 'Video uji.', sourceName: 'Kanal uji', sourceUrl: 'https://www.youtube.com/@uji', license: 'Hak cipta pemilik kanal' });
  await simpangkan('PUT', `${topik}/media/liar`, { kind: 'Video', videoId: 'M7lc1UVf-VE', alt: 'Media liar', sourceName: 'Kanal', sourceUrl: 'https://x.test', license: 'Lisensi' });
  await simpangkan('DELETE', `${topik}/media/ceramah`);

  const simpang = JSON.stringify((await api('GET', topik)).json);
  const c = jalankan(isiDir, slug);
  harus(c.kode === 1 && c.keluaran.includes('BEDA'), 'BEDA, kode 1');
  harus(c.keluaran.includes('overview: berkas ≠ server'), 'menyebut overview yang berbeda');
  harus(c.keluaran.includes("media 'demo': isi berbeda"), 'menyebut media yang isinya berbeda');
  harus(c.keluaran.includes("media 'liar' ada di server"), 'menyebut media berlebih di server');
  harus(c.keluaran.includes('--ganti'), 'menyebut jalan keluarnya (--ganti)');
  harus(!BARIS_TULIS.test(c.keluaran), 'tak mencetak baris tulis apa pun');
  harus(JSON.stringify((await api('GET', topik)).json) === simpang, 'server TIDAK berubah (sama persis, termasuk updatedAt)');

  // ---- D: --ganti ------------------------------------------------------------------
  console.log('\n== D. Dengan --ganti: server kembali sama dengan berkas');
  const d = jalankan(isiDir, '--ganti', slug);
  if (d.kode !== 0) console.log(d.keluaran);
  harus(d.kode === 0 && d.keluaran.includes('selesai'), 'kode 0 dan "selesai"');
  harus(d.keluaran.includes('rencana    ganti nama/ringkasan/overview topik'), 'rencana mencetak penggantian overview sebelum menulis');
  harus(d.keluaran.includes("rencana    ganti media 'demo'"), "rencana mencetak penggantian media 'demo'");
  harus(d.keluaran.includes("rencana    buang media 'liar'"), "rencana mencetak pembuangan media 'liar'");
  harus(d.keluaran.includes("rencana    tambah media 'ceramah'"), "rencana mencetak penambahan kembali media 'ceramah'");

  const sesudah = (await api('GET', topik)).json;
  harus(sesudah.overview === overview, 'overview kembali ke berkas');
  harus(sesudah.media.length === 3 && !sesudah.media.some((m) => m.key === 'liar'), 'media liar terbuang, ketiga milik berkas utuh');
  harus(sesudah.media.find((m) => m.key === 'demo')?.alt === 'Demo uji', 'media yang diganti kembali ke berkas');
  harus(sesudah.media.some((m) => m.key === 'ceramah'), 'media yang terbuang ditambahkan lagi');

  const dd = jalankan(isiDir, '--ganti', slug);
  harus(dd.kode === 0 && dd.keluaran.includes('sudah sama dengan berkas') && !dd.keluaran.includes('rencana') && !BARIS_TULIS.test(dd.keluaran), '--ganti sekali lagi: nol rencana, nol tulisan');

  // ---- D2: menghapus overview dari berkas ----------------------------------------------
  console.log('\n== D2. Overview dibuang dari berkas: tanpa --ganti BEDA, dengan --ganti dikosongkan');
  const tanpaOverview = { ...fixture({ media: media.filter((m) => m.key !== 'diagram') }) };
  delete tanpaOverview.overview;
  // Teks tak merujuk media lagi, jadi daftar media pun harus dikosongkan agar --cek sah.
  tanpaOverview.media = [];
  tulisFixture(tanpaOverview);
  const d2a = jalankan(isiDir, slug);
  harus(d2a.kode === 1 && d2a.keluaran.includes('overview: berkas ≠ server'), 'tanpa --ganti: BEDA menyebut overview');
  const d2b = jalankan(isiDir, '--ganti', slug);
  if (d2b.kode !== 0) console.log(d2b.keluaran);
  harus(d2b.kode === 0, '--ganti berhasil');
  const kosong = (await api('GET', topik)).json;
  harus(kosong.overview === null && kosong.media.length === 0, 'overview null dan semua media terbuang');

  // ---- E: --cek menolak berkas cacat -------------------------------------------------------
  console.log('\n== E. --cek menolak berkas yang cacat');
  tulisFixture(fixture({ overview: `${overview}\n\n::media[tak-didefinisikan]` }));
  let e = jalankan(isiDir, '--cek');
  harus(e.kode === 1 && e.keluaran.includes('::media[tak-didefinisikan] tak punya definisi'), 'rujukan tanpa definisi ditolak');

  tulisFixture(fixture({ media: [...media, { key: 'tak-dipakai', kind: 'Video', videoId: 'M7lc1UVf-VE', alt: 'Tak dipakai', sourceName: 'K', sourceUrl: 'https://x.test', license: 'L' }] }));
  e = jalankan(isiDir, '--cek');
  harus(e.kode === 1 && e.keluaran.includes("media 'tak-dipakai' didefinisikan tetapi tak dirujuk"), 'definisi tanpa rujukan ditolak');

  writeFileSync(join(folderGambar, 'diagram.svg'), '<svg viewBox="0 0 1 1"><script>alert(1)</script></svg>');
  tulisFixture(fixture());
  e = jalankan(isiDir, '--cek');
  harus(e.kode === 1 && e.keluaran.includes('<script>'), 'SVG yang membawa skrip ditolak');

  rmSync(join(folderGambar, 'diagram.svg'));
  e = jalankan(isiDir, '--cek');
  harus(e.kode === 1 && e.keluaran.includes('berkas tak ada'), 'gambar yang berkasnya tak ada ditolak');

  writeFileSync(join(folderGambar, 'diagram.svg'), SVG_SAH);
  e = jalankan(isiDir, '--cek');
  harus(e.kode === 0, 'kendali: berkas sah dan SVG bersih lolos --cek');
} finally {
  rmSync(folderGambar, { recursive: true, force: true });
  rmSync(isiDir, { recursive: true, force: true });
}

console.log('');
if (kegagalan.length > 0) {
  console.log(`${kegagalan.length} pemeriksaan GAGAL.`);
  process.exit(1);
}
console.log('SIAP: overview dan media dipasang, disamakan kembali, dan dijaga pemeriksa.');
