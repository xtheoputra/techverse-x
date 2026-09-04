# ADR-015 — Skema data V1: entitas, dua sumbu status, dan penyimpanan vektor

**Status:** Diterima sebagai rancangan. **Implementasi menunggu PR [#23](../../../../pull/23) di-merge.**
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
