# TechVerse X

**Explore. Learn. Build. Innovate.**

Platform pembelajaran teknologi terdepan. Satu tempat berisi peta teknologi, roadmap belajar, tutorial, proyek praktik, tools, berita, dan AI mentor — dengan isi yang diperbarui secara otomatis.

Sasaran pemakai: pemilik sendiri, mahasiswa, engineer, sampai perusahaan yang butuh onboarding teknologi baru.

---

## Status: Fase 1 — kerangka kode berjalan, Fase 0 **sudah terkunci**

Repositori ini sekarang punya **kode yang jalan**, tapi baru kerangkanya: fondasi yang dipakai semua fitur, belum fiturnya.

Yang sudah terbukti hidup (diverifikasi 2026-09-03): Next.js → API .NET → PostgreSQL, dengan Redis dan health check yang benar-benar menyentuh keduanya.

| Sudah jalan | Belum ada |
|---|---|
| API .NET 10 + PostgreSQL 17 + Redis 8 | Navigasi & taksonomi bidang |
| Irisan vertikal Technology (buat · ambil · cari) | Autentikasi |
| Migrasi EF Core, schema per bounded context | Kode AI apa pun |
| Health check `live`/`ready` yang menguji dependensi | Vector DB, Neo4j, event bus |
| Halaman web yang membaca API sungguhan | Pengambilan berita |
| CI: build ketat, uji, migrasi dari nol, pindai rahasia | Dockerfile, Terraform, Kubernetes |

> ✅ **Ke-22 keputusan Fase 0 dan Fase 1 sudah diambil pada 2026-09-04** dan tercatat di **[docs/KEPUTUSAN.md](docs/KEPUTUSAN.md)** — baca itu lebih dulu. Kolom "Belum ada" di tabel atas sekarang berarti *belum dibangun*, bukan lagi *belum diputuskan*.
>
> ⚠️ Yang masih menunggu tangan pemilik: Issue [#17](../../issues/17) — ketentuan layanan OpenAI dan Microsoft. **Langkah 1 sudah selesai (7 Sep 2026): keduanya sudah dibaca**; sisa langkah 2–3 (batas kutipan wajar, tinjauan hukum bila komersial). V1 karenanya hanya menerbitkan ulang **arXiv**.

---

## Menjalankan

Butuh **.NET SDK 10**, **Node 22+**, dan **Docker**.

```bash
# Windows                      # Linux / WSL / macOS
.\run.ps1 up                   make up        # Postgres + Redis
.\run.ps1 migrate              make migrate   # bikin tabelnya
.\run.ps1 api                  make api       # http://localhost:5080
.\run.ps1 seed                 make seed      # isi contoh (API harus jalan)
.\run.ps1 web                  make web       # http://localhost:3000
```

`.\run.ps1` tanpa argumen menampilkan seluruh perintah. Isi `run.ps1` dan `Makefile` sengaja dijaga sama: yang satu untuk Windows, yang lain untuk CI dan Linux.

Gerbang yang sama dengan CI, sebelum push:

```bash
.\run.ps1 verify               make verify
```

### Isi contoh itu bukan kurikulum

`seed` memasukkan lima entri sekadar supaya layar dan endpoint ada isinya. Isi sungguhan menunggu Issue [#10](../../issues/10) (siapa yang menulis) dan [#18](../../issues/18) — peta teknologinya sendiri masih perlu dikoreksi lebih dulu.

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
| **[docs/adr/](docs/adr/)** | Enam belas Architecture Decision Record. |
| **[docs/SESSION-LOG.md](docs/SESSION-LOG.md)** | Catatan sesi kerja, urutan terbaru di atas. |

### Kode

```
apps/api/            Host ASP.NET Core — health check, correlation id, OpenAPI
apps/web/            Next.js 16 App Router
services/technology/ Bounded context pertama — vertical slice + DDD ringan
packages/contracts/  Kontrak API yang dipakai bersama
tests/unit/          53 uji domain
tests/integration/   (menyusul di PR #31)
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

> ✅ Keduanya **sudah bisa hidup berdua**: nav utama berisi fungsi, dan tiap bidang punya URL kanonik `/teknologi/<slug>` — Issue [#1](../../issues/1) ditutup, lihat [ADR-009](docs/adr/ADR-009-tulang-punggung-navigasi.md). Bidangnya kini **empat belas**, bukan dua belas ([ADR-010](docs/adr/ADR-010-taksonomi-bidang.md)). Navigasinya sendiri belum ditulis.

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
