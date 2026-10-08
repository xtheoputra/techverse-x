# ADR-027 — Jalan isi sungguhan: berkas di repo, dipasang lewat workflow bergerbang, dan tidak pernah naik ke `tinjau`

**Status:** Diusulkan — **mesin dan penjaganya sudah dibangun dan diuji di pengembangan dan CI (2026-10-06); workflow-nya belum pernah DIJALANKAN di produksi.** Menjawab pertanyaan yang [ADR-021](ADR-021-jalan-menuju-tinjau.md) sengaja tinggalkan (*"dari mana isi sungguhan datang"*) dan membuka urutan kerja [#42](https://github.com/xtheoputra/techverse-x/issues/42).
**Tanggal:** 2026-10-06

## Konteks

Situs sudah tayang (web + API di Vercel, PostgreSQL di Neon) dan produksinya berisi 14 bidang dan **nol topik**. Jalan menuju `tinjau` sudah ada ([ADR-021](ADR-021-jalan-menuju-tinjau.md), `tinjau.yml`), tetapi ia hanya menaikkan topik yang **sudah ada**. Satu-satunya cara topik masuk ke Neon adalah endpoint tulis, dan permukaan tulis **mati** di produksi ([ADR-020](ADR-020-permukaan-tulis-api.md)). Hari ini, jadi, ukuran resmi — topik berstatus `tinjau` — tidak bisa bergerak dari nol bukan karena belum ada yang menulis, melainkan karena **tidak ada jalan membawa tulisan itu ke produksi**.

ADR-021 mencatat pertanyaan ini sebagai *"keputusan tersendiri yang belum layak diambil sekarang, sebab belum ada satu pun isi sungguhan untuk diuji bentuknya"*. Ia kini layak: ada produksi, ada jalan terakhir, dan satu topik pilot (Model Context Protocol) menguji bentuknya.

Tiga fakta lain membentuk keputusannya:

1. **[ADR-012](ADR-012-template-halaman.md):** `draf` = AI menyusun; `tinjau` = AI menyusun, **pemilik membaca dan menyunting**. Halaman `draf` selalu tampil berlabel. Dan *"klaim yang tidak bisa diverifikasi independen tidak masuk kurikulum sama sekali"*.
2. **Neon Free tidak punya cadangan terjadwal** dan riwayat pemulihannya hanya 6 jam ([#38](https://github.com/xtheoputra/techverse-x/issues/38), komentar 2026-09-17). Begitu topik masuk, kehilangan basis data berarti kehilangan tulisan — kecuali tulisan itu hidup di tempat lain.
3. **Pengetahuan model bukan sumber.** Saat pilot ditulis, spesifikasi MCP yang tayang ternyata revisi `2026-07-28` — protokolnya kini *stateless*, tiap permintaan membawa versi dan kapabilitasnya sendiri — berbeda dari yang diingat model penyusunnya. Itu contoh persis kenapa ADR-012 melarang klaim yang tak diverifikasi, dan kenapa isi harus dibaca dari sumber primer saat ditulis, bukan diingat.

## Keputusan

### 1. Isi sungguhan adalah berkas JSON di repo: `isi/<bidang>/<slug>.json`

Satu berkas per topik. **Bentuknya sama dengan muatan API** (`name`, `summary`, `fieldSlug`, `prerequisite`, `roadmap`, `tools`, `projects`, `resources`, `requires`) — tidak ada lapisan terjemahan yang bisa menyimpang.

Kenapa berkas, dan bukan sesuatu yang hidup hanya di Neon:

- **Diff PR adalah titik baca-manusia pertama.** Pemilik membaca teksnya di PR sebelum merge — itu langkah ADR-012 *"pemilik membaca"* yang akhirnya punya tempat.
- **Riwayat git adalah jejak.** Siapa menulis apa, kapan, dan kenapa berubah.
- **Neon menjadi proyeksi, bukan satu-satunya salinan.** Basis data yang hilang dibangun ulang dari berkas. Ini jawaban atas ketiadaan cadangan di fakta 2 — tanpa menambah mesin cadangan.

### 2. Satu pemasang, dan ia berbicara ke API yang sama

`database/isi/pasang.mjs` — Node 22, nol dependensi, tiga mode: `--cek` (periksa semua berkas, tanpa HTTP), `<slug>` (pasang satu topik), `--semua` (semua topik, prasyarat lebih dulu; untuk pengembangan dan CI). Ia memanggil **endpoint tulis yang sudah ada** (buat topik, prasyarat, langkah, alat, proyek, sumber, `/requires`, `/draf`). Tidak ada jalur kode kedua yang menulis data — ADR-021 menolak *"alat CLI baru di dalam citra"* karena alasan itu, dan alasannya berlaku utuh.

### 3. Produksi hanya lewat `isi.yml`, meniru `tinjau.yml`

`workflow_dispatch` + `environment: produksi` + `packages: read`; menyalakan citra `api` yang **sama persis** dengan yang tayang, di dalam runner, dengan `Editorial__WritesEnabled=true` terikat ke `127.0.0.1`; memanggilnya lewat localhost; membuangnya. Yang tayang di host API tetap tidak memasang satu pun endpoint tulis. Dua kekhususan:

- **Hanya dari `main`,** dan pemeriksaannya mendahului pemakaian secret. Cabang atau PR membawa berkas yang belum dibaca siapa pun, sementara environment `produksi` memegang rahasia Neon.
- **Hanya dua input — `sha` dan `slug` — sama dengan `tinjau.yml`.** Isinya datang dari berkas di `main`, bukan dari kotak teks. Input ketiga yang membawa isi akan membuka jalan memasang teks yang tak pernah dibaca di PR.

### 4. Pemasang paling jauh membawa topik ke `draf` — tidak pernah ke `tinjau`

Ini aturan paling penting, dan ia **memperkuat** ADR-021 §2. `tinjau` satu-satunya klaim kepercayaan produk ini, dan nama pemeriksanya hanya boleh lahir di `tinjau.yml` dari `github.actor`. Kalau pemasang isi bisa memanggil `/tinjau`, *"sudah diperiksa manusia"* bisa lahir dari satu klik pemasangan. Maka:

- Pemasang dan `isi.yml` tidak memuat kata `/tinjau`, `reviewer`, maupun `PEMERIKSA` — `IsiWorkflowGuardTests` menolaknya, dibuktikan merah dengan sabotase.
- Gerbang CI memeriksa dari luar: tiap topik yang baru dipasang harus berstatus `MachineDrafted` dan `reviewedAt` **null**.

### 5. Pemasang tidak pernah menimpa diam-diam

Berkas dan server **dibandingkan lebih dulu**. Kalau server punya sesuatu yang tak ada di berkas, atau nama/ringkasan/langkah roadmap berbeda, pemasang berhenti dengan `BEDA` **sebelum menulis apa pun** dan menyebut bedanya. Topik yang sudah `tinjau` **terkunci**: pemasang boleh membuktikan ia sama dengan berkas, tak boleh menyentuhnya. Pemasangan ulang yang berkas dan servernya sama **tidak menulis apa pun** — pemasang yang menggandakan langkah saat diulang adalah cacat yang sudah pernah menggigit seed. Keberhasilan dibuktikan dengan *rencana kosong* sesudahnya, bukan dengan "tidak ada galat".

### 6. Dua gerbang CI

- **`cek:isi`** (job frontend, sebelum `npm ci`, nol dependensi): bentuk, batas panjang, kunci yang salah ketik, URL, slug sama dengan nama berkas dan folder, tak ada duplikat, relasi hanya menunjuk berkas yang ada, tak ada siklus.
- **"Isi sungguhan dipasang dari nol, dua kali"** (job backend): basis data terpisah di service container yang sama, migrasi dari nol, API sungguhan dengan tulis hidup, `pasang.mjs --semua` dua kali — yang kedua **harus nol tulisan** — lalu invarian butir 4. Di sini **server** yang menilai berkas, jadi cermin batas panjang di pemeriksa yang menyimpang kelihatan di sini. *(Sejak 2026-10-07 langkah ini dan dua gerbang sesudahnya — `uji-ganti`, `uji-media` — satu berkas, [`database/isi/gerbang-isi.mjs`](../../database/isi/gerbang-isi.mjs), yang juga dijalankan `run.ps1 gerbang-isi` dan `run.ps1 ci`.)*

## Yang SENGAJA tidak diputuskan, dan batas yang jujur

- ✅ *Dijawab sebagian 2026-10-07 — lihat Pembaruan di bawah; yang di bawah dibiarkan sebagai rekaman keadaan 6 Oktober.* **Tak ada jalan mengubah `name`, `summary`, prasyarat, atau langkah roadmap sebuah topik yang sudah terpasang.** Yang ada hanya jalan membuang sumber/proyek/alat ([ADR-012](ADR-012-template-halaman.md) dan [ADR-021](ADR-021-jalan-menuju-tinjau.md), Pembaruan 2026-10-02). Pemasang menolak (`BEDA`) dan menyebut caranya. Ini keterbatasan nyata: salah ketik di ringkasan topik yang sudah tayang tak punya jalan perbaikan. Ia layak keputusan tersendiri (endpoint `PUT`, atau jalan "ganti topik") **sebelum** isi ke-2 diperbaiki, dan ditagih di [#79](https://github.com/xtheoputra/techverse-x/issues/79).
- ✅ *Mengubah dijawab 2026-10-07; menghapus belum.* **Katalog alat tak bisa diubah atau dihapus,** dan pemasang tak bisa membedakan "alat sudah ada" dari "alat sudah ada dengan definisi lain" untuk alat yang belum tertaut ke topik ini. Untuk alat yang tertaut, definisi yang berbeda dilaporkan `BEDA`.
- **`isi.yml` belum pernah dijalankan di produksi.** Yang dibuktikan: pemasangan dari nol ke API sungguhan, ulang tanpa tulisan, dan penjaganya merah di bawah sabotase. Yang **belum**: jalan terhadap Neon dan citra yang terbit. Itu dibuktikan oleh pemicu pertama pemilik.
- **Daftar 14 bidang tak dicerminkan di pemeriksa.** `fieldSlug` yang tak dikenal baru ditolak server — di gerbang kedua, bukan di `cek:isi`.
- **Bahasa dan bentuk teks.** Halaman web merender teks polos (tanpa Markdown), jadi berkas isi menulis teks polos. JSON menaruh satu paragraf di satu baris, sehingga diff PR menandai seluruh paragraf ketika satu kata berubah. Ditimbang ulang kalau itu terbukti menyulitkan pembacaan.

## Alternatif yang ditolak

| Alternatif | Kenapa ditolak |
|---|---|
| **Menulis di pengembangan lalu memindahkan ke Neon** (dump/restore) | Melewati validator dan `MarkDrafted` seperti SQL langsung (ADR-021), dan tak punya titik baca di PR. |
| **Endpoint tulis publik** | V1 nol autentikasi; ditutup ADR-020. |
| **Isi sebagai migrasi EF** | ADR-015 memodelkan isi sebagai **data**, bukan kode skema. Tiap koreksi teks menjadi migrasi. |
| **Berkas Markdown ber-*front matter*** | Lebih enak dibaca di diff, tapi butuh pengurai sendiri — jalur parse kedua di samping bentuk muatan API. Ditimbang ulang kalau diff JSON terbukti menyulitkan. |
| **Satu workflow memasang semua topik** | Jejak Actions per topik hilang, dan satu berkas cacat menghentikan yang lain. Satu `workflow_dispatch` per topik meniru `tinjau.yml` persis. |
| **Pemasang yang juga menaikkan ke `tinjau`** | Memecahkan satu-satunya klaim kepercayaan produk. Lihat butir 4. |

## Konsekuensi

- **#42 punya urutan yang bisa dijalankan:** berkas ditulis dan dibaca di PR → merge → `isi.yml` (topik `draf`, halamannya tampil berlabel) → pemilik membaca halaman jadinya → `tinjau.yml` → `tinjau`. Setiap panah punya pelaku, bukti, dan jejak.
- **Neon tidak lagi satu-satunya salinan tulisan.** Kehilangannya mahal hanya untuk status `tinjau` dan nama pemeriksa — dan itu tercatat di riwayat Actions.
- **Pencacah resmi akhirnya punya produsen dari ujung ke ujung.** Tapi ia tetap nol sampai pemilik menjalankan dua workflow.
- **Keterbatasan butir "tidak diputuskan" pertama menjadi utang yang kelihatan.**

## Cara membatalkan keputusan ini

Hapus `isi.yml`, `database/isi/`, `isi/`, kedua gerbang CI, dan `IsiWorkflowGuardTests`. Topik yang sudah terpasang tetap di Neon dan bisa dikelola lewat `tinjau.yml` dan jalan buang yang sudah ada. Karena belum ada isi yang dijalankan di produksi, membatalkannya hari ini berbiaya kecil.

## Pembaruan 2026-10-07 — `--ganti`: pemasang boleh menyamakan server dengan berkas yang berubah

Butir "tidak diputuskan" pertama di atas (*tak ada jalan mengubah `name`, `summary`, prasyarat, atau langkah
roadmap*) ditagih di [#79](https://github.com/xtheoputra/techverse-x/issues/79), dan pemicunya terpenuhi begitu pemilik menilai
isi pilot *"kurang detail"* (isi pilot perlu diganti, padahal topiknya sudah terpasang). Jalan ubahnya dibangun
sebagai Tahap 2 program ADR-028 ([PR #84](https://github.com/xtheoputra/techverse-x/pull/84) memuat ADR-nya;
Tahap 2 sengaja berdiri sendiri di atas `main`, jadi ADR itu belum bisa ditautkan dari sini).

> Catatan koreksi: baris Status di atas ditulis sebelum pemicu pertama. `isi.yml` **sudah dijalankan** sekali di
> produksi (run `37427804388`, 2026-10-06; SESSION-LOG Sesi 21, Lanjutan 5).

### Yang berubah pada butir 5 ("tidak pernah menimpa diam-diam")

Aturannya **tetap**: tanpa `--ganti`, berkas dan server dibandingkan dulu, dan kalau berbeda pemasang berhenti
dengan `BEDA` **sebelum menulis apa pun**. Yang baru adalah satu jalan menimpa yang **eksplisit**:

```
node database/isi/pasang.mjs --ganti <slug>        # atau: --ganti --semua (pengembangan)
```

Rencananya dicetak **lebih dulu** (`rencana    ganti langkah roadmap 3 (…)`), lalu server disamakan dengan
berkas: nama/ringkasan topik (`PUT` topik), langkah roadmap menurut nomornya, definisi alat katalog, proyek dan
sumber (sebagai himpunan), tautan alat, dan relasi. Yang **tetap menghentikan** pemasang walau `--ganti`:
memindahkan topik ke bidang lain dan **membuang langkah roadmap** (server punya langkah yang berkas tak punya) —
keduanya tanpa jalan, lihat [ADR-012 Pembaruan 2026-10-07](ADR-012-template-halaman.md).

- **Topik `tinjau` tetap terkunci**, walau `--ganti`: pemasang boleh membuktikan ia sama dengan berkas, tak boleh
  menyentuhnya. Topik `kurasi` (pemasangan yang terputus di tengah) boleh di-`--ganti`; tak ada pemeriksaan manusia
  yang perlu dilindungi di sana.
- **Menambah dulu, baru membuang.** Pemasangan yang terputus meninggalkan kelebihan (dibuang di jalan berikutnya),
  bukan bagian yang hilang. Diganti = tambah baru lalu buang lama, sebab proyek dan sumber tak punya endpoint ubah.
- **Pemeriksaan sesudahnya tak melunak:** keberhasilan tetap dibuktikan dengan *rencana kosong* tanpa `--ganti` —
  server harus sama dengan berkas, bukan sekadar "tak ada galat".
- **Alat bersama menggugurkan topik lain.** Mengganti definisi sebuah alat katalog menggugurkan tinjau **semua**
  topik yang menautkannya. Pemasang menyebutnya di rencana dan di log, tapi tak bisa mencegahnya lebih dulu.

### `isi.yml` mendapat satu sakelar — dan hanya yang boolean

Penjaga butir 4 menolak input yang bisa membawa teks. `ganti` **boolean** tak membawa isi: ia memilih cara memasang
berkas yang sama dari `main`. `IsiWorkflowGuardTests` kini menuntut himpunan input TEPAT `{ sha, slug, ganti }`
dan `type: boolean` untuk yang terakhir; pemasang dipanggil dengan `--ganti` yang tertulis di workflow, bukan
dengan nilai dari input, dan nilai dibandingkan dengan `"true"` persis.

### Gerbang ketiga: `uji-ganti.mjs`

Di CI, sesudah pemasangan-dua-kali, `database/isi/uji-ganti.mjs` **menyimpangkan** server dari berkas lewat API
(tujuh cara), lalu menjalankan pemasang sebagai proses terpisah: tanpa `--ganti` → `BEDA`, kode 1, server **tak
berubah**; dengan `--ganti` → kembali sama dengan berkas; ulang → nol tulisan; topik `tinjau` → `TERKUNCI`;
langkah roadmap berlebih → `BEDA`. Ia sengaja skrip tersendiri: pemasang itu jalan **produksi** dan dijaga dari
segala yang menyentuh tinjau, sedangkan gerbang ini perlu menaikkan satu topik ke tinjau untuk membuktikan
kuncinya. Ia menolak jalan di basis data yang topiknya bukan draf — tanda ia salah diarahkan.

**Terukur:** 36 pemeriksaan hijau terhadap API sungguhan; **9 merah** saat pembuangan proyek berlebih dimatikan
dan kunci `tinjau` dilepas (dipulihkan identik); penjaga workflow merah saat `ganti` diubah jadi `string`.

### Yang SENGAJA masih tidak diputuskan

- **Alat katalog masih tak bisa dihapus**, dan langkah roadmap tak bisa dibuang ([#79](https://github.com/xtheoputra/techverse-x/issues/79) tetap terbuka).
- **`--ganti` belum pernah dijalankan di produksi.** Terbukti terhadap API di Postgres lokal dan gerbang CI, bukan
  terhadap Neon dan citra yang terbit.
- **URL sumber dibandingkan sebagai teks apa adanya.** URL berkas yang bentuknya beda dari hasil normalisasi
  `Uri.ToString()` .NET membuat rencana tak pernah kosong, dan pemasang gagal keras (bukan diam-diam). Tak terjadi
  pada berkas yang ada.
