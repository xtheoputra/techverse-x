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

### Keadaan Bulan 1 per 8 September 2026

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
produksi sudah ada dan terbukti melayani situsnya dari basis data kosong (PR
[#31](../../issues/31)), berikut bundel migrasi dan gerbang pemindaian citra.

📦 **Sejak 8 September 2026 ketiganya bukan cuma "bisa dibangun" — ia sudah
TERBIT.** `rilis-citra.yml` berjalan untuk pertama kalinya di `main` `4bfbfff`
dan hijau: dibangun, dipindai (nol CRITICAL/HIGH), lalu didorong ke
`ghcr.io/<pemilik>/techverse-x/{api,migrate,web}` dengan tag SHA maupun `:main`.
Artinya langkah "cari artefaknya" sudah hilang dari daftar pekerjaan penyebaran —
platform apa pun tinggal menariknya, dan syarat menariknya kini tertulis di
[`PENYEBARAN.md`](PENYEBARAN.md#kredensial-untuk-menarik-citra).

Yang belum ada adalah **keputusan platform hosting** — ADR-016 menyebut Azure
hanya di tabel perkiraan biaya, bukan sebagai keputusan — plus **domain** dan
**sertifikat**. Ketiganya menyangkut akun dan uang.

⚠️ Ukuran keberhasilan Bulan 1 di tabel di atas adalah **URL publik yang bisa
dibuka orang lain**. Tiga dari empat butir selesai tidak membuat ukuran itu
tercapai — ia hanya tercapai kalau butir keempat tercapai.

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
