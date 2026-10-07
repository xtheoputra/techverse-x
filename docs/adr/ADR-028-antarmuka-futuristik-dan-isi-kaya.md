# ADR-028 — Antarmuka futuristik gelap-neon dan isi kaya: Markdown terbatas, blok media, dan jalan ubah

**Status:** Diusulkan — **Tahap 1 (antarmuka) dibangun dan diukur (2026-10-06); Tahap 2 (jalan ubah) dibangun dan diukur (2026-10-07, lihat Pembaruan di bawah); keduanya ter-merge ke `main` 2026-10-07; Tahap 3 dipecah 3a/3b dan keputusannya ditulis (Pembaruan (2)); Tahap 3a (Markdown terbatas) dibangun, diukur, dan ter-merge (#88; Pembaruan (2), butir 7); Tahap 3b (media dan `overview`) dibangun, diukur, ter-merge (#90), dan dimigrasi ke Neon (Pembaruan (3) dan (4)); Tahap 4 (pilot MCP ditulis ulang) dibangun dan diukur, belum ter-merge dan belum dipasang di produksi (Pembaruan (4)).** Keputusan arahnya diambil pemilik di percakapan yang sama dengan penulisan ADR ini; yang belum diputuskan tertulis di bagian *Yang SENGAJA tidak diputuskan*.
**Tanggal:** 2026-10-06

## Konteks

Topik pilot pertama (Model Context Protocol) terpasang di produksi sebagai `draf` ([ADR-027](ADR-027-jalan-isi-sungguhan.md)). Pemilik membaca halamannya dan menilainya:

> *"kontennya kurang detail, tidak readable sangat kaku. website yang saya inginkan adalah website teknologi futuristik, UI harus benar benar futuristik, isinya bermutu, detail bagus berisi gambar/video atau apapun sebagai informasi"*

Penilaian itu diukur sebelum dirancang jawabannya, dan ia **bukan hanya soal gaya**:

| Yang diukur | Hasil |
|---|---|
| Isi halaman pilot | **~1.074 kata** seluruhnya: ringkasan 138 kata, rata-rata 58 kata per langkah roadmap, satu proyek 110 kata |
| Bentuk isi | Teks polos satu blok per bagian; tak ada paragraf, daftar, kode, kutipan, atau tabel |
| Batas kolom | Ringkasan dan uraian langkah **2.000 karakter**, proyek 4.000, catatan alat 500 — dan tak ada entitas untuk gambar, diagram, atau video |
| Tampilan | CSS bawaan starter Next.js (putih/hitam, Arial); tak ada sistem desain |

Jadi menjawabnya menuntut **perubahan model isi dan tampilan sekaligus**. Dan ia menyalakan pemicu yang [#79](https://github.com/xtheoputra/techverse-x/issues/79) tulis (*"sebelum isi ke-2 diperbaiki"*): isi pilot perlu diganti, padahal topik yang sudah terpasang tidak punya jalan ubah.

## Keputusan

Empat jawaban pemilik, 2026-10-06:

### 1. Gaya: gelap-neon / aurora, satu tema

Hitam pekat, gradasi cyan–violet, kartu kaca, grid halus, huruf monospace sebagai aksen, animasi masuk yang halus. (Dua arah lain ditawarkan dan tidak dipilih: *HUD sci-fi/terminal* dan *spasial bersih*.)

Aturan yang mengikat, karena keluhan pemilik justru *"tidak readable"*:

- **Keterbacaan lebih tinggi daripada gaya.** Teks isi 17px+, jarak baris ~1,8, lebar baris dibatasi ~68 karakter; teks sekunder dijaga ≥ 7:1 terhadap latar.
- **Satu tema, bukan dua.** `color-scheme: dark`. Tema ganda berarti dua permukaan yang harus diuji kontrasnya untuk tiap komponen.
- **CSS murni.** Efek apa pun yang butuh JavaScript hanya boleh *memperindah*, tak pernah *dibutuhkan*: tier gratis sering melayani halaman sebelum bundel JS dimuat ([ADR-019](ADR-019-hosting-gratis-tanpa-kartu.md)), dan kotak cari tetap `<form>` biasa. Penanda bagian aktif di daftar isi adalah satu-satunya komponen klien baru, dan tanpanya daftar tetap berfungsi.
- **Tipografi:** paket `geist` (Geist Sans/Mono/Pixel, lisensi OFL) disajikan dari berkas sendiri — nol permintaan ke penyedia font, nol unduhan saat build. Geist Pixel hanya untuk angka besar dan nomor bagian.
- **Gerak mati sendiri** bila pembaca meminta gerak dikurangi (`prefers-reduced-motion`).

### 2. Model isi: Markdown terbatas + blok media, di data

Isi kaya tinggal di jalur yang sama dengan sekarang — berkas `isi/` → pemasang → Neon ([ADR-027](ADR-027-jalan-isi-sungguhan.md)) — sehingga gerbang `draf` → `tinjau`, pencarian, dan aturan *mengganti teks menurunkan tinjau* ([ADR-012 Pembaruan 2026-10-06](ADR-012-template-halaman.md)) tetap berlaku. (Ditolak: artikel MDX di repo; lihat *Alternatif*.)

- **Teks berformat:** paragraf, sub-judul, daftar, blok kode berbahasa, kutipan/peringatan, tabel, tautan http(s), tebal/miring/kode sebaris. **Tanpa HTML mentah**, dan tanpa sintaks gambar Markdown — media hanya lewat blok bertipe (di bawah), supaya setiap gambar punya teks alternatif dan sumber yang bisa diperiksa mesin.
- **Blok media:** gambar/diagram dan video, masing-masing dengan teks alternatif (wajib), keterangan, dan sumber/lisensi (wajib kecuali karya asli).

### 3. Media: keempat sumber boleh, dengan aturan masing-masing

| Sumber | Aturan |
|---|---|
| **Diagram SVG buatan sendiri** | Disimpan di repo, disajikan dari berkas sendiri. Karya asli: tanpa soal lisensi, tajam di semua layar, ikut tema. Sumber utama untuk topik teknologi. |
| **Video resmi (YouTube)** | Hanya ceramah atau demo dari kanal resmi proyeknya, disematkan lewat `youtube-nocookie.com`. **Setiap ID diverifikasi hidup lewat oEmbed saat ditulis** — tak ada ID yang ditebak. |
| **Tangkapan layar alat open-source** | Hanya dari proyek berlisensi permisif; lisensi dicek saat itu dan atribusi dicatat. |
| **Foto stok gratis** | Hanya bila benar-benar informatif (bukan hiasan); lisensi dan atribusi dicatat. |

Dan **tidak ada pembuat gambar AI berbayar** ([ADR-026](ADR-026-nol-biaya-gratis-mandiri.md)).

### 4. Jalan ubah dibangun lebih dulu ([#79](https://github.com/xtheoputra/techverse-x/issues/79))

`PUT` topik (`name`, `summary`), ganti langkah roadmap menurut nomornya, ubah katalog alat, dan pemasang `isi/` dengan mode `--ganti` **yang hanya berlaku untuk topik `draf`** (topik `tinjau` tetap dikunci). Semuanya di balik gerbang yang sama (ADR-020), masuk `SemuaEndpointTulis`, dan melewati penjaga domain — bukan `DELETE` langsung ke Neon.

## Tahap dan aturan PR

| Tahap | Isi | Menyentuh |
|---|---|---|
| 0 | Keputusan ini | `docs/` |
| **1** | **Fondasi antarmuka:** token desain, tipografi, beranda, bidang, topik, pencarian | `apps/web` |
| 2 | Jalan ubah (#79) | API, `isi/` |
| 3 | Isi kaya: Markdown terbatas, entitas media, penampil, pemeriksa berkas | semua lapis |
| 4 | Tulis ulang pilot MCP: mendalam, berdiagram, bervideo, diverifikasi ke sumber primer | `isi/`, media |

**Tiap PR berdiri sendiri dan berbasis `main`** — tak ada tumpukan. Tumpukan sudah sekali membuat dua PR ter-merge ke cabang perantara dan bukan ke `main` (SESSION-LOG Sesi 21, Lanjutan 4). Tahap 3 baru dicabangkan sesudah tahap 1 dan 2 masuk `main`.

## Yang SENGAJA tidak diputuskan, dan batas yang jujur

- **Skema basis data untuk media** dan **pustaka pengurai Markdown** — diputuskan di Tahap 3, sesudah Tahap 2 membuktikan jalan ubahnya. Memilih sekarang berarti memilih tanpa melihat satu pun isi kaya.
- **Pencarian isi bagian** tetap urusan [#54](https://github.com/xtheoputra/techverse-x/issues/54), dengan pemicunya sendiri.
- **Tema terang, bahasa lain, dan analitik** tidak dibuat.
- **Diukur di Chrome saja** (2026-10-06): layar lebar dan iframe 390px (`scrollWidth = clientWidth`), build produksi. Safari dan Firefox belum dicoba; animasi muncul-saat-gulir memakai `animation-timeline` dan, di peramban yang tak mendukungnya, elemennya tampil apa adanya. Tak ada uji regresi visual otomatis — yang terukur otomatis hanya teks dan struktur (pemindai CI di bawah).
- **Isi pilot belum berubah.** Tahap 1 hanya mengganti kulit; halaman MCP masih ~1.074 kata teks polos. Keluhan *"kurang detail"* baru terjawab di Tahap 3–4, dan ADR ini tidak mengklaim sebaliknya.

## Alternatif yang ditolak

- **Artikel MDX di repo, dirender web.** Menulis paling bebas (komponen interaktif, kode berwarna) — tetapi jalur terpisah dari basis data dan dari gerbang `tinjau` (apa yang "diperiksa" menjadi kabur), dan membongkar keputusan ADR-027 bahwa Neon adalah proyeksi dari berkas yang dibaca di PR.
- **Gaya HUD sci-fi/terminal** dan **spasial bersih.** Yang pertama paling berisiko bagi keterbacaan artikel panjang; yang kedua paling aman tetapi kurang *"teknologi tinggi"* secara kasat mata, padahal itu permintaan pemilik.
- **Tema terang + gelap.** Dua permukaan kontras untuk setiap komponen, demi permintaan yang tak pernah diajukan.
- **Pustaka komponen siap pakai.** Satu berkas token kecil dan sembilan komponen cukup, tanpa dependensi runtime baru selain font.

## Konsekuensi

- **Invarian ADR-024 tidak berubah:** label kematangan tak pernah hilang, ringkasan alat tampil, kerangka blok "Topik terhubung" sama, dan teks terlarang tetap dilarang. Diukur: pemindai halaman jadi (`periksa-halaman-web.mjs`) terhadap build produksi baru — **15 dari 15** halaman `/teknologi/*` tercapai, **4** ringkasan alat tampil, **0** teks terlarang, semua halaman 200.
- **Satu dependensi baru:** `geist` (font, tanpa dependensi turunan).
- **Pengguna yang memakai preferensi tema terang tetap melihat tema gelap.** Itu konsekuensi langsung keputusan 1, bukan kelalaian.
- Tiap tahap berikutnya menambah ADR-nya sendiri atau Pembaruan di sini, dan SESSION-LOG mencatat apa yang diukur.

## Cara membatalkan keputusan ini

Tahap 1 hanya menyentuh `apps/web` dan satu dependensi; mengembalikannya = membatalkan PR-nya, karena seluruh sistem desain tinggal di satu berkas token (`globals.css`) dan komponen yang memakainya. Tahap 2–4 tidak ada di sini sampai dibangun.

## Pembaruan 2026-10-07 — Tahap 2 (jalan ubah, [#79](https://github.com/xtheoputra/techverse-x/issues/79)) dibangun; Tahap 1 dan 2 ter-merge

Tahap 2 dibangun di [PR #85](https://github.com/xtheoputra/techverse-x/pull/85), berdiri sendiri di atas `main` dan tanpa menyentuh `apps/web` — jadi tak bergantung pada Tahap 1 ([#84](https://github.com/xtheoputra/techverse-x/pull/84)). Pemilik me-merge keduanya pada 2026-10-07 (Tahap 2 pukul 03:47 UTC, Tahap 1 pukul 03:48 UTC), sesudah tambalan `sharp` di [#86](https://github.com/xtheoputra/techverse-x/pull/86) (lihat bawah). Keputusan dan penalarannya **tidak diulang di sini**; tinggal di [ADR-012 Pembaruan 2026-10-07](ADR-012-template-halaman.md) (aturan "mengganti teks menggugurkan tinjau" di tiga pintu baru, koreksi atas `Update()`, alat bersama) dan [ADR-027 Pembaruan 2026-10-07](ADR-027-jalan-isi-sungguhan.md) (`--ganti`, sakelar `ganti` di `isi.yml`, gerbang ketiga). Berkas ini hanya memuat yang menyangkut program.

### Yang dibangun

| Lapis | Isi |
|---|---|
| API (tulis; hilang di produksi; masuk `SemuaEndpointTulis`) | `PUT /api/v1/technologies/{slug}` (`name`, `summary`) · `PUT …/{slug}/roadmap/{order}` (ganti langkah yang **sudah ada**; `0` = prasyarat) · `PUT /api/v1/tools/{slug}` |
| Pemasang | `database/isi/pasang.mjs --ganti`: rencana dicetak sebelum menulis; menambah dulu baru membuang; `tinjau` tetap terkunci |
| Workflow | `isi.yml` mendapat sakelar `ganti` — **boolean**, supaya tak bisa membawa teks |
| Gerbang CI | `database/isi/uji-ganti.mjs` terhadap API sungguhan |

### Di mana teks ADR ini di atas dikoreksi atau dipersempit

- **"`--ganti` hanya berlaku untuk topik `draf`"** (butir 4) → yang terkunci adalah **`tinjau`**; topik `kurasi` (pemasangan yang terputus) juga boleh di-`--ganti`, sebab tak ada pemeriksaan manusia yang perlu dilindungi di sana. Tanda kurung di teks aslinya (*"topik `tinjau` tetap dikunci"*) memang itu maksudnya.
- **"Ubah katalog alat"** (butir 4) membawa akibat yang tak tertulis: alat dipakai bersama, jadi mengganti `name`/`summary`-nya menggugurkan `tinjau` **semua topik yang menautkannya**, bukan hanya topik yang sedang dipasang.
- **"Ganti langkah roadmap menurut nomornya"** (butir 4) berarti nomor sebagai **alamat**, bukan isian: hanya langkah yang sudah ada yang bisa diganti; membuang langkah tetap tanpa jalan.

### Terukur

| Yang dibuktikan | Hasil |
|---|---|
| CI `main` sesudah ketiga merge | **hijau** untuk `CI` dan `Rilis citra` di `8e782bb` (#86), `4631e30` (#85), dan `41bf1bd` (#84) — termasuk gerbang `--ganti` dan pemindai citra |
| Gerbang `--ganti` di runner | 36 pemeriksaan `ok`, 0 `GAGAL` (tahap A–F) |
| Produksi sesudah merge | web 200 di `/`, `/cari`, `/teknologi/ai-agents`, `/teknologi/model-context-protocol`; HTML beranda memuat penanda antarmuka baru (`Geist`, `aurora`); API membaca **1 topik** (`model-context-protocol`, `MachineDrafted`); `PUT`/`POST`/`DELETE` tulis → **404 atau 405** (tak ada yang bocor) |

### Catatan jujur

- **#85 di-merge saat check "Citra peti kemas" PR-nya masih merah.** Penyebabnya bukan diff-nya: advisori `sharp` [GHSA-wq5f-xc86-pv6w](https://github.com/advisories/GHSA-wq5f-xc86-pv6w) terbit 2026-10-06 13:43 UTC, sesudah #84 hijau. Tambalannya #86 (hanya lockfile, `sharp` 0.35.4 → 0.35.5). Bahwa tambalan itu cukup dibuktikan oleh CI `main` yang hijau di `4631e30`, bukan oleh PR #85 sendiri.
- **Antarmuka baru di produksi belum saya lihat dengan mata**, hanya penandanya di HTML. Penilaian visualnya tetap milik pemilik.
- **`isi.yml` dengan `ganti` belum pernah dijalankan terhadap Neon.**
- **Tahap 3 belum dimulai.** Dua hal yang ADR ini tunda sampai Tahap 2 terbukti — skema basis data media dan pustaka pengurai Markdown — kini boleh diputuskan.

## Pembaruan 2026-10-07 (2) — Tahap 3 dipecah dua; pustaka Markdown dan skema media diputuskan

Pemilik menjawab tiga pertanyaan penutup Tahap 2: arah gelap-neon **sudah mantap**, #87 ter-merge, dan Tahap 3 **dilanjutkan**. Dua hal yang ADR ini tunda sampai jalan ubah terbukti (*skema basis data media* dan *pustaka pengurai Markdown*) diputuskan di sini — **sebelum satu baris kode Tahap 3**.

### 1. Tahap 3 dipecah dua, dan alasannya penyebaran, bukan ukuran

| | Isi | Menyentuh | Risiko penyebaran |
|---|---|---|---|
| **3a** | Markdown terbatas untuk teks: pengurai, penampil web, pemeriksa berkas | `apps/web`, `database/isi/`, `isi/` | **Nol** — tanpa migrasi, tanpa perubahan API |
| **3b** | Blok media, bagian `overview` berformat, penampil media, diagram | semua lapis, **termasuk migrasi Neon** | Ada — lihat butir 5 |

[PENYEBARAN.md](../PENYEBARAN.md#urutan-yang-mengikat) menuntut `migrate → api → web`, sedangkan Vercel membangun API otomatis saat merge. Skema baru berarti ada jendela beberapa menit di mana API baru membaca kolom yang belum ada. 3a tak punya jendela itu, dan sudah menjawab sebagian keluhan *"tidak readable, sangat kaku"* (daftar, kode, tabel, peringatan) pada isi yang ada; 3b menjawab *"gambar/video"*. Keduanya berdiri sendiri di atas `main`; 3b dicabangkan sesudah 3a ter-merge.

### 2. Keputusan: pengurai Markdown **tulis sendiri** untuk dialek terbatas, satu implementasi untuk penampil dan pemeriksa

Tak ada pustaka Markdown. `apps/web/src/lib/markdown/` berisi pengurai (JavaScript ES module tanpa dependensi, tipe lewat `.d.mts`), diuji dengan `node --test`; **penampil web dan pemeriksa berkas `pasang.mjs --cek` mengimpor berkas yang sama**.

Alasannya, berurutan menurut bobot:

1. **Apa yang lolos pemeriksa = apa yang tampil, oleh konstruksi.** Repo ini sudah beberapa kali digigit *cermin yang menyimpang* (batas panjang di pemeriksa vs kolom basis data; tiap kali ketahuan belakangan). Dengan pustaka di web, pemeriksa butuh pustaka yang sama atau tiruan regex — dua implementasi dialek yang berjalan sendiri-sendiri.
2. **Pemeriksa berkas harus jalan tanpa `npm ci`.** `cek:isi` sengaja nol dependensi supaya berkas isi yang cacat gagal dalam hitungan detik, dan `isi.yml` menjalankannya di runner produksi sebelum menyentuh citra atau Neon. Pustaka menjadikan keduanya butuh `npm ci`.
3. **Keluaran pohon, bukan HTML.** Pengurai menghasilkan pohon yang dirender jadi elemen React; **tak ada `dangerouslySetInnerHTML`**, jadi teks isi tak bisa menyuntik markup. Satu-satunya jalur berbahaya yang tersisa — skema URL tautan — dijaga daftar putih `http`/`https` dan diuji.
4. **Dialeknya sengaja kecil**, jadi pengurainya kecil dan bisa diuji tuntas. Dan ada keputusan dialek yang pustaka CommonMark akan membuatnya sulit: **`_` bukan penanda miring** (isi teknis penuh `snake_case`, `server_discover`; pilot MCP sudah memuat `_`), HTML mentah dan gambar Markdown **ditolak** bukan dilewatkan.

**Ditolak:** `react-markdown` + `remark-gfm` (puluhan dependensi transitif, dan alasan 1–2 di atas); `marked`/`markdown-it` (keluaran string HTML — butuh penyaring dan `innerHTML`, alasan 3); MDX (sudah ditolak di atas, *Alternatif yang ditolak*). **Harganya diakui:** kita memegang kode pengurai. Ia kecil, dan bahaya sebenarnya — XSS — tak lewat sana (butir 3).

**Letaknya di `apps/web`, bukan `packages/`.** Dockerfile web hanya menyalin `apps/web/` dan `package.json` akar; pustaka di `packages/` tidak masuk citra tanpa mengubah Dockerfile dan konfigurasi Vercel. Pemeriksa mengimpornya lewat jalur relatif dari `database/isi/`.

### 3. Dialek — apa yang diterima dan apa yang ditolak

| Diterima | Catatan |
|---|---|
| Paragraf | baris tunggal digabung; baris kosong memisahkan |
| Sub-judul `##` dan `###` | **dua tingkat**; tingkat HTML sebenarnya mengikuti konteks halaman (`#` dan judul bagian dimiliki halaman) |
| Daftar `-` dan `1.` | bersarang sampai dua tingkat; isi butir boleh paragraf, daftar, atau kode |
| Kode berpagar ```` ```bahasa ```` | **bahasa wajib** di pemeriksa (`text` bila tak ada); tanpa penyorotan sintaks (butir 6) |
| Kutipan `>` dan peringatan `> [!CATATAN]` / `[!TIPS]` / `[!PERINGATAN]` | tak bersarang |
| Tabel pipa | kolom tiap baris harus sama; dibungkus gulir-samping di ponsel |
| `**tebal**`, `*miring*`, `` `kode` ``, `[teks](https://…)`, `\` pelolos | tautan hanya `http`/`https` absolut |
| *(3b)* blok media `::media[kunci]` | sendirian di satu baris |

**Ditolak pemeriksa, dengan nomor barisnya:** HTML mentah (`<tag`), gambar `![](…)` (media hanya lewat blok bertipe, supaya tiap gambar punya teks alternatif dan sumber yang bisa diperiksa mesin), judul `#` dan `####`+, garis pemisah `---`, `_miring_`, tautan non-http(s), `<autolink>`, daftar tugas, catatan kaki, kode berpagar tanpa bahasa atau tak tertutup. **Penampil toleran**: teks apa pun tetap tampil (sebagai teks), tak pernah melempar — pemeriksa yang ketat, penampil yang tak bisa dijatuhkan.

### 4. Di mana Markdown berlaku

Deskripsi langkah roadmap (termasuk prasyarat) dan ringkasan proyek. **Bukan `summary`:** ia juga keterangan kartu dan hasil pencarian, yang menampilkan teks polos — tanda `**` di sana akan tercetak apa adanya. Bagian Overview yang berformat memakai medan baru (3b). Nama, ringkasan alat, catatan alat, dan judul sumber tetap teks polos.

### 5. Keputusan untuk 3b — diputuskan di sini; **dibangun di Pembaruan (3), dengan penyimpangan yang dicatat di sana**

- **Medan `overview`** pada topik: Markdown, boleh kosong, batas ± 20.000 karakter. `summary` tetap blurb polos dan tetap penentu bagian *Overview* di `MissingSections`; `overview` memperdalamnya.
- **Tabel `technology_media`** (anak topik, seperti `resources`), **bertipe**, bukan blok generik ([ADR-015](ADR-015-skema-data-v1.md)): `Id`, `TechnologyId`, `Key` (unik per topik), `Kind` (`Image` | `Video`), `Url` (berkas sendiri di `/media/…`, hanya untuk `Image`), `VideoId` (hanya `Video`), `Alt`/`Title` (wajib), `Caption`, `SourceName`, `SourceUrl`, `License`. Aturan "gambar wajib `Url`+`Alt`, video wajib `VideoId`+`Title`, lisensi wajib kecuali karya sendiri" dijaga **`CHECK` di basis data**, bukan hanya komentar.
- **Diubah per kunci:** `PUT …/{slug}/media/{key}` (idempoten) dan `DELETE`. Menambah dan membuang tak menggugurkan `tinjau`; **mengganti** `alt`/`caption`/sumber menggugurkan (aturan ADR-012 yang sama).
- **Aset:** diagram SVG buatan sendiri di `apps/web/public/media/<slug-topik>/`; video disematkan lewat `youtube-nocookie.com` dengan ID yang **diverifikasi hidup lewat oEmbed saat ditulis** (butuh jaringan, jadi bukan di pemeriksa).
- **Pemeriksa 3b:** tiap `::media[kunci]` harus punya definisi, tiap definisi dipakai (peringatan), berkas SVG harus ada, format ID video valid.
- **Penyebaran:** migrasi **aditif**. Jendela `migrate → api` ditegakkan tangan — **keputusan yang akan saya minta dari pemilik di PR 3b**, bukan diam-diam: merge lalu langsung jalankan *Migrasi produksi* (citranya baru terbit ± 3 menit sesudah merge), menerima beberapa menit di mana topik membalas 500 selagi produksi belum punya pengunjung yang bergantung padanya.

### 6. Yang SENGAJA tidak diputuskan

- **Penyorotan sintaks kode.** Butuh pustaka (berat) atau pembuat token buatan sendiri per bahasa; kode tampil monospasi dengan label bahasa dulu. Ditimbang ulang bila isi Tahap 4 menunjukkan itu menyulitkan.
- **Tautan antar-topik di dalam teks** (`/teknologi/<slug>`). Relasi sudah punya tempat sendiri (ADR-023); membukanya di dialek menambah jenis tautan yang harus diperiksa keberadaannya.
- **Daftar lebih dari dua tingkat, catatan kaki, rumus.**
- **Isi pilot belum berubah.** 3a hanya memberi teks yang ada kemampuan tampil berformat; halaman MCP tetap ± 1.074 kata teks polos sampai Tahap 4.

### 7. Hasil Tahap 3a — dibangun dan diukur

Satu PR berbasis `main`, **tanpa migrasi dan tanpa perubahan API**: `apps/web/src/lib/markdown/` (pengurai, tipe, uji), komponen `Markdown`, gaya tambahan di `globals.css`, `pasang.mjs --cek` yang memakai pengurai yang sama, dan uji pengurai di CI, `run.ps1 verify`, serta `Makefile`. Medan yang berformat: deskripsi langkah (termasuk prasyarat) dan ringkasan proyek, sesuai butir 4.

| Yang dibuktikan | Hasil |
|---|---|
| Uji pengurai (`node --test`, nol dependensi) | **44 hijau**, termasuk uji acak 4.000 masukan (tak pernah melempar, tak pernah membawa `href` selain http(s)) dan uji waktu (tanpa ledakan eksponensial) |
| Sabotase | pemeriksaan skema URL dimatikan → **4 merah**, termasuk uji acak yang menemukan `href` berbahaya sendiri; penanganan butir bersaudara dimatikan → **4 merah**. Dipulihkan identik (`cmp`) |
| Pemeriksa berkas | isi nyata lolos (± 760 kata di bagian yang diperiksa); berkas sengaja rusak → **11 laporan, masing-masing dengan nomor baris yang tepat** (HTML, judul `#`, kode tanpa bahasa, tautan `javascript:`, gambar, tabel tak rata, garis pemisah, media, daftar tiga tingkat, jenis peringatan tak dikenal); berkas sementara dihapus |
| Tampilan, Chrome, build produksi, konten demo di basis data sekali pakai (diisi lewat `PUT` Tahap 2) | daftar, daftar bersarang, sub-judul, tebal/miring, kotak catatan/tips/peringatan, kutipan, tabel berperataan, blok kode berlabel bahasa, tautan — **dilihat**, bukan hanya dibaca dari HTML |
| Ponsel (iframe 390 px) | halaman **tidak meluber** (`scrollWidth` 380 = `clientWidth` 380); tabel (isi 448 px) dan kode (isi 1.283 px) menggulir **di dalam** kotaknya (224 px) |
| `run.ps1 verify` | hijau (lihat PR) |

**Tiga hal yang ketahuan saat membangun**, dan sudah diperbaiki: (1) baris `****` ternyata garis pemisah menurut aturan sendiri — ujinya yang keliru, bukan pengurainya; (2) deteksi HTML terlalu rakus (`a<b` di dalam rumus dihitung tag) — kini hanya tag utuh, komentar, deklarasi, dan `<autolink>`; (3) butir berurut bersaudara (`2. b`) nyaris tertelan sebagai lanjutan malas butir sebelumnya, ketahuan saat membaca ulang sebelum diuji. Satu lagi datang dari luar: **pemeriksa tautan Markdown repo memindai berkas kode** dan menandai contoh `[x](y)` di komentar dan uji sebagai tautan relatif yang putus — gerbang `verify` merah sampai contohnya memakai URL absolut.

**Batas yang jujur:**

- **Lebar teks di ponsel sempit** — ± 224 px di dalam garis waktu roadmap (padding bawaan Tahap 1). Tabel dan kode tetap terbaca karena menggulir, tetapi baris kode panjang cepat tersembunyi. Pelonggaran padding di ponsel adalah keputusan tampilan tersendiri, tak diambil di sini.
- **Tanpa penyorotan sintaks**; label bahasa saja (butir 6).
- **`::media[kunci]` ditolak pemeriksa** sampai 3b memberinya data; penampil mengabaikannya.
- **Produksi belum menampilkannya**: isi pilot polos, jadi tak ada yang berubah di mata pengunjung sampai Tahap 4 menulis ulangnya. Konten demo hanya di basis data sekali pakai dan tak masuk repo.

## Pembaruan 2026-10-07 (3) — Tahap 3b (media dan `overview`) dibangun

Satu PR berbasis `main` (sesudah #88 dan #89 ter-merge), **dengan migrasi** `MediaDanOverview`: kolom `Overview` yang boleh kosong di `technologies`, dan tabel `technology_media`. Pemilik mengizinkan jalur penyebaran butir 5 ("merge lalu langsung *Migrasi produksi*") dengan memilih "lanjut"; **izin menjalankan migrasinya tetap diminta lagi sesudah PR ini ter-merge**, karena izin untuk #89 tak berlaku untuk migrasi lain.

### Yang dibangun, dan di mana ia **menyimpang** dari butir 5

| Butir 5 | Yang jadi | Kenapa |
|---|---|---|
| Medan `overview` | Ada, ≤ 20.000 karakter, boleh kosong. **Ditulis lewat `PUT` topik** (`UpdateTechnologyRequest` mendapat `Overview` opsional), bukan endpoint sendiri | Ia bagian topik, bukan anaknya. Konsekuensi yang disengaja: **`PUT` mengganti seluruhnya, jadi `overview` yang dihilangkan = dikosongkan** |
| `Alt`/`Title` | **Satu medan `alt`**; untuk video ia judul bingkai | Dua nama untuk satu teks wajib hanya menambah cara salah mengisi |
| Aturan gugur tinjau: "mengganti `alt`/`caption`/sumber menggugurkan" | **Mengganti medan mana pun** dari media yang sudah ada menggugurkan tinjau (juga `url`, `videoId`, lisensi). Menambah dan membuang tidak. Mengganti teks `overview` menggugurkan; menulis ulang teks yang **sama** tidak (`Update()` kini no-op bila isinya identik) | Lisensi dan sumber adalah klaim kepercayaan; daftar sempit mengundang celah. Aturan ADR-012: *mengganti teks menggugurkan; menambah/membuang/mengulang yang sama tidak* |
| "tiap definisi dipakai (**peringatan**)" | **Galat**, bukan peringatan | Pemeriksa tak punya saluran peringatan, dan definisi tanpa rujukan hampir selalu salah ketik di salah satu sisi. Dua arah sama keras: `::media[x]` tanpa definisi juga galat |
| `Key` | Wajib **slug** (huruf kecil, angka, tanda hubung), ≤ 80, unik per topik | Ia alamat dari teks (`::media[kunci]`); kunci yang boleh apa saja membuat rujukannya rapuh |
| `Url` "berkas sendiri di `/media/…`" | **`/media/<slug-topik>/nama.ext`** — gambar hidup di folder topiknya sendiri; ekstensi `svg`, `png`, `jpg`, `jpeg`, `webp`, `avif`; tanpa `..` atau `//` | Mencegah dua topik berebut nama berkas dan mempersempit apa yang boleh dicetak sebagai `<img src>` |

Tambahan yang tak ada di butir 5:

- **`CHECK` di basis data** (`ck_technology_media_{kind,kunci,bentuk,berkas_sendiri,alt,lisensi}`) mengulang aturan domain, diuji dengan **SQL mentah** yang melewati domain. `Karya sendiri` dibakukan huruf besar-kecilnya di domain; pemeriksa berkas menolak ejaan lain, kalau tidak "berkas = server" tak pernah tercapai.
- **Lapis terakhir ada di komponen web.** `MediaFigure` memeriksa lagi bentuk `url` dan `videoId` sebelum mencetaknya dan merender **nol** bila tak cocok — API lama atau baris yang diubah di luar domain tak bisa membuat `<img src>` ke alamat sembarang. Pola yang sama dengan skema URL di pengurai Markdown.
- **Pemeriksa SVG** (hanya mungkin di pemeriksa berkas, karena hanya ia yang melihat repo): `viewBox` wajib, dan `<script>`, `<foreignObject>`, elemen tersemat, atribut `on…=`, `javascript:`, `DOCTYPE`/`ENTITY`, `@import`, serta rujukan ke alamat luar ditolak. Berkas harus ada dan ≤ 300 KB (SVG) atau ≤ 800 KB (raster).
- **Penampil toleran terhadap API lama.** `getTechnology` memperlakukan `overview` yang hilang sebagai `null` dan `media` yang hilang sebagai daftar kosong, karena pada jendela `migrate → api → web` web baru boleh bertemu API lama.
- **`ISI_DIR`** (pemasang membaca berkas isi dari folder lain) ada **hanya untuk gerbang uji**; satu uji penjaga memastikan `isi.yml` tak pernah menyebutnya, supaya jalan produksi tetap hanya memasang berkas `isi/` di `main` (ADR-027).
- **Pemasang `--ganti` menyamakan media menurut kuncinya**: yang ada di berkas ditulis, yang ada di server tetapi tak di berkas **dibuang**. `overview` yang hilang dari berkas dikosongkan.

### Yang diukur

| Yang dibuktikan | Hasil |
|---|---|
| Uji domain `MediaOverviewTests` | **37 hijau**. Sabotase aturan sumber dan aturan gugur-saat-mengganti → **6 merah**; dipulihkan identik (`cmp`) |
| Uji integrasi `MediaDanOverviewEndpointTests` | **32 hijau**, stabil 3×, termasuk pelanggaran `CHECK` lewat SQL mentah. Sabotase `Include(Media)` → **3 merah** |
| Gerbang penulisan (`PermukaanTulisTests`) | `PUT` dan `DELETE …/media/{key}` tertutup 404/405 saat tulis mati; sabotase gerbang → merah |
| Aturan media berkas (`media.test.mjs`) | **17 hijau**; sabotase → **3 merah** |
| Gerbang `uji-media.mjs` (API sungguhan, basis data sekali pakai) | **36 pemeriksaan hijau** (dihitung dari keluarannya; skripnya punya 36 pemanggilan `harus`): pasang dari nol; pasang ulang tanpa tulis; berkas menyimpang → `BEDA` tanpa menulis; `--ganti` menyamakan media dan idempoten; `overview` dihapus; `--cek` menolak rujukan tanpa definisi, definisi tanpa rujukan, SVG bersekrip, dan berkas hilang. Sabotase (lewati `DELETE` media; lewati `PUT` media) → **merah**; dipulihkan identik |
| Migrasi | diterapkan ke basis data pengembangan, `Down` lalu `Up` terbukti; `database/migrations/technology.sql` dibuat ulang |
| `run.ps1 verify` | **hijau**: pengurai Markdown 44, aturan media 17, unit **186**, integrasi **133**, lint dan build web bersih, build ketat 0 peringatan |
| Tampilan, Chrome, build produksi, topik demo dipasang lewat **pemasang sungguhan** | Overview: pembuka polos lalu sub-judul, teks tebal, **diagram SVG** berbingkai dengan keterangan dan "Lisensi: Karya sendiri"; **bingkai video 16:9** dengan tautan "Tonton di YouTube", keterangan, dan "Sumber: … · Lisensi: …"; gambar yang sama dipakai ulang di langkah roadmap (3 figur di halaman) |
| Ponsel (iframe 390 px) | halaman **tidak meluber** (`scrollWidth` 376 = `clientWidth` 376); video 286 × 161 (16:9) |

### Catatan jujur

- **Video: "oEmbed 200" tidak membuktikan video bisa tampil.** Diukur: satu video resmi menjawab 200 di oEmbed dan tampil normal dari halaman https, tetapi bingkai yang sama menampilkan *"This video is unavailable"* ketika dimuat dari `http://192.168.22.230` — dengan **dan tanpa** `sandbox`, sedangkan video lain tampil di kedua tempat. Jadi `sandbox` pada `MediaFigure` **bukan** penyebab, dan butir 5 ("ID diverifikasi hidup lewat oEmbed") **tak cukup**: video baru boleh dinyatakan beres hanya sesudah bingkainya dilihat di halaman https sebenarnya. Aturan itu kini tertulis di `isi/README.md`. **Belum ada video sungguhan di repo** (Tahap 4), jadi belum ada yang diuji di produksi.
- **Belum ada media sungguhan di repo.** Halaman MCP pilot tak berubah (± 762 kata teks polos); keluhan *"kurang detail"* tetap terbuka sampai Tahap 4. Yang dibangun di sini hanya kemampuannya, dibuktikan dengan topik demo yang dihapus lagi.
- **Penilaian visual tetap milik pemilik.** Tampilan dilihat lewat build produksi di IP LAN dan iframe 390 px, bukan di ponsel sungguhan.
- **Produksi belum punya skemanya.** Sampai *Migrasi produksi* dijalankan, API baru yang dibangun otomatis oleh Vercel membaca kolom yang belum ada, dan halaman topik membalas 500 — jendela yang sudah diperingatkan di butir 5. Ia dijalankan hanya atas izin baru dari pemilik, dan sesudahnya produksi diperiksa ulang (jumlah bidang, kesehatan, topik terbaca, tulis tertutup).
- **Belum bisa** (tetap di [#79](https://github.com/xtheoputra/techverse-x/issues/79)): membuang langkah roadmap, menghapus alat dari katalog, memindahkan topik antar-bidang.

## Pembaruan 2026-10-07 (4) — 3b tayang (dengan jendela yang jauh lebih lebar dari rencana), dan Tahap 4: pilot MCP ditulis ulang

### Penyebaran 3b: jendela "beberapa menit" ternyata ± 3 jam 16 menit

#90 ter-merge **06:28:04 UTC**; `Rilis citra` untuk `39ac35a` hijau beberapa menit kemudian. *Migrasi produksi* baru dijalankan **09:44 UTC** (run `37602656011`: `Applying migration '20261007050935_MediaDanOverview'`, `Done.`), atas izin pemilik yang diminta di awal sesi berikutnya. Diukur sesaat sebelumnya (09:43 UTC): `GET /api/v1/technologies/model-context-protocol` → **500**, dan halaman topik di produksi menampilkan *"Halaman ini belum bisa diambil"*. Sesudah migrasi: API **200**, halaman tampil lagi, 17 bidang, kesehatan 200.

**Sebabnya ada di rencana, bukan di kode.** Butir 5 dan PR #90 menulis "merge, lalu langsung jalankan *Migrasi produksi*", tetapi 🏁 sesi itu juga menulis bahwa saya akan **meminta izin baru** sesudah merge — dan saat merge terjadi tak ada sesi yang berjalan. Langkah "langsung" itu tak punya pelaku. **Aturan untuk PR bermigrasi berikutnya:** badan PR menyebut pemicu migrasinya sebagai langkah pemilik sendiri tepat sesudah merge (dengan SHA merge), atau izinnya diberikan **sebelum** merge — bukan diminta sesudahnya.

Permukaan tulis produksi **tidak** diukur ulang kali ini (permintaan uji tulis ke produksi ditolak penjaga izin sesi); yang menjaganya tetap uji `PermukaanTulisTests` dan gerbang ADR-020 di CI `main`.

### Tahap 4 — yang ditulis

Berkas `isi/ai-agents/model-context-protocol.json` ditulis ulang untuk revisi spesifikasi **2026-07-28**, dan empat diagram SVG buatan sendiri ditaruh di `apps/web/public/media/model-context-protocol/`.

| | Sebelum (pilot 2026-10-06) | Sesudah |
|---|---|---|
| Kata (`pasang.mjs --cek`) | 762 | **2.795** |
| Overview berformat | — | ± 8.000 karakter: 9 sub-judul, 2 tabel, 2 blok kode, 2 kotak, 5 media |
| Langkah roadmap | 9, teks polos | **11**, berformat: 2 tabel, 7 blok kode, 4 kotak, 3 diagram |
| Proyek mini | 1 | 2, masing-masing dengan kriteria selesai bernomor |
| Sumber | 8 | 15 (termasuk 2 video) |
| Media | 0 | 4 diagram (arsitektur, dua lapis, jabat tangan → permintaan mandiri, MRTR) + 2 video resmi |

**Isi lama ternyata tak hanya tipis tetapi juga tak lengkap untuk revisi yang ia ajarkan.** Pilot 2026-10-06 sudah menyebut 2026-07-28 dan `server/discover`, tetapi tidak menyebut perubahan lain di revisi yang sama: MRTR (`InputRequiredResult`) yang menggantikan permintaan dari server ke client, `subscriptions/listen`, `resultType` wajib, petunjuk cache `ttlMs`/`cacheScope`, header `Mcp-Method`/`Mcp-Name`, dan status *deprecated* untuk Sampling, Roots, dan Logging. Ia juga membandingkan dengan "revisi 2025-06-18 yang masih memakai `initialize`" — jabat tangan itu bertahan sampai **2025-11-25**. Semuanya kini tertulis, dari sumber yang dibaca saat menulis.

### Sumber, dan cara memverifikasinya

- **Spesifikasi dibaca mentah**, bukan lewat ringkasan: berkas `.mdx` revisi 2026-07-28 dari repo `modelcontextprotocol/modelcontextprotocol` (commit `3f6e5e17e2`, 2026-09-22) — indeks, arsitektur, protokol dasar, versi, *changelog*, *deprecated*, `server/discover`, tools, resources, prompts, stdio, Streamable HTTP, MRTR, langganan, cache, elicitation, otorisasi dan pertimbangan keamanannya — serta dokumentasi 2026-07-28 (arsitektur, *Build an MCP server*, SDK, Inspector, *Security best practices*), README ketiga SDK, dan pengumuman donasi Anthropic (9 Desember 2025).
- **Kelima belas URL sumber menjawab 200.** Alamat tanpa tanggal (`/docs/…`, `/specification/latest`) dialihkan ke revisi 2026-07-28 (diukur `307`).
- **Video — kedua langkah di `isi/README.md` dijalankan, untuk pertama kalinya pada video sungguhan:** (1) oEmbed **200**, `author_name` **Anthropic**, judul sama dengan `alt`, dan halaman tontonnya menyatakan `playableInEmbed: true`; (2) **bingkai dilihat dan diputar di origin https produksi** — `https://techverse-x-web.vercel.app`, `isSecureContext` benar, atribut bingkai sama dengan `MediaFigure` (`sandbox`, `allow`, `referrerpolicy`): video pertama berputar (19:34, takarir jalan), yang kedua tampil dengan judulnya. Keduanya juga tampil dari `http://` IP LAN, tak seperti video uji di Pembaruan (3). Keduanya **direkam sebelum revisi 2026-07-28**, dan keterangannya mengatakan itu.

### Yang diukur

| Yang dibuktikan | Hasil |
|---|---|
| Pemeriksa berkas | sah, **2.795 kata** |
| Gerbang isi berbasis API (berkas `gerbang-isi.mjs` dari PR #91, disalin sementara) | **hijau**: pasang dari nol, pemasangan kedua nol tulisan; `uji-ganti` 36 ok; `uji-media` 36 ok |
| Jalur produksi ditiru (basis data sekali pakai) | isi **lama** dari `main` terpasang → isi baru tanpa `--ganti`: `BEDA`, kode 1, **tak menulis**; dengan `--ganti`: **36 baris rencana** dicetak dulu (9 langkah diganti, 2 ditambah, proyek diganti, 7 sumber, 4 alat, 6 media), lalu sama dengan berkas; ulang: **nol tulisan** |
| Tampilan, Chrome, build produksi | 6 figur (4 SVG, 2 video), 9 blok kode, 4 tabel, 6 kotak — **dilihat** bagian demi bagian |
| Ponsel (iframe 390 px) | **merah dulu**: halaman **510 px** di layar 378 px — empat kode sebaris panjang (`io.modelcontextprotocol/subscriptionId`, `notifications/subscriptions/acknowledged`, …) tak bisa patah di kolom roadmap 222 px. Sesudah perbaikan CSS: **378 = 378**; tabel tetap menggulir di dalam kotaknya |
| Pemindai halaman ADR-024 | 25 halaman, **0** teks terlarang, semua 200 |
| `run.ps1 verify` | **hijau** (276 detik): 44 uji pengurai, 17 uji media, 186 unit + 133 integrasi, lint, build web, build ketat 0 peringatan |

### Satu perubahan di luar `isi/`

`overflow-wrap: break-word` untuk kode sebaris di `.prose-tv` (`globals.css`). Ini cacat penampil yang hanya terlihat begitu isi sungguhan memakai token teknis panjang — persis jenis temuan yang hanya muncul saat halaman dilihat. `break-word`, bukan `anywhere`, supaya lebar min-content tabel tak berubah dan tabel tetap menggulir.

### Batas yang jujur

- **Belum dibaca pemilik, belum di produksi.** Topik tetap `draf`; jalannya merge → *Pasang isi* dengan **ganti** dicentang (pemilik) → pemilik membaca halaman jadinya → *Naikkan ke tinjau* bila layak. Angka resmi tetap **0 dari 22 `tinjau`**.
- **Label diagram kecil di ponsel.** Di lebar 286 px, label utama ± 8–9 px; terbaca dengan cubit-perbesar. Usul (belum dibangun): gambar bisa diketuk untuk membuka SVG-nya utuh.
- **Contoh kode dikutip dari tutorial dan spesifikasi, tidak saya jalankan.** Yang terverifikasi adalah teksnya sama dengan sumber yang tayang, bukan bahwa server cuacanya berjalan di mesin ini.
- **Isi ini mengikat revisi 2026-07-28.** Begitu revisi berikutnya terbit, tautan `/specification/latest` berpindah lebih dulu daripada teksnya; tabel "dulu/kini" dan bagian *deprecated* yang pertama perlu dicocokkan ulang.
- Sisa enam topik AI Agents **tetap menunggu** penilaian pemilik atas pilot ini (pola *pilot dulu*).
