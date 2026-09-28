# ADR-026 — Nol biaya: mandiri lebih dulu, gratis tanpa kartu kedua, berbayar tidak pernah

**Status:** Diterima — arahan pemilik, 2026-09-28. **Menggantikan** pagu USD 60 di
[ADR-016](ADR-016-pagu-biaya.md) butir 1 dan bawaan OpenAI di
[ADR-014](ADR-014-mcp-dan-penyedia-ai.md). Host API tetap milik
[ADR-025](ADR-025-host-api-pengganti-koyeb.md) (Diusulkan); yang berubah di sana
hanya jalan keluar yang berbayar, dan satu jalan mandiri yang ditambahkan.
Issue [#57](https://github.com/xtheoputra/techverse-x/issues/57).
**Tanggal:** 2026-09-28

## Konteks

Sesudah CI repo ini kembali merah dengan nol langkah dan anotasi *"recent account
payments have failed or your spending limit needs to be increased"*, pemilik
menulis:

> *"hindari penggunaan yang berbayar, cari semua mandiri dan secara gratis tapi
> powerfull"*

Tiga dokumen masih mengandaikan sebaliknya. [ADR-016](ADR-016-pagu-biaya.md)
menetapkan pagu **USD 60 per bulan** — Azure PostgreSQL, OpenAI dengan pagu keras
USD 20, domain. [ADR-014](ADR-014-mcp-dan-penyedia-ai.md) menjadikan OpenAI bawaan.
Bagian 6 [ADR-025](ADR-025-host-api-pengganti-koyeb.md) menawarkan Railway Hobby,
Koyeb Pro, dan domain untuk Cloudflare Tunnel, *"keduanya di dalam pagu $60"*.

### Apa yang sebenarnya menghabiskan kuota CI — diukur ulang, dan Sesi 15 keliru

Paket GitHub Free memberi **2.000 menit Actions per bulan untuk SEMUA repo privat
akun**, bukan per repo. Akun ini punya 21 repo privat. Laporan billing **harian**
(`GET /users/{user}/settings/billing/usage?year=&month=&day=`) September 2026:

| Repo | Menit 1–10 Sep | Catatan |
|---|---|---|
| `wellsy` | **1.053** | 585 + 393 menit pada 8–9 Sep saja |
| `zarastrade` | 481 | CI biasa + Dependabot |
| `techverse-x` | **558** | repo ini |
| `jago-bahasa`, `think-mate` | 47 | |
| **Jumlah** | **2.139** | kuota 2.000 terlewati **10 September** |

Sesudah itu hanya muncul 18–27 menit per hari, di hari-hari yang run `zarastrade`-nya
**hanya** Dependabot (21 dan 28 Sep: 6 run `dynamic`, nol run lain).

🔴 **Catatan Sesi 15 menyebut run Dependabot `zarastrade` "terus memakan kuota yang
sudah lewat batas". Itu tidak terbukti.** Dokumentasi GitHub menulis sebaliknya —
*"Running Dependabot on standard GitHub-hosted or self-hosted runners does not count
towards your included GitHub Actions minutes"* — dan menit-menit itu memang tercatat
dengan diskon penuh, bentuk yang sama untuk "gratis" maupun "masuk jatah". Sebab
kekeliruannya diukur: endpoint billing **bulanan** melaporkan seluruh 2.223 menit
September di bawah **satu** `repositoryName` (`zarastrade`), padahal hitungan harian
membaginya ke lima repo.

📏 Dua hal lain yang ikut terukur, dan membuat jalan keluarnya bukan sekadar
menunggu 1 Oktober:

- **Seperempat pemakaian repo ini adalah run ganda.** Setiap push ke cabang ber-PR
  menjalankan CI dua kali (`push` dan `pull_request`, commit yang sama): 30 dari 63
  commit, 147 dari 578 menit (dihitung per job, dibulatkan ke atas per menit —
  cocok dengan 558 di billing).
- **Endpoint `timing` GitHub tidak bisa dipakai untuk ini.** Ia melaporkan
  `billable` **0 ms** untuk semua 144 run `zarastrade` September, termasuk 51 run
  `push` yang berjalan 183 menit.

Dengan laju 1–10 September, repo ini sendiri menghabiskan kuota bersama dalam
sekitar tiga minggu — **tanpa** repo lain. Kuota yang pulih 1 Oktober karena itu
jalan yang rapuh, bukan jalan keluar.

### Cara riset ini dilakukan

Sama dengan ADR-025: tiga riset sumber primer (CI, AI, hosting), setiap klaim
dengan kutipan dan tanggal; satu pemeriksa terpisah yang tugasnya **membantah**;
dan setiap angka yang bisa diukur di mesin pemilik diukur, bukan dikutip.

**Hasil pemeriksa atas 20 klaim penentu: 17 bertahan, 3 bersyarat, 1 terbantah.**
Yang terbantah — *aksi `node20` berhenti bekerja* — dibetulkan di bagian 6; yang
bersyarat (kartu Tailscale, kartu Oracle, jeda Vercel) ditulis dengan syaratnya.

## Keputusan

### 1. Pagu: nol. Urutan pilihan: mandiri → gratis tanpa kartu → berbayar tidak pernah

- **Mandiri** — berjalan di mesin pemilik (i7-14700 20 inti, RAM 15,8 GB, tanpa GPU
  diskret; Docker, WSL, Ollama 0.34.4) atau perangkat lunak terbuka yang dipasang
  sendiri.
- **Gratis tanpa kartu** — tier gratis vendor, dengan syarat dan batasnya dikutip.
- **Berbayar** — boleh dicatat sebagai fakta, tidak pernah direkomendasikan. Termasuk
  *"naikkan batas belanja"*, *"beli domain"*, dan kunci API berbayar.

"*Powerful*" diukur, bukan diklaim: token/detik, detik per gerbang, halaman yang
tercapai.

### 2. Inventaris pos biaya

| Pos | Sebelumnya | Sekarang | Keadaan |
|---|---|---|---|
| **CI** | GitHub Actions, kuota bersama 2.000 menit | **CI mandiri** `.\run.ps1 ci` / `make ci` sebagai gerbang wajib; Actions sebagai bonus, dengan run ganda dibuang | ✅ dibangun, bagian 3 |
| **Pemindai** (gitleaks, Trivy) | tag bergerak `@v2`, `:latest` | tetap gratis, **disematkan SHA/digest** | ✅ bagian 6 |
| **Model bahasa** | OpenAI, pagu USD 20 | **Ollama lokal**, `qwen3:8b` | ✅ diukur, bagian 4 |
| **Embedding** | tersirat `text-embedding-3-large` (ADR-005) | **`qwen3-embedding:0.6b`**, 1024 dimensi | 📋 dipilih, belum dipasang |
| **Host web** | Vercel Hobby | tetap | gratis, non-komersial |
| **Host API** | ADR-025 (Railway → Vercel) | + jalan **mandiri**: PC pemilik lewat Tailscale Funnel | 📋 menunggu uji pemilik, bagian 5 |
| **PostgreSQL** | Azure (ADR-016) → Neon Free (ADR-019) | Neon Free, **atau** di PC pemilik bila API di sana | 📋 ikut host API |
| **Domain** | ~USD 1–2/bulan | `*.vercel.app` dan `*.ts.net` | ✅ nol |
| **Autentikasi (V1.1)** | Clerk | ditinjau saat V1.1 — Clerk tier gratis lawan OSS yang dipasang sendiri | ⏸ belum waktunya |

### 3. CI mandiri: gerbangnya dibawa pulang

`.\run.ps1 ci` / `make ci` = `verify` **+** `.github/scripts/ci-mandiri.sh`, dan
skrip itu menjalankan sisanya dengan urutan `ci.yml`: gitleaks atas **seluruh**
riwayat git, ketiga `docker build`, gerbang ADR-020, tumpukan produksi dari volume
kosong (= migrasi dari nol lewat bundel migrasi), pemindai halaman ADR-024, dan Trivy
atas ketiga citra. Satu skrip untuk Git Bash di Windows dan untuk Linux/WSL/macOS.

**Diukur 2026-09-28, bagian di luar `verify`: hijau dalam 178 detik.**

| Langkah | Detik | Hasil |
|---|---|---|
| gitleaks, seluruh riwayat | 7 | 80 commit (+ 20 merge yang tak berpatch), 0 bocor |
| tiga citra | 48 + 50 + 6 | — |
| gerbang ADR-020 | 2 | `health/live=200`, `POST=405` |
| tumpukan produksi | 13 | 6 migrasi dari nol, 14 bidang · 0 topik · 0 sisi |
| pemindai halaman | 2 | 16 halaman, 14 dari 14, 0 kena |
| Trivy | 40 | 0 CRITICAL/HIGH di ketiga citra |

**Dibuktikan merah, dua kali.** Rahasia palsu (`ghp_` + 36 karakter acak) di-commit
ke **klona sekali pakai** — repo aslinya tidak disentuh — dan perintah gitleaks yang
sama melaporkan *"81 commits scanned … leaks found: 1"*, exit 1. Port 8080 dipegang
pendengar milik uji itu sendiri: skrip berhenti di langkah pertama, menyebut portnya,
exit 1, dan **tidak mematikan apa pun**.

🐞 **Skripnya hijau, pintu masuknya merah — dua kali, di tempat yang sama.** Jalan
pertama `.\run.ps1 ci` gagal di langkah gitleaks yang sebenarnya lulus (*"80 commits
scanned"*). Dua sebab, keduanya diukur:

1. **`bash` di PowerShell adalah `C:\Windows\System32\bash.exe` — bash WSL**, bukan
   Git Bash tempat skripnya diuji. Distro WSL di mesin ini tidak punya `node`, jadi
   pemindai halaman pasti gagal di sana. `run.ps1` kini memanggil Git Bash dari
   samping `git`.
2. **PowerShell 5.1 mengubah setiap baris stderr program native jadi galat** begitu
   keluarannya dialihkan, dan dengan `ErrorActionPreference = 'Stop'` baris log `INF`
   pertama gitleaks menghentikan gerbang. Pelonggarannya hanya di sekitar panggilan
   itu; yang memutuskan tetap exit code skripnya.

Gerbang diukur lewat perintah yang akan diketik orang, bukan lewat skrip yang
dipanggilnya: skripnya sendiri sudah hijau dua kali sebelum ini ketahuan.

🐞 Percobaan merah pertama sempat hijau karena alasan yang salah: klonanya gagal
*checkout* (jalur Windows terlalu panjang), commit rahasia tidak pernah terjadi, dan
yang dipindai klona bersih. Angkanya — 80, bukan 81 — yang membongkarnya.

**Yang sengaja tidak ditiru:**

- **Job backend dan frontend** — itu `verify`. Uji integrasi menyematkan
  `localhost:5432`, jadi Postgres sekali pakai di port lain tidak bisa dipakai tanpa
  mengubah ujinya; "migrasi dari nol" dibuktikan bundel migrasi, bukan
  `dotnet ef database update`.
- **`act` (nektos/act)** — menjalankan `ci.yml` itu sendiri, tapi *bind mount*
  compose diselesaikan di host dan `permissions`/`timeout-minutes` diabaikan; hijau
  palsu lebih mahal daripada skrip yang jujur soal batasnya.

**Yang juga diubah di `ci.yml`:** `push` hanya untuk `main`. Cabang ber-PR tetap
diperiksa lewat `pull_request`, yang menguji hasil penggabungan; cabang tanpa PR
diperiksa CI mandiri. Hemat terukurnya seperempat menit repo ini.

**Batasnya, ditulis supaya tidak dibaca lebih:** CI mandiri tidak memberi tanda
centang di PR dan bergantung pada disiplin menjalankannya. Hasilnya dicatat di badan
PR, seperti selama #57. **Runner self-hosted** — gratis menurut dokumentasi GitHub,
dan dianjurkan hanya untuk repo privat — akan memberi tanda centang itu di mesin yang
sama, tapi mendaftarkannya adalah tindakan di akun pemilik, dan apakah job-nya lolos
blok billing belum terverifikasi. Ia diserahkan ke pemilik, bukan dipasang.

### 4. AI mandiri: diukur di mesin ini, dan batasnya ikut terukur

`qwen3:8b` (Apache-2.0) lewat Ollama (MIT), CPU saja, 2026-09-28:

| Uji | Hasil |
|---|---|
| Kecepatan keluaran | **5,3–5,6 token/detik**; prompt 49–370 token/detik; muat dingin 14,9 s |
| Memori | 6,7 GB, 100% CPU, konteks 8.192 |
| *"Apa itu MCP?"* **tanpa sumber** | 🔴 **salah** — *"protokol untuk mengatur dan membagikan konteks dalam sistem komunikasi … real-time"* |
| Pertanyaan yang sama **dengan sumber** (kutipan dokumentasi MCP) | ✅ setia pada sumber — *"standar open-source … menghubungkan aplikasi AI ke sistem eksternal"* — 21,5 s |

Dari situ keputusannya:

1. **Model lokal hanya menulis dari sumber yang sudah dikurasi.** Jalur
   `kurasi` → `draf` [ADR-012](ADR-012-template-halaman.md) memang begitu bentuknya,
   dan ukuran di atas memperlihatkan kenapa itu wajib, bukan gaya: tanpa sumber ia
   mengarang dengan tata bahasa yang meyakinkan. Label `draf` dan tinjauan manusia
   tetap satu-satunya jalan ke `tinjau`.
2. **Batch, bukan waktu-nyata.** Pada ~5,5 token/detik, satu bagian halaman
   (≈1.500 token) makan ±5 menit — wajar untuk antrean semalam, tidak wajar untuk
   pembaca yang menunggu. **AI Mentor untuk publik tidak dijalankan dari PC ini**
   sampai konkurensinya diukur.
3. **Bawaan [ADR-014](ADR-014-mcp-dan-penyedia-ai.md) pindah ke Ollama.** Abstraksinya
   sudah ada: Microsoft Agent Framework (1.0 untuk .NET, 3 April 2026; `Microsoft.Agents.AI`
   1.22.0 per 18 September) mendokumentasikan penyedia Ollama lewat **OllamaSharp** —
   paket pihak ketiga (MIT), bukan milik Microsoft — dan `OllamaApiClient`
   mengimplementasikan `IChatClient` **dan** `IEmbeddingGenerator`. Nama model tetap
   konfigurasi.
4. **Embedding: `qwen3-embedding:0.6b`, 1024 dimensi, kolom `vector(1024)` ber-HNSW.**
   Apache-2.0, 639 MB, multibahasa (MMTEB rata-rata 64,33 — di atas `bge-m3`
   59,56). 1024 di bawah batas indeks HNSW pgvector untuk `vector` (2.000), jadi
   jebakan dimensi yang [ADR-005](ADR-005-qdrant.md) tandai tidak menyala. Belum
   diunduh dan belum diukur — dipasang bersama embedding pertama.
5. **Model lain yang layak diuji, belum diukur:** `qwen3.5:9b` (6,6 GB) dan
   `gemma4:12b` (7,6 GB), keduanya Apache-2.0. Llama 3.1 **tidak**: lisensinya
   menuntut atribusi dan izin di atas 700 juta pengguna, dan bahasa Indonesia tidak
   ada di daftar bahasanya.
6. **Tier gratis ber-API hanya cadangan, hanya untuk isi publik.** Gemini tier gratis
   memakai data untuk memperbaiki produk Google dan dibaca peninjau manusia; tidak
   satu pun data pembaca boleh lewat sana. **GitHub Models sudah pensiun** (30 Juli
   2026) — jangan dirujuk.

### 5. Host: jalan mandiri ditambahkan, jalan berbayar dicoret

**Dicoret dari bagian 6 ADR-025:** Railway Hobby, Koyeb Pro, dan domain untuk
*named* Cloudflare Tunnel — ketiganya berbayar. **Oracle Always Free** juga tidak:
FAQ-nya menuntut kartu kredit atau debit untuk verifikasi identitas, tanpa
pengecualian, dan menolak kartu prabayar maupun virtual (Arm A1-nya pun kini 2 OCPU /
12 GB, bukan 24 GB).

**Ditambahkan — hibrida mandiri:** web tetap di Vercel Hobby
(`<proyek>.vercel.app`); API **dan** PostgreSQL berjalan di PC pemilik lewat
`docker-compose.prod.yml` — tumpukan yang sama yang CI mandiri nyalakan dalam 13
detik — dan diterbitkan lewat **Tailscale Funnel** (`<mesin>.<tailnet>.ts.net`,
tanpa domain).

| | Managed (ADR-025) | Hibrida mandiri |
|---|---|---|
| Tenaga | Railway Free: kredit USD 1/bulan, RAM 0,5 GB; Neon 0,5 GB | i7-14700, 15,8 GB, disk ratusan GB |
| Ketersediaan | vendor | selama PC menyala dan internet rumah hidup |
| Saat API mati | web tetap menyajikan halaman yang sudah tertembolok (dibuktikan ADR-019) | sama |
| Paparan | nol port rumah | satu port 443 lewat Funnel, **hanya** API — permukaan tulisnya mati di citra produksi (ADR-020) |
| Syarat vendor | Hobby non-komersial; melewati batas = fitur dijeda sampai 30 hari berlalu, tanpa tagihan | Hobby **dan** Tailscale Personal non-komersial; Funnel masih Beta, hanya port 443/8443/10000, batas bandwidth tidak dipublikasikan |

**Tidak dipilih di sini.** Keduanya gratis; yang membedakan adalah ketersediaan
lawan tenaga, dan itu keputusan pemilik. Uji Funnel butuh akun Tailscale — tindakan
di akun pemilik — dan tidak menuntut satu baris pun perubahan repo selain
`API_BASE_URL` web. ⚠️ Halaman harga Tailscale yang sekarang **tidak menyebut kartu
sama sekali**; pernyataan *"no credit card"* yang eksplisit hanya ada di blog harga
2023, sebelum skema harga April 2026. Paket Personal didapat dengan identitas domain
publik (GitHub, Gmail); email berdomain sendiri masuk uji coba bisnis. Aturan
ADR-025 berlaku: layar yang meminta kartu = uji gagal, berhenti.

### 6. Rantai pasok: gratis tetap rantai pasok

Maret 2026 Trivy disusupi (GHSA-69fq-xp46-6x23 / CVE-2026-33634): rilis v0.69.4, citra
Docker v0.69.5–v0.69.6, **`latest` selama jendela paparan**, dan 76 dari 77 tag
`aquasecurity/trivy-action` yang dipaksa-dorong. Advisory-nya menyebut citra yang
dirujuk lewat **digest** tidak terkena. Repo ini menarik `aquasec/trivy:latest` di
tiga tempat dengan soket Docker terpasang — salah satunya di `rilis-citra.yml`, job
yang sudah login ke GHCR dengan `packages: write`.

Dan Node 20 dicabut dari runner GitHub pada **23 September 2026**. Dibaca dari
`action.yml` di tag yang dipakai, **dua** aksi masih menyatakan `node20`:
`gitleaks/gitleaks-action@v2` dan `docker/login-action@v3`. Sejak tanggal itu runner
**memaksa** keduanya berjalan di Node 24 — tidak ditolak, tapi berjalan di runtime
yang tidak pernah diujikan penulisnya, dan opt-out
`ACTIONS_ALLOW_USE_UNSECURE_NODE_VERSION` sudah tidak berlaku.

🐞 Riset pertama menulis bahwa aksi `node20` *"berhenti bekerja"* — kalimat dari
catatan rilis gitleaks-action, yang juga masih menyebut tanggal rencana lama
(16 September). Pemeriksa membantahnya dari kode sumber runner (*"Always use Node 24
regardless of environment variables"*). Versi barunya tetap dipasang; alasannya yang
dibetulkan.

Aturannya sejak hari ini:

- **Aksi pihak ketiga disematkan per SHA** dengan versinya di komentar:
  `gitleaks-action` v3.0.0, `docker/login-action` v4.6.0 (keduanya `node24`, input
  sama). Aksi `actions/*` tetap per tag mayor — pemiliknya platform itu sendiri.
- **Citra alat disematkan per versi DAN digest:** Trivy 0.74.0
  (`sha256:62b1e65e…`, digest indeks multi-arsitektur), gitleaks v8.30.1
  (`sha256:c00b6bd0…`). Menaikkan versi berarti menaikkan digest di `ci.yml`,
  `rilis-citra.yml`, `ci-mandiri.sh`, dan `PENYEBARAN.md` bersamaan.
- **Digest Trivy itu diverifikasi, bukan dipercaya.** Tidak ada advisory yang
  menyebut 0.74.0 (rilis 14 Agustus 2026, jauh sesudah jendela paparan), dan
  tanda tangannya diperiksa 2026-09-28 dengan cosign v3.1.3: **dua tanda tangan**
  untuk digest tersebut, sertifikat untuk workflow `github.com/aquasecurity/trivy`
  dari penerbit OIDC GitHub Actions, tercatat di log transparansi. 🐞 cosign
  v2.5.3 melaporkan *"no signatures found"* untuk citra yang sama — Trivy menandatangani
  dengan format bundle Sigstore lewat OCI *referrers*, yang hanya dibaca v3. Hasil
  "tidak bertanda tangan" dari alat yang salah generasi bukan bukti apa pun.

## Konsekuensi

- **Pagu ADR-016 batal**, dan ketiga "nol" di tabelnya (Qdrant, Clerk, LangGraph)
  tetap nol karena alasan yang sama. Butir 4 (arXiv saja, tanpa ringkasan AI) tetap
  — alasan hukumnya tidak bergantung pada biaya.
- **CI mandiri adalah gerbang yang dihitung sampai Actions terbukti hidup lagi**, dan
  sesudahnya tetap jaring kedua: kuota yang dibagi 21 repo tidak bisa diandalkan.
- **Menit Actions repo ini turun kira-kira seperempat** tanpa gerbang yang hilang.
- **AI tetap belum ditulis** (ADR-007). Yang berubah: saat ditulis, penyedianya
  Ollama, dan batas "hanya dari sumber" sudah terukur.
- **Hibrida mandiri memindahkan tanggung jawab ke pemilik**: PC tidak boleh tidur,
  pembaruan OS, cadangan basis data. Itu harga "mandiri", dan ditulis supaya tidak
  ditemukan belakangan.

## Yang menunggu pemilik — tidak dijalankan di sini

| Keputusan | Kenapa bukan asisten |
|---|---|
| Pemakaian CI `wellsy` (1.053 menit dalam 4 hari) | repo lain; ia yang menentukan apakah kuota bersama bertahan sebulan |
| Mendaftarkan runner self-hosted untuk repo ini | tindakan di akun; apakah ia lolos blok billing belum terverifikasi |
| Akun Tailscale untuk uji Funnel | pendaftaran akun |
| Menjadikan repo publik | **tidak disarankan**: tidak terbukti membuka blok (laporan komunitas menyebut repo publik ikut terblok), membuka seluruh riwayat, dan membuat runner self-hosted berbahaya |

## Cara membatalkan keputusan ini

Menaikkan pagu di atas nol adalah keputusan pemilik yang ditulis sebagai ADR baru —
bukan pengecualian diam-diam per pos. Skrip CI mandiri, sematan SHA/digest, dan
pemicu `push: [main]` berdiri sendiri; ketiganya tetap benar pada pagu berapa pun.
