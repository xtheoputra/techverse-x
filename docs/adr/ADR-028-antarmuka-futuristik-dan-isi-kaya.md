# ADR-028 — Antarmuka futuristik gelap-neon dan isi kaya: Markdown terbatas, blok media, dan jalan ubah

**Status:** Diusulkan — **Tahap 1 (antarmuka) dibangun dan diukur (2026-10-06); Tahap 2–4 belum.** Keputusan arahnya diambil pemilik di percakapan yang sama dengan penulisan ADR ini; yang belum diputuskan tertulis di bagian *Yang SENGAJA tidak diputuskan*.
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
