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
// ⚠️ **TIDAK dijalankan di `verify` maupun di CI, dan itu disebutkan supaya tidak
// disalahpahami sebagai gerbang yang selalu menjaga.** Ia menuntut web yang sudah
// dibangun DAN API yang hidup; job `frontend` di `ci.yml` tidak punya keduanya, dan
// menyalakannya di sana berarti Postgres + API + `next start` di satu job. Jalan
// itu tertulis sebagai langkah berikutnya, bukan diklaim sudah ada. Sampai itu:
// dijalankan tangan lewat satu perintah, dan perintahnya yang tidak boleh lupa.
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

  for (const berikut of tautanInternal(hasil.html, jalur)) {
    if (!dikunjungi.has(berikut)) antre.push(berikut);
  }
}

// ---------------------------------------------- pembanding dari API (opsional)

const halamanTeknologi = [...dikunjungi.keys()].filter((j) => j.startsWith('/teknologi/'));
let barisApi = `${kuning}API_BASE_URL tidak diberikan — jumlah halaman tidak dibandingkan${mati}`;

if (API) {
  try {
    const [bidang, topik] = await Promise.all([
      fetch(`${API}/api/v1/fields`).then((r) => r.json()),
      fetch(`${API}/api/v1/technologies`).then((r) => r.json()),
    ]);
    const daftarBidang = Array.isArray(bidang) ? bidang : (bidang.items ?? []);
    const daftarTopik = Array.isArray(topik) ? topik : (topik.items ?? []);
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
  } catch (galat) {
    barisApi = `${kuning}API di ${API} tidak bisa dibaca: ${galat.message}${mati}`;
  }
}

// -------------------------------------------------------------------- laporan

console.log(`  ${WEB} — ${dikunjungi.size} halaman ditelusuri, ${halamanTeknologi.length} di antaranya /teknologi/*`);
console.log(`  ${barisApi}`);

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
