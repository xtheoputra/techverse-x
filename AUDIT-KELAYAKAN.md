# Audit Kelayakan — Sumber Berita & Tumpukan Teknologi

**Tanggal audit:** 2026-09-02
**Cara:** 10 agen paralel dengan pencarian web langsung — 6 memeriksa sumber berita, 4 memverifikasi pilihan arsitektur. Nol agen gagal.
**Yang diaudit:** `KERANGKA.md` bagian 2.5 (AI News Curator) dan 2.6 (Arsitektur Teknologi).

---

# Bagian A — Enam Sumber Berita

## Vonis: otomatisasinya LAYAK, tapi tidak dengan cara yang biasa dibayangkan

**Keenam sumber punya jalur resmi.** Tujuan nomor 4 Anda — *"isinya bisa terus diperbarui otomatis"* — tidak menabrak tembok teknis. Tapi ada tiga syarat yang mengubah rancangannya:

1. **Jangan pernah mengikis HTML.** Enam dari enam agen sampai pada saran yang sama. Halaman artikel OpenAI sudah terbukti membalas **403 Forbidden** ke pengambil otomatis hari ini juga.
2. **Hanya arXiv yang benar-benar boleh dipublikasi ulang.** Lima sumber lain berstatus *bersyarat*.
3. **GitHub Trending tidak punya API sama sekali** — harus dibangun sendiri.

## Tabel Ringkas

| Sumber | Jalur resmi | Kunci | Publikasi ulang | Catatan terpenting |
|---|---|---|---|---|
| **arXiv** | API + RSS | tidak | ✅ **boleh** | Metadata berlisensi **CC0**. Batas keras: 1 permintaan / 3 detik, satu koneksi. |
| **NVIDIA** | RSS/Atom | tidak | ⚠️ bersyarat | Paling ramah teknis: **konten penuh** di feed, plus feed per-tag (Jetson, Omniverse). |
| **Google DeepMind** | RSS | tidak | ⚠️ bersyarat | Feed ada, tapi **description-nya kosong** — halaman artikel tetap harus diambil. |
| **Microsoft** | RSS ×3 | tidak | ⚠️ bersyarat | **Ketentuannya paling ketat** dari semua. |
| **OpenAI** | RSS | tidak | ⚠️ bersyarat | Halaman artikel **memblokir bot (403)**. Feed hanya ringkasan pendek. |
| **GitHub Trending** | ❌ tidak ada | **ya** | ⚠️ bersyarat | Bukan produk API. Harus dibangun ulang dari REST API. |

## Temuan Per Sumber

### arXiv — satu-satunya yang bersih
- **API:** `https://export.arxiv.org/api/query?search_query=cat:cs.AI&sortBy=submittedDate&sortOrder=descending` — persis pola "paper terbaru per kategori" yang Anda butuhkan.
- **RSS harian per kategori:** `https://rss.arxiv.org/rss/{kategori}` — kelima kategori (cs.AI, cs.LG, cs.CR, quant-ph, q-bio) diverifikasi hidup 2026-09-02. Sudah memuat judul, **abstrak penuh**, penulis, dan kategori.
- **Lisensi:** *"You are free to use descriptive metadata about arXiv e-prints under the terms of the Creative Commons Universal (CC0 1.0) Public Domain Declaration."* Ini izin tertulis, bukan tafsiran.
- **⚠️ Batas keras:** maksimum **satu permintaan tiap 3 detik**, satu koneksi. Berlaku untuk API, RSS, dan OAI-PMH sekaligus. Jadwalkan setelah 04:00 UTC; Sabtu-Minggu normal tanpa terbitan.

### NVIDIA — paling ramah, tapi ketentuannya bertentangan sendiri
- Feed Atom memuat **konten penuh**, bukan cuplikan: `https://developer.nvidia.com/blog/feed/`
- Ada feed per-tag yang langsung berguna: `.../tag/jetson/feed/`, `.../tag/omniverse/feed/`, `.../tag/jetson-orin/feed/`
- Rilis pers: `https://nvidianews.nvidia.com/releases.xml`; per kategori: `.../cats/robotics.xml`, `.../cats/simulation_modeling_design.xml`
- robots.txt-nya memuat `Content-Signal: ai-train=yes, search=yes, ai-input=yes` dan menyebut perayap AI secara eksplisit.
- **⚠️ Tapi agen menemukan pertentangan langsung** antara robots.txt yang permisif itu dan Ketentuan Situs NVIDIA. Perlu keputusan sadar, bukan asumsi.

### Google DeepMind
- `https://deepmind.google/blog/rss.xml` — RSS 2.0 valid, 150 item.
- **Jebakan:** `description` di feed **kosong**, jadi halaman artikel tetap harus diambil satu per satu (untungnya server-rendered, cukup HTTP biasa).
- Jalur lama `deepmind.google/discover/blog/rss.xml` sudah **mati (404)** — jangan dipakai.
- Google ToS (efektif 30 Juli 2026) melarang akses otomatis *yang melanggar robots.txt*. robots.txt-nya permisif, jadi mengambil boleh — menerbitkan ulang tetap soal lain.

### Microsoft — ketentuan paling menghambat
Tiga feed terverifikasi hidup 2026-09-02:
- `https://azure.microsoft.com/en-us/blog/feed/` (memuat `content:encoded`)
- `https://www.microsoft.com/en-us/research/feed/`
- `https://blogs.microsoft.com/feed/`

**⚠️ `https://blogs.microsoft.com/ai/feed/` sudah MATI (HTTP 410 Gone)** — banyak tutorial lama masih menyebutnya.

**Ketentuannya yang jadi masalah:** isi layanan Microsoft dinyatakan *"for informational and non-commercial or personal use only and will not be copied or posted"*. Kalau TechVerse X Labs nanti komersial, klausul ini menyentuh Anda langsung. Kunci pengamanannya ada pada beda antara apa yang Anda **simpan** dan apa yang Anda **tampilkan**.

### OpenAI — dua jebakan yang mematikan rancangan naif
- Jalur satu-satunya: `https://openai.com/news/rss.xml` (RSS 2.0 valid, `lastBuildDate` terpantau bergerak).
- **⚠️ Halaman artikel membalas HTTP 403 ke pengambil otomatis.** Diuji langsung hari ini pada `openai.com/index/gpt-5-6`. Arsitektur berbasis pengikisan akan rusak sebelum sempat tayang.
- **⚠️ Feed hanya memuat ringkasan pendek (umumnya di bawah 150 karakter), tanpa `content:encoded`.** Artinya kalau Anda menyuruh LLM *"rangkum artikel ini"*, ia akan merangkum sebuah ringkasan — **risiko tinggi berhalusinasi detail yang tidak pernah ada di artikel aslinya.** Ini bukan risiko teoretis; ini konsekuensi langsung dari bentuk datanya.
- Halaman ketentuan layanannya sendiri juga 403, jadi statusnya **tidak terverifikasi**. Perlu dibuka manusia lewat peramban sungguhan sebelum tayang.

### GitHub Trending — tidak ada dan tidak akan ada
Halaman `github.com/trending` adalah HTML biasa, bukan produk API. Tidak ada RSS.

**Yang disarankan:** bangun "trending versi sendiri" di atas API resmi —
`GET /search/repositories?q=created:>{30 hari lalu}+stars:>{ambang}&sort=stars&order=desc&per_page=100` sekali sehari per bahasa, simpan jumlah bintang harian, lalu peringkatkan berdasarkan **delta bintang harian**. Itu justru definisi "naik daun" yang lebih transparan dan bisa Anda pertanggungjawabkan.

**Batas laju:** 60 permintaan/jam tanpa token, **5.000/jam dengan personal access token**. Jadi ini satu-satunya sumber yang butuh kunci.

## Konsekuensi untuk Rancangan Anda

1. **Bulan 4 ("News Automation + GitHub Trending") lebih berat dari kelihatannya** — GitHub Trending bukan integrasi, melainkan fitur yang harus dibangun dari nol berikut penyimpanan riwayat bintang harian.
2. **Pola pengambilan harus seragam:** RSS/API saja, sekali sehari, conditional GET (ETag / If-Modified-Since), User-Agent jujur yang memuat nama platform dan alamat kontak, backoff eksponensial pada 403/429, dan deduplikasi lewat `guid`.
3. **Rangkuman AI wajib diberi label.** Tanpa itu, pengunjung akan membaca rangkuman buatan LLM sebagai pernyataan resmi OpenAI atau NVIDIA. Itu risiko reputasi, dan untuk klaim keamanan/kemampuan model bisa menyesatkan.
4. **Selalu tampilkan judul asli + tanggal + tautan kanonis** ke sumbernya secara mencolok.
5. **Ada satu pekerjaan yang hanya bisa Anda lakukan sendiri:** membuka halaman ketentuan layanan OpenAI dan Microsoft di peramban sungguhan, dan memutuskan batas kutipan wajar. robots.txt hanya mengizinkan Anda **mengambil** — ia tidak pernah mengizinkan Anda **menerbitkan ulang**.

---

# Bagian B — Empat Vonis Arsitektur

## B1. `.NET 9` sudah kedaluwarsa — pakai `.NET 10`
**Keyakinan: tinggi.**

| Versi | Tipe | Akhir dukungan |
|---|---|---|
| **.NET 10** | **LTS** | **14 November 2028** ← satu-satunya jalur waras |
| .NET 9 | STS | 10 November 2026 — **~10 minggu lagi** |
| .NET 8 | LTS | 10 November 2026 — **tanggal yang sama** |
| .NET 11 | STS (preview) | dijadwalkan rilis 10 Nov 2026, bukan LTS |

**Jebakan paling mahal:** mengira ".NET 8 LTS" adalah jaring pengaman. Keduanya berakhir **10 November 2026**. Tim yang "main aman" ke .NET 8 justru melakukan dua migrasi (9→8→10) untuk hasil nol.

Kalau proyek baru dimulai di .NET 9 hari ini, kodenya masuk produksi kira-kira bersamaan dengan tanggal kematian runtime-nya — utang teknis lahir sebelum baris kode pertama ditulis. Ikutannya juga sering terlupa: setelah EOL, image `mcr.microsoft.com/dotnet/aspnet:9.0` berhenti menerima patch CVE, dan Azure App Service / Container Apps mencabut runtime stack yang sudah EOL menurut jadwal Microsoft, bukan jadwal Anda.

**Angka "9" akan tersebar di banyak tempat:** `TargetFramework` di tiap csproj, dua baris di Dockerfile (sdk + aspnet), `global.json`, workflow CI, dan konfigurasi Azure. `global.json` yang mem-pin SDK 9.0.x adalah bom waktu senyap — begitu runner CI baru hanya membawa SDK 10, build gagal dengan pesan menyesatkan.

## B2. LangGraph tidak punya SDK .NET — ini keputusan besar yang belum Anda ambil
**Keyakinan: tinggi.**

Tidak ada SDK .NET/C# resmi untuk LangGraph per September 2026. LangChain hanya menerbitkan SDK LangGraph untuk **Python dan JavaScript/TypeScript**. Pencarian NuGet hanya memunculkan paket komunitas dengan ratusan sampai ribuan unduhan.

Artinya rancangan Anda sekarang — .NET Web API + LangGraph — **diam-diam berarti dua runtime, dua bahasa, dua pipeline CI, dan dua permukaan pemindaian kerentanan.**

**Alternatif asli .NET yang matang:** **Microsoft Agent Framework**. Paket `Microsoft.Agents.AI` mencapai 1.0.0 stabil **2 April 2026** dan kini di 1.20.0 (31 Agustus 2026). Ini penerus resmi Semantic Kernel **dan** AutoGen — jadi menyambung langsung dengan temuan audit pertama.

**Dan MCP bukan alasan untuk memilih Python:** SDK C# resmi `ModelContextProtocol` sudah stabil sejak 1.0.0 (25 Februari 2026), kini 2.2.0. Agent Framework juga mendukung penyedia model non-Microsoft (OpenAI, Anthropic, Ollama) — jadi memakai OpenAI API dari .NET tidak memaksa Anda pindah ke Azure OpenAI.

**Empat jebakan dua-runtime yang baru terasa setelah dibangun:**
- **Streaming token lewat dua lompatan.** Next.js → .NET → Python berarti SSE harus diteruskan utuh dua kali; buffering di ASP.NET dan timeout idle di Azure Container Apps baru muncul saat integrasi.
- **Kepemilikan state jadi ganda.** LangGraph menyimpan thread dan checkpoint-nya sendiri di Postgres; EF Core juga punya tabel percakapan. Mana sumber kebenaran riwayat?
- **Identitas bocor di lompatan kedua.** .NET memvalidasi JWT pengguna, lalu memanggil Python dengan satu API key layanan — otorisasi per-pengguna hilang di perbatasan itu.
- **Retry berlapis melipatgandakan tagihan.** Polly di .NET + retry per-node LangGraph + retry SDK OpenAI = satu kegagalan sementara bisa jadi delapan panggilan model berbayar.

**Empat opsi**, ringkas: (a) buang LangGraph, pakai Agent Framework asli .NET; (b) layanan Python FastAPI terpisah; (c) Agent Server resmi LangGraph + klien HTTP .NET (**⚠️ terikat lisensi LangSmith/Plus/Enterprise — biaya yang mudah terlewat saat prototipe**); (d) hibrida — Agent Framework untuk jalur interaktif, Python untuk tugas latar.

## B3. Auth.js bukan penerbit token untuk backend .NET
**Keyakinan: tinggi.**

Cookie sesi Auth.js adalah **JWE terenkripsi** (A256CBC-HS512) dengan kunci turunan `AUTH_SECRET` — bukan JWT bertanda tangan biasa. Dokumentasi Auth.js sendiri menyatakan:

> *"Auth.js JWTs are meant to be used by the same app that issued them. If you need JWT authentication for your third-party API, you should rely on your Identity Provider instead."*

Backend .NET Anda adalah "pihak lain" dalam arti teknis itu. Jadi `Auth.js / Clerk` di tabel arsitektur bukan dua alternatif setara — yang satu tidak bisa mengerjakan tugas yang dimaksud tanpa pola tambahan.

**Tiga jebakan paling mahal:**
- **⚠️ CVE-2025-29927 (CVSS 9.1 Critical):** otorisasi yang diimplementasikan di Next.js middleware bisa dilewati lewat satu header. **Endpoint .NET WAJIB punya `[Authorize]` sendiri** — middleware hanya untuk pengalaman pengguna.
- Menandatangani JWT sendiri dengan HS256 + secret bersama "karena paling cepat": jalan di hari pertama, lalu tidak ada JWKS, tidak ada rotasi kunci, tidak ada revocation, dan logout di Next.js tidak mencabut token.
- `next-auth` v5 (Auth.js) **masih beta** per 2026-09-02 (`5.0.0-beta.32`). Yang macet kalau ia patah bukan fitur pinggiran — melainkan login.

**Empat pola yang benar:** (a) BFF/proxy — .NET tidak pernah dipanggil langsung dari browser; (b) Auth.js hanya sebagai klien OIDC Entra, access_token diteruskan ke .NET; (c) **Clerk sebagai IdP** — punya SDK .NET resmi (`Clerk.BackendAPI` 3.0.0, 11 Agustus 2026) dan verifikasi RS256 via JWKS; (d) Microsoft Entra External ID langsung.

**Dua catatan biaya dan bentuk produk:**
- **Harga Clerk berbasis pengguna aktif:** gratis sampai 50.000, lalu $0,02/pengguna/bulan. Platform belajar musiman — pendaftaran kelas massal — bisa melipatgandakan tagihan dalam satu bulan.
- **Kalau memilih Entra:** yang relevan untuk siswa/publik adalah **Entra External ID**, bukan tenant karyawan. Azure AD B2C sudah tidak dijual ke pelanggan baru sejak 1 Mei 2025. Salah pilih di awal bukan perkara ganti pengaturan.

## B4. Qdrant belum perlu — pgvector sudah memadai
**Keyakinan: sedang.**

Ukuran pembanding paling jujur datang dari Qdrant sendiri: **tier GRATIS Qdrant Cloud disebut sanggup menampung sekitar 1 juta vektor 768 dimensi.** Beban TechVerse — 41 halaman plus arus artikel dan paper harian — ada di kisaran ratusan ribu potongan. Artinya **satu tingkat di bawah kelas terkecil yang Qdrant jual.**

Hitungan penyimpanan: 200.000 potongan × 1536 dimensi ≈ **1,23 GB** (tipe `vector`) atau **616 MB** (`halfvec`) — masih muat di RAM instance Postgres menengah.

pgvector matang: rilis stabil 0.8.6 (29 Juli 2026), iterative index scan sejak 0.8.0.

**Tiga jebakan yang wajib diketahui sebelum memilih pgvector:**
- **⚠️ Batas 2.000 dimensi untuk indeks HNSW menggigit SETELAH embedding terlanjur dibuat.** `text-embedding-3-large` (3.072 dim) tidak bisa diindeks langsung — harus turun ke `halfvec` (batas 4.000) atau pakai parameter `dimensions`. Putuskan ini **sebelum** membuat embedding pertama.
- **Filter diterapkan setelah indeks dipindai.** `LIMIT 10` dengan filter bisa mengembalikan 3 baris — dan gejalanya bukan error, melainkan jawaban yang diam-diam melempem. Wajib mengatur `hnsw.iterative_scan` secara sadar.
- **Di Azure, ekstensinya harus di-allowlist manual** di parameter `azure.extensions`, dan namanya `vector`, bukan `pgvector`. Versi yang tersedia belum tentu terbaru — kalau < 0.8.4, Anda menanggung bug korupsi indeks HNSW saat vacuum yang baru ditambal Juni 2026.

**Argumen "nanti kalau besar susah pindah" sudah lemah per 2026.** Kedua backend punya konektor .NET stabil di bawah abstraksi `Microsoft.Extensions.VectorData` yang sama (`CommunityToolkit.VectorData.PgVector` dan `...Qdrant`), jadi menunda Qdrant tidak mengunci Anda.

**⚠️ Catatan yang mudah terlewat:** konektor lama `Microsoft.SemanticKernel.Connectors.PgVector` berstatus **DEPRECATED**; penggantinya `CommunityToolkit.VectorData.PgVector`.

**Satu alasan sah untuk tetap memilih Qdrant:** pencarian hibrida. Untuk berita dan paper, pencocokan kata kunci (nama model, nomor CVE, judul paper) penting dan embedding murni sering meleset. Qdrant membawa sparse vector + IDF bawaan. Ini sering baru disadari belakangan — jadi putuskan sekarang, sadar.

---

## ⚠️ Peringatan

1. **Keyakinan tidak merata.** B1, B2, B3 berkeyakinan **tinggi** dengan rujukan ke NuGet, dokumentasi resmi, dan registri npm. B4 berkeyakinan **sedang** — tidak ada angka pembanding resmi pgvector vs Qdrant pada skala ratusan ribu; halaman benchmark Qdrant terakhir diperbarui 2024 dan tidak menyertakan pgvector sama sekali.
2. **Status ketentuan layanan OpenAI belum diperiksa siapa pun** — halamannya memblokir bot. Itu pekerjaan manusia, dan harus dilakukan **sebelum** tayang, bukan sesudah.
3. **Berkas ini temuan, bukan keputusan.** Tabel arsitektur di `KERANGKA.md` 2.6 dibiarkan persis seperti yang Anda tulis.
