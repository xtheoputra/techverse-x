# TechVerse X

**Explore. Learn. Build. Innovate.**

Platform pembelajaran teknologi terdepan. Satu tempat berisi peta teknologi, roadmap belajar, tutorial, proyek praktik, tools, berita, dan AI mentor — dengan isi yang diperbarui secara otomatis.

Sasaran pemakai: pemilik sendiri, mahasiswa, engineer, sampai perusahaan yang butuh onboarding teknologi baru.

---

## Status: Fase 1 — kerangka kode berjalan, Fase 0 belum terkunci

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

> ⚠️ **Keempat blocker Fase 0 masih terbuka, dan tidak satu pun ikut terputuskan oleh kode ini.** Apa yang sengaja tidak dibangun, dan mengapa, dicatat lengkap di [ADR-008](docs/adr/ADR-008-batas-fase-1.md). Baca itu sebelum menambah fitur.

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
| **[docs/adr/](docs/adr/)** | Delapan Architecture Decision Record — termasuk keputusan untuk **belum** memutuskan. |
| **[docs/SESSION-LOG.md](docs/SESSION-LOG.md)** | Catatan sesi kerja, urutan terbaru di atas. |

### Kode

```
apps/api/            Host ASP.NET Core — health check, correlation id, OpenAPI
apps/web/            Next.js 16 App Router
services/technology/ Bounded context pertama — vertical slice + DDD ringan
packages/contracts/  Kontrak API yang dipakai bersama
tests/unit/          15 uji domain
database/            SQL migrasi hasil generate + skrip isi contoh
infrastructure/      Init Docker
```

Struktur yang dituju ada di [KERANGKA.md 4.7](KERANGKA.md#47-project-architecture). Yang ada sekarang baru sebagiannya — direktori untuk bagian yang belum diputuskan sengaja belum dibuat.

### Aturan pemisahan yang dipakai

`KERANGKA.md` merekam **apa yang dikatakan pemilik, apa adanya**. Hasil pemeriksaan fakta ditaruh di berkas audit terpisah, supaya kata-kata pemilik tidak tercampur dengan koreksi. Tidak ada koreksi audit yang diterapkan ke `KERANGKA.md` tanpa keputusan eksplisit — satu-satunya bagian tulisan asisten di sana adalah 4.20 *Rekonsiliasi*, dan itu sudah ditandai sebagai pengecualian.

---

## Dua Belas Bidang Teknologi

AI & Machine Learning · AI Agents · Cybersecurity · Robotics · Quantum Computing · Biotechnology · Cloud · Blockchain · Renewable Energy · Space Technology · IoT · AR/VR

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

> ⚠️ Daftar bidang dan daftar bagian aplikasi di atas **belum bisa hidup berdua sebagai navigasi utama**. Itu keputusan struktural terbesar yang masih terbuka — Issue [#1](../../issues/1). Karena itu web-nya belum punya navigasi sama sekali.

---

## Temuan Audit yang Paling Menentukan

Tiga audit dijalankan pada 2026-09-02 (total 29 agen, nol gagal). Yang paling perlu diketahui sebelum menambah kode:

1. **⏰ `.NET 9` berhenti didukung 10 November 2026.** Yang benar `.NET 10` (LTS sampai 14 November 2028). Turun ke `.NET 8` tidak menolong — berakhir di tanggal yang sama persis. → [B1](AUDIT-KELAYAKAN.md#b1-net-9-sudah-kedaluwarsa--pakai-net-10) · **sudah diterapkan**, [ADR-002](docs/adr/ADR-002-dotnet.md)
2. **LangGraph tidak punya SDK .NET.** Memakainya diam-diam berarti dua runtime. → [B2](AUDIT-KELAYAKAN.md#b2-langgraph-tidak-punya-sdk-net--ini-keputusan-besar-yang-belum-anda-ambil) · masih terbuka, [ADR-007](docs/adr/ADR-007-agent-platform.md)
3. **Auth.js secara desain bukan penerbit token untuk API di luar aplikasi Next.js-nya.** → [B3](AUDIT-KELAYAKAN.md#b3-authjs-bukan-penerbit-token-untuk-backend-net) · masih terbuka
4. **Kurikulumnya akan lahir usang kalau dipakai apa adanya** — AutoGen maintenance mode, Sora ditutup, GPT-5 dipensiunkan. → [Enam Kesalahan Paling Berbahaya](AUDIT-KESEGARAN.md#enam-kesalahan-paling-berbahaya)
5. **Otomatisasi berita layak, tapi GitHub Trending tidak punya API** dan hanya arXiv yang jelas boleh dipublikasi ulang. → [Bagian A](AUDIT-KELAYAKAN.md#bagian-a--enam-sumber-berita)

Kedua berkas audit membawa peringatan metodenya masing-masing. **Baca bagian peringatan di kaki tiap berkas sebelum mengutip angkanya.**

---

## Cara Membaca Repositori Ini

1. Buka **[ADR-008](docs/adr/ADR-008-batas-fase-1.md)** — batas apa yang sudah dan belum dibangun, berikut alasannya.
2. Buka **[KERANGKA.md](KERANGKA.md)** — bagian *[Yang Perlu Anda Putuskan atau Lengkapi](KERANGKA.md#yang-perlu-anda-putuskan-atau-lengkapi)*.
3. Buka **[Issues](../../issues)** — 29 dari 30 butir sudah jadi issue berlabel dan bermilestone. Satu butir (E10) belum.
4. Tutup milestone **Fase 0 — Kunci Kerangka** sebelum menambah fitur di atas kerangka ini.

---

## Lisensi

Belum ditentukan. Repositori privat.
