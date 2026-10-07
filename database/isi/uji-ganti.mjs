// Gerbang ADR-028 Tahap 2: `pasang.mjs --ganti` membuktikan dirinya terhadap API sungguhan.
//
//   node database/isi/uji-ganti.mjs [<slug>]
//
// Ia MENYIMPANGKAN server dari berkas isi/ lewat API (ganti teks, tambah yang tak ada di
// berkas), lalu menjalankan pemasang sebagai proses terpisah dan memeriksa tiap akibatnya:
//
//   A  tanpa --ganti     -> BEDA, kode 1, server TIDAK berubah (perilaku ADR-027 utuh)
//   B  dengan --ganti    -> server kembali sama dengan berkas; rencana dicetak dulu
//   C  tanpa --ganti     -> sekarang sama: nol tulisan
//   D  --ganti lagi      -> idempoten: tak ada rencana, tak ada tulisan
//   E  topik `tinjau`    -> TERKUNCI walau --ganti, server TIDAK berubah
//   F  langkah berlebih  -> BEDA walau --ganti (langkah roadmap tak bisa dibuang), tak berubah
//
// ⚠️ Ia MENULIS ke API di $API_BASE_URL dan meninggalkan sisa (alat dan topik penyimpang
// di katalog, satu langkah roadmap berlebih, status `tinjau`). Hanya untuk basis data
// SEKALI PAKAI: gerbang CI memakai `techversex_isi` yang lahir kosong tiap jalan. Ia menolak
// jalan kalau topiknya belum terpasang dan berstatus draf — tanda basis data yang salah.
//
// Kenapa skrip tersendiri dan bukan opsi di pasang.mjs: pemasang itu jalan PRODUKSI
// (isi.yml) dan `IsiWorkflowGuardTests` menjaganya dari segala yang menyentuh tinjau.
// Gerbang ini butuh menaikkan topik ke tinjau untuk membuktikan kuncinya, jadi ia tak
// boleh tinggal di sana.

import { spawnSync } from 'node:child_process';
import { readdirSync, readFileSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const API = process.env.API_BASE_URL ?? 'http://localhost:5080';
const FOLDER = dirname(fileURLToPath(import.meta.url));
const AKAR = resolve(FOLDER, '..', '..');
const PASANG = join(FOLDER, 'pasang.mjs');

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

/** Penyimpangan harus berhasil — kalau tidak, gerbangnya yang rusak, bukan pemasangnya. */
async function simpangkan(method, path, body) {
  const r = await api(method, path, body);
  if (r.status < 200 || r.status > 299) {
    console.log(`GERBANG RUSAK: ${method} ${path} -> ${r.status} ${r.teks}`);
    process.exit(2);
  }
  return r.json;
}

function jalankan(...argumen) {
  const hasil = spawnSync(process.execPath, [PASANG, ...argumen], {
    env: { ...process.env, API_BASE_URL: API, NO_COLOR: '1' },
    encoding: 'utf8',
  });
  return { kode: hasil.status, keluaran: `${hasil.stdout}${hasil.stderr}` };
}

// Baris yang berarti "pemasang MENULIS sesuatu" (lihat pasang.mjs): awalan lalu DUA spasi
// atau lebih. Baris BEDA memakai awalan yang sama tapi satu spasi (`  proyek 'X' ada di
// server...`), dan tak boleh terhitung sebagai tulisan.
const BARIS_TULIS = /^ {2}(dibuat|langkah( 0)?|proyek|sumber|sisi|draf|ganti|buang|lepas|media) {2,}/m;

function pilihBerkas(slugDiminta) {
  const kandidat = [];
  const folder = readdirSync(join(AKAR, 'isi'), { withFileTypes: true })
    .filter((e) => e.isDirectory())
    .map((e) => e.name)
    .sort();
  for (const bidang of folder) {
    for (const berkas of readdirSync(join(AKAR, 'isi', bidang)).sort()) {
      if (berkas.endsWith('.json')) {
        kandidat.push(join(AKAR, 'isi', bidang, berkas));
      }
    }
  }
  const dipilih = slugDiminta ? kandidat.find((k) => k.endsWith(`${slugDiminta}.json`)) : kandidat[0];
  if (!dipilih) {
    console.log(`Tak ada berkas isi${slugDiminta ? ` untuk '${slugDiminta}'` : ''}.`);
    process.exit(2);
  }
  return JSON.parse(readFileSync(dipilih, 'utf8'));
}

const d = pilihBerkas(process.argv[2]);
const slug = d.slug;
const topik = `/api/v1/technologies/${slug}`;
const baca = async () => (await api('GET', topik)).json;
const unik = Date.now().toString(36);

console.log(`Gerbang --ganti terhadap ${API}, topik ${slug}`);

const awal = await baca();
if (awal === null || awal.maturity !== 'MachineDrafted') {
  console.log(`GERBANG RUSAK: ${slug} harus sudah terpasang dan berstatus draf (maturity=${awal?.maturity}). Pasang dulu: node database/isi/pasang.mjs --semua`);
  process.exit(2);
}

// ---- Menyimpangkan server dari berkas ----------------------------------------

console.log('\n== Menyimpangkan server dari berkas');
await simpangkan('PUT', topik, { name: `${d.name} (simpang)`, summary: `${d.summary} (simpang)` });
await simpangkan('PUT', `${topik}/roadmap/1`, {
  title: `${d.roadmap[0].title} (simpang)`,
  description: `${d.roadmap[0].description} (simpang)`,
});
await simpangkan('POST', `${topik}/projects`, { title: 'Proyek penyimpang', brief: 'Tak ada di berkas.' });
await simpangkan('POST', `${topik}/resources`, { type: 'OfficialDocs', title: 'Sumber penyimpang', url: 'https://example.com/penyimpang' });

const alatBersama = d.tools[0];
await simpangkan('PUT', `/api/v1/tools/${alatBersama.slug}`, {
  name: alatBersama.name,
  summary: `${alatBersama.summary} (simpang)`,
  homepage: alatBersama.homepage,
});

const alatLiar = `penyimpang-${unik}`;
await simpangkan('POST', '/api/v1/tools', { name: 'Alat penyimpang', summary: 'Tak ada di berkas.', slug: alatLiar });
await simpangkan('POST', `${topik}/tools`, { toolSlug: alatLiar, note: 'Tak ada di berkas.' });

const topikLiar = `penyimpang-topik-${unik}`;
await simpangkan('POST', '/api/v1/technologies', { name: 'Topik penyimpang', summary: 'Prasyarat yang tak ada di berkas.', fieldSlug: d.fieldSlug, slug: topikLiar });
await simpangkan('POST', `${topik}/requires`, { topicSlug: topikLiar });

const simpang = JSON.stringify(await baca());

// ---- A: tanpa --ganti, perilaku ADR-027 utuh ----------------------------------

console.log('\n== A. Tanpa --ganti: BEDA, tak ada yang ditulis');
const a = jalankan(slug);
harus(a.kode === 1, 'kode keluar 1');
harus(a.keluaran.includes('BEDA'), 'melaporkan BEDA');
harus(a.keluaran.includes('--ganti'), 'menyebut jalan keluarnya (--ganti)');
harus(!BARIS_TULIS.test(a.keluaran), 'tak mencetak baris tulis apa pun');
harus(JSON.stringify(await baca()) === simpang, 'server TIDAK berubah (sama persis, termasuk updatedAt)');

// ---- B: --ganti menyamakan -----------------------------------------------------

console.log('\n== B. Dengan --ganti: server kembali sama dengan berkas');
const b = jalankan('--ganti', slug);
if (b.kode !== 0) console.log(b.keluaran);
harus(b.kode === 0, 'kode keluar 0');
harus(b.keluaran.includes('selesai'), 'berakhir dengan "selesai ... sama dengan berkas"');
for (const rencana of [
  // `PUT` topik mengganti nama, ringkasan, DAN overview sekaligus (ADR-028 Tahap 3b), jadi rencananya
  // menyebut ketiganya. Topik pilot belum punya overview; yang dijaga di sini hanya bahwa rencana dicetak.
  'ganti nama/ringkasan/overview topik',
  'ganti langkah roadmap 1',
  'buang proyek',
  'buang sumber',
  'lepas alat',
  'buang relasi',
  'ganti definisi katalog alat',
]) {
  harus(b.keluaran.includes(`rencana    ${rencana}`), `rencana mencetak "${rencana}" sebelum menulis`);
}

const sesudah = await baca();
harus(sesudah.name === d.name && sesudah.summary === d.summary, 'nama dan ringkasan kembali ke berkas');
harus(sesudah.roadmap.find((l) => l.order === 1).title === d.roadmap[0].title, 'langkah 1 kembali ke berkas, di tempatnya');
harus(sesudah.roadmap.length === d.roadmap.length + 1, `jumlah langkah tetap ${d.roadmap.length + 1} (prasyarat + roadmap) — tak ada yang digandakan`);
harus(sesudah.projects.length === d.projects.length, 'proyek penyimpang terbuang, milik berkas utuh');
harus(sesudah.resources.length === d.resources.length, 'sumber penyimpang terbuang, milik berkas utuh');
harus(!sesudah.tools.some((x) => x.slug === alatLiar), 'tautan alat penyimpang terlepas');
harus(sesudah.tools.find((x) => x.slug === alatBersama.slug)?.summary === alatBersama.summary, 'definisi katalog alat kembali ke berkas');
harus(!sesudah.requires.some((x) => x.slug === topikLiar), 'relasi penyimpang terbuang');
harus(sesudah.maturity === 'MachineDrafted', 'tetap draf — --ganti tak pernah menaikkan ke tinjau');
harus(sesudah.reviewedAt === null, 'reviewedAt tetap null');

// ---- C dan D: idempoten ---------------------------------------------------------

console.log('\n== C. Tanpa --ganti sesudahnya: sudah sama, nol tulisan');
const c = jalankan(slug);
harus(c.kode === 0 && c.keluaran.includes('sudah sama dengan berkas'), 'sudah sama dengan berkas, kode 0');
harus(!BARIS_TULIS.test(c.keluaran), 'tak menulis apa pun');

console.log('\n== D. --ganti sekali lagi: idempoten');
const sebelumD = JSON.stringify(await baca());
const dd = jalankan('--ganti', slug);
harus(dd.kode === 0 && dd.keluaran.includes('sudah sama dengan berkas'), 'sudah sama dengan berkas, kode 0');
harus(!dd.keluaran.includes('rencana') && !BARIS_TULIS.test(dd.keluaran), 'tak ada rencana dan tak ada tulisan');
harus(JSON.stringify(await baca()) === sebelumD, 'server tak berubah (updatedAt pun tak bergerak)');

// ---- E: tinjau tetap terkunci ---------------------------------------------------

console.log('\n== E. Topik tinjau terkunci walau --ganti');
await simpangkan('POST', `${topik}/tinjau`, { reviewer: 'Gerbang CI' });
await simpangkan('POST', `${topik}/projects`, { title: 'Proyek sesudah tinjau', brief: 'Menambah tak menggugurkan tinjau.' });
const sebelumE = JSON.stringify(await baca());
harus(JSON.parse(sebelumE).maturity === 'HumanReviewed', 'prasyarat uji: topik berstatus tinjau');

const e = jalankan('--ganti', slug);
harus(e.kode === 1, 'kode keluar 1');
harus(e.keluaran.includes('TERKUNCI'), 'melaporkan TERKUNCI');
harus(JSON.stringify(await baca()) === sebelumE, 'server TIDAK berubah (proyek tambahan masih di sana, tinjau utuh)');

// ---- F: kelebihan langkah roadmap tak punya jalan -------------------------------

console.log('\n== F. Langkah roadmap berlebih di server: tak bisa dibuang, tetap BEDA');
await simpangkan('POST', `${topik}/roadmap`, { title: 'Langkah berlebih', description: 'Tak ada di berkas.' });
const sebelumF = JSON.stringify(await baca());

const f = jalankan('--ganti', slug);
harus(f.kode === 1 && f.keluaran.includes('BEDA'), 'BEDA, kode 1');
harus(f.keluaran.includes('tak ada jalan membuang langkah'), 'menyebut alasannya');
harus(JSON.stringify(await baca()) === sebelumF, 'server TIDAK berubah');

console.log('');
if (kegagalan.length > 0) {
  console.log(`${kegagalan.length} pemeriksaan GAGAL.`);
  process.exit(1);
}
console.log('SIAP: --ganti menyamakan server dengan berkas, idempoten, dan tak pernah menyentuh tinjau.');
