# ADR-012 — Template halaman, tingkat kematangan konten, dan siapa yang menulis

**Status:** Diterima. Menutup Issue [#10](https://github.com/xtheoputra/techverse-x/issues/10), [#11](https://github.com/xtheoputra/techverse-x/issues/11), [#18](https://github.com/xtheoputra/techverse-x/issues/18), [#19](https://github.com/xtheoputra/techverse-x/issues/19).
**Tanggal:** 2026-09-04

## Konteks

Empat issue ini sebenarnya satu pertanyaan: **apa isi satu halaman, dan dari mana
isinya datang.** Menjawabnya terpisah-pisah menghasilkan jawaban yang saling
bertabrakan, jadi keempatnya dijawab di sini.

`KERANGKA.md` menyimpan dua template yang berbeda (2.4 versi tujuh bagian, dan
contoh halaman AI Agents versi lima bagian), belum menetapkan siapa penulis ±287
blok konten, dan dua tempat yang justru jantung produknya — 1.4 "Roadmap Engineer
Masa Depan" dan bagian "Learning Roadmap" di contoh halaman — **kosong sama
sekali**.

Di atas itu semua, audit menemukan **seluruh dua belas entri peta berstatus
sebagian usang.** Kalau kurikulum dibangun dari peta yang belum dikoreksi,
halaman-halamannya lahir usang sejak hari pertama — berapa pun cara penulisannya.

## Keputusan

### 1. Template: Versi B, dengan prasyarat dilipat ke dalam roadmap

Template wajib tiap halaman teknologi:

1. **Overview**
2. **Learning Roadmap** — **langkah 0-nya adalah prasyarat**
3. **Tools**
4. **Mini Project**
5. **Resources** — dokumentasi resmi, video, paper, repositori

*Skill prerequisite* dari Versi A tidak hilang; ia jadi langkah pertama roadmap.
Prasyarat pada hakikatnya **adalah** langkah pertama belajar, dan menaruhnya di
bagian terpisah membuat pembaca membacanya dua kali dan penulis menulisnya dua
kali.

Alasan memilih B, dan ini yang menentukan: **Versi A menjadikan "Berita terbaru"
dan "Paper terbaru" sebagai bagian wajib tiap halaman.** Artinya kelengkapan
setiap halaman bergantung pada pipeline berita — yang justru terganjal
[Issue #17](https://github.com/xtheoputra/techverse-x/issues/17), pertanyaan hukum yang hanya bisa dijawab
pemilik sendiri. Satu blocker hukum akan membuat 82 halaman berstatus "belum
lengkap" selamanya. Dengan B, berita dan paper adalah *resource* — halaman tetap
utuh tanpa mereka.

Selisih beban yang ikut hilang: 2 bagian × 82 topik = **164 blok konten yang
tidak perlu ditulis.**

### 2. Tiga tingkat kematangan, dan ditampilkan ke pembaca

Setiap halaman punya satu status yang **terlihat pembaca**, bukan hanya di basis
data:

| Status | Isi | Dibuat oleh |
|---|---|---|
| `kurasi` | Overview singkat + Resources terpilih. Tidak ada klaim panjang. | manusia memilih tautannya |
| `draf` | Kelima bagian terisi, **belum diperiksa manusia** | AI menyusun |
| `tinjau` | Kelima bagian terisi dan **sudah diperiksa manusia** | AI menyusun, pemilik menyunting |

**Aturan keras: halaman berstatus `draf` tidak pernah ditampilkan tanpa label
yang terbaca pembaca.** Ini syarat yang membuat opsi "AI membuat, pemilik
menyunting" jadi jujur alih-alih jadi tumpukan teks tak terverifikasi. Tanpa
label itu, platform "sumber belajar" berubah jadi mesin penyebar kekeliruan yang
kelihatan rapi.

Ini menjawab Issue #10 sebagai **gabungan ketiga opsinya, dengan urutan**: semua
halaman lahir `kurasi` (opsi 3 — paling jujur dan paling murah dirawat), naik ke
`draf` lewat AI (opsi 1), dan hanya naik ke `tinjau` lewat tangan manusia
(opsi 2). Yang ditolak bukan salah satu opsinya, melainkan **memilih satu opsi
untuk semua halaman sekaligus.**

### 3. Target isi yang konkret

Issue #10 menanyakan berapa halaman yang benar-benar terisi di akhir Bulan 1.
Jawabannya:

- **Akhir Bulan 1: satu bidang tuntas.** AI Agents — 7 topik berstatus `tinjau`.
  Seluruh 13 bidang lain tayang sebagai `kurasi`.
- Menu 82 halaman kosong bukan produk. **Satu bidang yang bisa diselesaikan
  seseorang dari awal sampai akhir adalah produk.**

Bidangnya AI Agents karena itu bidang yang paling dikuasai pemilik, dan karena
ia yang paling membutuhkan A2A dan MCP — dua hal yang juga jadi keputusan
arsitektur di [ADR-007](ADR-007-agent-platform.md) dan
[ADR-014](ADR-014-mcp-dan-penyedia-ai.md). Satu bidang ini karenanya sekaligus
menguji rancangan teknisnya sendiri.

### 4. Roadmap purwarupa (Issue #19)

**Satu roadmap dikerjakan lengkap lebih dulu: AI Agents.** Bentuknya lalu jadi
cetakan untuk semua roadmap lain, dan baru setelah itu skema roadmap di
[ADR-015](ADR-015-skema-data-v1.md) boleh dianggap terkunci.

`KERANGKA.md` 1.4 "Roadmap Engineer Masa Depan" **bukan roadmap teknologi** — itu
lintasan karier, dan isinya jadi milik Career Mode
([ADR-009](ADR-009-tulang-punggung-navigasi.md)), dijadwalkan V1.1. Membiarkannya
kosong di 1.4 bukan kelalaian; ia memang tidak punya tempat di sana.

### 5. Koreksi audit diterapkan sebelum menulis, bukan sesudah (Issue #18)

Peta teknologi dikoreksi **sebelum** satu halaman pun ditulis. Enam koreksi yang
paling berbahaya karena mengajari orang barang mati:

| Salah | Benar |
|---|---|
| AutoGen | maintenance mode Okt 2025 → **Microsoft Agent Framework** |
| Sora | app tutup 26 Apr 2026, API dijadwalkan tutup 24 Sep 2026 |
| GPT-5 | pensiun dari ChatGPT 13 Feb 2026 |
| "humanoid sudah dipakai di rumah" | tidak terverifikasi; >60% dari 22.000+ unit H1 2026 untuk hiburan & riset, gudang ~5% |
| Tesla Optimus sebagai bukti | contoh terlemah — Gen 3 belum diluncurkan |
| Windsurf, Gemini CLI | → Devin Desktop, → Antigravity CLI |

Ditambah lubang konseptual terbesarnya: **A2A tidak disebut sama sekali** —
sudah diperbaiki di [ADR-010](ADR-010-taksonomi-bidang.md).

**Aturan yang menyertainya: klaim yang tidak bisa diverifikasi independen tidak
masuk kurikulum sama sekali** — tidak dikutip, tidak dibantah, tidak disebut.
Contoh yang audit sendiri tandai: akuisisi Cursor oleh SpaceX senilai USD 60
miliar. Kurikulum bukan tempat menampung rumor, dan menuliskannya untuk kemudian
membantahnya tetap menyebarkannya.

## Konsekuensi

- **Status halaman jadi kolom di basis data, bukan konvensi**
  ([ADR-015](ADR-015-skema-data-v1.md)). Sesuatu yang harus terlihat pembaca
  tidak boleh hidup di kepala penulisnya.
- Halaman `kurasi` **tidak dianggap utang.** Ia bentuk akhir yang sah untuk
  bidang prioritas 3. Yang jadi utang cuma halaman `draf` yang tidak pernah naik
  ke `tinjau`.
- Ukuran kemajuan proyek berubah dari "berapa halaman ada" menjadi **"berapa
  halaman berstatus `tinjau`"**. Angka pertama mudah dipalsukan mesin; angka
  kedua tidak bisa.

---

## Pembaruan 2026-09-17 - relasi antar-topik bukan bagian keenam

[ADR-023](ADR-023-knowledge-graph-dasar.md) menambah blok **"Topik terhubung"** di
halaman topik. Supaya bagian 1 ADR ini tetap benar, aturannya ditulis di sini:

- Blok itu dirender **SESUDAH** kelima bagian template dan **tidak pernah**
  dihitung di `missingSections`. Ia disembunyikan kalau kosong - pengecualian
  sadar dari *"bagian kosong tetap ditampilkan"*, sebab relasi bukan sesuatu yang
  wajib diisi, dan baris "belum ada relasi" akan menandai setiap halaman tidak
  lengkap secara palsu.
- **Langkah 0 roadmap tetap satu-satunya prasyarat PROSA**: keterampilan yang
  belum tentu punya halaman di situs ini.
- **Sisi `Requires` adalah navigasi ke halaman yang ADA.** Topik yang sudah jadi
  tujuan sisi **tidak diulang** di langkah 0 - itu yang menjaga alasan bagian 1
  (*"membuat pembaca membacanya dua kali dan penulis menulisnya dua kali"*) tetap
  berlaku.
- Setiap judul yang ditautkan membawa **label kematangannya sendiri**. Aturan keras
  bagian 2 berlaku sampai ke tautan, dan dijamin tipe
  (`TechnologySummaryResponse`), bukan ketelitian.
