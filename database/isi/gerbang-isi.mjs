// Gerbang isi BERBASIS API — satu implementasi untuk ci.yml (job Backend), run.ps1, dan Makefile.
//
//   node database/isi/gerbang-isi.mjs          (.\run.ps1 gerbang-isi  /  make gerbang-isi)
//
// Tiga gerbang yang butuh API HIDUP di atas basis data KOSONG, dijalankan berurutan:
//
//   1  isi sungguhan dipasang dari nol, dua kali (ADR-027): pemasangan kedua nol tulisan, dan
//      setiap topik berkas `isi/` keluar berstatus draf tanpa pemeriksa
//   2  `uji-ganti.mjs`  — `pasang.mjs --ganti` dan kunci `tinjau` (ADR-028 Tahap 2)
//   3  `uji-media.mjs`  — `overview` dan media dipasang lalu disamakan kembali (ADR-028 Tahap 3b)
//
// 🔴 Kenapa berkas ini ada: sampai 2026-10-07 ketiganya hanya hidup sebagai langkah `ci.yml`, dan
// `run.ps1 verify` maupun `run.ps1 ci` — yang mengaku "seluruh gerbang ci.yml" — tak memuatnya.
// PR #90 merah di CI pertamanya karena itu: teks rencana `pasang.mjs` berubah, `uji-ganti.mjs` masih
// menunggu teks lama, dan `verify` hijau tak pernah menyentuhnya. Tiruan lokal di scratchpad
// menutup lubang itu untuk satu sesi; berkas ini menutupnya untuk seterusnya. CI memanggil berkas
// yang SAMA, jadi tiruan dan aslinya tak bisa menyimpang diam-diam (alasan yang sama dengan `seed`
// dan `cek-tautan`).
//
// Urutannya sama dengan PENYEBARAN.md (postgres -> migrate -> api):
//
//   a  basis data sekali pakai `techversex_isi` dibuang (bila ada) lalu dibuat — milik gerbang ini
//      saja; uji integrasi menulis ke `techversex`, dan "dari nol" tak boleh bergantung pada sisanya
//   b  `dotnet ef database update` ke basis data itu
//   c  API Release (`--no-build`) di $API_BASE_URL dengan tulis HIDUP, ditunggu sampai /health/ready
//   d  gerbang 1, 2, 3 — yang merah tak menghentikan yang lain, kecuali gerbang 1 (2 dan 3 berdiri
//      di atas basis data yang ia isi)
//   e  SELALU: API dihentikan (pohon prosesnya, karena `dotnet run` melahirkan anak), log API
//      dicetak bila ada yang merah, basis data dibuang
//
// Prasyarat: Postgres hidup di localhost:5432 (`.\run.ps1 up`; di CI service container), build
// Release sudah ada (`run.ps1 gerbang-isi` membangunnya), dan `dotnet ef` terpasang (dotnet tool
// restore). `psql` dipakai bila ada di PATH (runner CI); bila tidak — mesin Windows ini — perintah
// yang sama dijalankan di dalam container `techversex-postgres`.
//
// ⚠️ Port API (bawaan 5099) wajib kosong. Berkas ini TIDAK mematikan apa pun yang tak ia mulai:
// proses di mesin pengembang belum tentu milik repo ini (Sesi 15). Ia berhenti dan menyebut portnya.
//
// Kode keluar: 0 hijau; 1 ada gerbang merah; 2 gerbang tak bisa disiapkan (port terpakai, basis data,
// migrasi, API tak siap).

import { spawn, spawnSync } from 'node:child_process';
import { closeSync, openSync, readFileSync, readdirSync } from 'node:fs';
import { connect } from 'node:net';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const FOLDER = dirname(fileURLToPath(import.meta.url));
const AKAR = resolve(FOLDER, '..', '..');
const API = process.env.API_BASE_URL ?? 'http://127.0.0.1:5099';
const BASIS_DATA = process.env.GERBANG_BASIS_DATA ?? 'techversex_isi';
const KONTAINER = 'techversex-postgres';
const WINDOWS = process.platform === 'win32';

if (!/^[a-z_][a-z0-9_]*$/.test(BASIS_DATA) || BASIS_DATA === 'techversex') {
  // Nama masuk ke SQL apa adanya, dan basis data ini DIBUANG di awal dan akhir. `techversex` adalah
  // basis data pengembangan (dan basis data uji integrasi di CI) — tak pernah boleh jadi sasaran.
  console.log(`GERBANG_BASIS_DATA '${BASIS_DATA}' ditolak: harus nama sederhana dan bukan 'techversex'.`);
  process.exit(2);
}

const KONEKSI = `Host=localhost;Port=5432;Database=${BASIS_DATA};Username=techversex;Password=techversex_dev`;
const LOG_API = join(tmpdir(), `gerbang-isi-api-${process.pid}.log`);

const ringkasan = [];
let merah = false;

const judul = (teks) => console.log(`\n=== ${teks}`);

function catat(nama, hijau, keterangan = '') {
  ringkasan.push(`${hijau ? 'hijau' : 'MERAH'}  ${nama}${keterangan ? ` — ${keterangan}` : ''}`);
  if (!hijau) merah = true;
}

function berhenti(pesan) {
  console.log(`\nGERBANG TAK BISA DISIAPKAN: ${pesan}`);
  return 2;
}

// --- Postgres -------------------------------------------------------------------------------------

const adaPsql = spawnSync('psql', ['--version'], { encoding: 'utf8' }).status === 0;

function sql(perintah) {
  const [program, argumen] = adaPsql
    ? ['psql', ['-h', 'localhost', '-U', 'techversex', '-d', 'techversex', '-v', 'ON_ERROR_STOP=1', '-c', perintah]]
    : ['docker', ['exec', KONTAINER, 'psql', '-U', 'techversex', '-d', 'techversex', '-v', 'ON_ERROR_STOP=1', '-c', perintah]];
  const hasil = spawnSync(program, argumen, {
    env: { ...process.env, PGPASSWORD: process.env.PGPASSWORD ?? 'techversex_dev' },
    encoding: 'utf8',
  });
  return { ok: hasil.status === 0, keluaran: `${hasil.stdout ?? ''}${hasil.stderr ?? ''}${hasil.error ?? ''}`.trim() };
}

const buangBasisData = () => sql(`DROP DATABASE IF EXISTS ${BASIS_DATA} WITH (FORCE)`);

// --- API ------------------------------------------------------------------------------------------

function portTerpakai(url) {
  const { hostname, port } = new URL(url);
  return new Promise((selesai) => {
    const soket = connect({ host: hostname, port: Number(port) });
    soket.setTimeout(1500);
    soket.once('connect', () => {
      soket.destroy();
      selesai(true);
    });
    soket.once('timeout', () => {
      soket.destroy();
      selesai(false);
    });
    soket.once('error', () => selesai(false));
  });
}

let prosesApi = null;

function nyalakanApi() {
  const log = openSync(LOG_API, 'w');
  prosesApi = spawn(
    'dotnet',
    ['run', '--project', 'apps/api/TechVerseX.Api.csproj', '--no-build', '--configuration', 'Release', '--no-launch-profile', '--urls', API],
    {
      cwd: AKAR,
      env: {
        ...process.env,
        ConnectionStrings__Postgres: KONEKSI,
        ConnectionStrings__Redis: 'localhost:6379',
        Editorial__WritesEnabled: 'true',
      },
      stdio: ['ignore', log, log],
      // POSIX: grup proses sendiri, supaya anak `dotnet run` ikut dihentikan lewat -pid.
      detached: !WINDOWS,
      windowsHide: true,
    },
  );
  closeSync(log);
}

function hentikanApi() {
  if (!prosesApi || prosesApi.exitCode !== null) return;
  if (WINDOWS) {
    // `dotnet run` melahirkan proses API sebagai anak; /T menghentikan pohonnya, hanya pohon ini.
    spawnSync('taskkill', ['/PID', String(prosesApi.pid), '/T', '/F'], { encoding: 'utf8' });
  } else {
    try {
      process.kill(-prosesApi.pid, 'SIGTERM');
    } catch {
      // grupnya sudah tak ada
    }
  }
}

async function tungguSiap() {
  for (let i = 0; i < 30; i++) {
    if (prosesApi.exitCode !== null) return false;
    try {
      const r = await fetch(`${API}/health/ready`, { signal: AbortSignal.timeout(3000) });
      if (r.status === 200) return true;
    } catch {
      // belum mendengar
    }
    await new Promise((t) => setTimeout(t, 2000));
  }
  return false;
}

// --- Gerbang --------------------------------------------------------------------------------------

function node(skrip, ...argumen) {
  const hasil = spawnSync(process.execPath, [join(FOLDER, skrip), ...argumen], {
    cwd: AKAR,
    env: { ...process.env, API_BASE_URL: API, GERBANG_BASIS_DATA_SEKALI_PAKAI: 'ya' },
    encoding: 'utf8',
  });
  const keluaran = `${hasil.stdout ?? ''}${hasil.stderr ?? ''}`;
  process.stdout.write(keluaran);
  return { kode: hasil.status, keluaran };
}

// Baris yang hanya dicetak pemasang saat MENULIS. Sama dengan pola gerbang sejak ADR-027.
const BARIS_TULIS = /^ {2}(dibuat|langkah|proyek|sumber|sisi|draf|ganti|buang|lepas|media)/m;

async function gerbangPasangDuaKali() {
  judul('Gerbang 1 (ADR-027): isi sungguhan dipasang dari nol, dua kali');
  console.log('--- pemasangan pertama (dari nol)');
  const pertama = node('pasang.mjs', '--semua');
  if (pertama.kode !== 0) {
    catat('pasang dari nol', false, `pasang.mjs keluar ${pertama.kode}`);
    return false;
  }

  console.log('--- pemasangan kedua (tak boleh menulis apa pun)');
  const kedua = node('pasang.mjs', '--semua');
  const tulisan = kedua.keluaran.split('\n').filter((b) => BARIS_TULIS.test(b));
  if (kedua.kode !== 0 || tulisan.length > 0) {
    if (tulisan.length) console.log(`Pemasangan kedua menulis:\n${tulisan.join('\n')}`);
    catat('pasang dari nol, dua kali', false, kedua.kode !== 0 ? `pemasangan kedua keluar ${kedua.kode}` : `${tulisan.length} baris tulis`);
    return false;
  }

  // Invarian inti ADR-027: pemasang membawa topik paling jauh ke draf, tanpa nama pemeriksa.
  const slug = readdirSync(join(AKAR, 'isi'), { withFileTypes: true })
    .filter((d) => d.isDirectory())
    .flatMap((d) => readdirSync(join(AKAR, 'isi', d.name)).filter((f) => f.endsWith('.json')).map((f) => f.slice(0, -5)));
  const bukanDraf = [];
  for (const s of slug) {
    const r = await fetch(`${API}/api/v1/technologies/${s}`);
    const t = r.status === 200 ? await r.json() : null;
    if (!t || t.maturity !== 'MachineDrafted' || t.reviewedAt !== null) bukanDraf.push(`${s} (${r.status} ${t?.maturity ?? ''})`);
  }
  if (bukanDraf.length) {
    console.log(`Tidak berstatus draf tanpa pemeriksa: ${bukanDraf.join(', ')}`);
    catat('pasang dari nol, dua kali', false, 'ada topik yang bukan draf');
    return false;
  }

  console.log(`SIAP: ${slug.length} topik terpasang dari nol, sama dengan berkas, berstatus draf.`);
  catat('pasang dari nol, dua kali', true, `${slug.length} topik, pemasangan kedua nol tulisan`);
  return true;
}

function gerbangSkrip(nama, skrip) {
  judul(nama);
  const { kode, keluaran } = node(skrip);
  const ok = (keluaran.match(/^ {2}ok\s/gm) ?? []).length;
  const gagal = (keluaran.match(/^ {2}GAGAL\s/gm) ?? []).length;
  catat(nama, kode === 0, `${ok} ok, ${gagal} GAGAL, keluar ${kode}`);
}

// --- Jalan ----------------------------------------------------------------------------------------

async function jalan() {
  if (await portTerpakai(API)) {
    return berhenti(`${API} sudah dipakai proses lain. Bebaskan dulu, atau pakai API_BASE_URL lain; gerbang ini tak mematikan yang bukan miliknya.`);
  }

  judul(`Basis data sekali pakai '${BASIS_DATA}' (${adaPsql ? 'psql' : `docker exec ${KONTAINER}`})`);
  const buang = buangBasisData();
  const buat = buang.ok ? sql(`CREATE DATABASE ${BASIS_DATA}`) : buang;
  if (!buat.ok) return berhenti(`basis data tak bisa dibuat.\n${buat.keluaran}`);
  console.log('dibuat kosong');

  judul('Migrasi dari nol');
  const migrasi = spawnSync(
    'dotnet',
    ['ef', 'database', 'update', '--project', 'services/technology/TechVerseX.TechnologyService.csproj', '--startup-project', 'apps/api/TechVerseX.Api.csproj'],
    { cwd: AKAR, env: { ...process.env, ConnectionStrings__Postgres: KONEKSI, ConnectionStrings__Redis: 'localhost:6379' }, encoding: 'utf8' },
  );
  const keluaranMigrasi = `${migrasi.stdout ?? ''}${migrasi.stderr ?? ''}`.trim().split('\n');
  console.log(keluaranMigrasi.slice(-3).join('\n'));
  if (migrasi.status !== 0) return berhenti(`migrasi keluar ${migrasi.status}.\n${keluaranMigrasi.slice(-20).join('\n')}`);

  judul(`API Release di ${API}, tulis hidup`);
  nyalakanApi();
  if (!(await tungguSiap())) {
    merah = true;
    return berhenti('API tidak pernah SIAP dalam 60 detik (lognya di bawah).');
  }
  console.log('siap');

  if (await gerbangPasangDuaKali()) {
    gerbangSkrip('Gerbang 2 (ADR-028 Tahap 2): isi diganti, tinjau tetap terkunci', 'uji-ganti.mjs');
    gerbangSkrip('Gerbang 3 (ADR-028 Tahap 3b): overview dan media dipasang dan disamakan kembali', 'uji-media.mjs');
  } else {
    ringkasan.push('lewat  gerbang 2 dan 3 — keduanya berdiri di atas basis data yang diisi gerbang 1');
  }
  return merah ? 1 : 0;
}

let kode = 2;
try {
  kode = await jalan();
} finally {
  hentikanApi();
  if (kode !== 0) {
    try {
      const baris = readFileSync(LOG_API, 'utf8').trimEnd().split('\n');
      console.log(`\n--- 40 baris terakhir log API (${LOG_API})\n${baris.slice(-40).join('\n')}`);
    } catch {
      // API tak pernah dinyalakan
    }
  }
  // Sedikit jeda: Postgres menolak DROP selagi koneksi pool API yang baru dihentikan belum tertutup;
  // WITH (FORCE) memutusnya, tapi proses yang sedang mati bisa membuka ulang sesaat.
  await new Promise((t) => setTimeout(t, 1000));
  const buang = buangBasisData();
  console.log(`\nbasis data '${BASIS_DATA}' ${buang.ok ? 'dibuang' : `GAGAL dibuang:\n${buang.keluaran}`}`);
}

console.log('\nRingkasan gerbang isi berbasis API');
for (const baris of ringkasan) console.log(`  ${baris}`);
console.log(kode === 0 ? '\nVONIS: HIJAU' : kode === 1 ? '\nVONIS: MERAH' : '\nVONIS: TAK BISA DISIAPKAN');
process.exit(kode);
