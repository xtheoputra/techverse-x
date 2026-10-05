# TechVerse X

**Explore. Learn. Build. Innovate.**

Platform pembelajaran teknologi terdepan. Satu tempat berisi peta teknologi, roadmap belajar, tutorial, proyek praktik, tools, berita, dan AI mentor — dengan isi yang diperbarui secara otomatis.

Sasaran pemakai: pemilik sendiri, mahasiswa, engineer, sampai perusahaan yang butuh onboarding teknologi baru.

---

## Status: Fase 1 — semuanya siap tayang, tinggal tiga akun

Repositori ini punya **kode yang jalan** dan **citra produksi yang sudah terbit**, tapi isinya baru kerangka: fondasi yang dipakai semua fitur, belum fiturnya.

> ### 🔜 Langkah berikutnya, berurutan
>
> Semua yang bisa dikerjakan tanpa akun **sudah selesai.** Yang menahan situs tayang tinggal tiga pendaftaran ([ADR-019](docs/adr/ADR-019-hosting-gratis-tanpa-kartu.md)) — dan **satu di antaranya berubah**:
>
> 1. **[#38](https://github.com/xtheoputra/techverse-x/issues/38)** — Neon (PostgreSQL) → simpan URI **direct** (sakelar pooling mati) sebagai secret `NEON_DATABASE_URL`. ⚠️ Proyek sungguhannya dibuat **sesudah** region host API diketahui — region Neon tidak bisa diubah.
> 2. **[#39](https://github.com/xtheoputra/techverse-x/issues/39)** — host API. 🔴 **Koyeb tidak lagi gratis untuk akun baru** (sejak 17 Februari 2026) — jangan mendaftar ke sana. Penggantinya diuji menurut **[ADR-025](docs/adr/ADR-025-host-api-pengganti-koyeb.md)**: Railway Free lebih dulu, dengan syarat lulus tertulis di tiap langkah.
> 3. **[#40](https://github.com/xtheoputra/techverse-x/issues/40)** — Vercel (web) → **ini yang memberi URL publiknya**
>
> Sesudah tayang: **[#42](https://github.com/xtheoputra/techverse-x/issues/42)** — isi AI Agents menuju Bulan 2. [#41](https://github.com/xtheoputra/techverse-x/issues/41) (endpoint isi halaman + `/teknologi/<slug>`) **sudah selesai**, mendarat lewat PR #44.
>
> 🔴 **Tapi #42 belum bisa dikerjakan siapa pun — termasuk pemilik.** `MarkReviewed()` nol pemanggil di kode produksi, jadi tidak ada satu jalan pun untuk menaikkan halaman ke `tinjau`. Jalannya sudah diputuskan di [ADR-021](docs/adr/ADR-021-jalan-menuju-tinjau.md) — workflow bergerbang, nama pemeriksa dari `github.actor` — dan **sengaja belum dibangun sampai #40 tutup**, sebab Bulan 1 mengejar URL, bukan fitur.
>
> Langkah terincinya ada di **[docs/PENYEBARAN.md](docs/PENYEBARAN.md#menyebarkan-vercel--koyeb--neon)** — bagian Koyeb-nya kini rekaman, dan langkah Neon-nya sudah dikoreksi.

> ### 🔎 Dan sementara ketiganya menunggu, Bulan 3 dimulai duluan
>
> Ketiga akun di atas **tidak menghalangi pekerjaan lain**, jadi sasaran Bulan 3
> `docs/RENCANA-V1.md` — *"orang bisa menemukan halaman tanpa menebak URL"* —
> dikerjakan lebih dulu. **Pencarian teks penuh PostgreSQL sudah mendarat**
> ([ADR-022](docs/adr/ADR-022-pencarian-teks-penuh.md)): kotak cari di tiap
> halaman, `/cari?q=…`, dan **bidang ikut dicari** — sebelum ini `quantum`
> menjawab nol padahal situsnya punya bidang bernama *Quantum Computing*.
>
> **Knowledge Graph dasar juga sudah dibangun**
> ([ADR-023](docs/adr/ADR-023-knowledge-graph-dasar.md)): relasi antar-topik
> dijaga basis data dan tampil sebagai *"Topik terhubung"* di halaman topik —
> tapi **produksi nol sisi**, sebab produksi nol topik dan jalan masuknya isi
> adalah ADR-021. Explore dilayani rute yang sudah ada dan `/learn` menunggu
> pemicunya ([ADR-024](docs/adr/ADR-024-explore-learn-navigasi-v1.md)). Yang masih
> terbuka di milestone **Bulan 3 — Explore, Learn & Pencarian**:
> [#54](https://github.com/xtheoputra/techverse-x/issues/54) (pencarian belum menjangkau isi halaman) dan pemicu Learn.

Yang sudah terbukti hidup: rantai Next.js → API .NET → PostgreSQL, dengan health check yang benar-benar menyentuh dependensinya. Diverifikasi tiga kali — di mesin pengembang (2026-09-03), **dari dalam peti kemas produksi, tiga kontainer, volume basis data baru** (2026-09-07), dan **terhadap citra yang berlaku hari ini** (2026-09-10, [#48](https://github.com/xtheoputra/techverse-x/issues/48)) yang sekalian menegaskan bentuk produksi itu **tidak punya permukaan tulis**: 14 bidang, nol topik.

| Sudah jalan | Belum ada |
|---|---|
| API .NET 10 + PostgreSQL 17 | Navigasi tujuh bagian — **sengaja bertahap**: satu bagian masuk menu hanya kalau halamannya berisi di produksi ([ADR-024](docs/adr/ADR-024-explore-learn-navigasi-v1.md)) |
| Irisan vertikal Technology (buat · ambil · cari) | Autentikasi |
| Migrasi EF Core, schema per bounded context | Kode AI apa pun |
| Health check `live`/`ready` yang menguji dependensi | Vector DB, Neo4j, event bus |
| Halaman web yang membaca API sungguhan | Pengambilan berita |
| **Pencarian teks penuh PostgreSQL** — `tsvector` terhitung di `technologies` dan `fields`, halaman `/cari`, kotak cari di tiap halaman ([ADR-022](docs/adr/ADR-022-pencarian-teks-penuh.md)) | Pencarian ke dalam **isi** halaman ([#54](https://github.com/xtheoputra/techverse-x/issues/54)) — hari ini yang tercari nama dan ringkasan |
| CI: **8 dari 9 gerbang** blueprint menyala — Lint · Unit · Integrasi · SAST · Dependency Scan · Secret Scan · Container Scan · Build, plus penjaga "migrasi bisa dijalankan dari nol". Yang belum: **Contract Test** (belum ada kontrak antar-layanan) | **Platform hosting, domain, sertifikat** |
| **Citra produksi terbit ke GHCR** tiap `main` bergerak — API, web, dan bundel migrasi EF | Terraform, Kubernetes |
| **Kelima bagian template [ADR-012](docs/adr/ADR-012-template-halaman.md) punya tabelnya sendiri** — roadmap (langkah 0 = prasyarat), tools (m2m), mini project, resources, **berikut halaman `/teknologi/<slug>` dan endpoint isinya** (#41, PR #44) | `/learn` — **sengaja ditunda** sampai [#42](https://github.com/xtheoputra/techverse-x/issues/42) dan satu roadmap terisi di produksi ([ADR-024](docs/adr/ADR-024-explore-learn-navigasi-v1.md)) |
| **Relasi antar-topik (Knowledge Graph dasar)** — dibuat lewat endpoint tulis, tampil sebagai *"Topik terhubung"* di halaman topik, dijaga kunci asing di kedua ujung ([ADR-023](docs/adr/ADR-023-knowledge-graph-dasar.md)). **Di produksi nol sisi, sebab nol topik** | Jalan membuang relasi maupun bagian isi — diputuskan bersama alur [ADR-021](docs/adr/ADR-021-jalan-menuju-tinjau.md) |

> 🐳 **Redis dipakai di pengembangan saja.** [ADR-016](docs/adr/ADR-016-pagu-biaya.md) memutuskan ia **tidak di-provision di V1** — ia harus membuktikan dirinya dulu dengan beban yang benar-benar ada. Kesiapan API sudah tahu cara hidup tanpanya, dan itu diuji ([`tests/integration`](tests/integration)).

> ✅ **Ke-22 keputusan Fase 0 dan Fase 1 sudah diambil pada 2026-09-04** dan tercatat di **[docs/KEPUTUSAN.md](docs/KEPUTUSAN.md)** — baca itu lebih dulu. Kolom "Belum ada" di tabel atas sekarang berarti *belum dibangun*, bukan lagi *belum diputuskan*.
>
> ⚖️ **Sisi hukum sudah tuntas untuk V1** (Issue [#17](https://github.com/xtheoputra/techverse-x/issues/17), ditutup 8 Sep 2026). Keempat dokumen ketentuan layanan dibaca di peramban sungguhan, dan batas kutipan ditetapkan: **arXiv boleh metadata penuh (CC0); sumber lain hanya judul, tautan, nama sumber, dan tanggal — nol kutipan.** Rekamannya di [AUDIT-KELAYAKAN.md](AUDIT-KELAYAKAN.md#pembaruan-8-september-2026--dua-dokumen-terakhir-issue-17-sudah-dibaca).
>
> ⚠️ Dua hal yang mengikat begitu AI dinyalakan: **OpenAI §10 melarang memasang nama/logo mereka** di situs tanpa izin tertulis (*"Powered by OpenAI"* melanggarnya), dan *Sharing & Publication Policy* menuntut **manusia memikul tanggung jawab akhir** atas isi terbit — yang berbenturan dengan tingkat `draf`. Belum menyala: V1 tidak memanggil model sama sekali.

---

## Menjalankan

Butuh **.NET SDK 10**, **Node 22+**, dan **Docker**.

```bash
# Windows                      # Linux / WSL / macOS
.\run.ps1 up                   make up        # Postgres + Redis
.\run.ps1 migrate              make migrate   # bikin tabelnya
.\run.ps1 api                  make api       # http://localhost:5080
.\run.ps1 seed                 make seed      # isi contoh (API dari baris di atas)
.\run.ps1 web                  make web       # http://localhost:3000 - coba /cari?q=quantum
```

`.\run.ps1` tanpa argumen menampilkan seluruh perintah. Isi `run.ps1` dan `Makefile` sengaja dijaga sama: yang satu untuk Windows, yang lain untuk Linux/WSL/macOS (CI tidak memanggil keduanya). Perintah `seed` di keduanya menjalankan satu berkas yang sama, `database/seeds/seed.mjs` — sampai 2026-09-17 masing-masing punya salinan sendiri, dan salinan untuk `make` sudah menyimpang.

Gerbang yang sama dengan CI, sebelum push:

```bash
.\run.ps1 verify               make verify
```

⚠️ **`verify` dan `test` menuntut `up` LALU `migrate` lebih dulu.** Uji integrasi menulis baris ke PostgreSQL sungguhan — kesiapan yang diuji tanpa dependensi sungguhan tidak mengukur apa pun. Tanpa `migrate`, galatnya `relation "technology.technologies" does not exist`.

### Tautan di dokumen: issue dan PR ditulis ABSOLUT

Satu aturan, dan sengaja tanpa hitungan `../`:

> Tautan relatif hanya boleh menunjuk berkas **di dalam** repo. Issue, PR, milestone, dan label ditulis URL penuh `https://github.com/xtheoputra/techverse-x/...`.

Sebabnya bisa diukur: halaman blob GitHub meresolusi tautan relatif dengan aturan URL biasa, jadi jumlah `../` yang benar bergantung pada kedalaman berkasnya **dan** berapa segmen yang dimakan nama ref-nya. Cabang di repo ini bernama `fase-1/...` — satu garis miring, dua segmen — sementara `main` satu. Tidak ada satu angka yang benar di keduanya: sebelum aturan ini dipasang, **71 dari 222** tautan issue rusak dibaca di `main`, dan **221 dari 222** dibaca di cabang. Gerbangnya `.\run.ps1 tautan` / `make cek-tautan`, ikut di `verify` dan di CI, dan kepala [skripnya](.github/scripts/cek-tautan-markdown.mjs) memuat ketiga pengukuran yang melahirkan aturan itu.

Komentar kode ikut diperiksa, sebab dokumentasi XML C# dan JSDoc di repo ini ditulis dalam Markdown — dan dua tautan issue di `apps/web/src/lib/lingkungan.ts` memang rusak. Keduanya juga memperlihatkan kenapa aturannya **bukan** sekadar "jangan keluar dari pohon repo": dari berkas empat tingkat dalam, empat `../` mendarat tepat di akar repo, jadi ia tidak keluar dari pohon — ia menunjuk berkas bernama `issues/42` yang tidak ada.

### Gerbang halaman jadinya, di luar `verify`

Aturan teks pembaca [ADR-024](docs/adr/ADR-024-explore-learn-navigasi-v1.md) — tidak ada *"Bulan N"*, *"menyusul"*, nomor ADR, perintah `run.ps1`, atau alamat internal di teks yang **terlihat** — hanya bisa diukur di halaman yang sudah tergambar, bukan di kode web. Tiga dari sembilan pelanggaran pertamanya datang dari **data**, yaitu ringkasan bidang yang disemai migrasi; pemindai kode tidak akan pernah melihatnya.

```bash
.\run.ps1 halaman              make cek-halaman
```

Ia menuntut API hidup **dan** build produksi web (bukan `next dev` — aturannya memang membedakan keduanya), jadi ia **sengaja di luar `verify`**. Di **CI ia tetap jalan**: job `citra` menyalakan `docker-compose.prod.yml` lalu memanggil pemindai yang sama, di job yang memang sudah membangun ketiga citranya.

Selain memindai teks ia menelusuri dari `/`, membandingkan jumlah halaman `/teknologi/*` yang tercapai dengan yang dikenal API, dan menolak blok *"Topik terhubung"* yang tampil kosong (ADR-023). Angkanya berbeda di dua bentuk, dan keduanya benar:

| Bentuk | Halaman | `/teknologi/*` |
|---|---|---|
| **produksi** (yang dijalankan CI) | 16 | **14 dari 14** — nol topik, jadi empat belas bidang itulah seluruh isinya |
| **pengembangan** (6 topik contoh) | 22 | **20 dari 20** |

### Isi contoh itu bukan kurikulum

`seed` memasukkan enam topik contoh, mengisi kelima bagian satu di antaranya, dan mencatat satu relasi antar-topik — sekadar supaya layar dan endpoint ada isinya. Cara isi sungguhan lahir sudah diputuskan di [ADR-012](docs/adr/ADR-012-template-halaman.md) (`kurasi` → `draf` → `tinjau` oleh manusia), dimulai dari tujuh topik AI Agents di [#42](https://github.com/xtheoputra/techverse-x/issues/42), dan jalan terakhirnya ke produksi ada di [ADR-021](docs/adr/ADR-021-jalan-menuju-tinjau.md). Tidak satu pun lewat `seed`.

---

## Isi Repositori

### Dokumen

| Berkas | Isi |
|---|---|
| **[KERANGKA.md](KERANGKA.md)** | Dokumen utama. Empat bagian dikte pemilik — peta teknologi, produk, identitas & struktur aplikasi, dan Engineering Blueprint v1 — ditutup daftar 30 butir yang menunggu keputusan. |
| **[AUDIT-KESEGARAN.md](AUDIT-KESEGARAN.md)** | Dua audit isi. Bagian A memeriksa peta dua belas teknologi — seluruhnya berstatus sebagian usang. Bagian B memeriksa empat bidang baru/berubah. |
| **[AUDIT-KELAYAKAN.md](AUDIT-KELAYAKAN.md)** | Dua audit kelayakan. Bagian A memeriksa enam sumber berita, Bagian B memeriksa empat pilihan arsitektur. |
| **[docs/KEPUTUSAN.md](docs/KEPUTUSAN.md)** | **Ke-22 keputusan dalam satu halaman**, berikut cara membatalkan tiap keputusan. Berlaku di atas `KERANGKA.md` kalau keduanya berbeda. |
| **[docs/RENCANA-V1.md](docs/RENCANA-V1.md)** | Rencana enam bulan yang menggantikan `KERANGKA.md` 2.9. Deployment naik dari Bulan 6 ke Bulan 1. |
| **[docs/PENYEBARAN.md](docs/PENYEBARAN.md)** | Runbook penyebaran, **netral platform**. Kredensial menarik citra, urutan `postgres → migrate → api → web`, variabel lingkungan, dan health check mana yang dipakai untuk apa. |
| **[docs/adr/](docs/adr/)** | Dua puluh lima Architecture Decision Record. Yang menyangkut penyebaran: [ADR-019](docs/adr/ADR-019-hosting-gratis-tanpa-kartu.md) memilih **Vercel + Koyeb + Neon** (gratis, tanpa kartu) dan **menggantikan** [ADR-017](docs/adr/ADR-017-platform-hosting.md) — kaki Koyeb-nya gugur untuk akun baru, dan [ADR-025](docs/adr/ADR-025-host-api-pengganti-koyeb.md) menguji penggantinya; [ADR-018](docs/adr/ADR-018-rilis-citra-dan-reproducibility.md) mengatur kapan citra dibangun ulang. |
| **[docs/SESSION-LOG.md](docs/SESSION-LOG.md)** | Catatan sesi kerja, urutan terbaru di atas. |

### Kode

```
apps/api/            Host ASP.NET Core — health check, correlation id, OpenAPI
apps/web/            Next.js 16 App Router
services/technology/ Bounded context pertama — vertical slice + DDD ringan
packages/contracts/  Kontrak API yang dipakai bersama
tests/unit/          uji domain + infrastruktur
tests/integration/   uji host sungguhan + PostgreSQL sungguhan
database/            SQL migrasi hasil generate + skrip isi contoh
infrastructure/      Init Docker
```

Struktur yang dituju ada di [KERANGKA.md 4.7](KERANGKA.md#47-project-architecture). Yang ada sekarang baru sebagiannya — direktori untuk bagian yang belum diputuskan sengaja belum dibuat.

### Aturan pemisahan yang dipakai

`KERANGKA.md` merekam **apa yang dikatakan pemilik, apa adanya**. Hasil pemeriksaan fakta ditaruh di berkas audit terpisah, supaya kata-kata pemilik tidak tercampur dengan koreksi. Tidak ada koreksi audit yang diterapkan ke `KERANGKA.md` tanpa keputusan eksplisit — satu-satunya bagian tulisan asisten di sana adalah 4.20 *Rekonsiliasi*, dan itu sudah ditandai sebagai pengecualian.

---

## Empat Belas Bidang Teknologi

> ⚠️ Bagian ini dulu memuat **dua belas** bidang. Angka itu **dibatalkan
> [ADR-010](docs/adr/ADR-010-taksonomi-bidang.md)**; yang mengikat sekarang
> **empat belas**, dan daftarnya ditegakkan kode — `FieldCatalog` disemai
> migrasi, dan `FieldCatalogTests` menuntut `Assert.Equal(14, ...)`.
> Dua bidang yang sebelumnya tidak tercantum di sini sama sekali:
> **Data Engineering** dan **Edge AI**.

| Prioritas | Bidang |
|---|---|
| **Core** (6) | AI & Machine Learning · AI Agents · Cybersecurity · Cloud & Infrastructure · **Data Engineering** · IoT |
| **Supporting** (6) | **Edge AI** · Robotics · Quantum Computing · Biotechnology · Blockchain · Renewable Energy |
| **Peripheral** (2) | Space Technology · XR (AR/VR/MR) |

Tiga nama sengaja berbeda dari daftar lama: **Cloud & Infrastructure** (bukan
"Cloud" saja), **XR (AR/VR/MR)** (bukan "AR/VR"), dan **Edge AI** yang berdiri
sendiri — bukan anak IoT.

Ditambah satu kategori terbuka: **teknologi baru yang belum muncul**.

## Tujuh Bagian Aplikasi

```
TechVerse X
│
├── 🌐 Explore          Jelajahi teknologi
├── 🧠 Learn            Roadmap & pembelajaran
├── 🧪 Labs             Eksperimen & project
├── 🤖 AI               AI Mentor
├── 📰 Intelligence     Berita & perkembangan terbaru
├── 🕸 Knowledge Graph  Hubungan antar teknologi
└── 🚀 Future           Emerging technologies
```

> ✅ Keduanya **sudah bisa hidup berdua**: nav utama berisi fungsi, dan tiap bidang punya URL kanonik `/teknologi/<slug>` — Issue [#1](https://github.com/xtheoputra/techverse-x/issues/1) ditutup, lihat [ADR-009](docs/adr/ADR-009-tulang-punggung-navigasi.md). Bidangnya kini **empat belas**, bukan dua belas ([ADR-010](docs/adr/ADR-010-taksonomi-bidang.md)). Navigasinya sengaja bertahap — lihat [ADR-024](docs/adr/ADR-024-explore-learn-navigasi-v1.md) untuk pemicu tiap bagian.

---

## Temuan Audit yang Paling Menentukan

Tiga audit dijalankan pada 2026-09-02 (total 29 agen, nol gagal). Yang paling perlu diketahui sebelum menambah kode:

1. **⏰ `.NET 9` berhenti didukung 10 November 2026.** Yang benar `.NET 10` (LTS sampai 14 November 2028). Turun ke `.NET 8` tidak menolong — berakhir di tanggal yang sama persis. → [B1](AUDIT-KELAYAKAN.md#b1-net-9-sudah-kedaluwarsa--pakai-net-10) · **sudah diterapkan**, [ADR-002](docs/adr/ADR-002-dotnet.md)
2. **LangGraph tidak punya SDK .NET.** Memakainya diam-diam berarti dua runtime. → [B2](AUDIT-KELAYAKAN.md#b2-langgraph-tidak-punya-sdk-net--ini-keputusan-besar-yang-belum-anda-ambil) · **sudah diputuskan: LangGraph dibuang, pakai Microsoft Agent Framework**, [ADR-007](docs/adr/ADR-007-agent-platform.md)
3. **Auth.js secara desain bukan penerbit token untuk API di luar aplikasi Next.js-nya.** → [B3](AUDIT-KELAYAKAN.md#b3-authjs-bukan-penerbit-token-untuk-backend-net) · **sudah diputuskan: V1 tanpa login; kalau nanti dipasang, Clerk sebagai IdP**, [ADR-013](docs/adr/ADR-013-autentikasi.md)
4. **Kurikulumnya akan lahir usang kalau dipakai apa adanya** — AutoGen maintenance mode, Sora ditutup, GPT-5 dipensiunkan. → [Enam Kesalahan Paling Berbahaya](AUDIT-KESEGARAN.md#enam-kesalahan-paling-berbahaya)
5. **Otomatisasi berita layak, tapi GitHub Trending tidak punya API** dan hanya arXiv yang jelas boleh dipublikasi ulang. → [Bagian A](AUDIT-KELAYAKAN.md#bagian-a--enam-sumber-berita)

Kedua berkas audit membawa peringatan metodenya masing-masing. **Baca bagian peringatan di kaki tiap berkas sebelum mengutip angkanya.**

---

## Cara Membaca Repositori Ini

1. Buka **[docs/KEPUTUSAN.md](docs/KEPUTUSAN.md)** — ke-22 keputusan dalam satu halaman. **Ini yang berlaku.**
2. Buka **[docs/RENCANA-V1.md](docs/RENCANA-V1.md)** — apa yang dikerjakan bulan ini, dan satu angka untuk mengukur kemajuannya.
3. Buka **[ADR-008](docs/adr/ADR-008-batas-fase-1.md)** — batas apa yang sudah dan belum dibangun; bagian *Pembaruan* mencatat gerbang mana yang sudah terangkat.
4. **[KERANGKA.md](KERANGKA.md)** dibaca sebagai rekaman kata pemilik, bukan sebagai rencana yang berlaku.

---

## Lisensi

Belum ditentukan. Repositori privat.
