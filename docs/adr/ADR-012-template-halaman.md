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

---

## Pembaruan 2026-10-02 — bagian isi bisa dibuang (jalan perbaikan), roadmap belum

Alur bergerbang [ADR-021](ADR-021-jalan-menuju-tinjau.md) dibangun mendahului
[#40](https://github.com/xtheoputra/techverse-x/issues/40), dan ADR-021 menandai bahwa
jalan **membuang bagian isi** harus ikut ada — tanpanya, bagian yang keliru ditulis ke
produksi lewat gerbang tidak punya jalan perbaikan. Tiga dari empat bagian kini bisa
dibuang:

- `DELETE …/resources/{id}` dan `DELETE …/projects/{id}` (Id-nya dari respons),
  `DELETE …/tools/{toolSlug}` (slug katalognya — **melepas TAUTAN, bukan menghapus
  alat**; alatnya masih bisa ditautkan lagi).
- Semua **idempoten** (tujuan tak ada → 200 tanpa-operasi, seperti DELETE sisi
  [ADR-023](ADR-023-knowledge-graph-dasar.md)), di bawah cabang `editorialWrites`
  (ADR-020), masuk `SemuaEndpointTulis` → ketiga uji ADR-020 langsung menjaganya.
- **Tidak menurunkan `HumanReviewed`.** `Touch` dan bagian Konsekuensi ADR ini sudah
  menalarnya untuk MENAMBAH (*"memperbaiki satu tautan mati tidak boleh membuang nilai
  kerja pemeriksanya"*); buang adalah separuh lain dari "memperbaiki tautan mati", jadi
  ia mengikuti aturan yang sama. ⚠️ Satu akibat jujur: topik `tinjau` bisa menjadi
  tak-lengkap (`MissingSections` terisi) tanpa turun tingkat — re-review tanggung jawab
  penyunting, persis seperti menambah bagian tidak otomatis menaikkan tingkat.

🔴 **Langkah ROADMAP belum bisa dibuang.** Nomor langkah berurut tanpa lubang (bagian 1:
*"roadmap berlubang mustahil"*), jadi membuang satu langkah menuntut keputusan penomoran
ulang (geser 1..n, atau biarkan lubang dan ubah `AddRoadmapStep` ke `max+1`) yang belum
diambil. Mengganti prasyarat tetap lewat `PUT …/roadmap/prasyarat`. Ditinggalkan sebagai
langkah berikutnya.

**Terukur:** +4 uji unit (`BagianIsiHalamanTests`) + 3 uji integrasi
(`ContentSectionEndpointTests`, termasuk end-to-end *buang tidak menggugurkan tinjau*).
Hitungan repo → **112 unit + 76 integrasi**, `run.ps1 verify` hijau.

## Pembaruan 2026-10-06 — mengganti teks menggugurkan tinjau; menambah dan membuang tidak

Keputusan pemilik atas pertanyaan yang tersisa di [#79](https://github.com/xtheoputra/techverse-x/issues/79): pada topik `HumanReviewed`, **mengganti** teks yang sudah ada
**menurunkannya ke `MachineDrafted`** — seperti `Update()` — sedangkan **menambah** dan
**membuang** bagian tetap tidak (Pembaruan 2026-10-02 di atas, `Touch`).

- **Yang berubah, dua metode:** `Technology.SetPrerequisite` (judul atau uraian langkah 0
  diganti dengan yang berbeda) dan `Technology.AttachTool` (catatan alat yang *sudah
  tertaut* diganti, termasuk dikosongkan). Keduanya memakai satu pembantu privat
  `ExpireReview()` bersama `Update()`: turun ke `MachineDrafted`, `ReviewedAt` dan
  `ReviewedBy` dinolkan, `TechnologyReviewExpired` terbit. Satu tempat, supaya "apa yang
  terjadi saat pemeriksaan gugur" tak bisa menyimpang antar jalan.
- **Batasnya, sengaja sempit:** yang menggugurkan hanya isi tersimpan yang diganti dengan
  isi **berbeda** sesudah dipangkas. Mengulang isi yang sama (PUT idempoten, spasi di
  tepi) tidak — pemeriksa membaca teks yang persis sama. Menautkan alat **baru** adalah
  menambah, jadi tidak. Pada topik yang belum diperiksa tak terbit
  `TechnologyReviewExpired`.
- **Kenapa di sini, bukan menambah-buang:** menambah satu tautan dan membuang satu
  tautan tak membuat teks yang dibaca pemeriksa hilang; mengganti prasyarat atau catatan
  membuatnya hilang dan menempelkan "sudah diperiksa manusia" pada teks lain — persis yang
  bagian 1 larang. Jalan kembali tetap satu: `/tinjau` lagi.
- **Yang tidak berubah:** pemasang `isi/` tetap **mengunci** topik `tinjau` (ia tak pernah
  menyentuhnya); jalan ini melindungi pemanggil API langsung saat sakelar tulis
  ([ADR-020](ADR-020-permukaan-tulis-api.md)) menyala. Rute `PUT /api/v1/technologies/{slug}`
  untuk `name` dan `summary` **belum** dibangun — itu keputusan terpisah di #79 — tetapi
  begitu ada, `Update()` sudah menurunkan dengan benar.

**Terukur:** merah dulu lalu hijau di dua lapis. Unit: 11 uji baru
(`GantiTeksMenurunkanTinjauTests`), **4 merah** di domain lama (judul, uraian, catatan
diganti, catatan dikosongkan) dan 7 pagar hijau sejak awal (isi sama, spasi tepi, alat
baru, topik belum diperiksa, ulang pemeriksaan). HTTP: 2 uji integrasi baru
(`ContentSectionEndpointTests`), keduanya **merah** tanpa perubahan domain
(`Expected: "MachineDrafted" / Actual: "HumanReviewed"`) dan hijau dengannya — lewat
`TopicMutation` sungguhan, jadi yang terbukti termasuk bahwa penurunannya **tersimpan**.

## Pembaruan 2026-10-07 — tiga pintu "mengganti" baru, dan satu koreksi atas pembaruan di atas

ADR-028 Tahap 2 (ADR-nya sendiri masih di [PR #84](https://github.com/xtheoputra/techverse-x/pull/84), jadi belum bisa ditautkan dari `main`) membangun jalan ubah yang ditagih
[#79](https://github.com/xtheoputra/techverse-x/issues/79). Aturannya tetap satu — **mengganti teks yang dibaca
pemeriksa menggugurkan `tinjau`; mengulang teks yang sama tidak** — dan kini berlaku di tiga pintu baru.

- **`PUT /api/v1/technologies/{slug}`** (`name`, `summary`) memanggil `Technology.Update()`.
- **`PUT /api/v1/technologies/{slug}/roadmap/{order}`** memanggil `Technology.ReplaceRoadmapStep()`. Nomor di
  alamat itu **alamat langkah yang sudah ada, bukan isian**: nomor yang belum ada ditolak 400, jadi roadmap
  berlubang tetap mustahil. Nomor `0` sama dengan `PUT …/roadmap/prasyarat`. Langkah baru tetap hanya lewat
  `POST …/roadmap` di ujung. Mengganti langkah dilakukan **di tempat** (barisnya tetap barisnya).
- **`PUT /api/v1/tools/{slug}`** (`name`, `summary`, `homepage`) memanggil `Tool.Update()`.

### 🔴 Koreksi: `Update()` belum "menurunkan dengan benar"

Pembaruan 2026-10-06 di atas menulis bahwa begitu rute `PUT` topik ada, *"`Update()` sudah menurunkan dengan
benar"*. **Itu keliru, dan ketahuan tepat karena rute itu akhirnya dibangun.** `Update()` menggugurkan `tinjau`
pada panggilan **apa pun** — termasuk dengan teks yang persis sama — padahal `SetPrerequisite` dan `AttachTool`
sudah memakai aturan "hanya bila berbeda" sejak #81. Ia tak pernah ketahuan karena tak punya pemanggil produksi
([#68](https://github.com/xtheoputra/techverse-x/issues/68) butir 1). Terukur: dua uji baru memerah di domain lama
(`Expected: HumanReviewed / Actual: MachineDrafted`). Kini `Update()` **tanpa-operasi** bila nama, ringkasan, dan
bidangnya sama sesudah dipangkas: tak ada `TechnologyUpdated`, `UpdatedAt` tak bergerak, pemeriksaan bertahan.
Tanpa ini, satu `PUT` yang diulang klien akan membuang kerja pemeriksa tanpa alasan. Rute `PUT` topik kini
pemanggil produksi pertama `Update()`, dan menutup butir 1 di #68.

### Alat dipakai bersama, jadi menggantinya bisa menggugurkan topik lain

Nama dan ringkasan alat tampil di halaman **setiap** topik yang menautkannya ([ADR-015](ADR-015-skema-data-v1.md)),
termasuk topik yang sudah `tinjau`. Mengganti keduanya di katalog adalah mengganti teks yang dibaca pemeriksa —
sama dengan mengganti catatan alat — jadi `PUT …/tools/{slug}` menggugurkan **semua topik `tinjau` yang
menautkan alat itu**, dalam satu `SaveChanges` dengan katalognya (katalog dan topiknya tak pernah terlihat
setengah berubah). Hanya topik `tinjau` yang dimuat; topik lain tak punya pemeriksaan untuk digugurkan.

- **`homepage` bukan teks.** Ia tautan; mengubahnya tidak menggugurkan apa pun — alasan yang sama dengan
  menambah dan membuang sumber di Pembaruan 2026-10-02.
- **Slug alat tak bisa diubah**: ia identitas yang dirujuk berkas `isi/` dan tautan topik.
- Aturan ini punya akibat yang harus terbaca di tempat pemanggilnya: pemasang `isi/ --ganti`
  yang mengganti definisi alat bersama menyebutnya di log (*"topik tinjau yang menautkannya gugur ke draf"*).

### Lebar kolom dijaga di domain

Ketiga pintu — dan jalan tambah yang sudah ada (`SetPrerequisite`, `AddRoadmapStep`) — menolak teks yang
melebihi lebar kolomnya dengan **400 bermedan**, bukan membiarkannya sampai ke PostgreSQL sebagai 500 (kelas
cacat [#26](https://github.com/xtheoputra/techverse-x/issues/26)). Angkanya satu sumber: konstanta di
`Technology`, `RoadmapStep`, dan `Tool` dipakai konfigurasi EF dan validator. Muatan kepanjangan di jalan
tambah roadmap sebelumnya hanya ditolak kolom basis data; kini ditolak domain.

### Yang tetap tidak ada

Membuang **langkah roadmap** (nomornya berurut tanpa lubang), menghapus **alat katalog** (perlu pemeriksaan
"tak ada topik yang masih menautkan"), dan memindahkan topik **antar-bidang** (ADR-009) masih tanpa jalan.
Ketiganya tetap ditagih di #79.

**Terukur:** unit — 17 uji baru (`JalanUbahTests`), dua di antaranya merah di `Update()` lama; HTTP — 23 uji
baru (`JalanUbahEndpointTests`) plus tiga entri baru di `SemuaEndpointTulis`. Dua sabotase dibuktikan merah:
penggugur topik bersama dilepas dari `UpdateToolHandler` (uji alat bersama memerah), dan `MapUpdateTechnology`
dipindah ke atas batas `editorialWrites` (`Bawaan_TIDAK_memasang_satu_pun_endpoint_tulis` memerah). Keduanya
dipulihkan.
