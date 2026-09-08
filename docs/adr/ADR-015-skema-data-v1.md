# ADR-015 — Skema data V1: entitas, dua sumbu status, dan penyimpanan vektor

**Status:** Diterima. **Sebagian sudah mendarat** - lihat bagian Pembaruan di kaki berkas ini.
Menutup Issue [#20](../../../../issues/20).
**Tanggal:** 2026-09-04

## Konteks

`KERANGKA.md` 2.7 baru satu kalimat: *"Setiap teknologi menjadi satu simpul yang
saling terhubung dengan roadmap, resource, proyek, dan berita."* Belum ada
entitas, kolom, maupun relasi.

Issue #20 menyatakan prasyaratnya: template halaman dan taksonomi submenu harus
selesai lebih dulu. Keduanya sudah selesai —
[ADR-012](ADR-012-template-halaman.md) dan
[ADR-010](ADR-010-taksonomi-bidang.md) — jadi skema ini sekarang bisa dikunci.

Kode Fase 1 sudah memuat sebagiannya: agregat `Technology` (dengan `Slug`,
`Summary`, `Category` sebagai **teks bebas sementara**), `TechnologyRelationship`
dengan kunci unik `(From, To, Kind)`, dan enum `TechnologyStatus`.

## Keputusan

### 1. Dua sumbu status, dan keduanya TIDAK boleh digabung

Ini temuan dari membaca kode, bukan dari dokumen, dan yang paling mudah salah
nanti. `TechnologyStatus` yang sudah ada **bukan** tingkat kematangan konten dari
ADR-012. Keduanya sumbu berbeda:

| Sumbu | Enum | Menjawab |
|---|---|---|
| Daur hidup redaksi | `TechnologyStatus` (sudah ada: Draft · Discovered · Verified · Published · Archived) | **Boleh tayang?** |
| Kematangan isi | `ContentMaturity` (baru: `Kurasi` · `Draf` · `Tinjau`) | **Seberapa dipercaya isinya?** |

Keduanya bersilangan secara sah: halaman `Kurasi` yang `Published` adalah bentuk
akhir yang benar untuk bidang prioritas 3, dan halaman `Draf` yang `Published`
adalah keadaan normal — asalkan berlabel.

**Aturan yang menegakkannya:** `Published` + `Draf` wajib menampilkan label ke
pembaca. Ini penegakan ADR-012 di lapisan data, bukan di lapisan tampilan, supaya
tidak bisa hilang karena seseorang lupa memasang komponen.

Kalau suatu saat keduanya terasa mirip dan tergoda digabung: yang hilang adalah
kemampuan menerbitkan halaman kurasi yang jujur. Jangan digabung.

### 2. Entitas V1

| Entitas | Catatan |
|---|---|
| `Field` | 14 bidang dari ADR-010. **`Technology.Category` yang sekarang teks bebas naik jadi kunci asing ke sini** — taksonominya sudah diputuskan, jadi alasan menundanya hilang. |
| `Technology` | Satu topik. `Slug` adalah URL kanonik dari ADR-009. Menyimpan `Overview` (bagian 1 template). |
| `RoadmapStep` | Berurut, milik satu `Technology`. **Langkah 0 = prasyarat** (ADR-012). |
| `Tool` | Entitas sendiri, **relasi banyak-ke-banyak** ke `Technology` — satu tool dipakai lintas topik, dan menyalinnya per halaman menjamin data yang saling bertentangan. |
| `Project` | Mini Project, milik satu `Technology`. |
| `Resource` | Bertipe: dokumentasi resmi · video · paper · repositori. Bagian 5 template. |
| `TechnologyRelationship` | **Sudah ada.** Sisi Knowledge Graph. |
| `Article` | Arus Intelligence. V1 **hanya arXiv** — lihat [ADR-016](ADR-016-pagu-biaya.md). |
| `Chunk` | Potongan teks + vektor, untuk RAG. |

Yang **tidak** dibuat di V1: tabel pengguna, progres, dan lencana — tidak ada
autentikasi di V1 ([ADR-013](ADR-013-autentikasi.md)).

Lima bagian template ADR-012 sengaja jadi **tabel bertipe, bukan blok generik**.
Blok generik (`content_block` dengan kolom `type` dan `body`) terlihat lebih
lentur, tapi ia memindahkan seluruh aturan bentuk ke kode aplikasi dan membuat
"halaman ini lengkap atau belum" jadi pertanyaan yang tidak bisa dijawab basis
data. Bentuk yang ditegakkan tipe adalah bentuk yang tidak bisa dilanggar diam-diam.

### 3. Knowledge Graph tetap di PostgreSQL

Menegaskan [ADR-006](ADR-006-neo4j.md), sekarang sebagai keputusan V1 dan bukan
penundaan: **relasi disimpan di `technology_relationships` di PostgreSQL, Neo4j
tidak dipasang di V1.** Tabelnya sudah memuat penjaga yang tidak datang gratis di
graph database — kunci unik `(From, To, Kind)` dan penolakan simpul berelasi
dengan dirinya sendiri.

Pada 14 bidang dan 82 topik, jumlah sisi graf ada di orde ratusan. Itu bukan
skala yang membutuhkan mesin graf; itu skala yang membutuhkan satu `JOIN`.

### 4. Vektor: pgvector, 1536 dimensi, dengan tiga jebakan diputus di muka

Mengikuti [ADR-005](ADR-005-qdrant.md):

- **Embedding 1536 dimensi**, disimpan sebagai `vector(1536)`. Dipilih justru
  supaya **tetap di bawah batas 2.000 dimensi indeks HNSW pgvector** — batas yang
  menurut audit "menggigit belakangan". `text-embedding-3-large` (3.072 dim)
  tidak bisa diindeks langsung; kalau nanti dibutuhkan, jalannya `halfvec` atau
  parameter `dimensions`, dan itu keputusan baru — bukan penyesuaian diam-diam.
- **`hnsw.iterative_scan` wajib disetel.** Kita akan memfilter per bidang, dan
  filter di pgvector diterapkan **setelah** indeks dipindai. Gejalanya bukan
  error melainkan `LIMIT 10` yang diam-diam mengembalikan tiga baris — kegagalan
  paling berbahaya justru karena tidak berbunyi.
- **Di Azure**, ekstensinya harus di-allowlist manual di parameter
  `azure.extensions`, dan namanya **`vector`, bukan `pgvector`**. Versi minimum
  **0.8.4** — di bawah itu ada bug korupsi indeks HNSW saat vacuum yang baru
  ditambal Juni 2026.

### 5. Pencarian hibrida dijawab tanpa Qdrant

Audit menyebut satu alasan sah memilih Qdrant: pencarian hibrida, karena
pencocokan kata kunci (nama model, nomor CVE, judul paper) sering meleset kalau
hanya mengandalkan embedding.

Alasan itu diterima sebagai **kebutuhan**, tapi tidak sebagai **alasan menambah
basis data**. PostgreSQL sudah membawa pencarian teks penuh (`tsvector`);
gabungan peringkat leksikal dan vektor dilakukan di satu kueri. Yang kita hindari
bukan kebutuhannya, melainkan container kedua, konsistensi kedua, dan tagihan
kedua untuk memenuhinya.

### 6. Provenance: setiap potongan tahu dari mana asalnya

`Chunk` menyimpan sumbernya (`SourceType`, `SourceId`, urutan). Tanpa ini, AI
Mentor bisa menjawab benar tapi tidak bisa menunjukkan dari mana — dan jawaban
yang tidak bisa ditelusuri tidak lebih baik daripada tidak menjawab, di produk
yang mengklaim dirinya sumber belajar.

## Konsekuensi

- **Implementasinya belum ditulis, dan itu disengaja.** PR #23 masih menunggu
  review; menumpuk migrasi besar di atas cabang yang belum ditinjau membuat
  review berikutnya jadi dua pekerjaan sekaligus. Skema ini mendarat setelah #23
  masuk.
- `Technology.Category` berubah dari teks bebas jadi kunci asing. Itu migrasi
  yang merusak bentuk lama — dan justru karena itu dilakukan **sekarang**, saat
  isinya baru lima baris contoh dari `run.ps1 seed`.
- Kalau `ContentMaturity` nanti ditemukan menyatu dengan `TechnologyStatus` di
  kode, itu regresi — bukan penyederhanaan. Alasannya ada di bagian 1 di atas.

---

## Pembaruan 2026-09-04 - dua entitas pertama mendarat

Cabang `fase-1/skema-konten`, ditumpuk di atas `fase-1/skeleton`.

**Yang sudah ada di kode:** `Field` (14 baris disemai migrasi), `Technology.FieldId`
menggantikan `Category` teks bebas, dan `ContentMaturity` sebagai kolom tersendiri.
**Yang belum:** `RoadmapStep`, `Tool`, `Project`, `Resource`, `Article`, `Chunk`.
`Chunk` sengaja paling belakang - ia menuntut ekstensi pgvector dipasang di image
Postgres, dan itu perubahan infrastruktur yang berdiri sendiri.

> ✅ **Kalimat "yang belum" di atas sudah usang sejak 2026-09-08** - keempat yang
> pertama mendarat. Ia dibiarkan sebagai rekaman keadaan hari itu; yang berlaku
> ada di pembaruan di bawah.

## Pembaruan 2026-09-08 - keempat bagian isi mendarat

Cabang `fase-1/entitas-isi-halaman`. **`RoadmapStep`, `Tool` + `TechnologyTool`,
`Project`, dan `Resource` sudah ada di kode**, berikut migrasi
`EmpatBagianIsiHalaman` (lima tabel, murni penambahan - nol perubahan pada tabel
lama). **Yang belum tinggal `Article` dan `Chunk`.**

### Yang berubah lebih penting daripada bertambahnya empat tabel

`Technology.MarkDrafted()` selama ini **mengaku** berarti *"kelima bagian
terisi"* di ringkasannya sendiri, sementara ia meluluskan halaman yang sama
sekali kosong - tidak ada satu pun bagian yang bisa diperiksanya. Sekarang ada,
dan ia memeriksanya; `Technology.MissingSections` menyebut bagian **mana** yang
kurang alih-alih sekadar menolak.

**Enam uji lama memerah saat aturan itu dipasang** - keenamnya menaikkan halaman
ke `draf` tanpa satu pun bagian terisi. Itu buktinya bahwa kalimat di ringkasan
metode memang tidak pernah menjaga apa pun.

### Dua bentuk yang kini mustahil dilanggar, bukan sekadar tidak dianjurkan

- **Nomor langkah roadmap tidak pernah dikirim pemanggil.** `SetPrerequisite()`
  memegang langkah 0, `AddRoadmapStep()` menambah di ujung sambil memberi nomor
  sendiri. Roadmap berlubang (0, 1, 4) karenanya tidak punya jalan masuk, dan
  *"langkah 0 adalah prasyarat"* (ADR-012) berhenti bergantung pada ketelitian
  penulisnya.
- **`Technology.Tools` bertipe `IReadOnlyList<TechnologyTool>`, bukan
  `IReadOnlyList<Tool>`.** Alasan m2m di tabel entitas di atas ditegakkan tipe:
  tidak ada tempat untuk menyalin alat ke dalam halaman.

### Satu klaim yang dibuktikan KELIRU, dan dicatat apa adanya

Uji integrasi yang lahir bersama entitas ini mula-mula mengklaim ia menjaga baris
`UsePropertyAccessMode(PropertyAccessMode.Field)` di `TechnologyConfiguration`.
**Klaim itu diuji dengan mematikan barisnya - dan keenam uji tetap hijau.** Bahkan
keempat blok `HasMany`-nya sekalian dimatikan, tetap hijau: konvensi EF Core
sudah memetakan semuanya sendiri.

Konfigurasi eksplisitnya **dipertahankan**, karena ia menuliskan maksud - terutama
pilihan `Cascade`, yang justru bisa hilang tanpa satu pun uji memerah. Yang
diperbaiki klaimnya, bukan kodenya. Pelajarannya sama dengan pelajaran migrasi di
bawah: **penjaga yang belum pernah terlihat merah belum terbukti menjaga apa pun.**

### Nama enum di kode, supaya dokumen dan kode tidak hanyut

| ADR ini | Kode |
|---|---|
| `kurasi` | `ContentMaturity.Curated` |
| `draf` | `ContentMaturity.MachineDrafted` |
| `tinjau` | `ContentMaturity.HumanReviewed` |

Kata `Draft` sengaja **tidak** dipakai untuk tingkat kedua. `TechnologyStatus`
sudah punya anggota bernama `Draft`, dan dua sumbu yang bagian 1 di atas
susah-payah pisahkan tidak boleh memakai kata yang sama - itu jalan tercepat
menuju penggabungan yang justru dilarang.

### Satu aturan tambahan yang lahir saat menulis kodenya

**`Update()` menurunkan kembali halaman yang sudah `HumanReviewed`.** Pemeriksaan
manusia berlaku atas teks yang diperiksa, bukan atas nama halamannya; kalau
teksnya berubah, labelnya tidak boleh ikut bertahan. Naik lagi hanya lewat
`MarkReviewed(reviewer)`, yang menuntut nama pemeriksanya - jadi tidak ada cara
menandai sesuatu "sudah diperiksa manusia" tanpa menyebut manusianya.

### Penyimpangan dari rencana, dicatat apa adanya

Bagian Konsekuensi di atas menulis implementasinya **menunggu PR #23 di-merge**.
Yang dikerjakan bukan itu: kodenya mendarat di cabang terpisah yang ditumpuk di
atas #23, dan diajukan sebagai PR sendiri.

Alasannya sama dengan alasan aslinya - review #23 tetap satu unit yang utuh - tapi
tanpa mengunci pekerjaan di belakang review yang belum tentu segera datang.
Kalau pemilik lebih suka #23 di-merge dulu, cabang ini tinggal di-rebase.

### Migrasi ditulis tangan, dan penjaganya sudah dibuktikan merah

Hasil scaffold membuang kolom `Category` LEBIH DULU lalu mengisi `FieldId` dengan
Guid kosong - setiap baris lama kehilangan kategorinya dan langsung melanggar
kunci asing. Urutannya dibalik jadi: semai bidang -> tambah kolom yang boleh
kosong -> isi dari kategori lama -> **berhenti dengan pesan yang bisa
ditindaklanjuti** -> baru wajibkan dan buang kolom lama.

Penjaganya diuji dengan **sengaja merusak data lebih dulu**: satu baris berkategori
"Belum diputuskan" disisipkan, migrasi dijalankan, dan ia berhenti dengan pesan
yang menyebut kategori pelakunya berikut jalan keluarnya. Diperiksa juga bahwa
transaksinya benar-benar berguling balik - tabel `fields` nol, kolom `Category`
utuh. Gerbang yang belum pernah terlihat merah belum terbukti menjaga apa pun.


---

## Pembaruan 2026-09-07 - pertanyaan rebase itu sudah gugur

Bagian *"Penyimpangan dari rencana"* di atas menutup dengan satu tawaran:
*"Kalau pemilik lebih suka #23 di-merge dulu, cabang ini tinggal di-rebase."*

Tawaran itu **tidak lagi berlaku, dan tidak perlu dijawab.** Pemilik memilih
jalan yang sama: **#23 di-merge lebih dulu, baru #25** - berurutan, dengan
*merge commit* dan bukan squash, justru supaya commit yang jadi tumpuan #25
tidak ditulis ulang. Rebase tidak pernah dibutuhkan.

Sesudahnya **#27** ikut mendarat, memperbaiki satu cacat yang **lolos dari #23
maupun #25**: `POST /api/v1/technologies` membalas 500, bukan 400, untuk
`name`/`slug` yang tidak menyisakan satu huruf pun. Akarnya dua tempat yang
menyimpan angka 160 tanpa saling tahu; keduanya kini satu konstanta
`Technology.MaxSlugLength`.

Skema di ADR ini karena itu **sepenuhnya mendarat di `main`** - `Field`,
`ContentMaturity`, dan penegaknya - dan bisa diperiksa langsung, bukan lewat
cabang.
