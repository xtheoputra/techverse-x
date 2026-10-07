// Markdown terbatas TechVerse X — ADR-028 Tahap 3a.
//
// SATU implementasi dipakai dua pihak, dan itu alasan berkas ini ada tanpa pustaka:
//   - penampil web (`components/Markdown.tsx`) merender pohonnya jadi elemen React;
//   - pemeriksa berkas isi (`database/isi/pasang.mjs --cek`) memakai `problems`-nya.
// Apa yang lolos pemeriksa adalah apa yang tampil, oleh konstruksi — tak ada cermin
// yang bisa menyimpang. Berkas ini ES module tanpa dependensi supaya pemeriksa tetap
// jalan sebelum `npm ci`.
//
// 🔑 Dua sifat yang dijaga uji (`parse.test.mjs`):
//   1. TOTAL. `parse` menerima teks apa pun dan tak pernah melempar. Penampil tak
//      boleh jatuh karena satu baris ganjil di isi; yang menegur penulis adalah
//      `problems`, bukan galat runtime.
//   2. TAK ADA HTML. Keluarannya pohon, bukan string markup. Penampil memetakannya ke
//      elemen React, jadi teks isi tak bisa menyuntik markup. Satu-satunya jalur
//      berbahaya — skema URL tautan — dijaga di sini: `href` hanya http(s) absolut,
//      selain itu `null` dan penampil tak membuat tautan.
//
// Dialeknya sengaja kecil dan BUKAN CommonMark. Yang membedakannya, dengan alasan:
//   - `_` bukan penanda miring: isi teknis penuh `snake_case` dan `server_discover`.
//   - HTML mentah dan gambar `![]()` tidak didukung (dilaporkan, tak dilewatkan):
//     media hanya lewat blok bertipe supaya tiap gambar punya teks alternatif dan
//     sumber yang bisa diperiksa mesin.
//   - Judul hanya dua tingkat (`##`, `###`); `#` dan judul bagian dimiliki halaman.
//
// Alasan lengkap dan daftar yang ditolak: docs/adr/ADR-028 (Pembaruan 2026-10-07 (2)).

const MAKS_DAFTAR_BERSARANG = 2;

const RE_PAGAR = /^ {0,3}```(.*)$/;
const RE_PAGAR_TUTUP = /^ {0,3}```\s*$/;
const RE_JUDUL = /^ {0,3}(#{1,6})[ \t]+(.+?)[ \t]*$/;
const RE_KUTIPAN = /^ {0,3}>/;
const RE_GARIS = /^ {0,3}(?:-{3,}|\*{3,}|_{3,})[ \t]*$/;
const RE_ITEM = /^( {0,3})(?:(-)|(\d{1,9})\.)( +|$)(.*)$/;
const RE_MEDIA = /^::media\[([A-Za-z0-9][A-Za-z0-9-]*)\][ \t]*$/;
const RE_MEDIA_RUSAK = /^::media/;
const RE_PEMISAH_TABEL = /^ {0,3}\|?[ \t]*:?-+:?[ \t]*(?:\|[ \t]*:?-+:?[ \t]*)*\|?[ \t]*$/;
const RE_BAHASA = /^[A-Za-z0-9_+#.-]+$/;
const RE_URL = /^https?:\/\/[^\s<>"]+$/i;
const RE_PELOLOS = /[!-/:-@[-`{-~]/;
// Yang dihitung HTML mentah: tag utuh (`<b>`, `</b>`, `<br/>`, `<a href="x">`), komentar,
// deklarasi, dan <autolink>. "a<b" atau "3 < 4" di tengah kalimat BUKAN HTML dan tak
// dilaporkan; React pun menaruhnya sebagai teks biasa.
const RE_HTML = /^<(?:\/?[A-Za-z][A-Za-z0-9-]*(?:\s[^<>]*)?\/?>|!--|![A-Za-z]|\?|[A-Za-z][A-Za-z0-9+.-]*:[^\s<>]*>)/;

const JENIS_PERINGATAN = { CATATAN: 'catatan', TIPS: 'tips', PERINGATAN: 'peringatan' };

/**
 * @param {unknown} sumber
 * @returns {{ blocks: import('./parse.d.mts').Block[], problems: import('./parse.d.mts').Problem[] }}
 */
export function parse(sumber) {
  const teks = typeof sumber === 'string' ? sumber : '';
  const problems = [];
  const baris = teks
    .replace(/\r\n?/g, '\n')
    .split('\n')
    .map((t) => {
      // Tab di awal baris dibaca empat spasi (juga di dalam kode — hanya tampilan).
      const m = /^\t+/.exec(t);
      return m ? '    '.repeat(m[0].length) + t.slice(m[0].length) : t;
    })
    .map((t, i) => ({ t, n: i + 1 }));

  const blocks = parseBlocks(baris, { problems, daftar: 0, kutipan: false });
  return { blocks, problems };
}

// ---- Blok --------------------------------------------------------------------

const spasiAwal = (t) => t.length - t.trimStart().length;
const kosong = (t) => t.trim() === '';

/**
 * Apakah baris ini memutus paragraf yang sedang berjalan. Daftar hanya memutus bila
 * butirnya berisi, dan yang berurut hanya bila mulai dari 1 — "...pada tahun
 * 1986. Hari itu" tak boleh melahirkan daftar di tengah kalimat.
 */
function interrompeParagraf(t) {
  if (RE_PAGAR.test(t) || RE_JUDUL.test(t) || RE_KUTIPAN.test(t) || RE_GARIS.test(t) || RE_MEDIA_RUSAK.test(t)) return true;
  const m = RE_ITEM.exec(t);
  if (!m || m[5] === '') return false;
  return m[3] === undefined || Number(m[3]) === 1;
}

function parseBlocks(baris, ctx) {
  const out = [];
  let i = 0;

  while (i < baris.length) {
    const { t, n } = baris[i];

    if (kosong(t)) {
      i++;
      continue;
    }

    // Kode berpagar. Isinya apa adanya sampai pagar penutup.
    const pagar = RE_PAGAR.exec(t);
    if (pagar) {
      const info = pagar[1].trim();
      let bahasa = null;
      if (info === '') {
        ctx.problems.push({ line: n, message: 'kode berpagar tanpa bahasa — tulis ```bahasa (pakai ```text bila bukan kode).' });
      } else if (RE_BAHASA.test(info)) {
        bahasa = info;
      } else {
        ctx.problems.push({ line: n, message: `nama bahasa kode tak sah: '${info}'.` });
      }

      const isi = [];
      let j = i + 1;
      let tertutup = false;
      while (j < baris.length) {
        if (RE_PAGAR_TUTUP.test(baris[j].t)) {
          tertutup = true;
          break;
        }
        isi.push(baris[j].t);
        j++;
      }
      if (!tertutup) {
        ctx.problems.push({ line: n, message: 'kode berpagar tak tertutup — tambahkan ``` di baris sendiri.' });
      }
      out.push({ type: 'code', lang: bahasa, text: isi.join('\n') });
      i = tertutup ? j + 1 : j;
      continue;
    }

    // Tabel dicek sebelum garis pemisah: baris pemisah tabel bisa mirip `---`.
    const tabel = coba_tabel(baris, i, ctx);
    if (tabel) {
      out.push(tabel.node);
      i = tabel.berikut;
      continue;
    }

    if (RE_GARIS.test(t)) {
      ctx.problems.push({ line: n, message: 'garis pemisah (---) tidak didukung — pisahkan dengan sub-judul atau baris kosong.' });
      i++;
      continue;
    }

    const judul = RE_JUDUL.exec(t);
    if (judul) {
      const hash = judul[1].length;
      if (hash < 2 || hash > 3) {
        ctx.problems.push({
          line: n,
          message: `judul ${'#'.repeat(hash)} tidak didukung — pakai ## (sub-judul) atau ### (anak sub-judul); # dan judul bagian dimiliki halaman.`,
        });
      }
      out.push({ type: 'heading', level: hash <= 2 ? 2 : 3, children: parseInline(judul[2], n, ctx.problems) });
      i++;
      continue;
    }

    if (RE_KUTIPAN.test(t)) {
      const dalam = [];
      let j = i;
      while (j < baris.length && RE_KUTIPAN.test(baris[j].t)) {
        dalam.push({ t: baris[j].t.replace(/^ {0,3}> ?/, ''), n: baris[j].n });
        j++;
      }

      let jenis = null;
      const pembuka = /^\[!([A-Za-z]+)\]$/.exec(dalam[0].t.trim());
      if (pembuka) {
        jenis = JENIS_PERINGATAN[pembuka[1]] ?? null;
        if (jenis === null) {
          ctx.problems.push({
            line: n,
            message: `jenis peringatan [!${pembuka[1]}] tak dikenal — yang sah: ${Object.keys(JENIS_PERINGATAN).map((k) => `[!${k}]`).join(', ')}.`,
          });
        } else {
          dalam.shift();
        }
      }

      if (ctx.kutipan) {
        ctx.problems.push({ line: n, message: 'kutipan di dalam kutipan tidak didukung.' });
      }
      out.push({ type: 'quote', kind: jenis, children: parseBlocks(dalam, { ...ctx, kutipan: true }) });
      i = j;
      continue;
    }

    const media = RE_MEDIA.exec(t);
    if (media) {
      out.push({ type: 'media', key: media[1] });
      i++;
      continue;
    }
    if (RE_MEDIA_RUSAK.test(t)) {
      ctx.problems.push({ line: n, message: 'blok media harus ::media[kunci] sendirian di satu baris (kunci: huruf, angka, tanda hubung).' });
      // jatuh ke paragraf supaya teksnya tetap tampil
    }

    if (RE_ITEM.test(t)) {
      const daftar = parseDaftar(baris, i, ctx);
      out.push(daftar.node);
      i = daftar.berikut;
      continue;
    }

    // Paragraf: sampai baris kosong atau awal blok lain.
    const isi = [];
    let j = i;
    while (j < baris.length && !kosong(baris[j].t)) {
      if (j > i && interrompeParagraf(baris[j].t)) break;
      if (j > i && coba_tabel(baris, j, { problems: [] })) break;
      isi.push(baris[j].t.trim());
      j++;
    }
    out.push({ type: 'paragraph', children: parseInline(isi.join('\n'), n, ctx.problems) });
    i = j;
  }

  return out;
}

// Nama `coba_tabel` tetap bergaya Indonesia-campuran supaya terbaca sebagai kata kerja.
function coba_tabel(baris, i, ctx) {
  if (i + 1 >= baris.length) return null;
  const kepala = baris[i].t;
  const pemisah = baris[i + 1].t;
  if (!kepala.includes('|') || !pemisah.includes('|') || !RE_PEMISAH_TABEL.test(pemisah)) return null;

  const judulSel = bagiBaris(kepala);
  const sel = bagiBaris(pemisah);
  if (judulSel.length !== sel.length) {
    ctx.problems.push({ line: baris[i].n, message: `tabel: ${judulSel.length} kolom judul tetapi ${sel.length} kolom pemisah.` });
    return null;
  }

  const align = sel.map((s) => {
    const kiri = s.startsWith(':');
    const kanan = s.endsWith(':');
    return kiri && kanan ? 'center' : kanan ? 'right' : kiri ? 'left' : null;
  });

  const rows = [];
  let j = i + 2;
  while (j < baris.length && !kosong(baris[j].t) && baris[j].t.includes('|')) {
    let isi = bagiBaris(baris[j].t);
    if (isi.length !== judulSel.length) {
      ctx.problems.push({
        line: baris[j].n,
        message: `tabel: baris ini punya ${isi.length} sel, judulnya ${judulSel.length}.`,
      });
      isi = judulSel.map((_, k) => isi[k] ?? '');
    }
    rows.push(isi.map((s) => parseInline(s, baris[j].n, ctx.problems)));
    j++;
  }

  return {
    node: {
      type: 'table',
      align,
      head: judulSel.map((s) => parseInline(s, baris[i].n, ctx.problems)),
      rows,
    },
    berikut: j,
  };
}

/** Memecah satu baris tabel di `|` yang tak di-escape; `\|` menjadi `|` di dalam sel. */
function bagiBaris(t) {
  let s = t.trim();
  if (s.startsWith('|')) s = s.slice(1);
  if (s.endsWith('|') && !s.endsWith('\\|')) s = s.slice(0, -1);

  const sel = [];
  let cur = '';
  for (let k = 0; k < s.length; k++) {
    const c = s[k];
    if (c === '\\' && s[k + 1] === '|') {
      cur += '|';
      k++;
    } else if (c === '\\' && k + 1 < s.length) {
      cur += c + s[k + 1];
      k++;
    } else if (c === '|') {
      sel.push(cur.trim());
      cur = '';
    } else {
      cur += c;
    }
  }
  sel.push(cur.trim());
  return sel;
}

function parseDaftar(baris, mulai, ctx) {
  const pertama = RE_ITEM.exec(baris[mulai].t);
  const berurut = pertama[3] !== undefined;
  const awal = berurut ? Number(pertama[3]) : 1;
  const items = [];

  if (ctx.daftar >= MAKS_DAFTAR_BERSARANG) {
    ctx.problems.push({ line: baris[mulai].n, message: 'daftar bersarang lebih dari dua tingkat tidak didukung.' });
  }

  let i = mulai;
  while (i < baris.length) {
    const m = RE_ITEM.exec(baris[i].t);
    if (!m || berurut !== (m[3] !== undefined)) break;

    const ind = m[1].length;
    const panjangPenanda = berurut ? m[3].length + 1 : 1;
    const spasi = m[4].length;
    const celah = spasi >= 1 && spasi <= 4 && m[5] !== '' ? spasi : 1;
    const indentIsi = ind + panjangPenanda + celah;

    const isiBaris = [{ t: baris[i].t.slice(indentIsi), n: baris[i].n }];
    let j = i + 1;

    while (j < baris.length) {
      const L = baris[j];

      if (kosong(L.t)) {
        let k = j + 1;
        while (k < baris.length && kosong(baris[k].t)) k++;
        if (k < baris.length && spasiAwal(baris[k].t) >= indentIsi) {
          for (; j < k; j++) isiBaris.push({ t: '', n: baris[j].n });
          continue;
        }
        break;
      }

      const sa = spasiAwal(L.t);
      if (sa >= indentIsi) {
        isiBaris.push({ t: L.t.slice(indentIsi), n: L.n });
        j++;
        continue;
      }

      // Butir bersarang yang diketik kurang menjorok (dua spasi di bawah `1.`): tetap
      // milik butir ini, bukan daftar baru — kalau tidak, strukturnya berubah diam-diam.
      const m2 = RE_ITEM.exec(L.t);
      if (m2) {
        if (m2[1].length > ind) {
          isiBaris.push({ t: L.t.slice(sa), n: L.n });
          j++;
          continue;
        }
        break; // saudara (atau daftar lain): bukan lanjutan malas, apa pun nomornya
      }

      // Lanjutan malas: baris tak menjorok yang meneruskan kalimat butir ini.
      if (!kosong(isiBaris[isiBaris.length - 1].t) && !interrompeParagraf(L.t)) {
        isiBaris.push({ t: L.t.trim(), n: L.n });
        j++;
        continue;
      }

      break;
    }

    items.push(parseBlocks(isiBaris, { ...ctx, daftar: ctx.daftar + 1 }));
    i = j;
  }

  return { node: { type: 'list', ordered: berurut, start: awal, items }, berikut: i };
}

// ---- Inline ------------------------------------------------------------------

/**
 * @param {string} mentah teks dengan `\n` asli (nomor baris dihitung darinya)
 * @param {number} baris0 nomor baris karakter pertama
 * @param {Array} problems
 */
function parseInline(mentah, baris0, problems) {
  return rentang(mentah, 0, mentah.length, { s: mentah, baris0, problems, tautan: true });
}

const barisDi = (o, idx) => {
  let n = o.baris0;
  for (let k = 0; k < idx && k < o.s.length; k++) if (o.s[k] === '\n') n++;
  return n;
};

const spasi = (c) => c === undefined || /\s/.test(c);
const rapikan = (t) => t.replace(/[ \t]*\n[ \t]*/g, ' ');

/** Indeks sesudah penutup kode sebaris yang cocok dengan pembuka di `i`, atau null. */
function lewatiKode(s, i, akhir) {
  let n = 0;
  while (i + n < akhir && s[i + n] === '`') n++;
  let j = i + n;
  while (j < akhir) {
    if (s[j] === '`') {
      let m = 0;
      while (j + m < akhir && s[j + m] === '`') m++;
      if (m === n) return { akhir: j + m, panjang: n, isiDari: i + n, isiSampai: j };
      j += m;
    } else {
      j++;
    }
  }
  return null;
}

/** Indeks penutup penekanan (`*` atau `**`) mulai dari `dari`, atau -1. */
function cariPenutup(s, dari, akhir, panjang) {
  let j = dari;
  while (j < akhir) {
    const c = s[j];
    if (c === '\\') {
      j += 2;
      continue;
    }
    if (c === '`') {
      const k = lewatiKode(s, j, akhir);
      if (k) {
        j = k.akhir;
        continue;
      }
      while (j < akhir && s[j] === '`') j++;
      continue;
    }
    if (c === '*') {
      let k = 0;
      while (j + k < akhir && s[j + k] === '*') k++;
      const sebelumnyaSpasi = spasi(s[j - 1]);
      if (panjang === 1) {
        if (k === 1 && !sebelumnyaSpasi) return j;
        if (k >= 3 && !sebelumnyaSpasi) return j + k - 1; // *a **b*** : miring ditutup bintang terakhir
      } else if (k >= 2 && !sebelumnyaSpasi) {
        return j + k - 2; // **a *b*** : tebal ditutup dua bintang terakhir
      }
      j += k;
      continue;
    }
    j++;
  }
  return -1;
}

/** Mencari `]` yang cocok dengan `[` di `i`, memperhatikan bersarang, pelolos, dan kode. */
function cariKurungSiku(s, i, akhir) {
  let kedalaman = 0;
  for (let j = i; j < akhir; j++) {
    const c = s[j];
    if (c === '\\') {
      j++;
    } else if (c === '`') {
      const k = lewatiKode(s, j, akhir);
      if (k) j = k.akhir - 1;
    } else if (c === '[') {
      kedalaman++;
    } else if (c === ']') {
      kedalaman--;
      if (kedalaman === 0) return j;
    }
  }
  return -1;
}

/** `[teks](url)` yang bentuknya utuh di `i`; mengembalikan indeks-indeksnya atau null. */
function bentukTautan(s, i, akhir) {
  const r = cariKurungSiku(s, i, akhir);
  if (r < 0 || s[r + 1] !== '(') return null;

  let kedalaman = 0;
  for (let j = r + 1; j < akhir; j++) {
    const c = s[j];
    if (c === '(') kedalaman++;
    if (c === ')') {
      kedalaman--;
      if (kedalaman === 0) return { teksDari: i + 1, teksSampai: r, url: s.slice(r + 2, j), akhir: j + 1 };
    }
  }
  return null;
}

function rentang(s, dari, akhir, o) {
  const out = [];
  let buf = '';
  const kosongkan = () => {
    if (buf) {
      out.push({ type: 'text', text: rapikan(buf) });
      buf = '';
    }
  };

  let i = dari;
  while (i < akhir) {
    const c = s[i];

    if (c === '\\' && i + 1 < akhir && RE_PELOLOS.test(s[i + 1])) {
      buf += s[i + 1];
      i += 2;
      continue;
    }

    if (c === '`') {
      const k = lewatiKode(s, i, akhir);
      if (k) {
        kosongkan();
        let isi = rapikan(s.slice(k.isiDari, k.isiSampai));
        if (isi.length > 1 && isi.startsWith(' ') && isi.endsWith(' ') && isi.trim() !== '') isi = isi.slice(1, -1);
        out.push({ type: 'code', text: isi });
        i = k.akhir;
        continue;
      }
      let n = 0;
      while (i + n < akhir && s[i + n] === '`') n++;
      buf += s.slice(i, i + n);
      i += n;
      continue;
    }

    if (c === '*') {
      let k = 0;
      while (i + k < akhir && s[i + k] === '*') k++;
      const pembukaSah = i + k < akhir && !/\s/.test(s[i + k]);

      if (pembukaSah && k <= 2) {
        const tutup = cariPenutup(s, i + k, akhir, k);
        if (tutup > i + k) {
          kosongkan();
          out.push({ type: k === 2 ? 'strong' : 'em', children: rentang(s, i + k, tutup, o) });
          i = tutup + k;
          continue;
        }
      } else if (pembukaSah && k === 3) {
        // ***tebal-miring***
        const tutup = s.indexOf('***', i + 3);
        if (tutup > i + 3 && tutup < akhir && !spasi(s[tutup - 1])) {
          kosongkan();
          out.push({ type: 'em', children: [{ type: 'strong', children: rentang(s, i + 3, tutup, o) }] });
          i = tutup + 3;
          continue;
        }
      }

      buf += '*'.repeat(k);
      i += k;
      continue;
    }

    if (c === '!' && s[i + 1] === '[') {
      const t = bentukTautan(s, i + 1, akhir);
      if (t) {
        o.problems.push({ line: barisDi(o, i), message: 'gambar Markdown (tanda seru di depan tautan) tidak didukung — media hanya lewat blok ::media[kunci] (ADR-028).' });
        buf += s.slice(i, t.akhir);
        i = t.akhir;
        continue;
      }
    }

    if (c === '[') {
      const t = o.tautan ? bentukTautan(s, i, akhir) : null;
      if (t && t.teksSampai > t.teksDari) {
        kosongkan();
        const ok = RE_URL.test(t.url);
        if (!ok) {
          o.problems.push({
            line: barisDi(o, i),
            message: `tautan hanya boleh http(s) absolut, bukan '${t.url}'.`,
          });
        }
        out.push({
          type: 'link',
          href: ok ? t.url : null,
          children: rentang(s, t.teksDari, t.teksSampai, { ...o, tautan: false }),
        });
        i = t.akhir;
        continue;
      }
    }

    if (c === '<' && RE_HTML.test(s.slice(i, i + 300))) {
      o.problems.push({ line: barisDi(o, i), message: 'HTML mentah tidak didukung — tulis \\< bila memang maksudnya tanda kurang-dari.' });
    }

    buf += c;
    i++;
  }

  kosongkan();
  return out;
}

// ---- Pembantu untuk pemeriksa dan penampil -------------------------------------------

/** Kunci semua blok media di pohon, berurut kemunculan (boleh berulang). */
export function mediaKeys(blocks) {
  const hasil = [];
  const jalan = (daftar) => {
    for (const b of daftar) {
      if (b.type === 'media') hasil.push(b.key);
      else if (b.type === 'quote') jalan(b.children);
      else if (b.type === 'list') for (const item of b.items) jalan(item);
    }
  };
  jalan(blocks);
  return hasil;
}

/** Teks polos pohon, untuk menghitung kata dan keperluan serupa. */
export function plainText(blocks) {
  const inline = (daftar) =>
    daftar
      .map((n) => (n.type === 'text' || n.type === 'code' ? n.text : inline(n.children)))
      .join('');

  const blok = (b) => {
    switch (b.type) {
      case 'paragraph':
      case 'heading':
        return inline(b.children);
      case 'code':
        return b.text;
      case 'quote':
        return b.children.map(blok).join('\n');
      case 'list':
        return b.items.map((item) => item.map(blok).join('\n')).join('\n');
      case 'table':
        return [b.head, ...b.rows].map((baris) => baris.map(inline).join(' ')).join('\n');
      default:
        return '';
    }
  };

  return blocks.map(blok).join('\n\n');
}
