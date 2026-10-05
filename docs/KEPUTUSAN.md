# Daftar Keputusan — 22 issue Fase 0 dan Fase 1

**Tanggal:** 2026-09-04
**Diputuskan oleh:** asisten, atas pendelegasian pemilik ("saya serahkan ke Anda saja").

Berkas ini yang berlaku kalau isinya berbeda dari `KERANGKA.md`. `KERANGKA.md`
tetap disimpan **apa adanya** sebagai rekaman kata-kata pemilik — tidak ada satu
kalimat pun di sana yang diubah oleh keputusan-keputusan ini.

**Semua keputusan di bawah bisa dibatalkan.** Cara membatalkannya ada di bagian
akhir.

---

## Benang merah yang menyatukan ke-22 jawaban

> **Proyek ini mati karena empat puluh halaman setengah jadi, bukan karena kurang
> fitur.**

Setiap jawaban di bawah dipilih supaya konsekuensinya menuju satu arah: **sedikit
yang tuntas, bukan banyak yang menggantung.** Kalau ada satu jawaban yang tidak
masuk akal, kemungkinan besar yang perlu diperiksa adalah kalimat di atas — bukan
jawaban itu sendiri.

---

## Struktur & navigasi

| # | Pertanyaan | Keputusan | Rujukan |
|---|---|---|---|
| [1](https://github.com/xtheoputra/techverse-x/issues/1) | Tulang punggung navigasi | **Hibrida** — nav utama berisi fungsi, tiap bidang punya URL kanonik `/teknologi/<slug>` | [ADR-009](adr/ADR-009-tulang-punggung-navigasi.md) |
| [2](https://github.com/xtheoputra/techverse-x/issues/2) | Nama produk | **TechVerse X.** Repo sudah cocok, tidak perlu diganti. "Labs" tetap dipakai — tapi hanya sebagai nama bagian aplikasi | [ADR-011](adr/ADR-011-nama-produk.md) |
| [3](https://github.com/xtheoputra/techverse-x/issues/3) | Nasib 41 submenu | **Disusun ulang.** 14 bidang, 82 topik. Angka "41" dan "±287 blok" tidak berlaku lagi | [ADR-010](adr/ADR-010-taksonomi-bidang.md) |
| [4](https://github.com/xtheoputra/techverse-x/issues/4) | Batas AI & ML vs AI Agents | **Usul audit diterima** — AI & ML = fondasi (7), AI Agents = orkestrasi (7). **A2A wajib masuk** | [ADR-010](adr/ADR-010-taksonomi-bidang.md) |
| [5](https://github.com/xtheoputra/techverse-x/issues/5) | Rumah Edge AI & Data Engineering | **Keduanya jadi bidang sendiri.** Yang tinggal di IoT cuma satu halaman jembatan TinyML | [ADR-010](adr/ADR-010-taksonomi-bidang.md) |
| [6](https://github.com/xtheoputra/techverse-x/issues/6) | Isi ulang Renewable Energy | **Cakupan audit (8 topik) diterima.** Fusion pindah ke `/future`; Solid-State Battery tinggal di bawah "Integrasi jaringan & penyimpanan", berlabel tegas sebagai penyimpanan | [ADR-010](adr/ADR-010-taksonomi-bidang.md) |
| [7](https://github.com/xtheoputra/techverse-x/issues/7) | AR/VR → XR? | **Ya, jadi XR (AR/VR/MR)**, "AR/VR" tetap sebagai alias pencarian. Bidangnya **dipertahankan tapi tidak diinvestasikan** — prioritas 3 | [ADR-010](adr/ADR-010-taksonomi-bidang.md) |
| [8](https://github.com/xtheoputra/techverse-x/issues/8) | Cloud atau Cloud & Infrastructure | **Nama lebar dipertahankan.** Penyempitan di 3.2 dinilai penyingkatan penulisan, bukan keputusan | [ADR-010](adr/ADR-010-taksonomi-bidang.md) |
| [9](https://github.com/xtheoputra/techverse-x/issues/9) | Rumah enam fitur menggantung | Semua dapat rumah; lima masuk V1.1, **Badge & Achievement dicoret**. **Peran kerja hanya di Career Mode** — submenu "AI Engineer" dicabut dari taksonomi teknologi | [ADR-009](adr/ADR-009-tulang-punggung-navigasi.md) |

Butir **E10** (`KERANGKA.md` 3.2, "teknologi baru yang belum muncul") — satu-satunya
dari 30 butir yang belum pernah punya issue — ikut dijawab: ia **bagian `/future`,
bukan bidang ke-15.** Alasannya di [ADR-009](adr/ADR-009-tulang-punggung-navigasi.md).

## Konten

| # | Pertanyaan | Keputusan | Rujukan |
|---|---|---|---|
| [10](https://github.com/xtheoputra/techverse-x/issues/10) | Siapa menulis ±287 blok | **Ketiga opsi dipakai berurutan, bukan salah satu.** Semua halaman lahir `kurasi` → naik `draf` lewat AI → naik `tinjau` lewat tangan manusia. **Halaman `draf` tidak pernah tayang tanpa label** | [ADR-012](adr/ADR-012-template-halaman.md) |
| [11](https://github.com/xtheoputra/techverse-x/issues/11) | Template halaman | **Versi B (5 bagian)**, dengan *Skill prerequisite* dilipat jadi **langkah 0 roadmap**. Menghemat 164 blok, dan melepas kelengkapan halaman dari pipeline berita yang terganjal hukum | [ADR-012](adr/ADR-012-template-halaman.md) |
| [18](https://github.com/xtheoputra/techverse-x/issues/18) | Koreksi audit ke peta | **Diterapkan sebelum satu halaman pun ditulis.** Plus aturan: klaim yang tidak bisa diverifikasi independen tidak masuk kurikulum sama sekali | [ADR-012](adr/ADR-012-template-halaman.md) |
| [19](https://github.com/xtheoputra/techverse-x/issues/19) | Dua roadmap kosong | **Satu purwarupa dulu: AI Agents.** `KERANGKA.md` 1.4 bukan roadmap teknologi melainkan lintasan karier — isinya milik Career Mode | [ADR-012](adr/ADR-012-template-halaman.md) |

**Target isi yang konkret untuk pertanyaan turunan di #10** ("berapa halaman
benar-benar terisi di akhir Bulan 1"): 14 bidang tayang berstatus `kurasi` di
Bulan 1, dan **satu bidang tuntas (AI Agents, 7 topik `tinjau`) di Bulan 2**.

## Arsitektur

| # | Pertanyaan | Keputusan | Rujukan |
|---|---|---|---|
| [12](https://github.com/xtheoputra/techverse-x/issues/12) | Naik ke .NET 10 | **Ya** — sudah terpasang dan CI hijau di atasnya | [ADR-002](adr/ADR-002-dotnet.md) |
| [13](https://github.com/xtheoputra/techverse-x/issues/13) | Orkestrasi agen | **Opsi 1 — buang LangGraph, pakai Microsoft Agent Framework.** Untuk pengembang tunggal, dua runtime adalah kesalahan termahal yang bisa diambil tanpa terasa | [ADR-007](adr/ADR-007-agent-platform.md) |
| [14](https://github.com/xtheoputra/techverse-x/issues/14) | Pola autentikasi | **V1 tidak punya login sama sekali.** Kalau nanti dipasang: **Clerk sebagai IdP, .NET verifikasi RS256 via JWKS sendiri**, `[Authorize]` wajib di tiap endpoint. Syarat pindah sudah ditetapkan: MAU 40.000 | [ADR-013](adr/ADR-013-autentikasi.md) |
| [15](https://github.com/xtheoputra/techverse-x/issues/15) | pgvector atau Qdrant | **pgvector saja.** Pencarian hibrida dijawab dengan `tsvector` PostgreSQL, bukan dengan basis data kedua | [ADR-005](adr/ADR-005-qdrant.md) · [ADR-015](adr/ADR-015-skema-data-v1.md) |
| [16](https://github.com/xtheoputra/techverse-x/issues/16) | Peran MCP & penguncian penyedia | **MCP ke dalam untuk V1**; MCP ke luar dicatat sebagai peluang, syaratnya minimal satu bidang `tinjau`. **Penyedia tidak dikunci** — abstraksinya gratis dari keputusan #13 | [ADR-014](adr/ADR-014-mcp-dan-penyedia-ai.md) |
| [20](https://github.com/xtheoputra/techverse-x/issues/20) | Skema basis data | **Dirancang** — 9 entitas, **dua sumbu status yang tidak boleh digabung**, graf tetap di PostgreSQL, vektor 1536 dimensi. ✅ **Implementasinya sudah mendarat di `main`** lewat [#25](https://github.com/xtheoputra/techverse-x/issues/25) — `Field`, `ContentMaturity`, dan penegaknya bisa diperiksa langsung, bukan lewat cabang | [ADR-015](adr/ADR-015-skema-data-v1.md) |

## Rencana & biaya

| # | Pertanyaan | Keputusan | Rujukan |
|---|---|---|---|
| [21](https://github.com/xtheoputra/techverse-x/issues/21) | Lengkapi rencana 6 bulan | **Disusun ulang.** Perubahan terpenting: **Deployment naik dari Bulan 6 ke Bulan 1.** GitHub Trending dicoret; Badge dicoret; sisanya ke V1.1 | [RENCANA-V1](RENCANA-V1.md) |
| [22](https://github.com/xtheoputra/techverse-x/issues/22) | Anggaran biaya bulanan | **Pagu USD 60/bulan**, alarm di 80%. Tiga pos jadi nol **sebagai konsekuensi keputusan arsitektur**, bukan penghematan belakangan | [ADR-016](adr/ADR-016-pagu-biaya.md) |

## Yang TIDAK saya putuskan

| # | Pertanyaan | Kenapa tetap milik Anda |
|---|---|---|
| [17](https://github.com/xtheoputra/techverse-x/issues/17) | ToS OpenAI & Microsoft | **Tetap terbuka, tapi separuhnya sudah gugur.** ✅ **Langkah 1 selesai 7 Sep 2026: kedua ToS SUDAH DIBACA** di peramban sungguhan — blokir 403 hanya menyasar pengambil otomatis. 🔴 Dan pembacaannya membalik satu asumsi: **ToS OpenAI tidak menjawab pertanyaan penerbitan ulang berita** — ia mengatur *Services* (ChatGPT/DALL·E), bukan isi editorial situs; untuk API berlaku **Business Terms terpisah** yang belum dibaca. Microsoft ternyata **tiga lapis**, dan klausul terkenalnya adalah **syarat dari sebuah izin** (efek praktis sama: judul + tautan aman, ringkasan tidak). Rekaman lengkap di [`AUDIT-KELAYAKAN.md`](../AUDIT-KELAYAKAN.md#pembaruan-7-september-2026--ketentuan-layanan-sudah-dibaca). **Yang tetap milik Anda: langkah 2 (batas kutipan wajar) dan langkah 3 (tinjauan hukum bila komersial).** |

Yang saya kerjakan sebatas **memundurkan blocker-nya**: V1 hanya menerbitkan
ulang **arXiv** (satu-satunya sumber yang jelas boleh, metadatanya CC0), dan untuk
sumber lain berlaku aturan sementara "boleh disimpan, yang ditampilkan hanya
judul + tautan + sumber". Dengan itu **#17 berhenti menghalangi V1**, dan tetap
menghalangi apa pun di luar arXiv — sebagaimana mestinya. Aturan itu **pengurang
paparan, bukan nasihat hukum.**

---

## Kalau Anda tidak setuju

Tiap keputusan ditulis di ADR-nya lengkap dengan alasan **dan konsekuensi kalau
dibalik**. Yang perlu Anda lakukan cuma bilang nomornya.

Empat yang paling mudah dibalik, kalau Anda mau mulai dari yang murah:

| # | Membalikkannya berarti |
|---|---|
| [15](https://github.com/xtheoputra/techverse-x/issues/15) pgvector → Qdrant | Ganti tempat vektor disimpan. Keduanya duduk di bawah abstraksi `Microsoft.Extensions.VectorData` yang sama, dan keyakinan audit di butir ini memang cuma **sedang** |
| [2](https://github.com/xtheoputra/techverse-x/issues/2) nama produk | Ganti teks. Tidak ada URL yang memuat nama produk |
| [7](https://github.com/xtheoputra/techverse-x/issues/7) XR dinaikkan prioritasnya | Ganti satu kolom prioritas, lalu tulis kontennya |
| [22](https://github.com/xtheoputra/techverse-x/issues/22) pagu biaya | Angka $60 adalah perkiraan dari daftar pos, **belum pernah diuji tagihan sungguhan** |

Dua yang paling mahal dibalik, jadi paling layak Anda periksa sekarang:

| # | Membalikkannya berarti |
|---|---|
| [1](https://github.com/xtheoputra/techverse-x/issues/1) tulang punggung navigasi | Seluruh bentuk URL berubah. Semakin lama tayang, semakin mahal |
| [13](https://github.com/xtheoputra/techverse-x/issues/13) orkestrasi agen | Kembali ke dua runtime — dan bersamanya empat jebakan yang audit tandai, plus pagu biaya di [ADR-016](adr/ADR-016-pagu-biaya.md) yang batal dengan sendirinya |
