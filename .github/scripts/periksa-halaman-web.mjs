#!/usr/bin/env node
//
// Gerbang ADR-024 untuk `apps/web`: aturan TEKS PEMBACA dan PENELUSURAN halaman,
// diukur di halaman yang sudah jadi.
//
// 🔴 Kenapa ini ada. ADR-024 menutup bagian *Konsekuensi* dengan pengakuan:
// *"Aturan teks pembaca dan tampilan relasi tidak punya penjaga otomatis. Keduanya
// hanya bertahan selama pemindaian teks dan pemeriksaan halaman diulang tangan di
// build produksi; PR berikutnya bisa menanam 'Bulan N' lagi tanpa ada yang merah."*
// Berkas ini mengubah kedua pemindaian tangan itu jadi satu perintah.
//
// 🔑 Dipindai HALAMAN JADINYA, bukan kode web — dan itu bukan pilihan kenyamanan.
// Garis dasar ADR-024 memberi SEMBILAN kena di halaman muka, dan TIGA di antaranya
// datang dari DATA: ringkasan bidang Cloud & Infrastructure, Renewable Energy, dan
// XR yang disemai migrasi dari `FieldCatalog`, masing-masing menyuruh pengunjung
// *"lihat ADR-010"*. Pemindai yang membaca `apps/web/src/**` tidak akan pernah
// melihat ketiganya. Selama produksi nol topik, keempat belas kartu itulah isi
// utama halaman muka.
//
// ⚠️ **Di CI ia jalan, di `verify` TIDAK — dan bedanya penting.**
//
// Versi pertama berkas ini (pagi yang sama) menulis bahwa ia tidak jalan di CI dan
// menyebut jalan ke sana "langkah berikutnya". Langkah itu **sudah diambil**: job
// `citra` di `ci.yml` menyalakan `docker-compose.prod.yml` lalu memanggil berkas ini.
// Job itu yang dipilih, bukan `frontend`, karena ia sudah membangun ketiga citranya
// beberapa langkah di atas — jadi `compose up` di sana menabrak cache build.
//
// `verify` tetap **tidak** memanggilnya, dengan alasan yang sama seperti `seed`: ia
// menuntut tumpukan yang hidup, dan gerbang lokal yang menuntut Docker Compose
// berhenti bisa dijalankan sambil menulis kode.
//
// 📏 Angka yang diukur di kedua bentuk, dan keduanya dicatat supaya selisihnya tidak
// disalahartikan sebagai kemunduran:
//
//   - bentuk PRODUKSI (yang dijalankan CI): 16 halaman, **14 dari 14** — nol topik,
//     jadi empat belas halaman bidang itulah seluruh isinya;
//   - bentuk PENGEMBANGAN (API dev berisi 6 topik contoh): 22 halaman, **20 dari 20**.
//
// Pemakaian:
//   node .github/scripts/periksa-halaman-web.mjs
//   WEB_BASE_URL=http://localhost:3310 API_BASE_URL=http://localhost:5080 node …
//
// `API_BASE_URL` opsional. Kalau diberikan, jumlah halaman `/teknologi/*` yang
// tercapai dari `/` dibandingkan dengan yang DIKENAL API — ukuran "20 dari 20" dan
// "14 dari 14" yang ADR-024 hitung tangan. Tanpa itu, penelusurannya tetap jalan
// tapi tidak ada angka pembanding, dan laporannya mengatakan begitu.

const WEB = (process.env.WEB_BASE_URL ?? 'http://localhost:3000').replace(/\/+$/, '');
const API = process.env.API_BASE_URL?.replace(/\/+$/, '') ?? null;
const BATAS_HALAMAN = 300;

// Pola ADR-024, apa adanya, ditambah dua yang sudah pernah tercetak ke pembaca:
// port API dev dan port peti kemasnya. `localhost` sendiri sudah ada di pola ADR.
const TERLARANG =
  /Bulan \d|menyusul|beranda sementara|ADR-\d|run\.ps1|localhost|api:8080|\b5080\b|\b8080\b/g;

const merah = '\u001b[31m';
const kuning = '\u001b[33m';
const abu = '\u001b[90m';
const hijau = '\u001b[32m';
const mati = '\u001b[0m';

const ENTITAS = {
  amp: '&', lt: '<', gt: '>', quot: '"', apos: "'", nbsp: ' ', hellip: '…',
  mdash: '—', ndash: '–', laquo: '«', raquo: '»', rsquo: '’', lsquo: '‘',
  ldquo: '“', rdquo: '”', middot: '·', times: '×', deg: '°',
};

/**
 * Teks yang BENAR-BENAR dibaca orang. `<script>` dan `<style>` dibuang berikut
 * isinya — di halaman Next, muatan RSC hidup di dalam `<script>`, dan ia memuat
 * setiap string yang dilewati ke komponen klien. Tanpa membuangnya, gerbang ini
 * akan merah untuk teks yang tidak pernah tergambar.
 */
function teksTerlihat(html) {
  return html
    .replace(/<script\b[\s\S]*?<\/script>/gi, ' ')
    .replace(/<style\b[\s\S]*?<\/style>/gi, ' ')
    .replace(/<!--[\s\S]*?-->/g, ' ')
    .replace(/<[^>]+>/g, ' ')
    .replace(/&#(\d+);/g, (_, d) => String.fromCodePoint(Number(d)))
    .replace(/&#x([0-9a-f]+);/gi, (_, h) => String.fromCodePoint(parseInt(h, 16)))
    .replace(/&([a-z]+);/gi, (utuh, nama) => ENTITAS[nama.toLowerCase()] ?? utuh)
    .replace(/\s+/g, ' ')
    .trim();
}

/**
 * Halaman yang bisa dicapai pembaca dari sini: jangkar `<a href>` DAN sasaran
 * `<form method="get" action>`.
 *
 * 🐞 Versi pertama hanya mengikuti `<a href>`, dan itu langsung terukur salah:
 * penelusurannya berhenti di 21 halaman — beranda plus dua puluh halaman topik —
 * dan `/cari` TIDAK pernah dikunjungi, sebab satu-satunya jalan ke sana adalah
 * `<form action="/cari" method="get">` di tata letak. Akibatnya pemeriksaan
 * `id="q"` di bawah tidak pernah dijalankan sekali pun, dan gerbangnya hijau
 * karena tidak melihat, bukan karena bersih. Formulir GET adalah navigasi juga.
 */
function tautanInternal(html, dariJalur) {
  const keluar = new Set();
  const sumber = [
    ...html.matchAll(/<a\b[^>]*\bhref\s*=\s*"([^"]+)"/gi),
    // `method` boleh mendahului atau mengikuti `action`, dan boleh tidak ada sama
    // sekali (GET bawaan). POST bukan navigasi, jadi ia dibuang.
    ...[...html.matchAll(/<form\b([^>]*)>/gi)]
      .filter((f) => !/\bmethod\s*=\s*"post"/i.test(f[1]))
      .map((f) => /\baction\s*=\s*"([^"]+)"/i.exec(f[1]))
      .filter((f) => f !== null),
  ];
  for (const cocok of sumber) {
    const mentah = cocok[1];
    if (/^[a-z][a-z0-9+.-]*:/i.test(mentah) || mentah.startsWith('//')) continue;
    let url;
    try {
      url = new URL(mentah, `${WEB}${dariJalur}`);
    } catch {
      continue;
    }
    if (url.origin !== new URL(WEB).origin) continue;
    keluar.add(url.pathname + url.search);
  }
  return keluar;
}

async function ambil(jalur) {
  const res = await fetch(`${WEB}${jalur}`, { redirect: 'manual' });
  const html = res.headers.get('content-type')?.includes('text/html') ? await res.text() : '';
  return { status: res.status, html };
}

// ---------------------------------------------------------------- penelusuran

const dikunjungi = new Map();
// Teks terlihat tiap halaman yang menjawab 200 — dibaca lagi oleh sonde ringkasan
// alat di bawah, yang membandingkannya dengan jawaban API.
const teksHalaman = new Map();
const antre = ['/'];
const temuan = [];

while (antre.length > 0 && dikunjungi.size < BATAS_HALAMAN) {
  const jalur = antre.shift();
  if (dikunjungi.has(jalur)) continue;

  let hasil;
  try {
    hasil = await ambil(jalur);
  } catch (galat) {
    console.log(`${merah}Tidak bisa menghubungi ${WEB}${jalur}${mati}`);
    console.log(`  ${galat.message}`);
    console.log(`${abu}Nyalakan web-nya lebih dulu (build produksi), lalu ulangi:`);
    console.log(`  npm run build:web && npx next start -p 3310  (dari apps/web)${mati}`);
    process.exit(2);
  }

  dikunjungi.set(jalur, hasil.status);

  if (hasil.status !== 200) {
    temuan.push([jalur, `HTTP ${hasil.status}`, null]);
    continue;
  }

  const terlihat = teksTerlihat(hasil.html);
  teksHalaman.set(jalur, terlihat);
  TERLARANG.lastIndex = 0;
  const kena = [...new Set((terlihat.match(TERLARANG) ?? []))];
  for (const k of kena) {
    // Satu potongan kalimat di sekitarnya, supaya laporannya bisa langsung
    // dicocokkan ke halaman tanpa membuka peramban.
    const i = terlihat.indexOf(k);
    const sekitar = terlihat.slice(Math.max(0, i - 60), i + k.length + 60);
    temuan.push([jalur, `teks terlihat memuat "${k}"`, `…${sekitar}…`]);
  }

  // Cacat terukur ADR-024: `id="q"` dua kali di /cari, dan `<label>` kotak kedua
  // menunjuk input pertama. Dijaga di sini karena ia hanya terlihat di HTML jadinya.
  if (jalur.split('?')[0] === '/cari') {
    for (const id of ['q', 'q-halaman']) {
      const n = [...hasil.html.matchAll(new RegExp(`\\bid="${id}"`, 'g'))].length;
      if (n !== 1) temuan.push([jalur, `id="${id}" muncul ${n}x, seharusnya 1x`, null]);
    }
  }

  // Aturan ADR-023: blok "Topik terhubung" TIDAK PERNAH tampil kosong. Relasi bukan
  // bagian template yang wajib diisi, jadi judul berdaftar kosong menandai halaman
  // seolah setengah jadi - dan di produksi, yang nol topik, blok ini tidak boleh
  // terlihat sama sekali.
  //
  // Sampai hari ini penjaganya hanya sabotase TANGAN (Sesi 14: return awal dibuang
  // -> qiskit memuat "Topik terhubung Pelajari lebih dulu Dibutuhkan oleh"
  // berdaftar kosong). Invariannya bisa diperiksa di bentuk APA PUN, termasuk
  // produksi tempat ia vakum: kalau blok itu ada, ia wajib berisi setidaknya satu
  // butir.
  const blok = /<section[^>]*aria-labelledby="topik-terhubung"[^>]*>([\s\S]*?)<\/section>/i.exec(hasil.html);
  if (blok) {
    const butir = [...blok[1].matchAll(/<li\b/gi)].length;
    if (butir === 0) {
      temuan.push([jalur, 'blok "Topik terhubung" tampil dengan NOL butir (ADR-023)', teksTerlihat(blok[0])]);
    }
    for (const judul of ['Pelajari lebih dulu', 'Dibutuhkan oleh']) {
      // Judul sub-daftar hanya boleh ada kalau daftarnya sendiri ada isinya. Keduanya
      // muncul bersamaan di sabotase Sesi 14, jadi keduanya diperiksa.
      const sub = new RegExp(`<h3[^>]*>\\s*${judul}\\s*</h3>([\\s\\S]*?)(?=<h3\\b|$)`, 'i').exec(blok[1]);
      if (sub && [...sub[1].matchAll(/<li\b/gi)].length === 0) {
        temuan.push([jalur, `sub-daftar "${judul}" ada tapi NOL butir (ADR-023)`, null]);
      }
    }
  }

  for (const berikut of tautanInternal(hasil.html, jalur)) {
    if (!dikunjungi.has(berikut)) antre.push(berikut);
  }
}

// ------------------------------------------ sonde: kata kunci yang DIPENDEKKAN

// Satu permintaan yang SENGAJA dibuat, bukan hasil penelusuran — sebab tidak ada
// tautan di situs ini yang menuju kata kunci sepanjang ini.
//
// Aturan yang dijaga tertulis DUA KALI (`SearchResponse.Query` di kontrak dan
// `SearchResult.query` di klien): *"yang ditampilkan ke pembaca harus kata kunci
// yang benar-benar dipakai server"*. Sampai 2026-09-24 halaman `/cari` mencetak
// yang DIKETIK — diukur: 230 karakter dikirim, server memakai 200, dan halaman
// menampilkan 230 penuh di judul, di kotak cari, dan di kalimat nol-hasil.
//
// Sondenya tidak menyebut angka batasnya. Ia mengirim kata kunci yang jelas lebih
// panjang daripada batas mana pun yang masuk akal, lalu menuntut satu hal saja:
// teks yang TERLIHAT tidak boleh memuat kata kunci itu utuh. Dengan begitu batasnya
// tetap milik server, dan gerbang ini tidak jadi sumber kedua yang bisa hanyut.
//
// ⚠️ Diperiksa hanya di dalam `<body>`, dan itu pilihan sadar, bukan kelonggaran.
// `<title>` memantulkan PERMINTAAN pembaca — kata yang ia ketik, yang juga muncul di
// tab dan riwayat peramban — sementara badan halaman MELAPORKAN hasil pencariannya.
// Yang dilarang aturan ini adalah laporan yang menyebut kata kunci yang tidak pernah
// dicari. Halaman `/cari` juga `noindex` (ADR-024), jadi judulnya tidak diindeks.
{
  const panjang = 600;
  const kunci = 'zzq' + 'x'.repeat(panjang);
  const jalur = `/cari?q=${encodeURIComponent(kunci)}`;
  try {
    const { status, html } = await ambil(jalur);
    const badan = html.slice(Math.max(0, html.search(/<body\b/i)));
    if (status !== 200) {
      temuan.push([jalur, `sonde kata kunci panjang: HTTP ${status}`, null]);
    } else if (teksTerlihat(badan).includes(kunci)) {
      temuan.push([
        jalur,
        'teks terlihat memuat kata kunci UTUH padahal server memendekkannya — ' +
          'halaman menyebut sesuatu yang tidak pernah dicari',
        null,
      ]);
    }
  } catch (galat) {
    temuan.push([jalur, `sonde kata kunci panjang gagal: ${galat.message}`, null]);
  }
}

// ---------------------------------------------- pembanding dari API (opsional)

/**
 * SEMUA topik yang dikenal API, halaman demi halaman sampai `hasNextPage` salah.
 *
 * 🐞 Sampai 2026-09-28 pembanding ini membaca `GET /api/v1/technologies` SEKALI,
 * tanpa `pageSize` — jadi dengan bawaan server, 20. Topik ke-21 dan seterusnya
 * tidak pernah masuk hitungan "dikenal API", dan halaman mereka yang tidak
 * tercapai tidak pernah dilaporkan. Di skala hari ini (6 topik di pengembangan,
 * 0 di produksi) hitungannya kebetulan benar; target Bulan 4 di RENCANA-V1 saja
 * sudah 22.
 *
 * 📏 Diukur, bukan diduga: 55 topik sementara di bidang XR (61 seluruhnya).
 * Halaman bidang memotong di 50 dan beranda di 24, jadi lima topik memang tidak
 * punya satu tautan pun. Versi lama HIJAU, exit 0, sambil mencetak "70 dari 34" —
 * pembilang lebih besar daripada penyebutnya. Versi ini: "70 dari 75", kelima
 * topiknya disebut satu per satu, exit 1.
 *
 * Ini juga pembaca pertama `PagedResponse.HasNextPage` di repo ini.
 */
async function semuaTopik() {
  const semua = [];
  for (let halaman = 1; halaman <= 1000; halaman++) {
    const jawaban = await fetch(`${API}/api/v1/technologies?page=${halaman}&pageSize=100`).then((r) => r.json());
    if (Array.isArray(jawaban)) return jawaban;
    semua.push(...(jawaban.items ?? []));
    if (!jawaban.hasNextPage) return semua;
  }
  throw new Error('lebih dari 1000 halaman topik — hasNextPage tidak pernah false?');
}

const halamanTeknologi = [...dikunjungi.keys()].filter((j) => j.startsWith('/teknologi/'));
let barisApi = `${kuning}API_BASE_URL tidak diberikan — jumlah halaman tidak dibandingkan${mati}`;
let barisAlat = null;

if (API) {
  try {
    const [bidang, daftarTopik] = await Promise.all([
      fetch(`${API}/api/v1/fields`).then((r) => r.json()),
      semuaTopik(),
    ]);
    const daftarBidang = Array.isArray(bidang) ? bidang : (bidang.items ?? []);
    const diharap = new Set([
      ...daftarBidang.map((b) => `/teknologi/${b.slug}`),
      ...daftarTopik.map((t) => `/teknologi/${t.slug}`),
    ]);
    const tercapai = new Set(halamanTeknologi.map((j) => j.split('?')[0]));
    const hilang = [...diharap].filter((j) => !tercapai.has(j));

    barisApi = `penelusuran dari /: ${tercapai.size} dari ${diharap.size} halaman /teknologi/* yang dikenal API`;
    for (const j of hilang) {
      temuan.push([j, 'dikenal API tapi TIDAK tercapai dari / lewat tautan mana pun', null]);
    }

    // ------------------------- sonde: ringkasan alat yang dikirim API TAMPIL
    //
    // `Tool` menulis alasan keberadaannya sendiri: katalog alat berdiri sendiri
    // supaya lima halaman yang memakai LangGraph tidak berbeda pendapat soal APA
    // LangGraph itu. Jawaban "apa"-nya adalah `summary` alat — dan sampai
    // 2026-09-28 medan itu melintasi kabel ke setiap halaman topik tanpa pernah
    // tergambar: halaman hanya mencetak nama dan `note`. Diukur di contoh
    // pengembangan: "Alat memeriksa server MCP secara interaktif." ada di jawaban
    // API, tidak ada di teks terlihat halamannya, dan tidak ada pula di muatan RSC.
    //
    // Pola yang sama dengan `SearchResponse.Query` (#67): kontraknya menyatakan
    // untuk apa medan itu, kliennya melewatinya. Bandingkan dengan sonde kata kunci
    // di atas, yang lahir dari cacat itu.
    //
    // ⚠️ Di produksi hari ini sonde ini VAKUM — nol topik, jadi nol alat — dan
    // barisnya di laporan menyebut angkanya supaya kevakuman itu terbaca, bukan
    // disangka hijau.
    let diperiksa = 0;
    let halamanBeralat = 0;
    for (const t of daftarTopik) {
      const jalur = `/teknologi/${t.slug}`;
      const terlihat = teksHalaman.get(jalur);
      // Tidak tercapai atau bukan 200: sudah dilaporkan di atas, jangan dua kali.
      if (terlihat === undefined) continue;

      const detail = await fetch(`${API}/api/v1/technologies/${encodeURIComponent(t.slug)}`).then((r) => r.json());
      const alatBerringkasan = (detail.tools ?? []).filter((a) => a.summary?.trim());
      if (alatBerringkasan.length > 0) halamanBeralat++;

      for (const alat of alatBerringkasan) {
        diperiksa++;
        const ringkasan = alat.summary.replace(/\s+/g, ' ').trim();
        if (!terlihat.includes(ringkasan)) {
          temuan.push([jalur, `ringkasan alat "${alat.name}" dikirim API tapi TIDAK terlihat di halaman`, ringkasan]);
        }
      }
    }
    barisAlat = `ringkasan alat: ${diperiksa} diperiksa di ${halamanBeralat} halaman topik`;
  } catch (galat) {
    barisApi = `${kuning}API di ${API} tidak bisa dibaca: ${galat.message}${mati}`;
  }
}

// -------------------------------------------------------------------- laporan

console.log(`  ${WEB} — ${dikunjungi.size} halaman ditelusuri, ${halamanTeknologi.length} di antaranya /teknologi/*`);
console.log(`  ${barisApi}`);
if (barisAlat) console.log(`  ${barisAlat}`);

if (temuan.length === 0) {
  console.log(`  ${hijau}teks terlihat: 0 kena; setiap halaman 200${mati}`);
  process.exit(0);
}

console.log(`\n${merah}${temuan.length} temuan:${mati}\n`);
for (const [jalur, sebab, kutipan] of temuan) {
  console.log(`  ${jalur}`);
  console.log(`    ${sebab}`);
  if (kutipan) console.log(`    ${abu}${kutipan}${mati}`);
}
console.log(`\n${abu}Aturannya ADR-024; alasan tiap polanya ada di kepala berkas ini.${mati}`);
process.exit(1);
