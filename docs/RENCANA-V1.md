# Rencana V1 — enam bulan yang disusun ulang

**Status:** Menggantikan `KERANGKA.md` 2.9. Menutup Issue [#21](../../issues/21).
**Tanggal:** 2026-09-04

`KERANGKA.md` 2.9 tetap disimpan apa adanya sebagai rekaman rencana awal. Berkas
ini yang berlaku.

---

## Apa yang disebut "V1"

> Situs publik yang membuat **satu bidang bisa dipelajari sampai tuntas**, dengan
> peta empat belas bidang yang **jujur menyatakan status tiap halamannya**,
> pencarian yang bekerja, dan AI Mentor yang menjawab dari bahan sendiri.

Tanpa login. Tanpa lencana. Tanpa berita di luar arXiv.

Garis finis ini dipilih karena kegagalan yang paling mungkin menimpa proyek ini
bukan kekurangan fitur, melainkan **empat puluh halaman setengah jadi**. Menu
yang lengkap tapi kosong tidak bisa dipakai siapa pun, dan tidak bisa
dipamerkan — sementara satu bidang yang tuntas bisa keduanya.

---

## Rencana lama, dan apa yang salah dengannya

| Bulan | Rencana lama (2.9) | Vonis |
|---|---|---|
| 1 | Dashboard + Menu Teknologi | **Tidak punya target isi.** Menu 82 halaman kosong bukan produk. |
| 2 | Database Roadmap & Resource | Urutannya terbalik — skema lahir dari bentuk konten, bukan sebaliknya. |
| 3 | AI Mentor + Chat | Butuh konten dan indeks vektor yang belum ada di Bulan 3. |
| 4 | News Automation + GitHub Trending | **Terganjal hukum** (#17) dan **GitHub Trending tidak punya API** — harus dibangun dari nol berikut riwayat bintang harian. |
| 5 | Mini Project Generator | — |
| 6 | Knowledge Graph + **Deployment** | **Deployment di bulan terakhir adalah kesalahan terbesar rencana ini.** |

Tiga hal tidak kebagian jadwal sama sekali: **AI Roadmap Generator**, dan
**seluruh isi Visi 2.0** kecuali Knowledge Graph — Timeline Teknologi, Progress
Tracker, Lab Interaktif, Career Mode, Badge & Achievement.

**Kenapa Deployment di Bulan 6 fatal:** segala yang belum pernah tayang belum
pernah terbukti. Sertifikat, variabel lingkungan, migrasi di lingkungan asing,
CORS, dan tagihan pertama semuanya menunggu di sana. Menemukan semuanya di bulan
terakhir berarti proyek gagal tepat di garis finis, setelah seluruh biayanya
sudah dikeluarkan.

---

## Rencana yang berlaku

| Bulan | Target | Bukti selesai |
|---|---|---|
| **1** | **Tayang lebih dulu, isi menyusul.** Merge PR #23 · deploy `main` ke produksi · skema data ADR-015 · 14 bidang tayang berstatus `kurasi` | **URL publik yang bisa dibuka orang lain** |
| **2** | **Satu bidang tuntas: AI Agents.** 7 topik berstatus `tinjau`, termasuk roadmap purwarupa | 7 halaman `tinjau`, dan cetakan roadmap yang bisa ditiru |
| **3** | **Explore + Learn + pencarian.** Pencarian teks penuh PostgreSQL · Knowledge Graph dasar (relasi sudah ada di skema) | Orang bisa menemukan halaman tanpa menebak URL |
| **4** | **Bidang prioritas 1 berikutnya** (Cloud & Infrastructure, IoT) naik ke `tinjau` · Intelligence: arXiv saja, tanpa ringkasan AI | 3 bidang `tinjau`, arus arXiv jalan dengan biaya nol token |
| **5** | **AI Mentor.** Agent Framework + MCP ke dalam + pgvector di atas konten sendiri, dengan pagu harian keras | Mentor menjawab dan **menunjukkan sumbernya** |
| **6** | **Labs + Project Generator** · cadangan untuk yang meleset | Satu proyek bisa dikerjakan orang dari awal sampai selesai |

### Keadaan Bulan 1 per 10 September 2026

Rencana di atas tidak punya penanda kemajuan, jadi pertanyaan *"Bulan 1 sudah
selesai belum?"* tidak punya jawaban di dokumen ini. Ini jawabannya, dan tiap
baris diperiksa ke kode atau ke GitHub — bukan ke dokumen lain.

| Butir Bulan 1 | Keadaan |
|---|---|
| Merge PR [#23](../../issues/23) | ✅ **selesai** — ter-merge, bersama [#25](../../issues/25) dan [#27](../../issues/27) |
| Skema data [ADR-015](adr/ADR-015-skema-data-v1.md) | ✅ **selesai** — mendarat lewat #25 |
| 14 bidang tayang berstatus `kurasi` | ✅ **terbukti** — `FieldCatalog` disemai migrasi, `FieldCatalogTests` menuntut 14, dan halaman menampilkannya berikut label `MaturityBadge` |
| **Deploy `main` ke produksi** | 🛑 **belum** — lihat di bawah |

**Yang menahan hanya butir terakhir, dan penahannya bukan teknis.** Citra
produksi sudah ada dan terbukti melayani situsnya (PR
[#31](../../issues/31)), berikut bundel migrasi dan gerbang pemindaian citra.

⚠️ **Satu kata di kalimat itu dulu berlebihan.** Sampai 2026-09-10 ia berbunyi
*"dari basis data kosong"* — padahal `docker-compose.prod.yml` memasang skrip init
yang membuat skema `technology` lebih dulu, fasilitas yang **Neon tidak punya**.
Migrasi dari basis data yang benar-benar kosong dibuktikan terpisah, lewat
`CREATE DATABASE`; caranya di
[`PENYEBARAN.md`](PENYEBARAN.md#gladi-bersih-tanpa-satu-pun-akun).

📦 **Sejak 8 September 2026 ketiganya bukan cuma "bisa dibangun" — ia sudah
TERBIT.** `rilis-citra.yml` berjalan untuk pertama kalinya di `main` `4bfbfff`
dan hijau: dibangun, dipindai (nol CRITICAL/HIGH), lalu didorong ke
`ghcr.io/<pemilik>/techverse-x/{api,migrate,web}` dengan tag SHA maupun `:main`.
Artinya langkah "cari artefaknya" sudah hilang dari daftar pekerjaan penyebaran —
platform apa pun tinggal menariknya, dan syarat menariknya kini tertulis di
[`PENYEBARAN.md`](PENYEBARAN.md#kredensial-untuk-menarik-citra).

📋 **Keputusan platform SUDAH DIAMBIL — dan bukan yang diusulkan semula.**
[ADR-017](adr/ADR-017-platform-hosting.md) memeriksa lima kandidat dan
mengusulkan **Render** ([#33](../../issues/33)); usul itu **ditolak pemilik
beberapa jam kemudian** karena berbayar, dan digantikan
[ADR-019](adr/ADR-019-hosting-gratis-tanpa-kartu.md): **Vercel (web) + Koyeb
(api) + Neon (Postgres) — gratis, tanpa kartu.**

🔑 **Pelajaran yang membuat penggantinya mungkin: ADR-017 mencari SATU
platform, dan itu yang keliru.** Begitu soalnya dipecah tiga (db · web · api),
keberatan terbesarnya lenyap — yang ditolak sebenarnya bukan "gratis", melainkan
Postgres gratis Render yang menghapus dirinya sendiri setelah 30 hari. Neon tidak
begitu. Temuan ADR-017 yang tetap dipakai: **harga bukan penyeleksi yang paling
tajam** — `PENYEBARAN.md` menuntut citra `migrate` yang BERBEDA berjalan sampai
selesai, dan *pre-deploy command* kebanyakan PaaS menjalankan perintah di dalam
citra layanan itu sendiri. Itu yang melahirkan `/app/efbundle` di dalam citra
`api`.

🛑 **Jadi yang menahan Bulan 1 hari ini tinggal tiga pendaftaran akun**, dan
ketiganya gratis tanpa kartu tapi tetap harus atas nama pemilik: **#38** (Neon)
→ **#39** (Koyeb) → **#40** (Vercel). Domain dan sertifikat tidak lagi menahan:
ketiga platform memberi subdomain ber-TLS sendiri.

✅ **Segala yang bisa dilatih tanpa akun sudah dilatih**, dan latihannya memang
berbuah — dua cacat yang menghentikan langkah 3 (`channel_binding`, lalu SHA
pendek) ditemukan begitu, bukan dari uji. Per 2026-09-10 yang sudah terbukti:
tarikan citra privat dari Actions, bentuk *Root Directory* `apps/web` di kedua
setelan lockfile, `/app/efbundle` dari basis data yang benar-benar kosong, dan
bentuk produksi yang **tidak punya permukaan tulis sama sekali**
([#48](../../issues/48)).

⚠️ Ukuran keberhasilan Bulan 1 di tabel di atas adalah **URL publik yang bisa
dibuka orang lain**. Tiga dari empat butir selesai tidak membuat ukuran itu
tercapai — ia hanya tercapai kalau butir keempat tercapai.

### Keadaan Bulan 3 per 17 September 2026 — dikerjakan mendahului Bulan 1

Bulan 2 ([#42](../../issues/42)) terkunci di belakang [ADR-021](adr/ADR-021-jalan-menuju-tinjau.md),
yang sengaja menunggu #40 tutup. Ketiga pendaftaran akun menunggu pemilik.
**Yang tidak menunggu siapa pun adalah Bulan 3**, jadi ia dikerjakan lebih dulu.

Urutan bulan di tabel atas adalah urutan **prioritas**, bukan kunci — dan
mendahulukan yang tidak terhalang jelas lebih baik daripada menunggu.

| Butir Bulan 3 | Keadaan |
|---|---|
| **Pencarian teks penuh PostgreSQL** | ✅ **mendarat** — [ADR-022](adr/ADR-022-pencarian-teks-penuh.md). Kolom `tsvector` terhitung di `technologies` **dan** `fields`, indeks GIN, `websearch_to_tsquery`, peringkat berbobot, halaman `/cari`, kotak cari di layout. 14 uji integrasi + 8 uji unit baru. |
| **Knowledge Graph dasar** | 🟡 **mekanisme lengkap** — produsen pengembangan, pembaca, tampilan "Topik terhubung", dan penjaga basis data ([ADR-023](adr/ADR-023-knowledge-graph-dasar.md)). **Produksi NOL sisi** sampai topik masuk lewat jalan [ADR-021](adr/ADR-021-jalan-menuju-tinjau.md). |
| **Explore** | ✅ **dipenuhi rute yang sudah ada** — `/`, `/teknologi/<bidang>`, `/cari`, tautan antar-topik ([ADR-024](adr/ADR-024-explore-learn-navigasi-v1.md)). Diukur dengan penelusuran dari `/`: **20 dari 20** halaman yang dikenal API di pengembangan (14 bidang + 6 topik), **14 dari 14** di bentuk produksi (14 + 0). |
| **Learn** | ⏸ **sengaja ditunda** — pemicunya [#42](../../issues/42) tutup DAN satu roadmap terisi di produksi ([ADR-024](adr/ADR-024-explore-learn-navigasi-v1.md)). Sampai itu, Learn V1 = bagian Learning Roadmap di tiap halaman topik plus tautan *"Pelajari lebih dulu"*. |

🔴 **Satu frasa di tabel Bulan 3 ternyata menyesatkan: *"relasi sudah ada di
skema"*.** Benar tentang skemanya, dan itulah yang membuatnya menyesatkan.
Diperiksa ke kode: `TechnologyRelationship.Create()` punya **nol pemanggil di
kode produksi**, agregat `Technology` **tidak punya metode untuk menambah
relasi** sama sekali, tidak ada endpoint, tidak ada medan di
`TechnologyResponse`, tidak ada tempat menampilkannya, dan **nol baris** di
basis data. Yang ada baru tabelnya. Ditagih di
[#55](../../issues/55), berikut empat keputusan yang harus diambil lebih dulu.

> ✅ **Dijawab 2026-09-17 — [ADR-023](adr/ADR-023-knowledge-graph-dasar.md).**
> Keempat keputusannya diambil dan mekanismenya dibangun. Membangunnya membongkar
> bahwa tabel yang "sudah ada" itu pun tidak utuh: ujung tujuannya tanpa kunci
> asing, dan tulis pertamanya akan membalas 500. Paragraf di atas dibiarkan
> sebagai rekaman keadaan 11 September.

💡 Ini **pengulangan ketiga** pola yang sama di repo ini — sesudah `MarkReviewed()`
dan `Publish()` di Sesi 11. Sapuan yang menemukan ketiganya sama dan murah:
*"penegak/entitas ini punya berapa pemanggil di kode produksi?"*

⚠️ Ukuran selesai Bulan 3 di tabel atas — *"orang bisa menemukan halaman tanpa
menebak URL"* — **sudah tercapai untuk bidang dan topik**. Yang belum tercakup
pencarian: kelima bagian isi halaman ([#54](../../issues/54)); teks yang
benar-benar tercetak di situs — *"Dasar HTTP dan JSON-RPC"* — masih menjawab nol.

Sejak 2026-09-17 ukuran itu **diukur, bukan diklaim**: penelusuran dari `/` yang
mengikuti setiap tautan mencapai **20 dari 20** halaman `/teknologi/*` yang dikenal
API di pengembangan dan **14 dari 14** di bentuk produksi — dan kendalinya
dibuktikan merah (tautan topik dibuang → keenam topik tak tercapai). Di produksi
angka itu **14 karena produksi nol topik**, bukan karena ada yang tersembunyi.

---

### Empat perubahan yang paling menentukan

1. **Deployment naik dari Bulan 6 ke Bulan 1.** Ini perubahan terpenting di
   halaman ini. Yang tayang lebih awal punya lima bulan untuk memperbaiki
   kejutannya; yang tayang di bulan terakhir tidak punya waktu sama sekali.
2. **Bulan 1 tidak lagi mengejar fitur, tapi mengejar URL.** Menu yang tayang
   dengan status `kurasi` yang jujur sudah berguna bagi pembaca dan sudah bisa
   diperbaiki bertahap. Yang tidak bisa diperbaiki bertahap adalah yang tidak
   pernah tayang.
3. **Skema data pindah dari Bulan 2 ke Bulan 1, tapi setelah PR #23 masuk.**
   Bentuk kontennya sudah dikunci [ADR-012](adr/ADR-012-template-halaman.md), jadi
   skema tidak lagi menebak.
4. **News Automation dipecah.** Yang tersisa di rencana cuma arXiv, yang jelas
   boleh diterbitkan ulang dan tidak memakan token. GitHub Trending **dicoret**:
   ia bukan integrasi melainkan pembangunan dari nol berikut penyimpanan riwayat
   bintang harian — pekerjaan sebulan penuh untuk fitur pelengkap.

### Yang dicoret dari enam bulan, dan alasannya

| Dicoret | Alasan |
|---|---|
| **Badge & Achievement** | Menuntut autentikasi + penyimpanan progres + aturan permainan. Tiga pekerjaan demi lencana, sementara belum ada satu halaman pun yang tuntas. |
| **GitHub Trending** | Tidak ada API-nya. Dibangun dari nol berikut riwayat bintang harian. |
| **Progress Tracker** | Butuh login; login tidak dipasang di V1 ([ADR-013](adr/ADR-013-autentikasi.md)). |
| **Berita non-arXiv** | Terganjal [#17](../../issues/17), yang hanya bisa dijawab pemilik. |
| **Ringkasan AI untuk berita** | Biaya token **berulang tiap hari**. Menyusul setelah pagu terbukti aman sebulan penuh. |

Semuanya masuk **V1.1**, bukan hilang. Yang dicoret permanen hanya GitHub
Trending.

---

## Cara mengukur kemajuan

Satu angka, dan sengaja yang paling sulit dipalsukan:

> **Berapa topik berstatus `tinjau`.**

Bukan berapa halaman ada — mesin bisa membuat delapan puluh dua dalam semenit.
Bukan berapa baris kode. Bukan berapa fitur menyala.

| Titik | Target `tinjau` | Dari mana |
|---|---|---|
| Akhir Bulan 2 | **7** | AI Agents |
| Akhir Bulan 4 | **22** | + Cloud & Infrastructure (7), IoT (8) |
| Akhir Bulan 6 | **22** | tidak bertambah — Bulan 5 dan 6 membangun AI Mentor dan Labs, bukan menulis konten |
| V1.1 | 42 | + AI & ML (7), Cybersecurity (6), Data Engineering (7) |

Angkanya diambil dari kolom "Topik" di
[ADR-010](adr/ADR-010-taksonomi-bidang.md), bukan dari perkiraan.

Angka Bulan 6 sengaja **tidak** dinaikkan. Enam bulan ini hanya sanggup
menuntaskan **tiga dari enam bidang prioritas 1**, dan menuliskan 42 di sana
akan menjadikan rencana ini persis seperti rencana yang digantikannya — janji
yang tidak punya bulan untuk mengerjakannya. **V1 selesai kalau di akhir Bulan 6
angkanya 22 dan ketiga fitur di atas jalan.** Sisanya V1.1.
