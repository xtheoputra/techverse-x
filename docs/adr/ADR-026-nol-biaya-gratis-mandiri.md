# ADR-026 — Nol biaya, dan TechVerse X berdiri sendiri

**Status:** Diterima — arahan dan pilihan pemilik, 2026-09-28. **Menggantikan** pagu
USD 60 di [ADR-016](ADR-016-pagu-biaya.md) butir 1 dan bawaan OpenAI di
[ADR-014](ADR-014-mcp-dan-penyedia-ai.md). Host API tetap milik
[ADR-025](ADR-025-host-api-pengganti-koyeb.md) (Diusulkan); yang berubah di sana hanya
jalan keluar berbayar yang dicoret. Issue [#57](https://github.com/xtheoputra/techverse-x/issues/57) — **terjawab**:
repo publik sejak 2026-09-28, dan CI berjalan lagi.
**Tanggal:** 2026-09-28

## Konteks

Pemilik menulis dua pesan di hari yang sama:

> *"hindari penggunaan yang berbayar, cari semua mandiri dan secara gratis tapi
> powerfull"*

> *"bukan gunakan mesin ini, kalo agent tetap gunakan versi gratis, lalu apa
> hubungannya proyek ini dan proyek lain?, saya ingin membangun proyek ini sendiri
> tanpa terhubung dengan proyek lain"*

🔴 **Versi pertama ADR ini (commit `dba033c`) salah membaca kata "mandiri"** sebagai
"berjalan di mesin pemilik": Ollama lokal sebagai penyedia AI, API dan basis data di
PC pemilik lewat Tailscale Funnel, dan gerbang CI di mesin itu sebagai pengganti
Actions. Pesan kedua membetulkannya. **Mandiri berarti proyek ini berdiri sendiri** —
tidak berbagi kuota, akun, tagihan, atau mesin dengan proyek lain pemilik — dan
infrastrukturnya tidak bergantung pada PC siapa pun. Ketiga keputusan berbasis PC
ditarik; yang tersisa dari versi pertama adalah yang tetap benar di bawah arti itu.

### Keterhubungan yang sungguh ada — diukur

CI repo ini mati sejak 10 September dengan anotasi *"recent account payments have
failed or your spending limit needs to be increased"*. Sebabnya bukan kode, melainkan
**akun**: paket GitHub Free memberi 2.000 menit Actions per bulan untuk **semua** repo
privat sebuah akun, dan repo ini tinggal di akun pribadi bersama 20 repo privat lain.
Laporan billing **harian** September (`GET /users/{user}/settings/billing/usage?year=&month=&day=`):

| Repo | Menit 1–10 Sep |
|---|---|
| repo privat lain A | **1.053** — 978 di antaranya dalam dua hari |
| `techverse-x` | 558 |
| repo privat lain B | 481 |
| repo privat lain C | 4 |
| **Jumlah menit privat** | **2.096** — kuota terlewati **10 September** |

(Satu repo lain yang saat itu **publik** memakai 43 menit; menit repo publik tidak
masuk kuota.) Satu proyek lain menghabiskan **lebih dari separuh** kuota dalam empat
hari, dan proyek ini ikut mati. Itu jawaban atas *"apa hubungannya proyek ini dan
proyek lain"*.

**Angka ini bukan temuan baru.** Sesi 14 (16 September) sudah mengukurnya dengan
benar di [komentar #57](https://github.com/xtheoputra/techverse-x/issues/57#issuecomment-5695424090),
termasuk bahwa `push` di cabang `fase-*` memakan ±26% menit repo ini.

🐞 Yang salah datang sesudahnya: Sesi 15, lalu catatan pagi ini, menyalahkan run
Dependabot repo B. Keduanya salah: dokumentasi GitHub menulis bahwa Dependabot
*"does not count towards your included GitHub Actions minutes"*, dan endpoint billing
**bulanan** yang dibaca keduanya melaporkan seluruh 2.223 menit September di bawah
**satu** nama repo. Hitungan harian membaginya ke lima. Endpoint `timing` juga tidak
bisa dipakai: `billable` 0 ms untuk semua 144 run repo B, termasuk yang berjalan 183
menit.

📏 Dan seperempat menit repo ini sendiri terbuang: setiap push ke cabang ber-PR
menjalankan CI dua kali (`push` + `pull_request`, commit yang sama) — 30 dari 63
commit, 147 dari 578 menit (per job, dibulatkan ke atas; cocok dengan 558 di billing).

### Cara riset ini dilakukan

Seperti ADR-025: riset sumber primer dengan kutipan dan tanggal, lalu pemeriksa
terpisah yang tugasnya **membantah**. Dua putaran hari ini — 20 klaim (17 bertahan,
3 bersyarat, 1 terbantah) dan 17 klaim (sebagian besar bertahan, sembilan ditulis
ulang). Klaim di bawah ditulis dalam bentuk yang lolos pemeriksa, termasuk syaratnya.

## Keputusan

### 1. Prinsip

- **Nol biaya.** Berbayar boleh dicatat sebagai fakta, tidak pernah direkomendasikan —
  termasuk *"naikkan batas belanja"*, domain, dan kunci API berbayar.
- **Berdiri sendiri.** Tidak ada kuota, akun, tagihan, atau mesin yang dibagi dengan
  proyek lain. Di mana sebuah tier gratis dihitung **per akun**, akun itu dipakai
  **khusus** TechVerse X — dan tidak ada akun ganda untuk melipatgandakan jatah:
  aturan GitHub (*"no more than one free Account"*), Vercel (*"create multiple
  accounts"* di daftar larangan), dan Railway (*"evading usage or billing limits"*)
  melarangnya.
- **Bukan mesin pemilik.** PC pemilik hanya tempat menulis kode.
- **"Powerful" diukur, bukan diklaim.**

### 2. GitHub: akun yang sama, repo publik — pilihan pemilik, dan terbukti bekerja

Tiga jalur ditimbang; pemilik memilih **tetap di akun pribadi, repo dijadikan publik**
(dijalankan 2026-09-28 05:36Z).

| Jalur | Dipilih? | Sebab |
|---|---|---|
| **Akun pribadi, repo publik** | ✅ **pemilik** | tanpa pindah, tanpa ganti nama; menit runner standar untuk repo publik **tidak masuk kuota** yang dibagi repo privat |
| Organisasi gratis baru + repo publik | — | tagihan terpisah, tapi pemindahan membawa ganti nama, citra GHCR baru, dan lisensi untuk `gitleaks-action` |
| Organisasi gratis baru + repo privat | ❌ | Vercel Hobby **tidak bisa** men-deploy dari repo privat milik organisasi |

Yang menopangnya, per sumber primer: *"The use of standard GitHub-hosted runners is free:
In public repositories"* — gratis, **bukan tanpa batas**: 20 job serentak di paket
Free, 6 jam per job, dan *larger runner* selalu berbayar.

📏 **Yang tidak dijawab dokumentasi mana pun, dijawab dengan menjalankannya.** Apakah
blok billing akun (*"recent account payments have failed…"*) ikut menahan repo publik
milik akun yang sama? Laporan komunitas menyebut ya. Diukur: satu menit sesudah repo
dijadikan publik, run CI [#70](https://github.com/xtheoputra/techverse-x/pull/70)
(`36380051993`) **menjalankan langkahnya** — keempat job hijau, 05:36:54Z → 05:39:55Z.
Di runner: gitleaks **83 commit, 0 bocor**; **104 unit + 65 integrasi**; gerbang
ADR-020 `200/405`; tumpukan produksi dari nol, 16 halaman, 14 dari 14; Trivy **0**
di ketiga citra. Run pertama yang benar-benar berjalan sejak 10 September.

⚠️ **Yang tetap terhubung, ditulis supaya tidak dibaca lebih:** repo ini masih tinggal
di akun yang sama dengan repo privat lain. Menit dan tagihannya kini terpisah dalam
praktik — repo publik tidak memakai kuota — tapi akunnya satu. Kalau suatu hari repo
ini dijadikan privat lagi, keterhubungan #57 kembali utuh.

**Yang dibuka ke publik, dan sudah diperiksa sebelum dibuka:** seluruh riwayat git
dipindai gitleaks — 82 commit, **0 bocor**; `.env` tidak pernah ter-commit, hanya
`.env.example` berisi nilai pengembangan lokal. Workflow aman untuk repo publik:
`pull_request` dari fork berjalan dengan token baca-saja dan tanpa secret, dan
satu-satunya secret (`NEON_DATABASE_URL`) hanya dipakai `workflow_dispatch`. ⚠️ Yang
**ikut terbuka**: alamat email penulis di 188 commit; nama repo privat lain pemilik di
catatan Sesi 14 dan 15, di 5 commit riwayat, dan di komentar #57 (ADR ini dan catatan
sesi 2026-09-28 menyamarkannya). Repo juga belum punya berkas `LICENSE` — tanpanya
kode publik tetap *all rights reserved*.

### 3. CI: tetap GitHub Actions, di luar kuota bersama

- **Gerbang yang dihitung tetap `ci.yml`**, dan sejak repo publik ia berjalan lagi — di
  luar kuota yang dibagi repo privat.
- **`push` hanya untuk `main`**, plus `workflow_dispatch`. Cabang ber-PR diperiksa
  lewat `pull_request`, yang menguji hasil penggabungan. Sesi 14 sengaja menunda
  perubahan ini karena *"tidak bisa dibuktikan selama CI tidak berjalan"* — tapi run
  yang terblok tetap **dibuat**, jadi jumlahnya terukur: di
  [#70](https://github.com/xtheoputra/techverse-x/pull/70) **satu** run, bukan dua. Hemat
  menitnya sendiri baru terukur saat CI berjalan lagi.
- **gitleaks jalan sebagai CLI (MIT) dalam peti kemas**, bukan `gitleaks-action`: CLI-nya
  tanpa kunci, tanpa `GITHUB_TOKEN`, tanpa izin `pull-requests: read`, dan perintahnya
  sama dengan gerbang lokal. Aksinya bukan lagi MIT sejak v2 dan menuntut
  `GITLEAKS_LICENSE` begitu repo dimiliki organisasi — jalur yang kini tidak diambil,
  tapi tidak lagi jadi alasan gerbang ini bisa merah. Di runner sungguhan: **83 commit
  dipindai**, bukan nol.
- **`.\run.ps1 ci` / `make ci` adalah alat pengembang**, bukan pengganti CI: seluruh
  gerbang `ci.yml` sebelum push, hijau end-to-end dalam 161 detik, dibuktikan merah
  (rahasia palsu di klona sekali pakai → exit 1). Dua jebakan Windows yang ditemukan
  saat mengukurnya: `bash` di PowerShell adalah bash WSL (tanpa `node` di distro
  ini), dan PowerShell 5.1 mengubah stderr `INF` gitleaks jadi galat terminasi.
  `run.ps1` memanggil Git Bash dan menilai exit code saja.
- **Runner self-hosted: tidak.** Ia gratis, tapi berjalan di mesin pemilik.

### 4. AI: tier gratis layanan, tanpa kartu

| Pemakaian | Penyedia | Kenapa | Kapasitas gratis |
|---|---|---|---|
| **Draf dari sumber terkurasi** (batch) | **Groq**, `openai/gpt-oss-120b` | kartu hanya untuk naik tier; Services Agreement §4.2: *"Groq is not permitted to use Inputs or Outputs for training"* | 30 RPM · 1K RPD · 8K TPM · **200K token/hari** — ±40 draf/hari sebelum token penalaran |
| **AI Mentor** (pertanyaan pembaca) | **Cloudflare Workers AI** | *"Start building for free — no credit card required"*; *"Cloudflare does not use your Customer Content to (1) train any AI models"* | **10.000 neuron/hari**; `gpt-oss-120b` 31.818 / 68.182 neuron per juta token masuk/keluar |
| **Embedding** konten dan kueri | Workers AI, `@cf/qwen/qwen3-embedding-0.6b` | model sama untuk konten dan kueri | 1.075 neuron per juta token |

- **Semua lewat endpoint yang kompatibel OpenAI** — Groq
  `https://api.groq.com/openai/v1`, Workers AI `…/accounts/{id}/ai/v1` — jadi
  abstraksi [ADR-014](ADR-014-mcp-dan-penyedia-ai.md) cukup; penyedia dan model tetap
  konfigurasi.
- **Groq dan Cloudflare saling jadi cadangan**; keduanya menjanjikan tanpa pelatihan.
- **Jatah Cloudflare satu untuk semua**: Mentor dan embedding mengambil dari 10.000
  neuron yang sama. Habis = error 3036 / HTTP 429 sampai reset 00:00 UTC. Karena itu
  pagu panggilan harian global [ADR-016](ADR-016-pagu-biaya.md) butir 5 tetap berlaku,
  dan jawaban *"Mentor sedang penuh"* adalah perilaku yang dirancang, bukan galat.
- **Dimensi embedding hingga 1.024** menurut kartu model Qwen — Cloudflare sendiri
  tidak mendokumentasikannya, jadi diverifikasi dari `shape` panggilan pertama sebelum
  kolom `vector(…)` ditulis. 1.024 di bawah batas HNSW pgvector (2.000).
- **Tidak boleh menerima data pembaca:** tier gratis **Gemini** (isi dipakai untuk
  produk Google dan bisa dibaca peninjau manusia; paket gratis juga tidak boleh
  melayani pengguna di EEA, Swiss, dan Inggris), **Mistral** mode gratis (boleh
  melatih; *"testing and prototyping"*), dan model `:free` **OpenRouter** (sebagian
  penyedianya mencatat atau melatih).
- **Gugur:** Cerebras (API tidak aktif tanpa metode pembayaran), trial Cohere (*"not
  permitted to be used for production or commercial purposes"*), GitHub Models
  (pensiun 30 Juli 2026). Ollama Cloud tercatat tanpa-pelatihan, tapi kebutuhan kartunya
  belum terverifikasi dan tier gratisnya hanya *"starter models"*.
- **Akun Groq dan Cloudflare khusus TechVerse X** — batas Groq berlaku *"at the
  organization level"*, jatah Workers AI per akun.

**Satu pengukuran dari versi pertama yang tetap berlaku untuk penyedia mana pun.**
Model kecil (`qwen3:8b`, diukur di mesin pengembang) menjawab *"apa itu MCP?"* **salah
dengan yakin** tanpa sumber — *"protokol … sistem komunikasi real-time"* — dan benar
begitu diberi kutipan dokumentasinya. Jadi AI hanya menulis dari sumber yang sudah
dikurasi (jalur `kurasi` → `draf`, [ADR-012](ADR-012-template-halaman.md)), dan
tinjauan manusia tetap satu-satunya jalan ke `tinjau`.

### 5. Hosting: tier gratis vendor, akun khusus

- **Web — Vercel Hobby.** Jatahnya dihitung per tim Hobby untuk semua proyeknya, jadi
  akun itu khusus TechVerse X (pilihan pemilik). Non-komersial. Melewati batas =
  dijeda sampai 30 hari berlalu, tanpa tagihan. Repo tetap milik akun pribadi, jadi
  larangan Hobby atas repo organisasi tidak berlaku.
- **API — urutan uji [ADR-025](ADR-025-host-api-pengganti-koyeb.md) tetap.** Railway Free
  memberi USD 1 per bulan per akun (syarat R7 *"≤ $0,70"* sudah mengantisipasinya) dan
  tidak bisa menarik citra registry privat (*"Private registry credentials are available
  on the Pro plan"*); ia membangun dari Dockerfile repo, seperti rancangan R3.
- **PostgreSQL — Neon Free.** Batasnya per proyek (100 CU-jam, 0,5 GB), jadi satu
  proyek Neon untuk TechVerse X sudah terisolasi.
- **Dicoret dari ADR-025 bagian 6:** Railway Hobby, Koyeb Pro, domain untuk *named*
  Cloudflare Tunnel (berbayar), dan Oracle Always Free (menuntut kartu).
- **Ditarik:** hosting dari PC pemilik lewat Tailscale Funnel — versi pertama ADR ini.

### 6. Rantai pasok: gratis tetap rantai pasok

- **Trivy disusupi Maret 2026** (GHSA-69fq-xp46-6x23 / CVE-2026-33634): citra
  v0.69.4–v0.69.6, dan **`latest` selama jendela paparan**, membawa pencuri kredensial.
  Repo ini menariknya `:latest` di tiga tempat dengan soket Docker — satu di
  `rilis-citra.yml`, job yang sudah login ke GHCR dengan `packages: write`. Kini
  `0.74.0@sha256:62b1e65e…` (digest indeks multi-arsitektur), dan tanda tangannya
  **diverifikasi** dengan cosign v3.1.3: dua tanda tangan, sertifikat untuk workflow
  `github.com/aquasecurity/trivy`, tercatat di log transparansi. (cosign v2.5.3
  melaporkan *"no signatures found"* — format bundle baru hanya dibaca v3.)
- **Node 20 dicabut dari runner pada 23 September 2026**; aksi yang menyatakan `node20`
  kini **dipaksa** berjalan di Node 24 (bukan ditolak). `docker/login-action` v3 → v4.6.0,
  disematkan SHA.
- **Aturannya:** aksi pihak ketiga per SHA; citra alat per versi **dan** digest —
  gitleaks v8.30.1 (`sha256:c00b6bd0…`), Trivy 0.74.0. Menaikkan versi berarti
  menaikkan digest di `ci.yml`, `rilis-citra.yml`, `gerbang-ci-lokal.sh`, dan
  `PENYEBARAN.md` bersamaan.

## Konsekuensi

- **Pagu ADR-016 batal.** Butir 4 (arXiv saja, tanpa ringkasan AI) tetap — alasan
  hukumnya tidak bergantung pada biaya.
- **CI hidup lagi sejak repo publik** — tanpa menunggu 1 Oktober, dan tanpa bergantung
  pada pemakaian menit repo lain.
- **Kode dan riwayat terbuka.** Itu harga menit gratis di repo publik, dan pilihan
  pemilik. Menjadikannya privat lagi tidak menarik kembali apa yang sudah dibaca atau
  disalin orang — dan mengembalikan keterhubungan #57.
- **Nama repo, tautan, dan citra GHCR tidak berubah** — tidak ada pemindahan.
- **AI tetap belum ditulis** (ADR-007). Saat ditulis: Groq dan Cloudflare lewat
  endpoint kompatibel OpenAI, dengan pagu harian dan cadangan satu sama lain.

## Yang dikerjakan pemilik

1. ~~Menjadikan repo publik~~ — **selesai 2026-09-28**, atas perintah pemilik, dan CI
   terbukti berjalan lagi.
2. **`LICENSE`** — tanpanya kode publik tetap *all rights reserved*. Pilihan lisensinya
   milik pemilik.
3. **Nama repo privat lain** di catatan Sesi 14–15 dan komentar #57 — menyuntingnya bisa
   dibalik; riwayat git tidak ditulis ulang.
4. Akun **Vercel, Railway, Neon** khusus TechVerse X (pilihan pemilik) saat uji ADR-025
   dijalankan; akun **Groq** dan **Cloudflare** khusus TechVerse X saat kode AI pertama
   ditulis, bukan sekarang.

## Cara membatalkan keputusan ini

Menaikkan pagu di atas nol adalah keputusan pemilik yang ditulis sebagai ADR baru —
bukan pengecualian diam-diam per pos. Repo bisa dijadikan privat lagi, tapi itu tidak
menarik kembali apa yang sudah terbuka, dan kuota bersama #57 berlaku lagi. Sematan
SHA/digest, pemicu `push: [main]`, dan gitleaks CLI berdiri sendiri — tetap benar di
mana pun repo tinggal.

---

## Pembaruan 2026-09-29 - runner disematkan sebelum `ubuntu-latest` berpindah, dan citra GHCR tidak ikut terbuka

### Runner per versi Ubuntu

GitHub mengumumkan ([actions/runner-images#14748](https://github.com/actions/runner-images/issues/14748),
17 September) bahwa `ubuntu-latest` pindah ke Ubuntu 26.04 **bergilir** antara 19 Oktober
dan 19 November 2026. Anotasinya sudah muncul di run [#69](https://github.com/xtheoputra/techverse-x/pull/69)
tanggal 28 September, dan dicatat tanpa ditindak.

📏 **Diukur sebelum labelnya bergerak.** `ci.yml` dijalankan utuh di `ubuntu-26.04` dari
cabang sekali pakai di atas `bcbd18c` — kepala #70 — lewat `workflow_dispatch`. Cabang itu
berbeda dari `ci.yml` di dua hal saja: labelnya, dan `if` job `citra` yang dilonggarkan
supaya ikut berjalan saat dispatch. Cabangnya dihapus sesudahnya; run-nya tetap ada.

| | `ubuntu-24.04` — run `36385270062` | `ubuntu-26.04` — run `36512838453` |
|---|---|---|
| Citra runner | 20260920.314.1 | 20260920.143.1 |
| Backend | 104 unit + 65 integrasi, 6 migrasi dari nol | **sama** |
| Frontend | 303 tautan, lint, build | **sama** |
| Pindai rahasia | 86 commit, 0 bocor | 87 commit (+1: commit uji itu sendiri), 0 bocor |
| Citra | ADR-020 `200/405` · 16 halaman · 14 dari 14 · teks terlihat 0 kena · Trivy 0 di ketiga citra | **sama** |

Jadi perpindahan 19 Oktober **tidak** memerahkan `ci.yml` — diukur di commit yang sama,
bukan diduga dari daftar perangkat lunak citranya.

**Keputusan: keenam `runs-on` di ketiga workflow disematkan ke `ubuntu-24.04`.**

- Selama jendela bergilir, `ubuntu-latest` bisa memberi dua OS pada dua jalan
  berturut-turut, dan merah di sana punya dua tersangka.
- 24.04 adalah tempat **semua** jalan hijau sampai hari ini — termasuk `rilis-citra.yml`
  dan `migrasi-produksi.yml`, yang tidak bisa diuji ulang di 26.04 tanpa mendorong citra ke
  GHCR dari cabang yang belum di-merge, atau tanpa secret Neon yang belum ada.
- **Pindah ke 26.04 = mengganti label di keenam tempat itu.** Yang belum diukur di sana
  hanya langkah dorong ke GHCR dan langkah migrasi ke Neon.
- ⚠️ **Yang tidak dijamin sematan ini:** citra runner 24.04 tetap diperbarui GitHub tiap
  minggu — yang disematkan versi OS-nya, bukan isinya. Alat yang menentukan hasil sudah
  disematkan sendiri: SDK .NET lewat `global.json`, Node 22 lewat `setup-node`, kedua
  pemindai per digest.

Aturan bagian 6 bertambah satu baris: **runner per versi OS**, di samping aksi per SHA dan
citra alat per digest.

### Citra GHCR tidak ikut terbuka

Bagian 2 menulis apa yang ikut terbuka saat repo dijadikan publik. **Paket GHCR tidak
termasuk** — visibilitasnya terpisah dari repo. Diukur tanpa login: token anonim untuk
`xtheoputra/techverse-x/api`, `migrate`, dan `web` ditolak `UNAUTHORIZED`, sedangkan citra
publik pembanding (`aquasecurity/trivy`) langsung diberi token. Kredensial tarik di
[PENYEBARAN](../PENYEBARAN.md#kredensial-untuk-menarik-citra) karena itu tetap wajib.

Satu akibat untuk [ADR-025](ADR-025-host-api-pengganti-koyeb.md) — **dicatat, tidak
diputuskan**. Railway Free tidak punya kredensial registry privat, jadi R3 membangun dari
Dockerfile, dan yang tayang bukan bit yang dipindai Trivy. Kalau ketiga paket dibuka, host
tanpa kredensial bisa menarik citra yang sama persis dengan yang dipindai. Tapi apakah
Railway Free menarik citra GHCR publik **belum diukur**, dan membuka paket berarti membuka
artefak yang sampai hari ini privat. Pilihan pemilik, di samping `LICENSE`.
