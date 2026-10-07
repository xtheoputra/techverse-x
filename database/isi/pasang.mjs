// Jalan isi sungguhan — ADR-027.
//
//   node database/isi/pasang.mjs --cek        periksa SEMUA berkas isi/, tanpa HTTP
//   node database/isi/pasang.mjs <slug>       pasang satu topik ke $API_BASE_URL
//   node database/isi/pasang.mjs --semua      pasang SEMUA topik, prasyarat lebih dulu
//                                             (untuk pengembangan dan gerbang CI; produksi
//                                             satu topik per jalan isi.yml)
//   ... <slug>|--semua --ganti                samakan server dengan berkas yang SUDAH berubah
//                                             (ADR-028 Tahap 2); lihat catatan di bawah
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
// diam-diam. Berkas dan server dibandingkan dulu; kalau server berbeda dari berkas
// (punya sesuatu yang tak ada di berkas, atau teks yang lain), pemasang berhenti
// SEBELUM menulis apa pun dan menyebut bedanya. Topik yang sudah `tinjau` tak
// disentuh sama sekali.
//
// 🔑 `--ganti` (ADR-028 Tahap 2, #79) adalah SATU-SATUNYA jalan menimpa, dan ia
// eksplisit: rencananya dicetak lebih dulu, lalu server disamakan dengan berkas —
// nama/ringkasan, langkah roadmap menurut nomornya, definisi alat, proyek, sumber,
// tautan alat, dan relasi. Yang TIDAK bisa diganti tetap menghentikan pemasang:
// memindahkan topik ke bidang lain (ADR-009) dan membuang langkah roadmap (nomornya
// berurut tanpa lubang, ADR-012). Topik `tinjau` tetap terkunci walau memakai --ganti.
// Menambah dulu, baru membuang: pemasangan yang terputus meninggalkan kelebihan yang
// dibuang ulang di jalan berikutnya, bukan bagian yang hilang.
//
// Node 22+ (fetch bawaan, tanpa dependensi). Semua keluaran ke STDOUT, kegagalan
// dibawa kode keluar — alasannya dijelaskan di database/seeds/seed.mjs.

import { readdirSync, readFileSync, statSync } from 'node:fs';
import { basename, dirname, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

// 🔑 Pengurai Markdown terbatas YANG SAMA dengan penampil web (ADR-028 Tahap 3a): apa
// yang lolos pemeriksaan di sini adalah apa yang tampil, tak ada cermin yang bisa
// menyimpang. Letaknya di apps/web karena Dockerfile web hanya menyalin folder itu;
// berkasnya ES module tanpa dependensi, jadi `--cek` tetap jalan sebelum `npm ci`.
import { mediaKeys, parse, plainText } from '../../apps/web/src/lib/markdown/parse.mjs';

// Aturan media (ADR-028 Tahap 3b) tinggal di modulnya sendiri supaya bisa diuji terpisah.
import { cocokkanRujukan, periksaDaftarMedia } from './media.mjs';

const API = process.env.API_BASE_URL ?? 'http://localhost:5080';
const AKAR = resolve(dirname(fileURLToPath(import.meta.url)), '..', '..');

// ISI_DIR hanya untuk GERBANG UJI (`uji-media.mjs`): ia membaca berkas isi dari folder
// sementara berisi topik fixture. Tak pernah disetel di isi.yml — IsiWorkflowGuardTests
// menolak workflow itu menyebutnya, dan input workflow hanya sha, slug, dan sakelar boolean.
const FOLDER_ISI = process.env.ISI_DIR ? resolve(process.env.ISI_DIR) : join(AKAR, 'isi');

// Tempat berkas gambar topik: apps/web/public/media/<slug-topik>/…, disajikan situs di /media/….
const FOLDER_PUBLIK = join(AKAR, 'apps', 'web', 'public');

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
  overview: 20000,
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

const KUNCI_TOPIK = ['slug', 'name', 'fieldSlug', 'summary', 'overview', 'prerequisite', 'roadmap', 'tools', 'projects', 'resources', 'requires', 'media'];
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

/**
 * Teks yang berformat Markdown terbatas (deskripsi langkah, ringkasan proyek): diurai
 * dengan pengurai penampil, dan setiap laporannya jadi galat berikut nomor barisnya.
 * Teks polos tanpa tanda apa pun lolos tanpa laporan — isi yang ada tak perlu diubah.
 */
function periksaMarkdown(galat, tempat, nilai) {
  if (typeof nilai !== 'string') {
    return [];
  }
  const { blocks, problems } = parse(nilai);
  for (const p of problems) {
    galat.push(`${tempat} baris ${p.line}: ${p.message}`);
  }
  // Kunci media yang dirujuk teks ini; pencocokan dengan daftar `media` dilakukan sekali per
  // topik, di periksaTopik, karena satu media boleh dipakai di bagian mana pun.
  return mediaKeys(blocks);
}

/** Jumlah kata isi sebuah topik — angka yang dicetak `--cek`, supaya kedalaman isi terlihat. */
function hitungKata(d) {
  const kata = (t) => (typeof t === 'string' ? t.split(/\s+/).filter(Boolean).length : 0);
  const md = (t) => (typeof t === 'string' ? kata(plainText(parse(t).blocks)) : 0);
  return (
    kata(d.summary) +
    md(d.overview) +
    md(d.prerequisite?.description) +
    (d.roadmap ?? []).reduce((n, l) => n + md(l?.description), 0) +
    (d.projects ?? []).reduce((n, p) => n + md(p?.brief), 0)
  );
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

  // Setiap ::media[kunci] di seluruh teks topik ini, dikumpulkan lalu dicocokkan dengan daftar
  // `media` di akhir — satu media boleh dipakai di bagian mana pun.
  const dirujuk = [];

  // Pendalaman Overview (ADR-028 Tahap 3b): opsional, Markdown terbatas. `summary` tetap polos.
  if (d.overview !== undefined) {
    if (periksaTeks(galat, di('overview'), d.overview, BATAS.overview)) {
      dirujuk.push(...periksaMarkdown(galat, di('overview'), d.overview));
    }
  }

  if (periksaTeks(galat, di('fieldSlug'), d.fieldSlug, BATAS.slug) && d.fieldSlug !== basename(dirname(jalur))) {
    galat.push(`${di('fieldSlug')}: '${d.fieldSlug}' harus sama dengan nama foldernya ('${basename(dirname(jalur))}').`);
  }

  // Langkah 0 roadmap (ADR-012): prasyarat dilipat ke roadmap, dan AddRoadmapStep
  // menolak dipanggil sebelum ia ada.
  if (periksaKunci(galat, di('prerequisite'), d.prerequisite, KUNCI_LANGKAH)) {
    periksaTeks(galat, di('prerequisite.title'), d.prerequisite.title, BATAS.judulLangkah);
    if (periksaTeks(galat, di('prerequisite.description'), d.prerequisite.description, BATAS.uraianLangkah)) {
      dirujuk.push(...periksaMarkdown(galat, di('prerequisite.description'), d.prerequisite.description));
    }
  }

  if (periksaDaftar(galat, di('roadmap'), d.roadmap, { minimal: 1 })) {
    d.roadmap.forEach((l, i) => {
      if (periksaKunci(galat, di(`roadmap[${i}]`), l, KUNCI_LANGKAH)) {
        periksaTeks(galat, di(`roadmap[${i}].title`), l.title, BATAS.judulLangkah);
        if (periksaTeks(galat, di(`roadmap[${i}].description`), l.description, BATAS.uraianLangkah)) {
          dirujuk.push(...periksaMarkdown(galat, di(`roadmap[${i}].description`), l.description));
        }
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
        if (periksaTeks(galat, di(`projects[${i}].brief`), p.brief, BATAS.ringkasProyek)) {
          dirujuk.push(...periksaMarkdown(galat, di(`projects[${i}].brief`), p.brief));
        }
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

  // Media (ADR-028 Tahap 3b): bentuknya, berkasnya, lalu pencocokan DUA ARAH dengan rujukan di teks.
  const didefinisikan = periksaDaftarMedia(galat, di('media'), d.media, { slugTopik: d.slug, folderPublik: FOLDER_PUBLIK });
  cocokkanRujukan(galat, t, didefinisikan, dirujuk);

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

const samaProyek = (a, b) => a.title === b.title && a.brief === b.brief;
const samaSumber = (a, b) => a.url === b.url && a.type === b.type && a.title === b.title;
const samaDefinisiAlat = (a, b) => a.name === b.name && a.summary === b.summary && (a.homepage ?? null) === (b.homepage ?? null);

/** Dua media (berkas vs server) sama bila SEMUA medannya sama; yang tak diisi dibaca `null`. */
const samaMedia = (a, b) =>
  a.kind === b.kind &&
  (a.url ?? null) === (b.url ?? null) &&
  (a.videoId ?? null) === (b.videoId ?? null) &&
  a.alt === b.alt &&
  (a.caption ?? null) === (b.caption ?? null) &&
  (a.sourceName ?? null) === (b.sourceName ?? null) &&
  (a.sourceUrl ?? null) === (b.sourceUrl ?? null) &&
  a.license === b.license;

/** Muatan `PUT …/media/{key}` dari definisi di berkas. */
const muatanMedia = (m) => ({
  kind: m.kind,
  url: m.url ?? null,
  videoId: m.videoId ?? null,
  alt: m.alt,
  caption: m.caption ?? null,
  sourceName: m.sourceName ?? null,
  sourceUrl: m.sourceUrl ?? null,
  license: m.license,
});

/**
 * Membandingkan berkas dengan keadaan server dan menghasilkan:
 *   - `beda`: hal yang TIDAK BISA dipasang tanpa menimpa (pemasang berhenti),
 *   - `tulis`: daftar pekerjaan, untuk dicetak dan sebagai bukti "rencana kosong".
 *
 * Tanpa `ganti`, `tulis` hanya MENAMBAH dan segala selisih menjadi `beda`. Dengan `ganti`
 * (ADR-028 Tahap 2), selisih yang PUNYA jalan menjadi pekerjaan: nama/ringkasan,
 * langkah roadmap menurut nomornya, proyek/sumber yang tak sama, alat dan relasi yang
 * kelebihan. Yang tak punya jalan tetap `beda`: bidang topik dan kelebihan langkah roadmap.
 * Definisi katalog alat TIDAK direncanakan di sini — ia baru terbaca setelah alatnya
 * tertaut (lihat pasangTopik).
 */
function susunRencana(d, s, ganti = false) {
  const beda = [];
  const tulis = [];
  const kerja = { gantiTopik: false, gantiLangkah: [], tulisMedia: [] };
  const GANTI = ' — pasang dengan --ganti untuk menyamakannya';

  if (s === null) {
    tulis.push('buat topik');

    // POST topik tak membawa overview (ia bukan bagian pembuatan); ditetapkan sesudahnya lewat PUT.
    if (d.overview) {
      kerja.gantiTopik = true;
      tulis.push('tetapkan overview');
    }
  } else {
    // Bidang tak pernah diganti dari sini: ia soal taksonomi (ADR-009, ADR-010), bukan
    // soal menyunting tulisan, dan tak ada endpoint yang memindahkannya.
    if (s.fieldSlug !== d.fieldSlug) beda.push(`fieldSlug: berkas '${d.fieldSlug}' ≠ server '${s.fieldSlug}' (tak ada jalan memindahkan topik antar-bidang)`);

    // Overview kosong di salah satu sisi dibaca sama dengan "tanpa overview" (null dan '').
    const overviewBeda = (s.overview ?? '') !== (d.overview ?? '');

    if (s.name !== d.name || s.summary !== d.summary || overviewBeda) {
      if (ganti) {
        kerja.gantiTopik = true;
        tulis.push('ganti nama/ringkasan/overview topik');
      } else {
        if (s.name !== d.name) beda.push(`name: berkas '${d.name}' ≠ server '${s.name}'${GANTI}`);
        if (s.summary !== d.summary) beda.push(`summary: berkas ≠ server${GANTI}`);
        if (overviewBeda) beda.push(`overview: berkas ≠ server${GANTI}`);
      }
    }
  }

  const pras = s ? prasyaratServer(s) : null;
  if (pras === null || pras.title !== d.prerequisite.title || pras.description !== d.prerequisite.description) {
    // PUT mengganti langkah 0 — idempoten dan hanya mengubah satu butir milik berkas.
    tulis.push('tetapkan prasyarat');
  }

  // Roadmap: tiap langkah di server harus sama dengan langkah berkas bernomor sama.
  // Yang beda diganti di tempat (--ganti), selebihnya di ujung berkas ditambah —
  // AddRoadmapStep memberi nomor sendiri. Kelebihan langkah di server tak punya jalan
  // keluar: nomornya berurut tanpa lubang (ADR-012), jadi ia tetap `beda`.
  const ada = langkahServer(s);
  ada.forEach((l, i) => {
    const mestinya = d.roadmap[i];
    if (mestinya && mestinya.title === l.title && mestinya.description === l.description) {
      return;
    }
    if (ganti && mestinya) {
      kerja.gantiLangkah.push({ nomor: l.order, langkah: mestinya });
      tulis.push(`ganti langkah roadmap ${l.order} ('${l.title}' → '${mestinya.title}')`);
    } else if (!mestinya) {
      beda.push(`roadmap langkah ${l.order}: server punya '${l.title}', berkas lebih pendek — tak ada jalan membuang langkah (nomornya berurut tanpa lubang, ADR-012)`);
    } else {
      beda.push(`roadmap langkah ${l.order}: server '${l.title}' tidak cocok dengan berkas ('${mestinya.title}')${GANTI}`);
    }
  });
  for (const l of d.roadmap.slice(ada.length)) {
    tulis.push(`tambah langkah roadmap '${l.title}'`);
  }

  // Proyek dan sumber disamakan sebagai HIMPUNAN: yang di server tapi tak sama persis
  // dengan butir berkas dibuang (--ganti), yang di berkas tapi tak ada di server ditambah.
  // Teks proyek/sumber tak punya endpoint ubah — diganti = tambah baru lalu buang lama.
  const proyek = s?.projects ?? [];
  for (const p of proyek) {
    if (d.projects.some((x) => samaProyek(x, p))) continue;
    if (ganti) {
      tulis.push(`buang proyek '${p.title}' (tak sama dengan berkas)`);
    } else {
      const cocok = d.projects.find((x) => x.title === p.title);
      beda.push(cocok ? `proyek '${p.title}': isi berbeda dari berkas${GANTI}` : `proyek '${p.title}' ada di server, tak ada di berkas${GANTI}`);
    }
  }
  for (const p of d.projects) {
    if (!proyek.some((x) => samaProyek(x, p))) tulis.push(`tambah proyek '${p.title}'`);
  }

  const sumber = s?.resources ?? [];
  for (const r of sumber) {
    if (d.resources.some((x) => samaSumber(x, r))) continue;
    if (ganti) {
      tulis.push(`buang sumber ${r.url} (tak sama dengan berkas)`);
    } else {
      const cocok = d.resources.find((x) => x.url === r.url);
      beda.push(cocok ? `sumber ${r.url}: jenis/judul berbeda dari berkas${GANTI}` : `sumber ${r.url} ada di server, tak ada di berkas${GANTI}`);
    }
  }
  for (const r of d.resources) {
    if (!sumber.some((x) => samaSumber(x, r))) tulis.push(`tambah sumber ${r.url}`);
  }

  const alat = s?.tools ?? [];
  for (const a of alat) {
    const cocok = d.tools.find((x) => x.slug === a.slug);
    if (!cocok) {
      if (ganti) tulis.push(`lepas alat '${a.slug}' (tak ada di berkas)`);
      else beda.push(`alat '${a.slug}' tertaut di server, tak ada di berkas${GANTI}`);
    } else if (!samaDefinisiAlat(cocok, a)) {
      if (ganti) tulis.push(`ganti definisi katalog alat '${a.slug}' — ⚠ katalog dipakai bersama: topik tinjau yang menautkannya gugur ke draf (ADR-012)`);
      else beda.push(`alat '${a.slug}': definisi katalog berbeda dari berkas${GANTI}`);
    }
  }
  for (const a of d.tools) {
    const terpasang = alat.find((x) => x.slug === a.slug);
    if (!terpasang) tulis.push(`tautkan alat '${a.slug}'`);
    else if ((terpasang.note ?? null) !== (a.note ?? null)) tulis.push(`perbarui catatan alat '${a.slug}'`);
  }

  const sisi = (s?.requires ?? []).map((r) => r.slug);
  for (const r of sisi) {
    if (d.requires.includes(r)) continue;
    if (ganti) tulis.push(`buang relasi butuh '${r}' (tak ada di berkas)`);
    else beda.push(`relasi: server mencatat butuh '${r}', berkas tidak${GANTI}`);
  }
  for (const r of d.requires) {
    if (!sisi.includes(r)) tulis.push(`catat relasi butuh '${r}'`);
  }

  // Media (ADR-028 Tahap 3b), dikunci menurut KUNCI: PUT menetapkan (menambah atau mengganti),
  // jadi yang baru dan yang berbeda sama-sama satu PUT. Yang di server tapi tak di berkas
  // dibuang (--ganti) — DELETE per kunci sudah idempoten.
  const mediaServer = s?.media ?? [];
  const mediaBerkas = d.media ?? [];
  for (const m of mediaServer) {
    const cocok = mediaBerkas.find((x) => x.key === m.key);
    if (!cocok) {
      if (ganti) tulis.push(`buang media '${m.key}' (tak ada di berkas)`);
      else beda.push(`media '${m.key}' ada di server, tak ada di berkas${GANTI}`);
    } else if (!samaMedia(cocok, m)) {
      if (ganti) {
        kerja.tulisMedia.push(m.key);
        tulis.push(`ganti media '${m.key}'`);
      } else {
        beda.push(`media '${m.key}': isi berbeda dari berkas${GANTI}`);
      }
    }
  }
  for (const m of mediaBerkas) {
    if (!mediaServer.some((x) => x.key === m.key)) {
      kerja.tulisMedia.push(m.key);
      tulis.push(`tambah media '${m.key}'`);
    }
  }

  if (s === null || s.maturity === 'Curated') {
    tulis.push('naik ke draf');
  }

  return { beda, tulis, kerja };
}

// ---- Memasang --------------------------------------------------------------

async function pasangTopik(d, ganti = false) {
  judul(`Memasang ${d.slug} ke ${API}${ganti ? ' dengan --ganti' : ''} ...`);

  const sekarang = await bacaTopik(d.slug);
  const { beda, tulis, kerja } = susunRencana(d, sekarang, ganti);

  if (beda.length > 0) {
    gagal(`BEDA    ${d.slug}: berkas dan server tidak sama. Tidak ada yang ditulis.`, [
      ...beda,
      '',
      ganti
        ? 'Yang tersisa di atas tidak punya jalan ganti: memindahkan topik ke bidang lain (ADR-009) dan membuang langkah roadmap (ADR-012).'
        : 'Pemasang tidak menimpa. Untuk menyamakan server dengan berkas yang sudah berubah, jalankan lagi dengan --ganti (ADR-028 Tahap 2; topik tinjau tetap terkunci).',
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

  // Dengan --ganti rencana dicetak SEBELUM menulis: mengganti dan membuang tak boleh
  // jadi kejutan di log, dan log runner adalah satu-satunya catatan apa yang berubah.
  if (ganti) {
    for (const t of tulis) {
      console.log(warna('33', `  rencana    ${t}`));
    }
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

  // Nama, ringkasan, dan overview topik (--ganti; juga overview topik BARU, yang tak ikut POST
  // pembuatan). PUT mengganti ketiganya sekaligus — overview yang tak dikirim berarti
  // dikosongkan, jadi dikirim SELALU dari berkas. Di topik `tinjau` ia akan menggugurkannya,
  // tapi topik `tinjau` sudah ditolak di atas.
  if (kerja.gantiTopik) {
    await wajib('PUT', `/api/v1/technologies/${d.slug}`, { name: d.name, summary: d.summary, overview: d.overview ?? null });
    console.log(`  ganti      nama/ringkasan/overview ${d.slug}`);
  }

  // Media (ADR-028 Tahap 3b): PUT per kunci, idempoten — menambah yang baru, mengganti yang beda.
  for (const kunci of kerja.tulisMedia) {
    const m = (d.media ?? []).find((x) => x.key === kunci);
    await wajib('PUT', `/api/v1/technologies/${d.slug}/media/${kunci}`, muatanMedia(m));
    console.log(`  media      ${kunci}`);
  }

  // Prasyarat WAJIB lebih dulu: AddRoadmapStep menolak dipanggil sebelum langkah 0 ada.
  // PUT, jadi mengulanginya mengganti langkah 0, bukan menambah — tapi hanya dipanggil
  // kalau rencananya memang memuatnya, supaya pemasangan ulang tidak menulis yang tak perlu.
  if (tulis.includes('tetapkan prasyarat')) {
    await wajib('PUT', `/api/v1/technologies/${d.slug}/roadmap/prasyarat`, d.prerequisite);
    console.log(`  langkah 0  ${d.prerequisite.title}`);
  }

  // Langkah yang sudah ada diganti di tempatnya menurut nomor (--ganti); nomornya
  // alamat yang sudah ada di server, jadi tak ada lubang yang bisa tercipta.
  for (const g of kerja.gantiLangkah) {
    await wajib('PUT', `/api/v1/technologies/${d.slug}/roadmap/${g.nomor}`, g.langkah);
    console.log(`  ganti      langkah ${g.nomor}  ${g.langkah.title}`);
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

  // Definisi katalog baru terbaca SESUDAH alatnya tertaut: alat yang sudah ada di katalog
  // (dibuat topik lain) membalas 409 di atas tanpa menyebut isinya. Katalog dipakai
  // bersama, jadi API menggugurkan tinjau topik lain yang menautkannya — dan pemasang
  // menyebutnya terang-terangan, bukan membiarkannya jadi kejutan.
  if (ganti) {
    const tertaut = await bacaTopik(d.slug);
    for (const a of d.tools) {
      const di = tertaut.tools.find((x) => x.slug === a.slug);
      if (di && !samaDefinisiAlat(a, di)) {
        await wajib('PUT', `/api/v1/tools/${a.slug}`, { name: a.name, summary: a.summary, homepage: a.homepage });
        console.log(warna('33', `  ganti      definisi alat ${a.slug} (katalog bersama: topik tinjau yang menautkannya gugur ke draf)`));
      }
    }
  }

  // Menambah DULU, baru membuang: kalau pemasangan terputus di antaranya, yang tersisa
  // adalah kelebihan (dibuang di jalan berikutnya), bukan bagian yang hilang.
  for (const p of d.projects.filter((x) => !(baru.projects ?? []).some((y) => samaProyek(x, y)))) {
    await wajib('POST', `/api/v1/technologies/${d.slug}/projects`, p);
    console.log(`  proyek     ${p.title}`);
  }

  for (const r of d.resources.filter((x) => !(baru.resources ?? []).some((y) => samaSumber(x, y)))) {
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

  // Kelebihan di server dibuang terakhir (--ganti), dibaca ulang karena baris yang
  // baru ditambah sudah punya Id. Semuanya idempoten di API.
  if (ganti) {
    const kini = await bacaTopik(d.slug);

    for (const p of (kini.projects ?? []).filter((y) => !d.projects.some((x) => samaProyek(x, y)))) {
      await wajib('DELETE', `/api/v1/technologies/${d.slug}/projects/${p.id}`);
      console.log(`  buang      proyek ${p.title}`);
    }

    for (const r of (kini.resources ?? []).filter((y) => !d.resources.some((x) => samaSumber(x, y)))) {
      await wajib('DELETE', `/api/v1/technologies/${d.slug}/resources/${r.id}`);
      console.log(`  buang      sumber ${r.url}`);
    }

    for (const a of (kini.tools ?? []).filter((y) => !d.tools.some((x) => x.slug === y.slug))) {
      await wajib('DELETE', `/api/v1/technologies/${d.slug}/tools/${a.slug}`);
      console.log(`  lepas      alat ${a.slug}`);
    }

    for (const r of (kini.requires ?? []).map((x) => x.slug).filter((slug) => !d.requires.includes(slug))) {
      await wajib('DELETE', `/api/v1/technologies/${d.slug}/requires/${r}`);
      console.log(`  buang      relasi ${d.slug} butuh ${r}`);
    }

    for (const m of (kini.media ?? []).filter((y) => !(d.media ?? []).some((x) => x.key === y.key))) {
      await wajib('DELETE', `/api/v1/technologies/${d.slug}/media/${m.key}`);
      console.log(`  buang      media ${m.key}`);
    }
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
  const semuaArgumen = process.argv.slice(2);

  // --ganti hanya pengubah: ia tak berdiri sendiri, dan tak ikut --cek (yang tanpa jaringan).
  const ganti = semuaArgumen.includes('--ganti');
  const argumen = semuaArgumen.filter((a) => a !== '--ganti');

  const { galat, topik } = bacaDanPeriksa();

  if (galat.length > 0) {
    gagal(`${galat.length} masalah di isi/:`, galat);
  }

  if (argumen.length === 1 && argumen[0] === '--cek') {
    if (ganti) {
      gagal('--cek tak memakai --ganti: ia hanya memeriksa berkas, tanpa jaringan.');
    }
    console.log(warna('32', `isi/ — ${topik.length} berkas diperiksa, semuanya sah.`));
    for (const { data } of topik) {
      console.log(`  ${data.slug}  ${hitungKata(data)} kata`);
    }
    return;
  }

  if (argumen.length === 1 && argumen[0] === '--semua') {
    for (const slug of urutanPasang(topik).urut) {
      await pasangTopik(topik.find((b) => b.data.slug === slug).data, ganti);
    }
    return;
  }

  if (argumen.length !== 1 || argumen[0].startsWith('--')) {
    gagal('Pemakaian: node database/isi/pasang.mjs --cek | [--ganti] --semua | [--ganti] <slug>');
  }

  const dipilih = topik.find((b) => b.data.slug === argumen[0]);
  if (!dipilih) {
    gagal(`Tak ada berkas isi untuk '${argumen[0]}'.`, `Yang ada: ${topik.map((b) => b.data.slug).join(', ') || '(kosong)'}`);
  }

  await pasangTopik(dipilih.data, ganti);
}

try {
  await main();
} catch (err) {
  if (!(err instanceof Berhenti)) {
    throw err;
  }
  process.exitCode = 1;
}
