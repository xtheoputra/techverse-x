// Uji pengurai Markdown terbatas. Tanpa dependensi: `node --test apps/web/src/lib/markdown/`.
//
// Dua sifat yang dijaga paling keras (lihat kepala parse.mjs): pengurai TOTAL (tak pernah
// melempar) dan keluarannya tak pernah membawa tautan selain http(s) — uji acak di bawah
// menjaga keduanya pada ribuan masukan ganjil, bukan hanya pada contoh yang ditulis tangan.

import assert from 'node:assert/strict';
import { test } from 'node:test';

import { mediaKeys, parse, plainText } from './parse.mjs';

const blok = (s) => parse(s).blocks;
const masalah = (s) => parse(s).problems;
const teks = (text) => ({ type: 'text', text });

// ---- paragraf dan inline ----------------------------------------------------------

test('baris tunggal digabung dengan spasi, baris kosong memisahkan paragraf', () => {
  assert.deepEqual(blok('satu\ndua\n\ntiga'), [
    { type: 'paragraph', children: [teks('satu dua')] },
    { type: 'paragraph', children: [teks('tiga')] },
  ]);
});

test('teks polos tanpa tanda apa pun lolos tanpa masalah — isi lama tetap sah', () => {
  const r = parse('Dasar JSON-RPC 2.0 dan JSON Schema. Pahami request, response, dan notification.');
  assert.deepEqual(r.problems, []);
  assert.equal(r.blocks.length, 1);
});

test('tebal, miring, kode sebaris, dan tautan', () => {
  assert.deepEqual(blok('a **b** *c* `d` [e](https://x.test/p)')[0].children, [
    teks('a '),
    { type: 'strong', children: [teks('b')] },
    teks(' '),
    { type: 'em', children: [teks('c')] },
    teks(' '),
    { type: 'code', text: 'd' },
    teks(' '),
    { type: 'link', href: 'https://x.test/p', children: [teks('e')] },
  ]);
});

test('penekanan bersarang: miring di dalam tebal, dan ***tebal-miring***', () => {
  assert.deepEqual(blok('**a *b* c**')[0].children, [
    { type: 'strong', children: [teks('a '), { type: 'em', children: [teks('b')] }, teks(' c')] },
  ]);
  assert.deepEqual(blok('***x***')[0].children, [{ type: 'em', children: [{ type: 'strong', children: [teks('x')] }] }]);
  assert.deepEqual(blok('*a **b***')[0].children, [{ type: 'em', children: [teks('a '), { type: 'strong', children: [teks('b')] }] }]);
});

test('underscore BUKAN penanda miring: snake_case dan _meta utuh', () => {
  assert.deepEqual(blok('pakai server_discover dan _meta lalu _miring_')[0].children, [
    teks('pakai server_discover dan _meta lalu _miring_'),
  ]);
});

test('bintang yang tak berpasangan atau dikelilingi spasi tetap huruf biasa', () => {
  assert.deepEqual(blok('2 * 3 * 4')[0].children, [teks('2 * 3 * 4')]);
  assert.deepEqual(blok('a ** b')[0].children, [teks('a ** b')]);
  assert.deepEqual(blok('*tak ditutup')[0].children, [teks('*tak ditutup')]);
  assert.deepEqual(blok('x ****')[0].children, [teks('x ****')]);

  // Baris yang hanya bintang adalah garis pemisah (dilaporkan, dibuang) — bukan teks.
  assert.deepEqual(blok('****'), []);
});

test('pelolos membuat tanda jadi huruf biasa', () => {
  assert.deepEqual(blok('\\*bukan miring\\* dan \\[bukan tautan\\](https://x.test) dan \\\\')[0].children, [
    teks('*bukan miring* dan [bukan tautan](https://x.test) dan \\'),
  ]);
  assert.deepEqual(blok('a\\b')[0].children, [teks('a\\b')]);
});

test('kode sebaris: isinya apa adanya, pagar ganda boleh memuat satu tanda kutip-balik', () => {
  assert.deepEqual(blok('`a *b* [c](https://x.test)`')[0].children, [{ type: 'code', text: 'a *b* [c](https://x.test)' }]);
  assert.deepEqual(blok('`` a`b ``')[0].children, [{ type: 'code', text: 'a`b' }]);
  assert.deepEqual(blok('`tak ditutup')[0].children, [teks('`tak ditutup')]);
});

test('tebal tidak menembus kode sebaris yang berisi bintang', () => {
  assert.deepEqual(blok('**a `**` b**')[0].children, [
    { type: 'strong', children: [teks('a '), { type: 'code', text: '**' }, teks(' b')] },
  ]);
});

// ---- tautan: satu-satunya jalur berbahaya --------------------------------------------

test('tautan non-http(s) menjadi href null dan dilaporkan', () => {
  for (const url of ['javascript:alert(1)', 'data:text/html,x', 'JAVASCRIPT:alert(1)', '/relatif', '//x.test', 'ftp://x.test', 'mailto:a@b.test']) {
    const r = parse(`[klik](${url})`);
    const tautan = r.blocks[0].children[0];
    assert.equal(tautan.type, 'link', url);
    assert.equal(tautan.href, null, url);
    assert.equal(r.problems.length, 1, url);
    assert.match(r.problems[0].message, /http\(s\)/);
  }
});

test('tautan http dan https sah, tanpa masalah', () => {
  assert.equal(parse('[a](http://x.test)').problems.length, 0);
  assert.equal(parse('[a](HTTPS://x.test/a_b?c=1&d=2#e)').blocks[0].children[0].href, 'HTTPS://x.test/a_b?c=1&d=2#e');
});

test('tanda kurung seimbang di dalam URL diterima; spasi di dalam URL menolak', () => {
  assert.equal(parse('[a](https://x.test/a_(b))').blocks[0].children[0].href, 'https://x.test/a_(b)');
  assert.equal(parse('[a](https://x.test/a b)').blocks[0].children[0].href, null);
});

test('teks tautan boleh berformat, tetapi tautan tak bersarang', () => {
  assert.deepEqual(blok('[**tebal** dan `kode`](https://x.test)')[0].children[0].children, [
    { type: 'strong', children: [teks('tebal')] },
    teks(' dan '),
    { type: 'code', text: 'kode' },
  ]);
  const dalam = blok('[a [b](https://dalam.test) c](https://luar.test)')[0].children[0];
  assert.equal(dalam.href, 'https://luar.test');
  assert.deepEqual(dalam.children, [teks('a [b](https://dalam.test) c')]);
});

test('kurung siku tanpa (url) tetap huruf biasa, tautan kosong tak jadi tautan', () => {
  assert.deepEqual(blok('daftar [a] dan [](https://x.test)')[0].children, [teks('daftar [a] dan [](https://x.test)')]);
});

test('gambar Markdown dilaporkan dan tak menjadi tautan', () => {
  const r = parse('lihat ![diagram](https://x.test/a.png) ini');
  assert.equal(r.problems.length, 1);
  assert.match(r.problems[0].message, /gambar Markdown/);
  assert.deepEqual(r.blocks[0].children, [teks('lihat ![diagram](https://x.test/a.png) ini')]);
});

test('HTML mentah dilaporkan tetapi tak pernah menjadi markup; kurang-dari biasa lolos', () => {
  const r = parse('a <script>alert(1)</script> b');
  assert.equal(r.problems.length, 2);
  assert.deepEqual(r.blocks[0].children, [teks('a <script>alert(1)</script> b')]);

  assert.deepEqual(masalah('3 < 4 dan a<b'), []);
  assert.deepEqual(masalah('`<div>` di dalam kode sah'), []);
  assert.equal(masalah('<https://x.test>').length, 1);
});

// ---- judul ---------------------------------------------------------------------

test('judul ## dan ### sah; # dan #### dilaporkan tetapi tetap tampil', () => {
  assert.deepEqual(blok('## Sub\n\n### Anak'), [
    { type: 'heading', level: 2, children: [teks('Sub')] },
    { type: 'heading', level: 3, children: [teks('Anak')] },
  ]);
  assert.deepEqual(masalah('## a\n### b'), []);

  const satu = parse('# Besar');
  assert.equal(satu.problems.length, 1);
  assert.equal(satu.blocks[0].level, 2);

  const empat = parse('#### Dalam');
  assert.equal(empat.problems.length, 1);
  assert.equal(empat.blocks[0].level, 3);
});

test('# tanpa spasi bukan judul (tagar di tengah isi)', () => {
  assert.deepEqual(blok('#belajar bareng')[0].children, [teks('#belajar bareng')]);
});

test('judul boleh memuat kode sebaris', () => {
  assert.deepEqual(blok('## Memakai `initialize`')[0].children, [teks('Memakai '), { type: 'code', text: 'initialize' }]);
});

// ---- kode berpagar -------------------------------------------------------------------

test('kode berpagar: isi apa adanya, tak diurai, bahasa tercatat', () => {
  const r = parse('```json\n{ "a": [1, 2], "#": "- x" }\n\n  indent  \n```');
  assert.deepEqual(r.problems, []);
  assert.deepEqual(r.blocks, [{ type: 'code', lang: 'json', text: '{ "a": [1, 2], "#": "- x" }\n\n  indent  ' }]);
});

test('kode berpagar tanpa bahasa atau tak tertutup dilaporkan, isi tetap tampil', () => {
  const tanpa = parse('```\nx\n```');
  assert.equal(tanpa.problems.length, 1);
  assert.equal(tanpa.blocks[0].lang, null);

  const terbuka = parse('```ts\nconst a = 1;\nsisa teks');
  assert.equal(terbuka.problems.length, 1);
  assert.match(terbuka.problems[0].message, /tak tertutup/);
  assert.equal(terbuka.blocks[0].text, 'const a = 1;\nsisa teks');

  assert.equal(parse('```c++ ok\n```').problems.length, 1);
});

test('tab di awal baris dibaca empat spasi dan tak dilaporkan (kode Go sah memakai tab)', () => {
  const r = parse('```go\n\tfmt.Println()\n```');
  assert.deepEqual(r.problems, []);
  assert.equal(r.blocks[0].text, '    fmt.Println()');
});

// ---- daftar --------------------------------------------------------------------------

const paragrafDari = (item) => item.map((b) => (b.type === 'paragraph' ? b.children.map((c) => c.text ?? '').join('') : b.type));

test('daftar butir dan daftar berurut, termasuk nomor awal bukan 1', () => {
  const [a, b] = blok('- satu\n- dua\n\n3. tiga\n4. empat');
  assert.equal(a.ordered, false);
  assert.deepEqual(a.items.map(paragrafDari), [['satu'], ['dua']]);
  assert.equal(b.ordered, true);
  assert.equal(b.start, 3);
  assert.deepEqual(b.items.map(paragrafDari), [['tiga'], ['empat']]);
});

test('butir berurut bersaudara tidak tertelan jadi lanjutan malas', () => {
  const [d] = blok('1. a\n2. b\n3. c');
  assert.equal(d.items.length, 3);
  assert.deepEqual(d.items.map(paragrafDari), [['a'], ['b'], ['c']]);
});

test('daftar bersarang dua spasi di bawah "-" dan juga di bawah "1." (kurang menjorok tetap bersarang)', () => {
  const [d] = blok('- induk\n  - anak\n  - anak dua\n- saudara');
  assert.equal(d.items.length, 2);
  assert.equal(d.items[0][1].type, 'list');
  assert.equal(d.items[0][1].items.length, 2);

  const [e] = blok('1. induk\n  - anak\n2. kedua');
  assert.equal(e.items.length, 2);
  assert.equal(e.items[0][1].type, 'list');
  assert.equal(e.items[0][1].ordered, false);
});

test('lanjutan malas dan butir berparagraf banyak', () => {
  const [d] = blok('- kalimat yang\nditeruskan di baris berikut\n- dua');
  assert.deepEqual(d.items.map(paragrafDari), [['kalimat yang diteruskan di baris berikut'], ['dua']]);

  const [e] = blok('- paragraf satu\n\n  paragraf dua\n- tiga');
  assert.equal(e.items.length, 2);
  assert.deepEqual(e.items[0].map((b) => b.type), ['paragraph', 'paragraph']);
});

test('kode berpagar di dalam butir daftar', () => {
  const [d] = blok('1. jalankan:\n\n   ```bash\n   npm test\n\n   npm run lint\n   ```\n2. selesai');
  assert.equal(d.items.length, 2);
  assert.equal(d.items[0][1].type, 'code');
  assert.equal(d.items[0][1].text, 'npm test\n\nnpm run lint');
});

test('daftar lebih dari dua tingkat dilaporkan, tetap tampil', () => {
  const r = parse('- a\n  - b\n    - c');
  assert.equal(r.problems.length, 1);
  assert.match(r.problems[0].message, /dua tingkat/);
  assert.equal(masalah('- a\n  - b').length, 0);
});

test('angka dengan titik di tengah kalimat tidak melahirkan daftar', () => {
  assert.deepEqual(blok('Dirilis pada tahun\n1986. Hari itu cerah.'), [
    { type: 'paragraph', children: [teks('Dirilis pada tahun 1986. Hari itu cerah.')] },
  ]);
  // Tetapi "1." memang boleh memutus paragraf, dan "-" juga.
  assert.equal(blok('pembuka\n1. satu')[1].type, 'list');
  assert.equal(blok('pembuka\n- satu')[1].type, 'list');
});

test('bintang di awal baris BUKAN penanda daftar — tampil sebagai teks', () => {
  assert.equal(blok('* satu')[0].type, 'paragraph');
});

// ---- kutipan dan peringatan --------------------------------------------------------

test('kutipan biasa dan tiga jenis peringatan', () => {
  assert.deepEqual(blok('> kutipan\n> dua baris'), [
    { type: 'quote', kind: null, children: [{ type: 'paragraph', children: [teks('kutipan dua baris')] }] },
  ]);

  for (const [kunci, jenis] of [['CATATAN', 'catatan'], ['TIPS', 'tips'], ['PERINGATAN', 'peringatan']]) {
    const r = parse(`> [!${kunci}]\n> isi **penting**`);
    assert.deepEqual(r.problems, []);
    assert.equal(r.blocks[0].kind, jenis);
    assert.equal(r.blocks[0].children[0].type, 'paragraph');
  }
});

test('jenis peringatan tak dikenal dilaporkan dan penandanya tetap tampil', () => {
  const r = parse('> [!CATATAN-X]\n> isi');
  assert.equal(r.blocks[0].kind, null);
  const tak = parse('> [!AWAS]\n> isi');
  assert.equal(tak.problems.length, 1);
  assert.equal(tak.blocks[0].kind, null);
});

test('kutipan bersarang dilaporkan; peringatan boleh memuat daftar dan kode', () => {
  assert.equal(parse('> a\n> > b').problems.length, 1);
  const r = parse('> [!PERINGATAN]\n> - satu\n> - dua\n>\n> ```bash\n> rm -rf x\n> ```');
  assert.deepEqual(r.problems, []);
  assert.deepEqual(r.blocks[0].children.map((b) => b.type), ['list', 'code']);
});

// ---- tabel -----------------------------------------------------------------------------

test('tabel dengan perataan, sel berformat, dan pipa yang di-escape', () => {
  const r = parse('| Nama | Nilai | Ket |\n|:--|--:|:-:|\n| `a` | 1 | **x** |\n| b\\|c | 2 | y |');
  assert.deepEqual(r.problems, []);
  const t = r.blocks[0];
  assert.equal(t.type, 'table');
  assert.deepEqual(t.align, ['left', 'right', 'center']);
  assert.deepEqual(t.head.map((c) => c[0].text), ['Nama', 'Nilai', 'Ket']);
  assert.equal(t.rows.length, 2);
  assert.deepEqual(t.rows[0][0], [{ type: 'code', text: 'a' }]);
  assert.deepEqual(t.rows[1][0], [teks('b|c')]);
});

test('tabel tanpa pipa di tepi dan tabel yang memutus paragraf', () => {
  const r = parse('pembuka\na | b\n--|--\n1 | 2');
  assert.deepEqual(r.blocks.map((b) => b.type), ['paragraph', 'table']);
  assert.deepEqual(r.blocks[1].rows[0].map((c) => c[0].text), ['1', '2']);
});

test('jumlah kolom yang tak cocok dilaporkan; baris dipadatkan, bukan dibuang', () => {
  const r = parse('| a | b |\n|---|---|\n| 1 | 2 | 3 |\n| 4 |');
  assert.equal(r.problems.length, 2);
  assert.deepEqual(r.blocks[0].rows.map((b) => b.length), [2, 2]);

  const tak = parse('| a | b |\n|---|');
  assert.equal(tak.problems.length, 1);
  assert.equal(tak.blocks[0].type, 'paragraph');
});

// ---- garis pemisah dan blok media -------------------------------------------------------

test('garis pemisah dilaporkan dan dibuang', () => {
  const r = parse('a\n\n---\n\nb');
  assert.equal(r.problems.length, 1);
  assert.deepEqual(r.blocks.map((b) => b.type), ['paragraph', 'paragraph']);
});

test('blok media: kunci sah, kunci rusak dilaporkan dan teksnya tetap tampil', () => {
  assert.deepEqual(blok('::media[arsitektur-mcp]'), [{ type: 'media', key: 'arsitektur-mcp' }]);

  for (const rusak of ['::media[]', '::media[a b]', '::media[a] tambahan', '::media arsitektur', '::media[_a]']) {
    const r = parse(rusak);
    assert.equal(r.problems.length, 1, rusak);
    assert.equal(r.blocks[0].type, 'paragraph', rusak);
  }
});

test('mediaKeys menemukan blok media di dalam kutipan dan daftar', () => {
  const r = parse('::media[a]\n\n> ::media[b]\n\n- ::media[c]\n- teks');
  assert.deepEqual(mediaKeys(r.blocks), ['a', 'b', 'c']);
});

// ---- nomor baris laporan -----------------------------------------------------------------

test('laporan memuat nomor baris ASLI, juga di dalam daftar, kutipan, tabel, dan paragraf panjang', () => {
  assert.equal(parse('a\nb\n\n## ok\n\n#### salah').problems[0].line, 6);
  assert.equal(parse('x\n\n- satu\n- dua <b>\n').problems[0].line, 4);
  assert.equal(parse('x\n\n> a\n> ![i](https://x.test/i.png)').problems[0].line, 4);
  assert.equal(parse('baris satu\nbaris dua\nbaris tiga <i>x</i> empat').problems[0].line, 3);
  assert.equal(parse('| a |\n|---|\n| [x](ftp://x) |').problems[0].line, 3);
});

// ---- plainText ----------------------------------------------------------------------------

test('plainText membuang tanda dan menyisakan kata untuk penghitung', () => {
  const { blocks } = parse('## Judul\n\nIsi **tebal** `kode`.\n\n- satu\n- dua\n\n```js\nx()\n```');
  const kata = plainText(blocks).split(/\s+/).filter(Boolean);
  assert.deepEqual(kata, ['Judul', 'Isi', 'tebal', 'kode.', 'satu', 'dua', 'x()']);
});

// ---- SIFAT KUNCI: total dan aman -------------------------------------------------------------

/** Generator acak deterministik (mulberry32) supaya kegagalan bisa diulang dari benihnya. */
function acak(benih) {
  let a = benih >>> 0;
  return () => {
    a = (a + 0x6d2b79f5) >>> 0;
    let t = a;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

const POTONGAN = [
  '*', '**', '***', '_', '`', '``', '```', '```js', '[', ']', '(', ')', '[x](', '](https://a.test)', '![', '<', '<b>', '</', '\\',
  '|', '|---|', ':--', '> ', '>', '- ', '-', '1. ', '2.', '  ', '    ', '\t', '\n', '\n\n', '\r\n', '#', '## ', '#### ',
  '---', '::media[k]', '::media[', '[!PERINGATAN]', '[!AWAS]', 'kata', 'snake_case', 'javascript:', 'https://x.test', '&', '"', "'", '🙂',
];

function rakit(rng, panjang) {
  let s = '';
  for (let i = 0; i < panjang; i++) s += POTONGAN[Math.floor(rng() * POTONGAN.length)];
  return s;
}

/** Pohon harus utuh: hanya jenis yang dikenal, dan setiap href hanya http(s) atau null. */
function periksaPohon(blocks, masukan) {
  const inline = (daftar) => {
    assert.ok(Array.isArray(daftar), 'children inline harus larik');
    for (const n of daftar) {
      switch (n.type) {
        case 'text':
        case 'code':
          assert.equal(typeof n.text, 'string');
          break;
        case 'strong':
        case 'em':
          inline(n.children);
          break;
        case 'link':
          assert.ok(n.href === null || /^https?:\/\//i.test(n.href), `href berbahaya ${JSON.stringify(n.href)} dari ${JSON.stringify(masukan)}`);
          inline(n.children);
          break;
        default:
          assert.fail(`jenis inline tak dikenal: ${n.type}`);
      }
    }
  };

  const satu = (b) => {
    switch (b.type) {
      case 'paragraph':
      case 'heading':
        inline(b.children);
        break;
      case 'code':
        assert.equal(typeof b.text, 'string');
        break;
      case 'quote':
        b.children.forEach(satu);
        break;
      case 'list':
        assert.ok(Array.isArray(b.items));
        b.items.forEach((item) => item.forEach(satu));
        break;
      case 'table':
        b.head.forEach(inline);
        b.rows.forEach((baris) => {
          assert.equal(baris.length, b.head.length, 'baris tabel harus selebar judulnya');
          baris.forEach(inline);
        });
        break;
      case 'media':
        assert.match(b.key, /^[A-Za-z0-9][A-Za-z0-9-]*$/);
        break;
      default:
        assert.fail(`jenis blok tak dikenal: ${b.type}`);
    }
  };

  blocks.forEach(satu);
}

test('TOTAL dan AMAN: 4000 masukan acak tak pernah melempar dan tak pernah membawa href berbahaya', () => {
  const rng = acak(20261007);
  for (let i = 0; i < 4000; i++) {
    const masukan = rakit(rng, 1 + Math.floor(rng() * 40));
    let hasil;
    assert.doesNotThrow(() => {
      hasil = parse(masukan);
    }, `melempar pada ${JSON.stringify(masukan)}`);
    periksaPohon(hasil.blocks, masukan);
    for (const m of hasil.problems) {
      assert.ok(Number.isInteger(m.line) && m.line >= 1, `nomor baris tak sah pada ${JSON.stringify(masukan)}`);
      assert.equal(typeof m.message, 'string');
    }
  }
});

test('masukan bukan string tak melempar', () => {
  for (const x of [undefined, null, 42, {}, [], true]) {
    assert.deepEqual(parse(x), { blocks: [], problems: [] });
  }
});

test('masukan besar dan bersarang dalam selesai dalam waktu wajar (tanpa ledakan eksponensial)', () => {
  const awal = Date.now();
  parse('*'.repeat(5000));
  parse('['.repeat(2000) + ']'.repeat(2000));
  parse('**a *b '.repeat(800));
  parse('`'.repeat(3000));
  parse(Array.from({ length: 300 }, (_, i) => `${'  '.repeat(i % 4)}- butir ${i}`).join('\n'));
  parse(`${'> '.repeat(200)}x`);
  assert.ok(Date.now() - awal < 5000, `terlalu lama: ${Date.now() - awal} ms`);
});
