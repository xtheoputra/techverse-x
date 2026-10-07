// Uji aturan media berkas isi. Tanpa dependensi: `node --test database/isi/media.test.mjs`.
//
// Dua arah dijaga untuk setiap aturan: yang SAH lolos (kendali — tanpanya semua penolakan di
// bawah bisa hijau karena fixture-nya sendiri yang cacat) dan yang cacat ditolak dengan pesan
// yang menyebut medan dan jalurnya.

import assert from 'node:assert/strict';
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { after, before, test } from 'node:test';

import { BATAS_MEDIA, cocokkanRujukan, periksaDaftarMedia, periksaSvg } from './media.mjs';

const SLUG = 'uji-topik';
const SVG_SAH = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10"/></svg>';

let publik;

before(() => {
  publik = mkdtempSync(join(tmpdir(), 'media-uji-'));
  mkdirSync(join(publik, 'media', SLUG), { recursive: true });
  writeFileSync(join(publik, 'media', SLUG, 'sah.svg'), SVG_SAH);
});

after(() => {
  rmSync(publik, { recursive: true, force: true });
});

const gambar = (ubah = {}) => ({
  key: 'arsitektur',
  kind: 'Image',
  url: `/media/${SLUG}/sah.svg`,
  alt: 'Diagram arsitektur',
  caption: 'Tiga peran.',
  license: 'Karya sendiri',
  ...ubah,
});

const video = (ubah = {}) => ({
  key: 'demo',
  kind: 'Video',
  videoId: 'dQw4w9WgXcQ',
  alt: 'Demo resmi',
  sourceName: 'Kanal resmi',
  sourceUrl: 'https://www.youtube.com/@kanal',
  license: 'Hak cipta pemilik kanal',
  ...ubah,
});

function periksa(daftar) {
  const galat = [];
  const kunci = periksaDaftarMedia(galat, 'T media', daftar, { slugTopik: SLUG, folderPublik: publik });
  return { galat, kunci };
}

// ---- yang sah --------------------------------------------------------------------

test('media yang sah lolos tanpa galat dan kuncinya dikembalikan', () => {
  const { galat, kunci } = periksa([gambar(), video()]);
  assert.deepEqual(galat, []);
  assert.deepEqual([...kunci], ['arsitektur', 'demo']);
});

test('tanpa daftar media sama sekali sah (media opsional), dan daftar kosong juga', () => {
  assert.deepEqual(periksa(undefined), { galat: [], kunci: new Set() });
  assert.deepEqual(periksa([]).galat, []);
});

test('lisensi selain karya sendiri sah asal sumber lengkap; karya sendiri boleh tanpa sumber', () => {
  assert.deepEqual(periksa([gambar({ license: 'CC BY 4.0', sourceName: 'Penulis', sourceUrl: 'https://x.test/a' })]).galat, []);
  assert.deepEqual(periksa([gambar({ sourceName: 'Saya', sourceUrl: 'https://x.test' })]).galat, []);
});

// ---- bentuk umum ------------------------------------------------------------------

test('bukan daftar, bukan objek, kunci tak dikenal, dan kelebihan jumlah ditolak', () => {
  assert.match(periksa({}).galat[0], /harus berupa daftar/);
  assert.match(periksa(['teks']).galat[0], /harus berupa objek/);
  assert.match(periksa([gambar({ salahKetik: 1 })]).galat.join('\n'), /kunci 'salahKetik' tidak dikenal/);

  const banyak = Array.from({ length: BATAS_MEDIA.perTopik + 1 }, (_, i) => video({ key: `v-${i}` }));
  assert.match(periksa(banyak).galat.join('\n'), /batas per topik 40/);
});

test('kunci: harus slug, unik, wajib', () => {
  assert.match(periksa([gambar({ key: 'Besar' })]).galat.join('\n'), /key: 'Besar' bukan slug/);
  assert.match(periksa([gambar({ key: 'dua kata' })]).galat.join('\n'), /bukan slug/);
  assert.match(periksa([gambar({ key: undefined })]).galat.join('\n'), /key: wajib/);
  assert.match(periksa([gambar(), gambar()]).galat.join('\n'), /muncul dua kali/);
});

// ---- gambar -------------------------------------------------------------------------

test('jenis harus Image atau Video persis', () => {
  for (const kind of ['image', 'GIF', undefined, 1]) {
    assert.match(periksa([gambar({ kind })]).galat.join('\n'), /kind: harus 'Image' atau 'Video'/, String(kind));
  }
});

test('gambar: url wajib, berkas sendiri di folder topiknya, berekstensi gambar, tanpa naik direktori', () => {
  assert.match(periksa([gambar({ url: undefined })]).galat.join('\n'), /url: gambar wajib punya url/);

  for (const url of ['https://x.test/a.png', '//x.test/a.png', `/media/${SLUG}/../lain/a.svg`, `/media/${SLUG}/a.exe`, `/media/${SLUG}/a`, `/lain/${SLUG}/a.svg`]) {
    assert.match(periksa([gambar({ url })]).galat.join('\n'), /url: harus jalur berkas sendiri di \/media\//, url);
  }

  assert.match(periksa([gambar({ url: '/media/topik-lain/sah.svg' })]).galat.join('\n'), /hidup di \/media\/uji-topik\//);
  assert.match(periksa([gambar({ url: `/media/${SLUG}/ada-di-spesifikasi.svg` })]).galat.join('\n'), /berkas tak ada/);
  assert.match(periksa([gambar({ videoId: 'dQw4w9WgXcQ' })]).galat.join('\n'), /gambar tak punya videoId/);
});

test('gambar: berkas terlalu besar ditolak (svg dan raster punya batas sendiri)', () => {
  writeFileSync(join(publik, 'media', SLUG, 'besar.svg'), `${SVG_SAH}<!--${'x'.repeat(BATAS_MEDIA.bytSvg)}-->`);
  writeFileSync(join(publik, 'media', SLUG, 'besar.png'), Buffer.alloc(BATAS_MEDIA.bytRaster + 1));
  writeFileSync(join(publik, 'media', SLUG, 'kecil.png'), Buffer.alloc(1024));

  assert.match(periksa([gambar({ url: `/media/${SLUG}/besar.svg` })]).galat.join('\n'), /batasnya 300 KB/);
  assert.match(periksa([gambar({ url: `/media/${SLUG}/besar.png` })]).galat.join('\n'), /batasnya 800 KB/);
  assert.deepEqual(periksa([gambar({ url: `/media/${SLUG}/kecil.png` })]).galat, []);
});

test('svg yang tak aman di berkasnya ikut ditolak, dengan jalur berkas disebut', () => {
  writeFileSync(join(publik, 'media', SLUG, 'jahat.svg'), '<svg viewBox="0 0 1 1"><script>alert(1)</script></svg>');

  const galat = periksa([gambar({ url: `/media/${SLUG}/jahat.svg` })]).galat.join('\n');

  assert.match(galat, /\/media\/uji-topik\/jahat\.svg/);
  assert.match(galat, /<script>/);
});

// ---- video -----------------------------------------------------------------------------

test('video: ID sebelas karakter, tanpa url', () => {
  for (const videoId of [undefined, 'pendek', 'dQw4w9WgXc!', 'dQw4w9WgXcQQ']) {
    assert.match(periksa([video({ videoId })]).galat.join('\n'), /videoId: harus ID YouTube 11 karakter/, String(videoId));
  }
  assert.match(periksa([video({ url: `/media/${SLUG}/sah.svg` })]).galat.join('\n'), /video tak punya url/);
});

// ---- teks, lisensi, sumber ----------------------------------------------------------------

test('teks alternatif wajib, dan tak boleh berspasi di tepi atau kepanjangan', () => {
  assert.match(periksa([gambar({ alt: undefined })]).galat.join('\n'), /alt: wajib/);
  assert.match(periksa([gambar({ alt: '   ' })]).galat.join('\n'), /alt: wajib/);
  assert.match(periksa([gambar({ alt: ' dengan spasi ' })]).galat.join('\n'), /ada spasi di awal atau akhir/);
  assert.match(periksa([gambar({ alt: 'a'.repeat(BATAS_MEDIA.alt + 1) })]).galat.join('\n'), /batasnya 500/);
  assert.match(periksa([gambar({ caption: 'c'.repeat(BATAS_MEDIA.caption + 1) })]).galat.join('\n'), /caption: .*batasnya 1000/);
  assert.match(periksa([gambar({ caption: '' })]).galat.join('\n'), /caption: bila ada, harus teks tak kosong/);
});

test('lisensi wajib; "Karya sendiri" harus persis (server membakukannya)', () => {
  assert.match(periksa([gambar({ license: undefined })]).galat.join('\n'), /license: wajib/);
  assert.match(periksa([gambar({ license: 'karya sendiri' })]).galat.join('\n'), /tulis persis 'Karya sendiri'/);
  assert.match(periksa([gambar({ license: 'KARYA SENDIRI' })]).galat.join('\n'), /tulis persis 'Karya sendiri'/);
});

test('selain karya sendiri sumber wajib lengkap, dan URL sumber harus http(s) absolut', () => {
  const tanpa = periksa([gambar({ license: 'CC BY 4.0' })]).galat.join('\n');
  assert.match(tanpa, /sumber wajib — sourceName DAN sourceUrl/);

  assert.match(periksa([gambar({ license: 'CC BY 4.0', sourceName: 'Penulis' })]).galat.join('\n'), /sumber wajib/);
  assert.match(periksa([gambar({ license: 'CC BY 4.0', sourceName: 'Penulis', sourceUrl: 'ftp://x.test' })]).galat.join('\n'), /sourceUrl: harus http\/https absolut/);
  assert.match(periksa([gambar({ license: 'CC BY 4.0', sourceName: 'Penulis', sourceUrl: 'relatif/saja' })]).galat.join('\n'), /sourceUrl: harus http\/https absolut/);
});

// ---- SVG ---------------------------------------------------------------------------------

test('periksaSvg: yang sah lolos, termasuk komentar, prolog XML, style, dan rujukan internal', () => {
  assert.deepEqual(periksaSvg(SVG_SAH), []);
  assert.deepEqual(periksaSvg(`<?xml version="1.0" encoding="UTF-8"?>\n<!-- dibuat tangan -->\n${SVG_SAH}`), []);
  assert.deepEqual(
    periksaSvg('<svg viewBox="0 0 4 4"><style>.a{fill:#22d3ee}</style><defs><g id="x"><rect width="1" height="1"/></g></defs><use href="#x"/><use xlink:href="#x"/></svg>'),
    [],
  );
  // Kata "script" di teks atau komentar bukan elemen script.
  assert.deepEqual(periksaSvg('<svg viewBox="0 0 4 4"><!-- tanpa <script> --><text>transkrip</text></svg>'), []);
});

test('periksaSvg: tiap bentuk berbahaya ditolak', () => {
  const bungkus = (dalam) => `<svg viewBox="0 0 4 4">${dalam}</svg>`;
  const berbahaya = {
    '<script>': bungkus('<script>alert(1)</script>'),
    '<SCRIPT': bungkus('<SCRIPT src="x.js"></SCRIPT>'),
    '<foreignObject>': bungkus('<foreignObject><div/></foreignObject>'),
    'elemen tersemat': bungkus('<iframe src="x"></iframe>'),
    'penangan peristiwa': bungkus('<rect onclick="x()" width="1" height="1"/>'),
    'penangan onload': '<svg viewBox="0 0 4 4" onload="x()"></svg>',
    'URL javascript:': bungkus('<a href="javascript:alert(1)"><rect/></a>'),
    'data:text/html': bungkus('<a href="data:text/html,x"><rect/></a>'),
    'DOCTYPE atau ENTITY': `<!DOCTYPE svg [<!ENTITY a "b">]>${bungkus('')}`,
    '@import': bungkus('<style>@import url(x.css);</style>'),
    'rujukan ke alamat luar': bungkus('<image href="https://x.test/a.png"/>'),
    'rujukan luar xlink': bungkus('<image xlink:href="//x.test/a.png"/>'),
    'url() ke alamat luar': bungkus('<rect style="fill:url(https://x.test/g)"/>'),
  };

  for (const [nama, svg] of Object.entries(berbahaya)) {
    assert.ok(periksaSvg(svg).length > 0, `${nama} harus ditolak`);
  }
});

test('periksaSvg: wajib akar <svg> dan viewBox', () => {
  assert.match(periksaSvg('<html><body/></html>').join('\n'), /bukan SVG/);
  assert.match(periksaSvg('<svg><rect/></svg>').join('\n'), /wajib punya viewBox/);
  assert.match(periksaSvg('').join('\n'), /bukan SVG/);
});

// ---- pencocokan rujukan ------------------------------------------------------------------------

test('cocokkanRujukan: rujukan tanpa definisi dan definisi tanpa rujukan, dua-duanya galat', () => {
  const galat = [];
  cocokkanRujukan(galat, 'T', new Set(['a', 'b']), ['a', 'c']);

  assert.equal(galat.length, 2);
  assert.match(galat[0], /::media\[c\] tak punya definisi/);
  assert.match(galat[1], /media 'b' didefinisikan tetapi tak dirujuk/);

  const bersih = [];
  cocokkanRujukan(bersih, 'T', new Set(['a']), ['a', 'a']); // dirujuk dua kali boleh
  assert.deepEqual(bersih, []);
});
