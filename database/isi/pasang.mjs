// Jalan isi sungguhan — ADR-027.
//
//   node database/isi/pasang.mjs --cek        periksa SEMUA berkas isi/, tanpa HTTP
//   node database/isi/pasang.mjs <slug>       pasang satu topik ke $API_BASE_URL
//   node database/isi/pasang.mjs --semua      pasang SEMUA topik, prasyarat lebih dulu
//                                             (untuk pengembangan dan gerbang CI; produksi
//                                             satu topik per jalan isi.yml)
//
// Isi sungguhan hidup sebagai berkas JSON di `isi/<bidang>/<slug>.json`, bukan di
// kepala siapa pun dan bukan di Neon saja. Berkas itulah yang dibaca pemilik di
// diff PR sebelum merge — itu titik baca-manusia pertama. Neon hanyalah proyeksi
// dari berkas, jadi basis data yang hilang bisa dibangun ulang darinya.
//
// 🔑 Berkas ini BERBICARA KE API YANG SAMA dengan seed dan alur `tinjau`: tidak ada
// jalur tulis kedua (ADR-021 menolak "alat CLI baru di dalam citra"). Bentuk berkas
// sengaja SAMA dengan bentuk muatan API (name/summary/fieldSlug, prerequisite,
// roadmap, tools, projects, resources) supaya tak ada lapisan terjemahan yang bisa
// menyimpang.
//
// 🔴 BERKAS INI TIDAK PERNAH MENAIKKAN KE `tinjau`, dan tak boleh. `tinjau` adalah
// satu-satunya klaim kepercayaan produk ini — "sudah diperiksa manusia" — dan ia
// punya jalannya sendiri (`tinjau.yml`, nama pemeriksa dari github.actor).
// `IsiWorkflowGuardTests` menolak kata itu di sini maupun di isi.yml. Pemasang
// paling jauh membawa topik ke `draf`.
//
// ⚠️ Aturan yang membedakannya dari seed.mjs: pemasang ini TIDAK PERNAH menimpa
// diam-diam. Berkas dan server dibandingkan dulu; kalau server punya sesuatu yang
// tak ada di berkas (atau nama/ringkasan berbeda — tak ada endpoint untuk
// mengubahnya), pemasang berhenti SEBELUM menulis apa pun dan menyebut bedanya.
// Topik yang sudah `tinjau` tak disentuh sama sekali.
//
// Node 22+ (fetch bawaan, tanpa dependensi). Semua keluaran ke STDOUT, kegagalan
// dibawa kode keluar — alasannya dijelaskan di database/seeds/seed.mjs.

import { readdirSync, readFileSync, statSync } from 'node:fs';
import { basename, dirname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const API = process.env.API_BASE_URL ?? 'http://localhost:5080';
const AKAR = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');
const FOLDER_ISI = join(AKAR, 'isi');

// ---- Keluaran --------------------------------------------------------------

const berwarna = process.stdout.isTTY === true && !process.env.NO_COLOR;
const warna = (kode, teks) => (berwarna ? `\x1b[${kode}m${teks}\x1b[0m` : teks);
const judul = (teks) => console.log(warna('36', teks));

class Berhenti extends Error {}

function gagal(baris, rinci) {
  console.log(warna('31', baris));
  for (const r of [rinci ?? []].flat()) {
    console.log(`  ${r}`);
  }
  throw new Berhenti();
}

// ---- Batas panjang ---------------------------------------------------------
// Cermin dari konfigurasi EF (ContentSectionConfiguration, TechnologyConfiguration).
// Server tetap yang berwenang: gerbang CI "isi dipasang dari nol" memasang berkas
// sungguhan ke API sungguhan, jadi cermin yang menyimpang kelihatan di sana.

const BATAS = {
  slug: 160,
  nama: 200,
  ringkasan: 2000,
  judulLangkah: 200,
  uraianLangkah: 2000,
  judulProyek: 200,
  ringkasProyek: 4000,
  judulSumber: 300,
  url: 1000,
  catatanAlat: 500,
  homepageAlat: 500,
};

const JENIS_SUMBER = ['OfficialDocs', 'Video', 'Paper', 'Repository'];
const POLA_SLUG = /^[a-z0-9]+(-[a-z0-9]+)*$/;

const KUNCI_TOPIK = ['slug', 'name', 'fieldSlug', 'summary', 'prerequisite', 'roadmap', 'tools', 'projects', 'resources', 'requires'];
const KUNCI_LANGKAH = ['title', 'description'];
const KUNCI_ALAT = ['slug', 'name', 'summary', 'homepage', 'note'];
const KUNCI_PROYEK = ['title', 'brief'];
const KUNCI_SUMBER = ['type', 'title', 'url'];

// ---- Membaca berkas --------------------------------------------------------

function cariBerkas() {
  const hasil = [];
  if (!statSync(FOLDER_ISI, { throwIfNoEntry: false })?.isDirectory()) {
    return hasil;
  }
  for (const bidang of readdirSync(FOLDER_ISI).sort()) {
    const folderBidang = join(FOLDER_ISI, bidang);
    if (!statSync(folderBidang).isDirectory()) {
      continue;
    }
    for (const berkas of readdirSync(folderBidang).sort()) {
      if (berkas.endsWith('.json')) {
        hasil.push(join(folderBidang, berkas));
      }
    }
  }
  return hasil;
}

const nama = (jalur) => relative(AKAR, jalur).split('\\').join('/');

// ---- Pemeriksaan -----------------------------------------------------------

const adalahTeks = (v) => typeof v === 'string' && v.trim().length > 0;
const rapi = (v) => v === v.trim();

function periksaTeks(galat, tempat, nilai, maks) {
  if (!adalahTeks(nilai)) {
    galat.push(`${tempat}: wajib teks tak kosong.`);
    return false;
  }
  if (!rapi(nilai)) {
    galat.push(`${tempat}: ada spasi di awal atau akhir.`);
  }
  if (nilai.length > maks) {
    galat.push(`${tempat}: ${nilai.length} karakter, batasnya ${maks}.`);
  }
  return true;
}

function periksaUrl(galat, tempat, nilai, maks) {
  if (!periksaTeks(galat, tempat, nilai, maks)) {
    return;
  }
  try {
    const url = new URL(nilai);
    if (url.protocol !== 'http:' && url.protocol !== 'https:') {
      galat.push(`${tempat}: harus http/https absolut, bukan '${nilai}'.`);
    }
  } catch {
    galat.push(`${tempat}: bukan URL absolut: '${nilai}'.`);
  }
}

function periksaKunci(galat, tempat, objek, diizinkan) {
  if (objek === null || typeof objek !== 'object' || Array.isArray(objek)) {
    galat.push(`${tempat}: harus berupa objek.`);
    return false;
  }
  for (const k of Object.keys(objek)) {
    if (!diizinkan.includes(k)) {
      // Salah ketik kunci ("resorces") dibiarkan lolos akan terbaca "tak ada sumber"
      // — dan baru ketahuan di MarkDrafted, jauh dari sebabnya.
      galat.push(`${tempat}: kunci '${k}' tidak dikenal. Yang sah: ${diizinkan.join(', ')}.`);
    }
  }
  return true;
}

function periksaDaftar(galat, tempat, nilai, { minimal }) {
  if (!Array.isArray(nilai)) {
    galat.push(`${tempat}: harus berupa daftar.`);
    return false;
  }
  if (nilai.length < minimal) {
    galat.push(`${tempat}: minimal ${minimal} butir (MarkDrafted menolak bagian kosong).`);
  }
  return true;
}

function unik(galat, tempat, nilai) {
  const lihat = new Set();
  for (const n of nilai) {
    if (lihat.has(n)) {
      galat.push(`${tempat}: '${n}' muncul dua kali.`);
    }
    lihat.add(n);
  }
}

/** Memeriksa satu berkas yang SUDAH diurai. Mengembalikan daftar galat. */
function periksaTopik(jalur, d) {
  const galat = [];
  const t = nama(jalur);
  const di = (bagian) => `${t} ${bagian}`;

  if (!periksaKunci(galat, t, d, KUNCI_TOPIK)) {
    return galat;
  }

  if (periksaTeks(galat, di('slug'), d.slug, BATAS.slug)) {
    if (!POLA_SLUG.test(d.slug)) {
      galat.push(`${di('slug')}: '${d.slug}' bukan slug (huruf kecil, angka, tanda hubung tunggal).`);
    }
    if (d.slug !== basename(jalur, '.json')) {
      galat.push(`${di('slug')}: '${d.slug}' harus sama dengan nama berkasnya ('${basename(jalur, '.json')}').`);
    }
  }
  periksaTeks(galat, di('name'), d.name, BATAS.nama);
  periksaTeks(galat, di('summary'), d.summary, BATAS.ringkasan);

  if (periksaTeks(galat, di('fieldSlug'), d.fieldSlug, BATAS.slug) && d.fieldSlug !== basename(dirname(jalur))) {
    galat.push(`${di('fieldSlug')}: '${d.fieldSlug}' harus sama dengan nama foldernya ('${basename(dirname(jalur))}').`);
  }

  // Langkah 0 roadmap (ADR-012): prasyarat dilipat ke roadmap, dan AddRoadmapStep
  // menolak dipanggil sebelum ia ada.
  if (periksaKunci(galat, di('prerequisite'), d.prerequisite, KUNCI_LANGKAH)) {
    periksaTeks(galat, di('prerequisite.title'), d.prerequisite.title, BATAS.judulLangkah);
    periksaTeks(galat, di('prerequisite.description'), d.prerequisite.description, BATAS.uraianLangkah);
  }

  if (periksaDaftar(galat, di('roadmap'), d.roadmap, { minimal: 1 })) {
    d.roadmap.forEach((l, i) => {
      if (periksaKunci(galat, di(`roadmap[${i}]`), l, KUNCI_LANGKAH)) {
        periksaTeks(galat, di(`roadmap[${i}].title`), l.title, BATAS.judulLangkah);
        periksaTeks(galat, di(`roadmap[${i}].description`), l.description, BATAS.uraianLangkah);
      }
    });
    unik(galat, di('roadmap (judul)'), d.roadmap.map((l) => l?.title));
  }

  if (periksaDaftar(galat, di('tools'), d.tools, { minimal: 1 })) {
    d.tools.forEach((a, i) => {
      if (periksaKunci(galat, di(`tools[${i}]`), a, KUNCI_ALAT)) {
        if (periksaTeks(galat, di(`tools[${i}].slug`), a.slug, BATAS.slug) && !POLA_SLUG.test(a.slug)) {
          galat.push(`${di(`tools[${i}].slug`)}: '${a.slug}' bukan slug.`);
        }
        periksaTeks(galat, di(`tools[${i}].name`), a.name, BATAS.nama);
        periksaTeks(galat, di(`tools[${i}].summary`), a.summary, BATAS.ringkasan);
        periksaUrl(galat, di(`tools[${i}].homepage`), a.homepage, BATAS.homepageAlat);
        periksaTeks(galat, di(`tools[${i}].note`), a.note, BATAS.catatanAlat);
      }
    });
    unik(galat, di('tools (slug)'), d.tools.map((a) => a?.slug));
  }

  if (periksaDaftar(galat, di('projects'), d.projects, { minimal: 1 })) {
    d.projects.forEach((p, i) => {
      if (periksaKunci(galat, di(`projects[${i}]`), p, KUNCI_PROYEK)) {
        periksaTeks(galat, di(`projects[${i}].title`), p.title, BATAS.judulProyek);
        periksaTeks(galat, di(`projects[${i}].brief`), p.brief, BATAS.ringkasProyek);
      }
    });
    unik(galat, di('projects (judul)'), d.projects.map((p) => p?.title));
  }

  if (periksaDaftar(galat, di('resources'), d.resources, { minimal: 1 })) {
    d.resources.forEach((s, i) => {
      if (periksaKunci(galat, di(`resources[${i}]`), s, KUNCI_SUMBER)) {
        if (!JENIS_SUMBER.includes(s.type)) {
          galat.push(`${di(`resources[${i}].type`)}: '${s.type}' tidak dikenal. Yang sah, persis huruf besar-kecilnya: ${JENIS_SUMBER.join(', ')}.`);
        }
        periksaTeks(galat, di(`resources[${i}].title`), s.title, BATAS.judulSumber);
        periksaUrl(galat, di(`resources[${i}].url`), s.url, BATAS.url);
      }
    });
    // Sumber dikenali dari URL-nya, bukan judulnya (alasan yang sama dengan seed).
    unik(galat, di('resources (url)'), d.resources.map((s) => s?.url));
  }

  if (Array.isArray(d.requires)) {
    d.requires.forEach((r, i) => {
      if (!adalahTeks(r) || !POLA_SLUG.test(r)) {
        galat.push(`${di(`requires[${i}]`)}: harus slug topik.`);
      }
    });
    unik(galat, di('requires'), d.requires);
  } else {
    galat.push(`${di('requires')}: harus berupa daftar (boleh kosong).`);
  }

  return galat;
}

/** Membaca SEMUA berkas dan memeriksa tiap berkas serta hubungan antar-berkas. */
function bacaDanPeriksa() {
  const galat = [];
  const topik = [];

  for (const jalur of cariBerkas()) {
    let data;
    try {
      data = JSON.parse(readFileSync(jalur, 'utf8'));
    } catch (err) {
      galat.push(`${nama(jalur)}: bukan JSON yang sah (${err.message}).`);
      continue;
    }
    galat.push(...periksaTopik(jalur, data));
    topik.push({ jalur, data });
  }

  // Antar-berkas: slug topik unik; relasi hanya menunjuk topik yang ada di isi/
  // (di produksi semua topik datang dari sini); satu slug alat berarti SATU alat.
  const slugTopik = topik.map((b) => b.data?.slug);
  unik(galat, 'isi/ (slug topik)', slugTopik);

  for (const { jalur, data } of topik) {
    for (const r of Array.isArray(data?.requires) ? data.requires : []) {
      if (r === data.slug) {
        galat.push(`${nama(jalur)} requires: topik tak boleh menjadi prasyarat dirinya sendiri.`);
      } else if (!slugTopik.includes(r)) {
        galat.push(`${nama(jalur)} requires: '${r}' tak punya berkas di isi/ — di produksi relasi hanya bisa menunjuk topik yang dipasang dari sini.`);
      }
    }
  }

  // Siklus: server menolaknya juga (ADR-023), tapi di sini ia gagal di PR, bukan di
  // tengah pemasangan yang sudah menulis separuh.
  if (galat.length === 0 && urutanPasang(topik).siklus) {
    galat.push(`isi/ requires: ada siklus prasyarat (${urutanPasang(topik).siklus.join(' → ')}).`);
  }

  const alat = new Map();
  for (const { jalur, data } of topik) {
    for (const a of Array.isArray(data?.tools) ? data.tools : []) {
      const kunci = JSON.stringify([a?.name, a?.summary, a?.homepage]);
      const sebelumnya = alat.get(a?.slug);
      if (sebelumnya && sebelumnya.kunci !== kunci) {
        galat.push(`${nama(jalur)} tools: alat '${a.slug}' sudah didefinisikan berbeda di ${sebelumnya.jalur}. Katalog alat dipakai bersama — satu slug, satu definisi.`);
      } else if (!sebelumnya) {
        alat.set(a?.slug, { jalur: nama(jalur), kunci });
      }
    }
  }

  return { galat, topik };
}

/**
 * Urutan pasang: topik yang jadi prasyarat lebih dulu (POST …/requires menolak
 * tujuan yang belum ada). Mengembalikan { urut, siklus }.
 */
function urutanPasang(topik) {
  const peta = new Map(topik.map((b) => [b.data.slug, b.data]));
  const urut = [];
  const selesai = new Set();
  const jalan = [];
  let siklus = null;

  const kunjungi = (slug) => {
    if (selesai.has(slug) || siklus) {
      return;
    }
    if (jalan.includes(slug)) {
      siklus = [...jalan.slice(jalan.indexOf(slug)), slug];
      return;
    }
    jalan.push(slug);
    for (const r of peta.get(slug)?.requires ?? []) {
      if (peta.has(r)) {
        kunjungi(r);
      }
    }
    jalan.pop();
    selesai.add(slug);
    urut.push(slug);
  };

  for (const slug of peta.keys()) {
    kunjungi(slug);
  }
  return { urut, siklus };
}

// ---- HTTP ------------------------------------------------------------------

async function panggil(method, path, body) {
  let response;
  try {
    response = await fetch(`${API}${path}`, {
      method,
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch (err) {
    gagal(`API tidak terjangkau di ${API}. (${err.cause?.code ?? err.message})`);
  }

  const text = await response.text();
  let json = null;
  try {
    json = text ? JSON.parse(text) : null;
  } catch {
    json = null;
  }

  return { status: response.status, json, text };
}

function jelaskanPermukaanTulisTertutup(status) {
  // 404/405 pada rute tulis hampir selalu berarti satu hal: permukaan tulis memang
  // sengaja tidak dipasang (ADR-020). Jangan teruskan sisa pemasangan.
  gagal(`Endpoint tulis tidak dipasang di ${API} (HTTP ${status}).`, [
    'Itu bentuk PRODUKSI menurut ADR-020. Pemasangan ke produksi hanya lewat isi.yml,',
    'yang menyalakan citra api dengan Editorial__WritesEnabled=true di dalam runner.',
    'Untuk pengembangan: jalankan API dengan `run.ps1 api` atau `make api`.',
  ]);
}

/** Panggilan tulis yang WAJIB berhasil. */
async function wajib(method, path, body) {
  const respons = await panggil(method, path, body);
  if (respons.status === 404 || respons.status === 405) {
    // 404 juga arti "topik tak ada" — tapi topiknya baru dibaca/dibuat di atas.
    jelaskanPermukaanTulisTertutup(respons.status);
  }
  if (respons.status < 200 || respons.status > 299) {
    gagal(`GAGAL   ${method} ${path} (HTTP ${respons.status})`, respons.text);
  }
  return respons;
}

async function bacaTopik(slug) {
  const respons = await panggil('GET', `/api/v1/technologies/${slug}`);
  if (respons.status === 404) {
    return null;
  }
  if (respons.status !== 200) {
    gagal(`GAGAL   GET /api/v1/technologies/${slug} (HTTP ${respons.status})`, respons.text);
  }
  return respons.json;
}

// ---- Rencana: bandingkan berkas dengan server, SEBELUM menulis ---------------

/** Langkah roadmap di server, tanpa prasyarat (langkah 0), berurut. */
const langkahServer = (s) => (s?.roadmap ?? []).filter((l) => !l.isPrerequisite).sort((a, b) => a.order - b.order);
const prasyaratServer = (s) => (s?.roadmap ?? []).find((l) => l.isPrerequisite) ?? null;

/**
 * Membandingkan berkas dengan keadaan server dan menghasilkan:
 *   - `beda`: hal yang TIDAK BISA dipasang tanpa menimpa (pemasang berhenti),
 *   - `tulis`: daftar pekerjaan yang aman (hanya MENAMBAH).
 */
function susunRencana(d, s) {
  const beda = [];
  const tulis = [];

  if (s === null) {
    tulis.push('buat topik');
  } else {
    // Tak ada endpoint untuk mengubah nama, ringkasan, atau bidang sebuah topik.
    if (s.name !== d.name) beda.push(`name: berkas '${d.name}' ≠ server '${s.name}'`);
    if (s.summary !== d.summary) beda.push('summary: berkas ≠ server (tak ada endpoint untuk mengubahnya)');
    if (s.fieldSlug !== d.fieldSlug) beda.push(`fieldSlug: berkas '${d.fieldSlug}' ≠ server '${s.fieldSlug}'`);
  }

  const pras = s ? prasyaratServer(s) : null;
  if (pras === null || pras.title !== d.prerequisite.title || pras.description !== d.prerequisite.description) {
    // PUT mengganti langkah 0 — idempoten dan hanya mengubah satu butir milik berkas.
    tulis.push('tetapkan prasyarat');
  }

  // Roadmap: server HARUS berupa awalan dari berkas. Selebihnya ditambah di ujung —
  // AddRoadmapStep memberi nomor sendiri, dan tak ada jalan membuang langkah.
  const ada = langkahServer(s);
  ada.forEach((l, i) => {
    const mestinya = d.roadmap[i];
    if (!mestinya || mestinya.title !== l.title || mestinya.description !== l.description) {
      beda.push(`roadmap langkah ${i + 1}: server '${l.title}' tidak cocok dengan berkas${mestinya ? ` ('${mestinya.title}')` : ' (berkas lebih pendek)'}`);
    }
  });
  for (const l of d.roadmap.slice(ada.length)) {
    tulis.push(`tambah langkah roadmap '${l.title}'`);
  }

  const proyek = s?.projects ?? [];
  for (const p of proyek) {
    const cocok = d.projects.find((x) => x.title === p.title);
    if (!cocok) beda.push(`proyek '${p.title}' ada di server, tak ada di berkas`);
    else if (cocok.brief !== p.brief) beda.push(`proyek '${p.title}': isi berbeda dari berkas`);
  }
  for (const p of d.projects) {
    if (!proyek.some((x) => x.title === p.title)) tulis.push(`tambah proyek '${p.title}'`);
  }

  const sumber = s?.resources ?? [];
  for (const r of sumber) {
    const cocok = d.resources.find((x) => x.url === r.url);
    if (!cocok) beda.push(`sumber ${r.url} ada di server, tak ada di berkas`);
    else if (cocok.type !== r.type || cocok.title !== r.title) beda.push(`sumber ${r.url}: jenis/judul berbeda dari berkas`);
  }
  for (const r of d.resources) {
    if (!sumber.some((x) => x.url === r.url)) tulis.push(`tambah sumber ${r.url}`);
  }

  const alat = s?.tools ?? [];
  for (const a of alat) {
    const cocok = d.tools.find((x) => x.slug === a.slug);
    if (!cocok) {
      beda.push(`alat '${a.slug}' tertaut di server, tak ada di berkas`);
    } else if (cocok.name !== a.name || cocok.summary !== a.summary || (cocok.homepage ?? null) !== (a.homepage ?? null)) {
      beda.push(`alat '${a.slug}': definisi katalog berbeda dari berkas (tak ada endpoint untuk mengubah katalog)`);
    }
  }
  for (const a of d.tools) {
    const terpasang = alat.find((x) => x.slug === a.slug);
    if (!terpasang) tulis.push(`tautkan alat '${a.slug}'`);
    else if ((terpasang.note ?? null) !== (a.note ?? null)) tulis.push(`perbarui catatan alat '${a.slug}'`);
  }

  const sisi = (s?.requires ?? []).map((r) => r.slug);
  for (const r of sisi) {
    if (!d.requires.includes(r)) beda.push(`relasi: server mencatat butuh '${r}', berkas tidak`);
  }
  for (const r of d.requires) {
    if (!sisi.includes(r)) tulis.push(`catat relasi butuh '${r}'`);
  }

  if (s === null || s.maturity === 'Curated') {
    tulis.push('naik ke draf');
  }

  return { beda, tulis };
}

// ---- Memasang --------------------------------------------------------------

async function pasangTopik(d) {
  judul(`Memasang ${d.slug} ke ${API} ...`);

  const sekarang = await bacaTopik(d.slug);
  const { beda, tulis } = susunRencana(d, sekarang);

  if (beda.length > 0) {
    gagal(`BEDA    ${d.slug}: berkas dan server tidak sama. Tidak ada yang ditulis.`, [
      ...beda,
      '',
      'Pemasang tidak menimpa. Memperbaiki isi yang sudah terpasang memakai jalan buang yang sudah ada',
      '(DELETE sumber/proyek/alat, ADR-012 dan ADR-021 Pembaruan 2026-10-02) lalu memasang ulang.',
      'Mengubah name/summary/langkah roadmap belum punya jalan — itu keputusan tersendiri (ADR-027).',
    ]);
  }

  if (sekarang?.maturity === 'HumanReviewed') {
    if (tulis.length > 0) {
      gagal(`TERKUNCI ${d.slug} sudah tinjau; pemasang tidak menyentuhnya.`, tulis.map((t) => `akan ${t}`));
    }
    console.log(warna('32', `  ada     ${d.slug} sudah tinjau dan sama dengan berkas`));
    return;
  }

  if (tulis.length === 0) {
    console.log(warna('32', `  ada     ${d.slug} sudah sama dengan berkas`));
    return;
  }

  // Topiknya dulu. 409 punya dua arti (seed.mjs menuliskan alasannya): slug sudah
  // dipakai topik, atau dipegang BIDANG. Karena topik baru saja dibaca (404), 409
  // di sini hanya mungkin soal bidang — dan itu galat, bukan "sudah ada".
  if (sekarang === null) {
    const respons = await panggil('POST', '/api/v1/technologies', {
      name: d.name,
      slug: d.slug,
      summary: d.summary,
      fieldSlug: d.fieldSlug,
    });
    if (respons.status === 404 || respons.status === 405) {
      jelaskanPermukaanTulisTertutup(respons.status);
    }
    if (respons.status !== 201) {
      gagal(`GAGAL   buat ${d.slug} (HTTP ${respons.status})`, respons.text);
    }
    console.log(`  dibuat  ${d.name}`);
  }

  // Alat dulu: menautkan menuntut alatnya sudah ada di katalog. 409 di sini hanya
  // berarti slug alatnya sudah terpakai (katalog tak berbagi ruang nama dengan bidang).
  for (const a of d.tools) {
    const respons = await panggil('POST', '/api/v1/tools', {
      name: a.name,
      slug: a.slug,
      summary: a.summary,
      homepage: a.homepage,
    });
    if (respons.status === 201) {
      console.log(`  dibuat  alat ${a.slug}`);
    } else if (respons.status !== 409) {
      gagal(`GAGAL   alat ${a.slug} (HTTP ${respons.status})`, respons.text);
    }
  }

  // Prasyarat WAJIB lebih dulu: AddRoadmapStep menolak dipanggil sebelum langkah 0 ada.
  // PUT, jadi mengulanginya mengganti langkah 0, bukan menambah — tapi hanya dipanggil
  // kalau rencananya memang memuatnya, supaya pemasangan ulang tidak menulis yang tak perlu.
  if (tulis.includes('tetapkan prasyarat')) {
    await wajib('PUT', `/api/v1/technologies/${d.slug}/roadmap/prasyarat`, d.prerequisite);
    console.log(`  langkah 0  ${d.prerequisite.title}`);
  }

  const baru = await bacaTopik(d.slug);
  const sudahLangkah = langkahServer(baru).length;
  for (const l of d.roadmap.slice(sudahLangkah)) {
    await wajib('POST', `/api/v1/technologies/${d.slug}/roadmap`, l);
    console.log(`  langkah    ${l.title}`);
  }

  for (const a of d.tools) {
    await wajib('POST', `/api/v1/technologies/${d.slug}/tools`, { toolSlug: a.slug, note: a.note });
  }

  for (const p of d.projects.filter((x) => !(baru.projects ?? []).some((y) => y.title === x.title))) {
    await wajib('POST', `/api/v1/technologies/${d.slug}/projects`, p);
    console.log(`  proyek     ${p.title}`);
  }

  for (const r of d.resources.filter((x) => !(baru.resources ?? []).some((y) => y.url === x.url))) {
    await wajib('POST', `/api/v1/technologies/${d.slug}/resources`, r);
    console.log(`  sumber     ${r.url}`);
  }

  for (const r of d.requires) {
    // Idempoten di API (ADR-023). 400 berarti topik tujuannya belum terpasang.
    const respons = await panggil('POST', `/api/v1/technologies/${d.slug}/requires`, { topicSlug: r });
    if (respons.status !== 200) {
      gagal(`GAGAL   ${d.slug} butuh ${r} (HTTP ${respons.status})`, [
        respons.text,
        `Pasang '${r}' lebih dulu, lalu jalankan pemasangan ${d.slug} lagi.`,
      ]);
    }
    console.log(`  sisi       ${d.slug} butuh ${r}`);
  }

  // Naik ke draf. Ia menolak kalau satu bagian pun masih kosong, jadi berhasilnya
  // baris ini sekaligus bukti panggilan di atas benar-benar mendarat.
  const sebelum = await bacaTopik(d.slug);
  if (sebelum.maturity === 'Curated') {
    await wajib('POST', `/api/v1/technologies/${d.slug}/draf`, {});
    console.log(warna('32', '  draf       kelima bagian terisi'));
  }

  // Terakhir: baca ulang dan buktikan server == berkas. Bukan "tidak ada galat",
  // melainkan "rencana kosong" — kalau masih ada yang kurang, pemasangan gagal.
  const akhir = susunRencana(d, await bacaTopik(d.slug));
  if (akhir.beda.length > 0 || akhir.tulis.length > 0) {
    gagal(`GAGAL   ${d.slug}: sesudah dipasang, server masih berbeda dari berkas.`, [...akhir.beda, ...akhir.tulis.map((t) => `masih harus: ${t}`)]);
  }
  console.log(warna('32', `  selesai    ${d.slug} sama dengan berkas`));
}

// ---- Jalankan --------------------------------------------------------------

async function main() {
  const argumen = process.argv.slice(2);
  const { galat, topik } = bacaDanPeriksa();

  if (galat.length > 0) {
    gagal(`${galat.length} masalah di isi/:`, galat);
  }

  if (argumen.length === 1 && argumen[0] === '--cek') {
    console.log(warna('32', `isi/ — ${topik.length} berkas diperiksa, semuanya sah.`));
    return;
  }

  if (argumen.length === 1 && argumen[0] === '--semua') {
    for (const slug of urutanPasang(topik).urut) {
      await pasangTopik(topik.find((b) => b.data.slug === slug).data);
    }
    return;
  }

  if (argumen.length !== 1 || argumen[0].startsWith('--')) {
    gagal('Pemakaian: node database/isi/pasang.mjs --cek | --semua | <slug>');
  }

  const dipilih = topik.find((b) => b.data.slug === argumen[0]);
  if (!dipilih) {
    gagal(`Tak ada berkas isi untuk '${argumen[0]}'.`, `Yang ada: ${topik.map((b) => b.data.slug).join(', ') || '(kosong)'}`);
  }

  await pasangTopik(dipilih.data);
}

try {
  await main();
} catch (err) {
  if (!(err instanceof Berhenti)) {
    throw err;
  }
  process.exitCode = 1;
}
