#!/usr/bin/env node
//
// Gerbang tautan di dalam berkas Markdown.
//
// Aturannya SATU, dan sengaja tidak berisi hitungan `../`:
//
//   Tautan relatif hanya boleh menunjuk berkas DI DALAM repo ini.
//   Apa pun di luar itu — issue, PR, milestone, label — wajib URL absolut.
//
// 🔑 Kenapa aturannya begitu, dan bukan "pakai jumlah `../` yang benar":
// halaman blob GitHub meresolusi tautan relatif dengan aturan URL biasa, dengan
// basis `https://github.com/<pemilik>/<repo>/blob/<ref>/<direktori>/`. Artinya
// jumlah `../` yang benar BERGANTUNG pada dua hal sekaligus: kedalaman berkasnya
// DAN berapa segmen yang dimakan nama ref-nya. Repo ini memakai cabang bernama
// `fase-1/muatan-cacat-60-61` — satu garis miring, jadi DUA segmen — sementara
// `main` cuma satu. Tidak ada satu angka pun yang benar di keduanya.
//
// 📏 Ketiga klaim di atas DIUKUR di halaman GitHub yang hidup, bukan disimpulkan
// dari dokumentasi (2026-09-24, repo publik, tanpa login):
//
//   1. `enquirer/enquirer` `docs/install.md` menautkan `../../../issues/new` dan
//      GitHub merendernya jadi `href="/enquirer/enquirer/issues/new"`.
//      -> dari `docs/` di ref tanpa garis miring, yang benar TIGA `../`.
//   2. `nana-4/materia-theme` `TODO.md` (di AKAR) menulis `../../issues/106`
//      dan jadi `href="/nana-4/materia-theme/issues/106"`.
//      -> dari akar, yang benar DUA. Jumlahnya memang bergantung kedalaman, dan
//         inilah jebakannya: benar di README.md, kurang satu di docs/.
//   3. `dotnet/runtime` di cabang `release/8.0`, berkas `docs/project/glossary.md`,
//      menulis `../design/...` dan jadi
//      `href="/dotnet/runtime/blob/release/8.0/docs/design/..."`.
//      -> nama ref bergaris miring benar-benar memakan dua segmen path.
//
// Sebelum aturan ini dipasang, 71 dari 222 tautan permukaan GitHub di repo ini
// rusak saat dibaca di `main` (semuanya di `docs/*.md`, yang memakai dua `../`
// padahal butuh tiga), dan 221 dari 222 rusak saat dibaca di cabang `fase-*`.
// Yang kedua itu penting: badan issue di repo ini menautkan ADR lewat blob
// CABANG, jadi dokumennya justru dibaca di ref yang paling banyak merusaknya.
//
// Yang TIDAK diperiksa di sini, dan sengaja: apakah issue nomor itu benar-benar
// ada. Itu butuh jaringan dan token, dan gerbang yang butuh jaringan berhenti
// bisa dipercaya begitu jaringannya yang rusak.

import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, dirname, posix, relative, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const AKAR = join(dirname(fileURLToPath(import.meta.url)), '..', '..');
const REPO = 'https://github.com/xtheoputra/techverse-x';

// Direktori yang tidak pernah berisi dokumen repo ini.
const LEWATI = new Set(['node_modules', '.git', 'bin', 'obj', '.next', 'dist', 'TestResults']);

// Segmen yang menandai permukaan GitHub, bukan berkas di dalam pohon. Dipakai
// hanya untuk memilih KATA-KATA galatnya; aturan lulus/gagalnya tidak memakai
// daftar ini, jadi permukaan baru (`security/`, `graphs/`) tetap tertangkap.
const PERMUKAAN_GITHUB =
  /(?:^|\/)(issues|pull|discussions|milestone|milestones|labels|compare|commit|commits|releases|actions|wiki|projects|security|graphs|blob|tree|raw)(?:\/|$)/;

// Komentar kode ikut diperiksa — dan bukan demi kerapian. Dokumentasi XML C# dan
// JSDoc di repo ini DITULIS dalam Markdown (daftar berbutir, penekanan, tautan),
// dan `apps/web/src/lib/lingkungan.ts` memang menautkan dua issue dengan hitungan
// `../` yang tidak pernah benar di mana pun. Yang tidak diperiksa di berkas kode:
// keberadaan berkas tujuan — jalur relatif di dalam komentar tidak punya basis yang
// jelas — jadi hanya aturan permukaan GitHub yang berlaku di sana.
const EKSTENSI_KODE = new Set([
  '.cs', '.ts', '.tsx', '.js', '.mjs', '.cjs', '.ps1', '.sh', '.yml', '.yaml', '.sql', '.props', '.csproj', '.slnx',
]);

function berkasTeks(dir) {
  const hasil = [];
  for (const entri of readdirSync(dir, { withFileTypes: true })) {
    if (entri.isDirectory()) {
      if (LEWATI.has(entri.name)) continue;
      hasil.push(...berkasTeks(join(dir, entri.name)));
      continue;
    }
    const titik = entri.name.lastIndexOf('.');
    const ext = titik === -1 ? '' : entri.name.slice(titik);
    if (ext === '.md' || EKSTENSI_KODE.has(ext)) {
      hasil.push(join(dir, entri.name));
    }
  }
  return hasil;
}

// Potongan kode SEBARIS juga dibuang, dan itu bukan kehati-hatian berlebih:
// versi pertama berkas ini melaporkan tiga tautan rusak yang tidak ada, semuanya
// dari prosa yang MENGUTIP kode — `<Link href="/learn">` di catatan sesi terbaca
// sebagai jangkar HTML sungguhan. Tautan Markdown sungguhan tidak pernah hidup
// di dalam potongan kode, jadi membuangnya tidak bisa menyembunyikan temuan.
function tanpaKodeSebaris(teks) {
  return teks.replace(/(`+)(?:(?!\1)[\s\S])*?\1/g, (cocok) => ' '.repeat(cocok.length));
}

// Blok kode dilewati. Berkas ini sendiri buktinya kenapa: komentar dan dokumen
// yang MENJELASKAN aturan ini harus boleh menulis contoh yang melanggarnya.
function barisDiLuarBlokKode(isi) {
  const baris = isi.split(/\r?\n/);
  const keluar = [];
  let pembatas = null;
  for (let i = 0; i < baris.length; i += 1) {
    const teks = baris[i];
    const pagar = /^\s{0,3}(`{3,}|~{3,})/.exec(teks);
    if (pembatas) {
      if (pagar && pagar[1][0] === pembatas[0] && pagar[1].length >= pembatas.length) pembatas = null;
      continue;
    }
    if (pagar) {
      pembatas = pagar[1];
      continue;
    }
    keluar.push([i + 1, teks]);
  }
  return keluar;
}

// Tautan sebaris, definisi rujukan gaya `[id]: …`, dan jangkar HTML.
const POLA = [
  /\[[^\]]*\]\(\s*<?([^)\s>]+)>?(?:\s+"[^"]*")?\s*\)/g,
  /^\s{0,3}\[[^\]]+\]:\s*<?([^\s>]+)>?/g,
  /href\s*=\s*"([^"]+)"/g,
];

const temuan = [];
const berkas = berkasTeks(AKAR).sort();
let diperiksa = 0;
let berkasMd = 0;

for (const jalur of berkas) {
  const rel = relative(AKAR, jalur).split(sep).join('/');
  const markdown = rel.endsWith('.md');
  if (markdown) berkasMd += 1;
  const isi = readFileSync(jalur, 'utf8');

  // Di berkas kode hanya bentuk tautan sebarisnya yang dicari. `href=` dan
  // definisi rujukan di sana lebih sering kode daripada tautan.
  const polaBerlaku = markdown ? POLA : POLA.slice(0, 1);

  for (const [nomor, baris] of barisDiLuarBlokKode(isi)) {
    const teks = tanpaKodeSebaris(baris);
    for (const pola of polaBerlaku) {
      pola.lastIndex = 0;
      let cocok;
      while ((cocok = pola.exec(teks)) !== null) {
        const tujuan = cocok[1];
        if (tujuan.startsWith('#') || /^[a-z][a-z0-9+.-]*:/i.test(tujuan)) {
          // Absolut atau jangkar sehalaman. Satu hal masih diperiksa: skema.
          if (/^http:\/\/github\.com\//i.test(tujuan)) {
            temuan.push([rel, nomor, tujuan, 'http:// ke github.com — pakai https://', tujuan.replace(/^http:/i, 'https:')]);
          }
          continue;
        }
        if (tujuan.startsWith('/')) {
          // Usulnya `https://github.com` + jalurnya apa adanya, BUKAN REPO +
          // jalurnya: versi pertama menyarankan
          // `…/techverse-x/xtheoputra/techverse-x/issues/61` untuk tautan berakar
          // yang sudah memuat nama repo — penjaga yang mengusulkan perbaikan rusak.
          if (markdown) {
            temuan.push([
              rel,
              nomor,
              tujuan,
              'tautan berakar "/" ikut berubah arti antar ref — pakai URL absolut',
              `https://github.com${tujuan}`,
            ]);
          }
          continue;
        }

        const [jalurSaja] = tujuan.split('#');
        if (jalurSaja === '') continue; // hanya jangkar, sudah ditangani di atas

        const sasaran = posix.normalize(posix.join(posix.dirname(rel), jalurSaja));

        if (sasaran.startsWith('..')) {
          // Keluar dari akar repo. Di halaman blob inilah tautan yang jatuh ke
          // `…/blob/<sesuatu>` alih-alih ke tujuannya.
          diperiksa += 1;
          const ekor = tujuan.slice(tujuan.lastIndexOf('../') + 3);
          const sebab = PERMUKAAN_GITHUB.test(ekor)
            ? 'permukaan GitHub ditunjuk relatif — jumlah ../ tidak pernah benar di semua ref'
            : 'keluar dari pohon repo';
          temuan.push([rel, nomor, tujuan, sebab, `${REPO}/${ekor}`]);
          continue;
        }

        // ⚠️ Keberadaan berkas diperiksa juga di berkas kode, dan versi pertama
        // berkas ini TIDAK melakukannya — lubang yang langsung terbukti. Dua tautan
        // issue di `apps/web/src/lib/lingkungan.ts` (empat tingkat dalam) memakai
        // empat `../`, yang secara JALUR mendarat tepat di akar repo. Jadi ia tidak
        // "keluar pohon" dan lolos, padahal di halaman blob ia jatuh ke
        // `…/blob/main/issues/42`: berkas yang tidak ada. Satu-satunya aturan yang
        // menangkap keduanya adalah aturan yang sama untuk semua berkas.
        diperiksa += 1;
        try {
          statSync(join(AKAR, sasaran.split('/').join(sep)));
        } catch {
          const ekor = tujuan.includes('../') ? tujuan.slice(tujuan.lastIndexOf('../') + 3) : tujuan;
          temuan.push([
            rel,
            nomor,
            tujuan,
            PERMUKAAN_GITHUB.test(ekor)
              ? `permukaan GitHub ditunjuk relatif — jatuh ke berkas yang tidak ada: ${sasaran}`
              : `berkas tidak ada: ${sasaran}`,
            PERMUKAAN_GITHUB.test(ekor) ? `${REPO}/${ekor}` : null,
          ]);
        }
      }
    }
  }
}

const merah = '\u001b[31m';
const abu = '\u001b[90m';
const hijau = '\u001b[32m';
const mati = '\u001b[0m';

if (temuan.length === 0) {
  console.log(
    `  ${berkasMd} berkas Markdown + ${berkas.length - berkasMd} berkas kode, ` +
      `${diperiksa} tautan relatif diperiksa — ${hijau}semuanya menunjuk ke dalam repo${mati}`,
  );
  process.exit(0);
}

console.log(`${merah}${temuan.length} tautan tidak akan sampai ke tujuannya:${mati}\n`);
let berkasTerakhir = null;
for (const [rel, nomor, tujuan, sebab, usul] of temuan) {
  if (rel !== berkasTerakhir) {
    console.log(`  ${rel}`);
    berkasTerakhir = rel;
  }
  console.log(`    baris ${String(nomor).padStart(4)}  ${tujuan}`);
  console.log(`      ${abu}${sebab}${mati}`);
  if (usul) console.log(`      ganti jadi: ${usul}`);
}
console.log(
  `\n${abu}Alasan aturannya, berikut tiga pengukuran yang melahirkannya, ada di kepala` +
    `\n.github/scripts/cek-tautan-markdown.mjs.${mati}`,
);
process.exit(1);
