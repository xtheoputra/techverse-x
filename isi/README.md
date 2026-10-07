# isi/ — isi sungguhan halaman teknologi

Satu berkas JSON per topik: `isi/<bidang>/<slug>.json`. Berkas inilah **sumber** isi;
Neon hanya proyeksinya. Keputusannya dan alasannya ada di
[ADR-027](../docs/adr/ADR-027-jalan-isi-sungguhan.md).

## Bentuk berkas

Sama dengan muatan API. Kunci yang tak dikenal ditolak (salah ketik `resorces` tidak
boleh lolos jadi "tak ada sumber").

| Kunci | Isi | Batas |
|---|---|---|
| `slug` | sama dengan nama berkas | 160 |
| `name` | nama topik | 200 |
| `fieldSlug` | sama dengan nama folder (salah satu bidang [ADR-010](../docs/adr/ADR-010-taksonomi-bidang.md)) | |
| `summary` | **Overview** (bagian 1), pembuka singkat — teks polos; ia juga keterangan kartu dan hasil pencarian | 2000 |
| `overview` | *(boleh dihilangkan)* pendalaman Overview, **Markdown terbatas**, boleh memuat `::media[kunci]`. Tampil di bawah `summary` | 20000 |
| `media` | *(boleh dihilangkan)* daftar gambar dan video yang dirujuk `::media[kunci]` di teks — lihat [Media](#media) | 40 butir |
| `prerequisite` | `{title, description}` — **langkah 0** roadmap ([ADR-012](../docs/adr/ADR-012-template-halaman.md)). `description` **Markdown terbatas** | 200 / 2000 |
| `roadmap` | daftar `{title, description}`, minimal 1, berurut. `description` **Markdown terbatas** | 200 / 2000 |
| `tools` | daftar `{slug, name, summary, homepage, note}`, minimal 1. Satu slug = satu definisi di **seluruh** `isi/` | |
| `projects` | daftar `{title, brief}`, minimal 1. `brief` **Markdown terbatas** | 200 / 4000 |
| `resources` | daftar `{type, title, url}`, minimal 1. `type` persis salah satu `OfficialDocs`, `Video`, `Paper`, `Repository`. URL http/https absolut dan unik | 300 / 1000 |
| `requires` | daftar slug topik prasyarat; hanya yang punya berkas di `isi/`, tanpa siklus | |

Contoh lengkap: [`ai-agents/model-context-protocol.json`](ai-agents/model-context-protocol.json).

## Format teks: Markdown terbatas

Empat medan — `overview`, `description` langkah (termasuk prasyarat), dan `brief` proyek — memakai **Markdown
terbatas** ([ADR-028](../docs/adr/ADR-028-antarmuka-futuristik-dan-isi-kaya.md), Tahap 3a dan 3b).
Teks polos tanpa tanda apa pun tetap sah dan tampil seperti biasa; isi yang ada tak perlu diubah.
`summary` **tetap teks polos**: ia juga keterangan kartu dan hasil pencarian, yang tak merender tanda.
Itu sebabnya pendalaman Overview punya medan sendiri (`overview`), bukan `summary` yang diperpanjang.

Dialeknya sengaja kecil dan **bukan CommonMark**. Pemeriksa (`.\run.ps1 isi`) memakai pengurai yang
sama dengan penampil di situs, jadi yang lolos di sini adalah yang tampil, dan yang tak didukung
ditolak **dengan nomor barisnya**.

| Tulis | Hasil |
|---|---|
| baris kosong | memisahkan paragraf (baris tunggal digabung) |
| `## Sub-judul`, `### Anak sub-judul` | dua tingkat saja; tingkat HTML mengikuti halaman |
| `- butir` dan `1. butir` | daftar; bersarang sampai dua tingkat (dua spasi) |
| ```` ```bahasa ```` … ```` ``` ```` | blok kode; **bahasa wajib** (`text` bila bukan kode) |
| `> kutipan` | kutipan |
| `> [!CATATAN]`, `> [!TIPS]`, `> [!PERINGATAN]` | kotak berlabel; isinya boleh daftar atau kode |
| `\| a \| b \|` + `\|---\|---\|` | tabel; kolom tiap baris harus sama; `:--`, `:-:`, `--:` meratakan |
| `**tebal**`, `*miring*`, `` `kode` `` | penekanan dan kode sebaris |
| `[teks](https://…)` | tautan; **hanya http(s) absolut** |
| `::media[kunci]` | gambar atau video, **sendirian di satu baris** — lihat [Media](#media) |
| `\*` | tanda yang ditulis apa adanya |

**Yang ditolak, dan kenapa:**

- **`_miring_` bukan miring.** Isi teknis penuh `snake_case` dan `server_discover`; garis bawah selalu huruf biasa.
  Pakai `*miring*`.
- **Gambar `![](…)` dan HTML mentah.** Media hanya lewat blok bertipe `::media[kunci]` (bagian [Media](#media)),
  supaya tiap gambar punya teks alternatif dan sumber yang bisa diperiksa mesin. Untuk tanda kurang-dari biasa (`a < b`) tak perlu apa-apa;
  yang ditolak hanya yang berbentuk tag (`<b>`, `<br/>`, `<https://…>`) — bungkus dengan kode sebaris bila memang
  maksudnya menulis tag.
- **Judul `#` dan `####`, garis pemisah `---`, daftar `*`, tautan non-http(s), kode berpagar tanpa bahasa atau tak tertutup,
  daftar lebih dari dua tingkat, kutipan bersarang, tabel berkolom tak rata.**
- Tab di awal baris dibaca empat spasi (juga di dalam kode); tulis dengan spasi.

`.\run.ps1 isi` juga mencetak **jumlah kata** tiap topik. Itu ukuran kasar kedalaman isi — pilot MCP saat ini
± 760 kata di bagian yang diperiksa, dan keluhan *"kurang detail"* belum terjawab sampai Tahap 4 menulis ulangnya.

## Media

Gambar, diagram, dan video ([ADR-028](../docs/adr/ADR-028-antarmuka-futuristik-dan-isi-kaya.md), Tahap 3b)
**didefinisikan sekali** di daftar `media`, lalu **ditaruh di teks** dengan `::media[kunci]` sendirian di satu
baris — di `overview`, `description` langkah, atau `brief` proyek. Satu media boleh dirujuk berkali-kali.

```json
"overview": "## Tiga peran\n\nHost memegang client.\n\n::media[arsitektur]\n\nLalu server menjawab.",
"media": [
  {
    "key": "arsitektur",
    "kind": "Image",
    "url": "/media/model-context-protocol/arsitektur.svg",
    "alt": "Tiga kotak, Host, Client, dan Server, dihubungkan panah dari kiri ke kanan.",
    "caption": "Host memegang client; setiap client berbicara dengan satu server.",
    "license": "Karya sendiri"
  }
]
```

| Kunci | Gambar (`Image`) | Video (`Video`) |
|---|---|---|
| `key` | slug (huruf kecil, angka, tanda hubung), unik per topik, ≤ 80 | sama |
| `kind` | persis `Image` | persis `Video` |
| `url` | **wajib**, berkas sendiri `/media/<slug-topik>/nama.svg` (juga `png`, `jpg`, `jpeg`, `webp`, `avif`) | **tidak boleh ada** |
| `videoId` | tidak boleh ada | **wajib**, ID YouTube 11 karakter |
| `alt` | **wajib**, teks alternatif (≤ 500) | **wajib**, judul bingkai (≤ 500) |
| `caption` | boleh, keterangan di bawah gambar (≤ 1000) | sama |
| `license` | **wajib** (≤ 200) | **wajib** |
| `sourceName`, `sourceUrl` | **wajib** kecuali `license` persis `Karya sendiri`; keduanya sekaligus, `sourceUrl` http(s) | sama |

**Yang ditegakkan pemeriksa** (`.\run.ps1 isi`, lalu diulang oleh API dan oleh `CHECK` di basis data):

- **Gambar hanya berkas sendiri.** Letaknya `apps/web/public/media/<slug-topik>/…` dan alamatnya `/media/<slug-topik>/…`;
  gambar hotlink ke situs lain ditolak, begitu pula berkas yang tak ada. Batas ukuran: SVG 300 KB, raster 800 KB.
- **SVG berdiri sendiri.** Wajib punya `viewBox` (tanpanya ia meluber di ponsel). Ditolak bila memuat `<script>`,
  `<foreignObject>`, elemen tersemat, atribut `on…=`, `javascript:`, `DOCTYPE`/`ENTITY`, `@import`, atau rujukan ke
  alamat luar. Situs menyajikannya lewat `<img>` (skrip tak jalan di sana), tetapi SVG juga punya alamat sendiri.
- **Lisensi dan sumber selalu ikut tampil** di bawah gambar. `Karya sendiri` ditulis persis begitu (server membakukannya).
- **Dua arah, dua-duanya galat:** `::media[x]` tanpa definisi `x`, dan definisi tanpa satu rujukan pun. Pesannya menyebut kuncinya.
- **Paling banyak 40 media per topik.**

**Yang tidak bisa diperiksa mesin ini, dan jadi tugas penulis:** apakah **video itu benar-benar ada dan bisa diputar
di situs**. Pemeriksa sengaja tanpa jaringan, jadi dua langkah ini dikerjakan saat menulis:

1. **Ada dan publik.** Buka `https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v=<ID>&format=json`: status 200
   dan `title` yang cocok dengan yang dijanjikan `alt`. (ID ngawur menjawab 404.)
2. **Bisa disematkan — lihat bingkainya di halaman https yang sebenarnya.** Status 200 **tidak cukup**: diukur di sini,
   satu video resmi menjawab 200 di oEmbed dan tampil normal dari halaman https, tetapi menampilkan
   *"This video is unavailable"* ketika bingkai yang sama dimuat dari `http://` di IP LAN, sedangkan video lain
   tampil di kedua tempat. Pemilik video bisa membatasi penyematan menurut asal halaman, dan oEmbed tak mengatakannya.
   Karena itu video baru dinyatakan beres setelah dilihat di pratinjau atau produksi https, bukan di mesin pengembangan.

Video disematkan lewat `youtube-nocookie.com`; atribut `sandbox` di bingkai **bukan** penyebab video tak tampil (diuji:
hasilnya sama dengan dan tanpa `sandbox`).

Setiap video selalu disertai tautan **Tonton di YouTube** di bawah bingkainya — untuk pembaca yang memblokir bingkai,
memakai pembaca layar, atau menemui video yang penyematannya dibatasi.

## Aturan isi — bukan sekadar bentuk

- **Setiap klaim harus bisa ditelusuri ke sumber primer yang dibaca saat menulis**, bukan
  diingat ([ADR-012 §5](../docs/adr/ADR-012-template-halaman.md): *klaim yang tidak bisa
  diverifikasi independen tidak masuk kurikulum*). Pilot MCP menunjukkan kenapa: spesifikasi
  yang tayang ternyata revisi `2026-07-28`, berbeda dari yang diingat penyusunnya.
- **Tautan sumber diperiksa hidup** (HTTP 200) sebelum masuk berkas. Utamakan alamat yang
  stabil (`/specification/latest`) daripada yang menyebut tanggal.
- Tulis **kriteria selesai** di proyek mini, bukan hanya perintahnya.

## Alur kerja

```
tulis berkas  →  .\run.ps1 isi              (periksa bentuk, tanpa jaringan)
              →  PR                         (pemilik MEMBACA teksnya di diff — langkah baca pertama)
              →  merge ke main
              →  Actions → "Pasang isi"     (sha citra + slug; hanya dari main)  →  topik berstatus draf
              →  pemilik membaca halaman jadinya di situs
              →  Actions → "Naikkan ke tinjau"                                    →  topik berstatus tinjau
```

Dua workflow terakhir **sengaja dua langkah**: `Pasang isi` paling jauh membawa topik ke
`draf` dan tak punya nama pemeriksa; hanya `Naikkan ke tinjau` yang melahirkan klaim
"sudah diperiksa manusia" ([ADR-021](../docs/adr/ADR-021-jalan-menuju-tinjau.md)), dengan
nama dari `github.actor`.

## Di mesin sendiri

```powershell
.\run.ps1 isi           # periksa semua berkas (sama dengan gerbang CI `cek:isi`)
.\run.ps1 up; .\run.ps1 migrate; .\run.ps1 api     # API pengembangan dengan tulis hidup
.\run.ps1 isi-pasang    # pasang SEMUA berkas ke API itu, prasyarat lebih dulu
```

⚠️ Basis data berisi contoh `seed` punya topik bernama sama dengan isi sungguhan
(`model-context-protocol`). Pemasang tidak menimpa — ia berhenti dengan `BEDA`. Pakai
`.\run.ps1 reset` lalu `migrate` untuk basis data kosong.

## Pemasang tidak menimpa — kecuali diminta

Berkas dan server dibandingkan dulu. Kalau berbeda, pemasang berhenti **sebelum menulis apa
pun** dan menyebut bedanya (`BEDA`). Topik yang sudah `tinjau` **terkunci**.

Berkas yang sudah terpasang boleh diperbaiki: ubah berkasnya, merge ke `main`, lalu pasang dengan
`--ganti` (di Actions: centang **ganti** pada `Pasang isi`; di mesin sendiri:
`node database/isi/pasang.mjs --ganti <slug>`). Rencananya dicetak **sebelum** ada yang ditulis, lalu
server disamakan dengan berkas — nama/ringkasan/`overview`, **media menurut kuncinya**, langkah roadmap
menurut nomornya, definisi alat, proyek, sumber, tautan alat, dan relasi.

- **Topik `tinjau` tetap terkunci**, walau `--ganti`. Mengganti teksnya akan menggugurkan tinjau, dan itu
  bukan tugas pemasang.
- **`overview` yang dihilangkan dari berkas menghapusnya di server** (PUT topik mengganti seluruhnya), dan
  **media di server yang tak ada di berkas ikut dibuang** saat `--ganti`. Menghapus media tak menggugurkan tinjau;
  mengganti `alt`, keterangan, sumber, atau lisensinya menggugurkan.
- ⚠️ **Alat dipakai bersama.** Mengganti `name`/`summary` sebuah alat menggugurkan tinjau **setiap** topik yang
  menautkannya — bukan hanya topik yang sedang dipasang. Pemasang menyebutnya di log, tak bisa mencegahnya.
- **Yang belum punya jalan sama sekali:** membuang langkah roadmap (nomornya berurut tanpa lubang — berkas
  yang lebih pendek dari server tetap `BEDA`), menghapus alat dari katalog, dan memindahkan topik antar-bidang.
  Tercatat di [#79](https://github.com/xtheoputra/techverse-x/issues/79).
