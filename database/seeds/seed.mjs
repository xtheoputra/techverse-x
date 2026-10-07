// Isi contoh untuk pengembangan lokal.
//
// ⚠️ Ini BUKAN kurikulum. Isinya sengaja hanya beberapa entri sekadar supaya
// layar dan endpoint ada isinya. Cara isi sungguhan lahir sudah diputuskan di
// ADR-012 (kurasi -> draf -> tinjau oleh manusia), dan jalan terakhirnya ke
// produksi di ADR-021 — tidak satu pun lewat berkas ini, yang hanya bicara ke
// API pengembangan.
//
// 🔑 SATU implementasi, dipanggil `run.ps1 seed` DAN `make seed`.
//
// Sampai 2026-09-17 keduanya punya salinan sendiri — blok PowerShell di run.ps1
// dan database/seeds/seed.sh — dan yang kedua sudah menyimpang tanpa ada yang
// merah. Dijalankan 2026-09-16 ke basis data KOSONG (bukan dibaca), seed.sh mencetak
// 'ada AI Agents', 'ada Edge AI', 'ada Quantum Computing' dan meninggalkan DUA
// topik dengan nol bagian terisi: ketiga nama itu slug milik BIDANG, API
// menjawab 409, dan skripnya membaca setiap 409 sebagai "sudah ada". Ia juga
// tidak pernah mengisi bagian, dan keluar dengan kode 0 sesudah mencetak GAGAL.
// README berbunyi "isi run.ps1 dan Makefile sengaja dijaga sama" — untuk seed,
// kalimat itu sudah lama tidak benar. Satu berkas tidak bisa menyimpang dari
// dirinya sendiri.
//
// Node 22+ (fetch bawaan, tanpa dependensi) — syarat yang sudah dituntut README,
// package.json, dan CI.

const API = process.env.API_BASE_URL ?? 'http://localhost:5080';

// ---- Keluaran --------------------------------------------------------------
// 🔴 SEMUA baris ke STDOUT, termasuk galat; kegagalan dibawa kode keluar, bukan
// aliran. Diukur 2026-09-17 di Windows PowerShell 5.1 dengan versi pertama berkas
// ini, yang menulis galat ke stderr: `.\run.ps1 seed` biasa mencetak penjelasan
// ADR-020 dan keluar 1 dengan benar, TAPI `.\run.ps1 seed 2>&1` berhenti dengan
// NativeCommandError berpesan KOSONG dan penjelasannya tidak pernah tercetak.
// PowerShell 5.1 membungkus tiap baris stderr program native jadi galat begitu
// alirannya dialihkan, dan run.ps1 menyetel ErrorActionPreference Stop. Cacat itu
// hanya muncul persis saat keluarannya disimpan untuk dibaca belakangan.
//
// Warna hanya kalau keluarannya terminal: blok PowerShell yang digantikan berkas
// ini berwarna, tapi keluaran yang dialirkan ke berkas tidak boleh penuh kode ANSI.

const berwarna = process.stdout.isTTY === true && !process.env.NO_COLOR;
const warna = (kode, teks) => (berwarna ? `\x1b[${kode}m${teks}\x1b[0m` : teks);

const judul = (teks) => console.log(warna('36', teks));

/** Menghentikan seed dengan kode keluar 1 SESUDAH pesannya tercetak. */
class Berhenti extends Error {}

function gagal(baris, respons) {
  console.log(warna('31', baris));
  if (respons?.text) {
    console.log(`  ${respons.text}`);
  }
  throw new Berhenti();
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
    gagal(`API tidak terjangkau di ${API}. Jalankan run.ps1 api atau make api. (${err.cause?.code ?? err.message})`);
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

/** Panggilan yang WAJIB berhasil: setiap jawaban selain 2xx menghentikan seed. */
async function wajib(method, path, body) {
  const respons = await panggil(method, path, body);
  if (respons.status < 200 || respons.status > 299) {
    // Sampai 2026-09-17 kegagalan di sini dicetak 'lewat' lalu seed jalan terus.
    // Itu membuat langkah berikutnya gagal dengan sebab yang menyesatkan — atau
    // lebih buruk, berhasil di atas halaman yang setengah terisi.
    gagal(`GAGAL   ${method} ${path} (HTTP ${respons.status})`, respons);
  }
  return respons;
}

function jelaskanPermukaanTulisTertutup(status) {
  // 404/405 di sini hampir selalu berarti satu hal, dan tanpa kalimat ini
  // gejalanya menyesatkan: endpointnya BUKAN hilang, ia memang sengaja tidak
  // dipasang (ADR-020). Jangan biarkan orang menebak - dan jangan teruskan sisa
  // seed-nya.
  const kuning = (teks) => console.log(warna('33', teks));
  console.log('');
  kuning(`  Endpoint tulis tidak dipasang di ${API} (HTTP ${status}).`);
  kuning('  Itu bentuk PRODUKSI menurut ADR-020 - dan seed memang tidak boleh jalan di sana.');
  kuning('  Untuk pengembangan: jalankan APInya dengan `run.ps1 api` atau `make api`. Keduanya');
  kuning('  memakai dotnet run, yang membaca launchSettings.json dan menyalakan');
  kuning('  Editorial__WritesEnabled. API yang dijalankan lewat peti kemas atau');
  kuning('  --no-launch-profile TIDAK menyalakannya.');
  throw new Berhenti();
}

// ---- Topik -----------------------------------------------------------------

// fieldSlug harus salah satu bidang ADR-010 - lihat GET /api/v1/fields.
//
// PENTING: nama contoh di sini TIDAK BOLEH sama dengan nama bidang.
// /teknologi/<slug> dipakai bersama bidang dan topik (ADR-009), jadi topik
// bernama 'AI Agents' akan menuntut slug 'ai-agents' yang sudah dipegang
// bidangnya. Sampai Sesi 10 seed ini memang membuat tiga tabrakan seperti
// itu - 'AI Agents', 'Edge AI', 'Quantum Computing' - dan sejak API
// menolaknya, ketiganya diganti nama topik yang sebenarnya.
//
// Ini juga taksonomi yang lebih jujur: topik hidup DI BAWAH bidang, ia
// bukan bidang itu sendiri.
//
// Slug ditulis EKSPLISIT, bukan diturunkan server dari nama: 409 hanya boleh
// dibaca "sudah ada" sesudah GET slug yang sama menjawab 200, dan menurunkan
// slug di sini berarti menyalin aturan server ke berkas kedua.
const contoh = [
  { name: 'Model Context Protocol', slug: 'model-context-protocol', summary: 'Protokol terbuka yang menstandarkan cara model bahasa menjangkau alat dan data.', fieldSlug: 'ai-agents' },
  { name: 'Kuantisasi Model', slug: 'kuantisasi-model', summary: 'Memampatkan bobot model supaya muat dan cepat di perangkat.', fieldSlug: 'edge-ai' },
  { name: 'Qiskit', slug: 'qiskit', summary: 'Kerangka kerja Python untuk menyusun dan menjalankan sirkuit kuantum.', fieldSlug: 'quantum-computing' },
  { name: 'Digital Twin', slug: 'digital-twin', summary: 'Kembaran digital dari objek atau sistem nyata.', fieldSlug: 'iot' },
  { name: 'Cybersecurity Berbasis AI', slug: 'cybersecurity-berbasis-ai', summary: 'Ancaman otomatis, maka pertahanannya ikut otomatis.', fieldSlug: 'cybersecurity' },
  // Ada untuk satu hal: menjadi ujung TUJUAN relasi contoh di bawah. Tidak satu
  // pun dari kelima contoh di atas sungguh prasyarat yang lain, dan relasi
  // palsu di antara mereka akan mengajarkan hal yang salah kepada siapa pun yang
  // membuka halamannya.
  { name: 'Tool Use', slug: 'tool-use', summary: 'Cara LLM memanggil fungsi di luar dirinya lewat skema masukan yang dideklarasikan.', fieldSlug: 'ai-agents' },
];

async function buatTopik(topik) {
  const respons = await panggil('POST', '/api/v1/technologies', topik);

  if (respons.status === 201) {
    console.log(`  dibuat  ${topik.name}`);
    return;
  }

  if (respons.status === 409) {
    // 409 punya DUA arti di API ini: slugnya sudah dipakai topik (aman diulang),
    // atau dipegang BIDANG (ADR-009) - dan yang kedua bukan "ada", melainkan
    // contoh yang tidak akan pernah bisa dibuat. Hanya GET yang bisa
    // membedakannya.
    const ada = await panggil('GET', `/api/v1/technologies/${topik.slug}`);
    if (ada.status === 200) {
      console.log(`  ada     ${topik.name}`);
      return;
    }

    gagal(`BENTROK ${topik.slug}: slug dipegang bidang atau tidak bisa dibaca (GET HTTP ${ada.status}).`, respons);
  }

  if (respons.status === 404 || respons.status === 405) {
    jelaskanPermukaanTulisTertutup(respons.status);
  }

  gagal(`GAGAL   ${topik.name} (HTTP ${respons.status})`, respons);
}

// ---- Alat ------------------------------------------------------------------

async function buatAlat(alat) {
  // Katalog alat tidak berbagi ruang nama dengan bidang, jadi 409 di sini
  // memang hanya berarti slug alatnya sudah terpakai. Tidak ada GET alat untuk
  // memeriksanya lebih jauh.
  const respons = await panggil('POST', '/api/v1/tools', alat);

  if (respons.status === 201) {
    console.log(`  dibuat  ${alat.name}`);
  } else if (respons.status === 409) {
    console.log(`  ada     ${alat.name}`);
  } else {
    gagal(`GAGAL   ${alat.name} (HTTP ${respons.status})`, respons);
  }
}

// ---- Kelima bagian satu topik ----------------------------------------------

// Satu topik diisi LENGKAP kelima bagiannya.
//
// Tanpa ini halaman /teknologi/<slug> memang bisa dibuka, tapi setiap
// bagiannya berbunyi 'Belum diisi' - dan bentuk halaman yang sudah jadi
// tidak pernah terlihat oleh siapa pun yang menjalankan proyek ini di
// mesinnya sendiri.
async function isiKelimaBagian(slug) {
  judul(`Mengisi kelima bagian ${slug} ...`);

  // PENTING: SEED HARUS BISA DIJALANKAN BERKALI-KALI, dan sampai 2026-09-09
  // bagian ini TIDAK bisa.
  //
  // Membuat topiknya memang sudah idempoten sejak awal. Mengisi bagiannya
  // tidak: dari kelima bagian, hanya prasyarat (PUT, mengganti langkah 0) dan
  // tautan alat (AttachTool memperbarui catatan, tidak menambah baris kedua)
  // yang aman diulang. Tiga sisanya menambah di ujung, jadi menjalankan seed
  // dua kali meninggalkan langkah roadmap, proyek, dan sumber yang KEMBAR -
  // persis di halaman yang dibuat untuk memperlihatkan bentuk halaman yang
  // sudah jadi.
  //
  // KENAPA LOLOS SELAMA INI: `MarkDrafted()` tetap hijau. Ia memeriksa bagian
  // yang KOSONG, bukan yang kembar, jadi baris terakhir tetap mencetak
  // 'kelima bagian terisi' di atas halaman yang rusak. Ketahuannya bukan dari
  // uji - dari MENJALANKAN seed dua kali.
  //
  // Diperbaiki di sini, bukan dengan melarang duplikat di domain: dua sumber
  // ber-URL sama memang layak ditolak, tapi itu aturan baru yang menuntut
  // keputusan tersendiri. Yang jelas salah adalah seed yang menjanjikan 'ada'
  // lalu menggandakan.
  //
  // Keadaan sekarang dibaca SEKALI, dari JSON yang diurai - bukan dengan
  // mencocokkan potongan teks.
  const sekarang = (await wajib('GET', `/api/v1/technologies/${slug}`)).json;
  const sudahAda = (daftar, medan, nilai) => (daftar ?? []).some((item) => item[medan] === nilai);

  // Prasyarat WAJIB lebih dulu - AddRoadmapStep menolak dipanggil sebelum
  // langkah 0 ada, dan nomor langkahnya tidak pernah dikirim dari sini.
  // PUT, jadi mengulanginya mengganti langkah 0, bukan menambah.
  await wajib('PUT', `/api/v1/technologies/${slug}/roadmap/prasyarat`, {
    title: 'Dasar HTTP dan JSON-RPC',
    description: 'Paham request/response dan bentuk pesan JSON-RPC.',
  });

  // Urutan langkah ditentukan server dari posisi, jadi tambahkan berurutan
  // dan lewati yang judulnya sudah ada.
  const langkah = [
    { title: 'Menjalankan server MCP pertama', description: 'Pasang SDK, jalankan server contoh, sambungkan ke klien.' },
    { title: 'Menulis tool sendiri', description: 'Deklarasikan skema masukan dan tangani pemanggilannya.' },
  ];
  for (const l of langkah) {
    if (!sudahAda(sekarang.roadmap, 'title', l.title)) {
      await wajib('POST', `/api/v1/technologies/${slug}/roadmap`, l);
    }
  }

  // AttachTool sengaja upsert - aman diulang, dan catatannya ikut diperbarui.
  await wajib('POST', `/api/v1/technologies/${slug}/tools`, {
    toolSlug: 'mcp-inspector',
    note: 'Dipakai sejak langkah pertama untuk melihat pesan yang lewat.',
  });

  const proyek = { title: 'Server MCP untuk catatan lokal', brief: 'Bangun server yang mengekspos folder catatan sebagai resource, lalu bacalah dari klien.' };
  if (!sudahAda(sekarang.projects, 'title', proyek.title)) {
    await wajib('POST', `/api/v1/technologies/${slug}/projects`, proyek);
  }

  // Sumber dikenali dari URL-nya, bukan judulnya: judul boleh ditulis ulang,
  // alamatnya yang menentukan ia sumber yang sama.
  const sumber = { type: 'OfficialDocs', title: 'Spesifikasi Model Context Protocol', url: 'https://modelcontextprotocol.io/specification' };
  if (!sudahAda(sekarang.resources, 'url', sumber.url)) {
    await wajib('POST', `/api/v1/technologies/${slug}/resources`, sumber);
  }

  // Naik ke draf. Ia menolak kalau ada satu bagian pun yang masih kosong,
  // jadi berhasilnya baris ini sekaligus bukti panggilan-panggilan di atas
  // benar-benar mendarat.
  await wajib('POST', `/api/v1/technologies/${slug}/draf`, {});
  console.log(warna('32', '  draf    kelima bagian terisi'));
}

// ---- Relasi antar-topik ----------------------------------------------------

async function catatRelasi(asal, tujuan) {
  // Relasi BUKAN bagian template (ADR-023), jadi judulnya sendiri - bukan di
  // bawah "kelima bagian".
  judul('Mencatat relasi antar-topik ...');

  // Klaim contoh, bukan kurikulum. Pijakannya dua: ADR-010 menaruh tool use
  // dan MCP sebagai topik AI Agents, dan roadmap contoh MCP di atas sendiri
  // mengajarkan "deklarasikan skema masukan" - yaitu tool use.
  //
  // POST ini idempoten di API (ADR-023): mengulangnya 200 tanpa baris kedua,
  // jadi tidak perlu diperiksa lebih dulu seperti bagian yang menambah di ujung.
  const respons = await panggil('POST', `/api/v1/technologies/${asal}/requires`, { topicSlug: tujuan });
  if (respons.status !== 200) {
    gagal(`GAGAL   ${asal} butuh ${tujuan} (HTTP ${respons.status})`, respons);
  }

  console.log(`  sisi    ${asal} butuh ${tujuan}`);
}

// ---- Jalankan --------------------------------------------------------------

async function main() {
  judul(`Mengisi contoh ke ${API} ...`);

  for (const topik of contoh) {
    await buatTopik(topik);
  }

  // Alat dulu: menautkan menuntut alatnya sudah ada di katalog.
  await buatAlat({
    name: 'MCP Inspector',
    summary: 'Alat memeriksa server MCP secara interaktif.',
    homepage: 'https://modelcontextprotocol.io',
    slug: 'mcp-inspector',
  });

  await isiKelimaBagian('model-context-protocol');
  await catatRelasi('model-context-protocol', 'tool-use');
}

try {
  await main();
} catch (err) {
  if (!(err instanceof Berhenti)) {
    throw err;
  }
  // exitCode, bukan process.exit(): keluaran yang masih di penyangga tetap
  // tercetak sebelum proses selesai.
  process.exitCode = 1;
}
