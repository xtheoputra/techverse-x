# Catatan Sesi Kerja

Urutan terbaru di atas. Berkas ini mencatat **apa yang terjadi dan kapan** — bukan isi temuannya. Temuan tinggal di berkas auditnya masing-masing.

---

## 2026-09-24 — Sesi 15: tiga pekerjaan yang tidak menunggu siapa pun, dan tiga penjaga yang hijau karena tidak melihat

Permintaan pemilik: *"lanjutkan semua tugas yang belum terselesaikan, sesuai dokumen, kerjakan dengan sempurna"*.

### Yang diwarisi — dibaca dari catatan sesi, lalu DIUKUR ULANG

Sesi 14 (lanjutan) berakhir rapi: semua cabang ter-push, pohon bersih, nol stash. Yang tertinggal bukan pekerjaan setengah jalan melainkan **daftar tunggu**, dan tiap barisnya diperiksa dulu sebelum dipercaya:

| Yang tertulis | Keadaan sesudah diukur hari ini |
|---|---|
| CI mati karena kuota ([#57](https://github.com/xtheoputra/techverse-x/issues/57)) | **masih mati** — jalan-ulang run `35189801706` attempt 2: `02:05:20Z` → `02:05:27Z`, **7 detik**, `steps=0` di keempat job |
| Uji Railway R1–R6, Neon, Vercel | tetap menunggu pemilik; **tidak satu akun pun dibuat** |
| Rantai PR #56←#58←#62←#63 | belum satu pun di-merge; **tidak dicoba** |
| *"Belum terverifikasi"*: tautan `../../issues/N` di `docs/*.md` | **terbukti rusak**, dan lebih luas daripada dugaannya |
| Harness JS `apps/web` — *"keputusan tersendiri"* | dikerjakan **separuh**, dan separuh yang tidak dikerjakan ditulis |
| Siklus transitif — *"wajib mendarat lebih dulu"* ([#59](https://github.com/xtheoputra/techverse-x/issues/59)) | **dibangun** |

🔑 Yang dikerjakan hari ini dipilih dengan satu saringan: **tidak menunggu akun, merge, atau pemicu tertulis.** Tiga pekerjaan lolos saringan itu, dan ketiganya jadi PR sendiri.

### 🔴 Klaim tautan sesi lalu benar — dan jalan keluarnya juga keliru

Diukur di halaman GitHub yang **hidup**, tiga repo publik, tanpa login:

| # | Sumber | Ditulis | Dirender GitHub |
|---|---|---|---|
| 1 | `enquirer/enquirer` `docs/install.md` | `../../../issues/new` | `href="/enquirer/enquirer/issues/new"` |
| 2 | `nana-4/materia-theme` `TODO.md` (**akar**) | `../../issues/106` | `href="/nana-4/materia-theme/issues/106"` |
| 3 | `dotnet/runtime` cabang **`release/8.0`** | `../design/…` | `href="/dotnet/runtime/blob/release/8.0/docs/design/…"` |

Pengukuran 1+2 memperlihatkan jebakannya: **dua `../` benar di akar repo, kurang satu di `docs/`** — persis bentuk cacatnya di sini, konvensi `README.md` yang disalin ke `docs/`. Pengukuran 3 membantah jalan keluar yang sesi lalu usulkan (*"tautan baru memakai `../../../`"*): nama ref bergaris miring memakan **dua** segmen, jadi cabang `fase-1/...` menggeser hitungannya lagi.

**71 dari 222** rusak dibaca di `main`; **221 dari 222** dibaca di cabang. Yang kedua yang paling merugikan — badan issue di repo ini menautkan ADR lewat blob **cabang**.

Jadi yang diperbaiki bukan hitungannya melainkan aturannya: tautan relatif hanya untuk berkas **di dalam** repo. 222 tautan di 32 berkas, dan buktinya bukan pembacaan mata: tiap hasil dibandingkan **bita per bita** dengan HEAD yang ditransformasi ulang → 24 sama persis, 8 sama sesudah akhir baris dinormalkan (CRLF yang **sudah** ada di HEAD), **nol beda**.

### 🐞 Ketiga penjaga baru pernah hijau karena TIDAK MELIHAT

Ini benang merah sesi ini, dan ketiganya ketahuan karena dijalankan, bukan dibaca:

1. **Penjaga tautan melaporkan tiga temuan yang tidak ada** — prosa yang **mengutip** kode (`` `<Link href="/learn">` `` di catatan sesi) terbaca sebagai jangkar HTML. Potongan kode sebaris dibuang lebih dulu.
2. **Aturan *"jangan keluar dari pohon repo"* punya lubang.** Dari berkas empat tingkat dalam, empat `../` mendarat **tepat di akar repo** — jadi dua tautan issue di `apps/web/src/lib/lingkungan.ts` lolos hijau, padahal di halaman blob keduanya jatuh ke `…/blob/main/issues/42`, berkas yang tidak ada. Sekarang resolusi jalur **dan** keberadaan berkas diperiksa, untuk semua berkas.
3. **Pemindai halaman berhenti di 21 halaman dan `/cari` tidak pernah dikunjungi** — satu-satunya jalan ke sana `<form action="/cari" method="get">`, bukan `<a href>`. Akibatnya pemeriksaan `id="q"` tidak pernah berjalan sekali pun. **Angka 21 itulah yang membocorkannya** (beranda + 20 topik, tanpa `/cari`). Formulir GET kini ikut ditelusuri; POST tidak, sebab ia bukan navigasi.

Dan penjaga tautan sempat **mengusulkan perbaikan yang rusak**: untuk tautan berakar yang sudah memuat nama repo, usulnya `…/techverse-x/xtheoputra/techverse-x/issues/61`.

💡 *Penjaga yang belum pernah merah bukan penjaga. Tapi penjaga yang merahnya belum pernah DIHITUNG juga belum terbukti — tiga dari empat cacat di atas hanya terlihat setelah angka keluarannya dibaca, bukan setelah exit code-nya dibaca.*

### 🔴 Siklus bertiga diterima 200, dan ramalan ADR-023 kurang satu kata

Pemeriksaan siklus V1 hanya melihat **satu lompatan**. Diukur lewat API sungguhan sebelum disentuh: `a→b` 200, `b→c` 200, lalu `c→a` **juga 200** — lingkaran tertutup, barisnya tersimpan, uji barunya berbunyi `Expected: BadRequest / Actual: OK`.

Yang diganti hanya isi argumen kedua (penutupan transitif, disapu berlapis dengan LINQ EF biasa; **CTE rekursif ditolak** — ia akan jadi SQL mentah pertama di lapisan fitur, dan menuntut nama tabel ditulis tangan terlepas dari pemetaan EF). Harganya ditulis jujur: **kedalaman + 1** perjalanan.

🔴 Tapi *"peningkatannya hanya menyentuh handler"* benar untuk **tanda tangannya**, bukan untuk kalimat galatnya: *"Dua topik tidak boleh saling mensyaratkan"* adalah pernyataan yang **salah** untuk lingkaran bertiga — ia menyuruh penulisnya mencari sisi kebalikan yang tidak ada.

🐞 **Dan komentar saya sendiri terbantah sebelum di-commit**, kedua kalinya di repo ini. Versi pertamanya berbunyi *"sisi A→B yang sudah tersimpan menaruh A di dalam penutupan B"* — keliru; penutupan B berisi yang **dibutuhkan** B. Yang benar lebih sempit dan hanya mengenai baris lama yang sudah berputar. Kalimat soal titik awal juga dipersempit dengan cara yang sama.

### 🔴 Sabotase saya sendiri kurang satu tempat

Untuk membuktikan penjaga penelusuran bisa merah, `<Link>` topik dibuang dari `TechnologyList` — dan hasilnya **tetap 20 dari 20**. Halaman bidang punya `<Link>` sendiri di `app/teknologi/[slug]/page.tsx`. Sesudah keduanya dibuang: **14 dari 20**, dengan keenam topiknya disebut satu per satu. Itu sebabnya catatan Sesi 14 menyebut **dua** tempat — dan itu ketahuan lagi hari ini hanya karena angkanya dibaca.

### Yang dibangun

| PR | Cabang | Isi |
|---|---|---|
| [#64](https://github.com/xtheoputra/techverse-x/pull/64) | `fase-1/tautan-markdown-absolut` | 222 tautan jadi absolut + `.github/scripts/cek-tautan-markdown.mjs`, ikut `verify` dan CI |
| [#65](https://github.com/xtheoputra/techverse-x/pull/65) | `fase-2/siklus-prasyarat-transitif` | penutupan transitif di `RequireTopicHandler`, pesan galat baru, pembaruan ADR-023 |
| [#66](https://github.com/xtheoputra/techverse-x/pull/66) | `fase-2/gerbang-halaman-web` | `.github/scripts/periksa-halaman-web.mjs`, pembaruan ADR-024 |

### 🔎 Koreksi #57: "pemblokiran se-AKUN" terlalu luas

Endpoint billing lama sudah **410** (*"This endpoint has been moved"*); yang baru `GET /users/{user}/settings/billing/usage`. Dari sana, dan dari API run:

Di repo **privat** `zarastrade`, run `35547048935` ber-`event: dynamic` (Dependabot Updates) **menjalankan 5 langkah** dan sukses, `00:14:25Z` → `00:19:30Z` **21 September** — dua pekan sesudah run biasa mulai ditolak. Dan itu **ditagih**: baris `Actions Linux` 27 menit (11 Sep), 19 (14 Sep), **20 (21 Sep)**.

Jadi yang ditolak adalah run `push`/`pull_request`; run Dependabot lolos dan terus memakan kuota yang sudah lewat batas. `techverse-x` **tidak** punya `.github/dependabot.yml`, jadi repo ini tidak menambah apa pun lewat jalan itu. Total menit repo privat September: **2.162** dari kuota 2.000. Perkiraan pulih 1 Oktober 2026 tetap **belum terbukti**. Diposting ke [#57](https://github.com/xtheoputra/techverse-x/issues/57#issuecomment-5806628640).

💡 *Menyonde CI tidak berbiaya: nol langkah berarti nol menit. Yang mahal justru menganggapnya masih mati tanpa memeriksa.*

### Ukuran

`run.ps1 verify` hijau: Release **0 peringatan**, **104 unit + 63 integrasi** (dari 102 + 61), cek tautan, lint, build web. Penjaga halaman terhadap build produksi + API dev: **22 halaman, 20 dari 20 yang dikenal API, teks terlihat 0 kena, setiap halaman 200**.

Basis data pengembang ditinggalkan seperti saat sesi mulai: 6 topik contoh, 1 sisi, **nol sisa uji**, nol ringkasan bidang menyebut ADR, nol basis data `gladi%`.

### Yang TIDAK dikerjakan, dan kenapa

- **Tidak ada yang di-merge, dan tidak dicoba.** Rantainya kini **tujuh** PR dalam dan urutannya wajib dari bawah.
- **`make cek-tautan` dan `make cek-halaman` tidak dijalankan**: `make` tidak terpasang di mesin ini. Keduanya ditulis, tidak diklaim terukur.
- **Penjaga halaman tidak masuk `verify` maupun CI.** Ia menuntut web dibangun + API hidup; job `frontend` tidak punya keduanya. Jalan ke CI (Postgres + API + `next start` di satu job) tertulis sebagai langkah berikutnya.
- **Uji komponen `apps/web`** (`TopikTerhubung` dan kawannya) belum ada. Harness JS penuh tetap keputusan tersendiri; yang mendarat hari ini pemindai halaman jadinya, bukan penggantinya.
- **[#54](https://github.com/xtheoputra/techverse-x/issues/54) tidak disentuh** — pemicunya tertulis: sesudah ada isi sungguhan.
- **Riset Bulan 4 (arus arXiv) tetap tidak dituangkan ke ADR.** Isinya hanya ada di transkrip sesi lain, jalur produksi pertamanya tetap butuh dua workflow yang mati di bawah #57, dan Bulan 4 bukan prioritas selama Bulan 1 tertahan tiga akun.
- **Komentar kode dan teks workflow yang menyebut Koyeb dibiarkan**, `Dockerfile.vercel` tidak dibuat, dan ulang-sekali `getJson` untuk 502 tidak dipasang — ketiganya menunggu pemicu yang sudah tertulis.

### 🏁 Keadaan akhir sesi — tempat sesi berikutnya mulai

**Rantai PR, belum satu pun di-merge — urutan merge wajib dari bawah:**

| PR | Cabang | Base |
|---|---|---|
| [#56](https://github.com/xtheoputra/techverse-x/pull/56) | `fase-2/pencarian-teks-penuh` | `main` |
| [#58](https://github.com/xtheoputra/techverse-x/pull/58) | `fase-2/knowledge-graph-dasar` | #56 |
| [#62](https://github.com/xtheoputra/techverse-x/pull/62) | `fase-1/adr-025-host-api` | #58 |
| [#63](https://github.com/xtheoputra/techverse-x/pull/63) | `fase-1/muatan-cacat-60-61` | #62 |
| [#64](https://github.com/xtheoputra/techverse-x/pull/64) | `fase-1/tautan-markdown-absolut` | #63 |
| [#65](https://github.com/xtheoputra/techverse-x/pull/65) | `fase-2/siklus-prasyarat-transitif` | #64 |
| [#66](https://github.com/xtheoputra/techverse-x/pull/66) | `fase-2/gerbang-halaman-web` | #65 |

CI ketujuhnya merah dalam hitungan detik dengan **nol langkah** — #57, bukan isinya.

**Menunggu pemilik:** #57 (kuota menit Actions se-akun, diukur ulang hari ini masih mati); uji Railway R1–R6 (ADR-025, [#39](https://github.com/xtheoputra/techverse-x/issues/39)) → proyek Neon ([#38](https://github.com/xtheoputra/techverse-x/issues/38)) → Vercel ([#40](https://github.com/xtheoputra/techverse-x/issues/40)); merge rantai di atas.

**Menunggu pemicu tertulis, bukan macet:** [#42](https://github.com/xtheoputra/techverse-x/issues/42) · [#54](https://github.com/xtheoputra/techverse-x/issues/54) · [#59](https://github.com/xtheoputra/techverse-x/issues/59) — **prasyaratnya tinggal kedua pemicunya**, sebab siklus transitif sudah mendarat di #65 · [#55](https://github.com/xtheoputra/techverse-x/issues/55) tutup saat #58 masuk · [#60](https://github.com/xtheoputra/techverse-x/issues/60) dan [#61](https://github.com/xtheoputra/techverse-x/issues/61) tutup saat #63 sampai `main`.

**Sudah terverifikasi sesi ini** (dulu *"belum terverifikasi"*): resolusi tautan relatif di halaman blob GitHub. Tidak perlu diklik lagi — aturannya kini dijaga skrip, dan skripnya dibuktikan merah lima kali.

**Lingkungan pengembangan:** `techversex-postgres` dan `techversex-redis` tetap menyala seperti saat sesi mulai. API :5080 dan `next start` :3310 yang dinyalakan sesi ini **sudah dimatikan** — port 3000, 3310, 3311, dan 5080 semuanya kosong saat sesi ditutup. Yang tersisa hanya proses `dotnet` node-reuse MSBuild dari build terakhir, dan itu bawaan `dotnet build`.

---

## 2026-09-17 — Sesi 14 (lanjutan): sisa Bulan 3 mendarat, dan halaman JADINYA menemukan yang tidak akan ditemukan membaca kode

Permintaan pemilik: *"lanjutkan tugas yang belum selesai"*.

### Yang diwarisi — dibaca dari transkrip, bukan diingat

Sesi kemarin berakhir **di tengah tahap**. Transkrip dan catatan kerjanya memperlihatkan keadaan persisnya:

| | Keadaan saat sesi ini mulai |
|---|---|
| Langkah 4 ADR-023 (endpoint `/requires`) | kode dan seluruh bukti merahnya selesai, **belum di-commit**; `run.ps1 verify` **terputus di Lint web** |
| Langkah 5 (seed), 6 (web), 8 (dokumen) | belum disentuh |
| Riset pengganti Koyeb (#39) | penyapuan dan peringkat selesai, **pemeriksa-pembantahnya tidak pernah kembali** |

🔑 **Tidak satu pun diterima begitu saja.** `verify` diulang penuh sebelum langkah 4 di-commit (hijau: 0 peringatan, 102 unit, 47 integrasi, lint, build web), dan laporan tahapnya disusun ulang dari keluaran perintah yang tercatat — bukan dari ringkasan.

### Yang dibangun

| Commit | Isi |
|---|---|
| `35ca3a3` | Langkah 4: irisan `RequireTopic`, `POST …/requires` di bawah batas tulis ADR-020 |
| `3ef78ce` | Langkah 5: `database/seeds/seed.mjs` jadi satu-satunya seed; contoh keenam Tool Use dan sisi pertama |
| `5bf0b07` | Tiga ringkasan bidang berhenti menyebut ADR — **temuan garis dasar web**, migrasi `RingkasanBidangTanpaRujukanADR` |
| `ae74e68` | Langkah 6: blok "Topik terhubung", fallback selisih versi, aturan teks pembaca, `not-found.tsx`, `typedRoutes` |
| `84eb82e` | Langkah 8: [ADR-023](adr/ADR-023-knowledge-graph-dasar.md), [ADR-024](adr/ADR-024-explore-learn-navigasi-v1.md), Pembaruan bertanggal di ADR-006/009/012/015/020/021/022, README, RENCANA-V1 |

### 🔴 Garis dasar merah menemukan DATA, bukan kode

Aturan teks pembaca ADR-024 dibuktikan dengan memindai teks yang **terlihat** di build produksi. Garis dasarnya diambil dari kode lama lebih dulu — dan halaman muka memberi **9 kena**, bukan enam yang diramal rancangan:

| Kena | Asal |
|---|---|
| "Bulan 1", "menyusul", "beranda sementara", ADR-009/010/012 | spanduk halaman muka — **diramal** |
| **tiga "lihat ADR-010"** | **ringkasan BIDANG** Cloud & Infrastructure, Renewable Energy, XR — disemai migrasi dari `FieldCatalog` |

Rancangannya memindai kode web, dan dari sana ketiganya **tidak akan pernah terlihat**. Selama produksi nol topik, keempat belas kartu itulah isi utama halaman muka — dan tiga di antaranya menyuruh pengunjung membuka dokumen di repositori privat.

Diperbaiki dengan migrasi, bukan dengan menyaring teks di web. Buktinya: `ModelMigrasiTests` **merah** sesudah `FieldCatalog` diubah dan sebelum scaffold; Up/Down masing-masing tepat tiga `UpdateData`; dev DB 3 → 0 ringkasan menyebut ADR dan `search_vector` ikut dihitung ulang sendiri (`cloud @@ 'lebar'` true → false); Down → 3 lagi; dari nol 6 migrasi, 9 tabel, 14 bidang; `technology.sql` dijalankan dua kali ke basis data kosong, keduanya exit 0; kelima belas kata kunci ADR-022 identik.

💡 **Aturan tentang teks yang TERLIHAT harus diukur di halaman jadinya.** Kode web hanya salah satu sumber teksnya.

### 🐞 Alat ukur saya sendiri berbohong — lagi, dua kali

1. **Versi pertama `seed.mjs` menulis galat ke stderr.** `.\run.ps1 seed` biasa benar: penjelasan ADR-020, exit 1. Tapi **`.\run.ps1 seed 2>&1` di Windows PowerShell 5.1 berhenti dengan `NativeCommandError` berpesan KOSONG, tanpa satu baris penjelasan pun** — 5.1 membungkus stderr program native jadi galat begitu dialihkan, dan `run.ps1` menyetel `ErrorActionPreference Stop`. Blok PowerShell lama tidak pernah kena karena hanya memakai `Write-Host`. Sekarang semua keluaran ke stdout; `2>&1` dan `*>&1` diukur ulang: penjelasan tercetak, `LASTEXITCODE` 1. 💡 *Cacatnya hanya muncul persis saat keluarannya disimpan untuk dibaca belakangan — keadaan yang tidak pernah dicoba kalau cuma menjalankannya di terminal.*
2. **Sabotase W6 di rancangan tidak bisa dikompilasi.** "Buang `?? []`" menggagalkan `next build` (`TS2322 … 'TechnologySummary[] | undefined' is not assignable`) sebelum ada yang bisa diukur saat berjalan. Itu dicatat sebagai **penjaga kedua** (tipe), dan bukti runtime-nya memakai cast: API lama tiruan → **HTTP 500**, `Cannot read properties of undefined (reading 'length')`.

### Seed: satu berkas, dibuktikan dari nol

- **S1** (dev DB berisi lima contoh lama), dua kali: jalan 1 `dibuat Tool Use` + `sisi model-context-protocol butuh tool-use`; jalan 2 hanya `ada`. Nol duplikat, roadmap MCP 0·1·2.
- **Dari nol** (`CREATE DATABASE gladi_seed`, API :5091 tulis hidup): semua `dibuat`, 6 topik, MCP `MachineDrafted`, 1 sisi; jalan kedua semua `ada`. Pembandingnya pengukuran kemarin: `seed.sh` ke basis data kosong → **2 topik dan tiga `ada` palsu**.
- **S2**: API tulis mati (`--no-launch-profile`, dan `docker-compose.prod.yml`) → exit 1 dengan penjelasan ADR-020, lewat `node` maupun `run.ps1`.
- **S3 merah**: contoh sementara ber-slug `ai-agents` → `BENTROK ai-agents … (GET HTTP 404)`, exit 1, berhenti sebelum bagian isi. **Kendali** dengan logika lama (409 = "ada" tanpa GET) → `ada AI Agents`, exit 0, jalan terus.
- **S4**: keluaran Git Bash sama persis dengan `run.ps1 seed`. `make` tidak terpasang di mesin ini — `make seed` **tidak** dijalankan, dan tidak diklaim.
- **Kata kunci ADR-022**: hanya `ai` (topik 3 → 4) dan `agent` (1 → 2) bergeser, keduanya `+tool-use`. SQL membuktikan `tool-use` cocok **hanya lewat bidangnya** — dugaan rancangan, kini terukur.

### Web: diukur di tiga keadaan API

| Keadaan | Hasil |
|---|---|
| **API dev** (6 topik + 1 sisi) | 37 cek halaman lulus; teks terlihat **0 kena** di enam halaman (dari 9); `/cari` `id="q"` 1× dan `id="q-halaman"` 1× (dari 2× `q`); rute build identik; **nol cast** untuk `typedRoutes` |
| **API mati** (fetch-cache dihapus) | halaman topik "Halaman ini belum bisa diambil.", tanpa `localhost`/`5080` di HTML; sebabnya di stdout next; kendali API hidup → blok kembali |
| **API lama** tiruan (tanpa kedua larik) | 200, template utuh, tanpa blok relasi, nol galat |

**Penelusuran dari `/`** — ukuran selesai Bulan 3 yang kini diukur, bukan diklaim: **20 dari 20** di pengembangan (14 bidang + 6 topik), **14 dari 14** di bentuk produksi.

**Bentuk produksi** (`docker-compose.prod.yml`, volume baru, lalu `down -v`): `migrate` menerapkan **6 migrasi dari nol**, exit 0; 14 bidang, 0 topik, **0 sisi**, 0 ringkasan menyebut ADR, kelima constraint relasi ada; `POST …/requires` **404**, `POST /api/v1/technologies` **405**; teks terlihat 0 kena.

Bukti merah web, masing-masing dipulihkan dan diff-nya dicek sama dengan referensi bersih:

| Penjaga | Sabotase | Teramati |
|---|---|---|
| Blok tersembunyi saat kosong | buang return awal | qiskit memuat "Topik terhubung Pelajari lebih dulu Dibutuhkan oleh" berdaftar kosong |
| Penelusuran | buang `<Link>` topik di halaman bidang dan daftar muka | 14 tercapai, **keenam topik hilang**, exit 1 |
| `typedRoutes` | `<Link href="/learn">` | `next build` gagal `TS2322 '"/learn"' is not assignable to type 'UrlObject \| RouteImpl<"/learn">'` |
| Pemberitahuan pemotongan | `pageSize` 1 | "6 topik tercatat menampilkan 1 teratas", "Menampilkan 1 dari 2 topik." |
| Fallback selisih versi | lihat 🐞 butir 2 | HTTP 500 / `TS2322` |

⚠️ **Satu yang diukur dan SENGAJA tidak diperbaiki:** `notFound()` dari `/teknologi/[slug]` tetap **404 + `noindex`**, tapi HTML-nya kerangka galat Next berbadan kosong; halaman 404-nya baru tergambar sesudah JavaScript. **Garis dasar sama persis** (dulu teks bawaan Inggris). Itu harga status 404 sungguhan — Next hanya bisa mengirim status sebelum badan dialirkan — dan dicatat di `not-found.tsx` serta ADR-024.

### 🧹 GitHub: PR bertumpuk, dan satu kelas cacat yang ketahuan saat menulis issue

- **PR [#58](https://github.com/xtheoputra/techverse-x/pull/58)** dibuka dengan base `fase-2/pencarian-teks-penuh`, berlabel dan bermilestone. CI-nya merah dalam **3–4 detik** dengan **`steps: 0`** dan anotasi billing yang sama — #57, bukan kodenya; diperiksa per job, bukan dianggap.
- Komentar: [#55](https://github.com/xtheoputra/techverse-x/issues/55) — keempat pertanyaannya dipetakan ke ADR-023, ditutup saat #58 di-merge, bukan sebelumnya; [#56](https://github.com/xtheoputra/techverse-x/pull/56) — badan #40 wajib disunting **sesudah** #56 masuk, sebab `main` hari ini **masih** mencetak *"Keempat belas bidang di bawah tetap bisa dibuka"* (diperiksa ke `git show main:`), jadi menyuntingnya sekarang adalah klaim yang mendahului kenyataan; [#54](https://github.com/xtheoputra/techverse-x/issues/54) — kalimat batas di `/cari` wajib diubah bersamanya.
- Issue baru: [#59](https://github.com/xtheoputra/techverse-x/issues/59) pemicu Learn; [#60](https://github.com/xtheoputra/techverse-x/issues/60) jenis sumber ber-angka.
- 🔴 **Dan [#61](https://github.com/xtheoputra/techverse-x/issues/61) lahir dari memeriksa klaim #60 sebelum menulisnya.** Kemarin yang diukur hanya `"type":"0"`; issue-nya mau menyebut `"1"`–`"3"` juga. Diukur dulu: `"3"` → `Repository`, `" 1 "` → `Video`, `"5"` → 400 — dan **`"type": 0` sebagai angka JSON → 500**. Menyusul lima permintaan lain: `name` berupa angka, `topicSlug` berupa angka, JSON terpotong, `title` boolean, `name` larik — **keenamnya 500** di lima endpoint tulis berbeda. `BadHttpRequestException` membawa status 400, tapi `UseExceptionHandler()` menangkapnya lebih dulu. Ini kelas cacat #26 (*"500 untuk muatan yang salah"*) yang kembali lewat **bentuk** muatan, bukan nilainya. Produksi tidak terdampak (tidak ada rute berbadan JSON di sana); alur ADR-021 kelak terdampak. Tabel status ADR-023 diberi catatan supaya tidak mengklaim lebih dari yang benar.
- Deskripsi milestone Bulan 3 diperbarui: yang sudah dibangun, produksi nol sisi, dan kenapa ia **sengaja tetap terbuka** (#54, #59). Audit akhir: **issue tanpa label 0 · issue tanpa milestone 0 · PR tanpa label 0 · PR tanpa milestone 0**.

💡 *Klaim di badan issue diperiksa seperti klaim di kode: mengukur satu kasus lagi sebelum menulisnya menemukan cacat yang lebih besar daripada yang sedang ditulis.*

### 🔎 Pengganti Koyeb: pemeriksa-pembantah dijalankan ulang, satu kandidat terbantah, dan ADR-025

Tiga pemeriksa yang kemarin tidak pernah kembali dijalankan ulang, satu per kandidat teratas, masing-masing bertugas **membantah**:

| Kandidat | Hasil | Penentunya |
|---|---|---|
| Back4App Containers | ❌ **terbantah** | kode dasbornya sendiri: URL app gratis *"temporary and will be live for 60 minutes"*, dan `CreditCardValidationRequiredError` |
| Railway Free | ✅ **bersyarat** | gratis dan tanpa kartu benar; verifikasi otomatis menimbang umur akun GitHub (pemilik: 15 Juni 2026), dan jalan keluarnya kartu atau Hobby |
| Vercel Functions dari citra kontainer | ✅ **Beta** | kuotanya dibagi dengan web, dan Vercel sendiri tidak menyarankan Beta untuk produksi |

🔑 **Laporan pemeriksa tidak disalin begitu saja.** Kutipan penentunya diambil ulang dari 13 halaman Railway, Vercel, dan Koyeb yang hidup plus bundel dasbor Back4App, lalu dicocokkan dengan skrip pencari frasa di atas teks yang dinormalkan. Semuanya cocok. Kaki Neon dan Vercel diperiksa ulang dengan cara yang sama — dan satu klaim riset **kemarin** ikut terbantah: setelan monorepo Vercel memang menyala secara bawaan sejak 2020, jadi badan #40 yang benar.

🔴 **Tabel survei ADR-019 ternyata keliru di dua baris sejak hari ditulis, dengan arah berlawanan:** Koyeb (gratisnya hilang Februari 2026) dan Railway (gratisnya kembali Agustus 2025).

Dituangkan sebagai [ADR-025](adr/ADR-025-host-api-pengganti-koyeb.md), berstatus **Diusulkan**. Host API belum dipilih; yang diusulkan uji Railway lebih dulu (R1–R7) lalu Vercel (V1–V5), dengan syarat lulus per langkah, dan berhenti begitu ada layar yang meminta kartu. Koreksi kaki Neon (string *direct*, region terkunci, 100 CU-jam) dan Vercel (log satu jam) masuk ke ADR-019, PENYEBARAN, README, dan RENCANA-V1.

GitHub: PR [#62](https://github.com/xtheoputra/techverse-x/pull/62) ditumpuk di atas #58. Hasil riset diposting ke [#39](https://github.com/xtheoputra/techverse-x/issues/39#issuecomment-5708360833) — janji kemarin ditepati — dan koreksinya ke [#38](https://github.com/xtheoputra/techverse-x/issues/38#issuecomment-5708361264) serta [#40](https://github.com/xtheoputra/techverse-x/issues/40#issuecomment-5708361549). #39 berganti judul dan berlabel `keputusan`; deskripsi milestone Fase 1 diperbarui, termasuk bahwa #57 **tidak** menahan jalur Railway.

💡 *Back4App lolos penyapuan kemarin karena halaman harganya menulis "no credit card required", dan terbantah hari ini karena pemeriksanya membaca kode dasbornya. Membaca halaman pemasaran dengan lebih teliti tidak akan pernah menemukannya.*

### 🐞 #60 dan #61 diperbaiki — dan #61 ternyata punya dua jalan

Permintaan kedua pemilik hari ini: *"lanjutkan semua tugas"*. Yang tidak menunggu akun, merge, atau pemicu tertulis tinggal dua bug di permukaan tulis. PR [#63](https://github.com/xtheoputra/techverse-x/pull/63), ditumpuk di atas #62.

**#60 — jenis sumber diurai berdasarkan nama.** Yang diukur lebih dulu adalah yang **tersimpan**, bukan hanya statusnya: `"0"` → `OfficialDocs`, `"3"` → `Repository`, `" 1 "` dan `" video "` → `Video`, dan **gabungan bendera `"Video, Paper"` → `Repository`** (1 | 2) — bentuk yang belum ada di issue. Kelimanya 200 sebelum perbaikan; kendali `video`/`REPOSITORY`/`OfficialDocs` hijau sebelum dan sesudah.

**#61 — diukur ulang sebelum disentuh, di dua lingkungan:**

| | Muatan cacat ke 8 endpoint berbadan | `?pageSize=abc` (permukaan BACA) |
|---|---|---|
| `Development` | 500 + baris `fail` | 500 |
| `Production` | 400 tanpa sebab | 400 tanpa sebab |

`ThrowOnBadRequest` bawaannya hidup hanya di `Development`, dan seluruh uji integrasi berjalan di sana — **jalan produksi tidak pernah dilihat satu uji pun**. Arah perbaikan di badan issue-nya (`StatusCodeSelector`), yang ditulis pagi ini tanpa dicoba, tidak akan menyatukan kedua jalan itu. Dipasang: sakelar hidup di semua lingkungan + `PermintaanCacatHandler`, dengan `detail` berisi pesan kerangka kerja (`Path: $.type`). Sesudahnya, lewat API sungguhan: kedua lingkungan identik, 415 dan 404 kendali tidak berubah, nol baris `fail`.

| Bukti | Development | Production |
|---|---|---|
| Kode lama (uji baru) | 🔴 24 + 2 kueri | 🔴 24 + 2 kueri |
| Sabotase: tanpa sakelar | hijau | 🔴 400 tanpa sebab |
| Sabotase: tanpa handler | 🔴 500 | 🔴 **500** — sakelar tanpa handler memperburuk produksi |
| Sabotase: tanpa pesan `JsonException` | 🔴 8 | 🔴 8 |

🐞 **Komentar saya sendiri terbantah sebelum sempat di-commit.** `PermintaanCacatHandler` mengembalikan `true` walau badannya tak bisa ditulis, dan komentarnya semula berbunyi *"mengembalikan false justru menyerahkannya kembali ke jalan 500"*. Diukur dengan klien yang hanya menerima `text/html`: statusnya **tetap 400** — bedanya baris `fail` palsu muncul di log. Komentarnya ditulis ulang sesuai yang terukur.

⚠️ **Satu yang dilihat dan tidak dikejar:** di jalan produksi LAMA, Kestrel sekali per jalan ukur mencatat `Connection processing ended abnormally … Reading is already in progress`. Koneksi keep-alive tetap dipakai ulang dengan benar (400 lalu 200 di koneksi yang sama), dan peringatan itu tidak muncul di jalan ukur yang sama sesudah perbaikan. Tidak diklaim sebagai hasil perbaikan.

**Ukuran:** `run.ps1 verify` hijau — Release 0 peringatan, **102 unit + 61 integrasi** (dari 47: +8 #60, +2 sapuan muatan cacat, +4 parameter kueri), lint dan build web. Basis data pengembang: nol topik atau alat uji tersisa, 6 topik contoh dan 1 sisi utuh. Hasilnya diposting ke [#60](https://github.com/xtheoputra/techverse-x/issues/60#issuecomment-5708835301) dan [#61](https://github.com/xtheoputra/techverse-x/issues/61#issuecomment-5708834827); keduanya tertutup saat rantainya sampai ke `main`.

### Ukuran

**102 unit + 47 integrasi** (dari 95 + 32 di ujung #56), build Release 0 peringatan, `run.ps1 verify` hijau. Dev DB ditinggalkan: 6 migrasi, 6 topik contoh, 1 sisi, nol sisa uji; tidak ada basis data `gladi%`, tidak ada proses latar.

### Yang TIDAK dikerjakan, dan kenapa

- **Tidak ada yang di-merge**, dan PR #58 tetap ditumpuk di atas #56. Syarat merge-nya tertulis: #57 selesai, #56 masuk, CI hijau di PR ini — termasuk *"Migrasi bisa dijalankan dari nol"* yang kini menghadapi **tiga** migrasi baru di atas `main`.
- **Harness JS untuk `apps/web`.** Aturan teks pembaca dan tampilan relasi hanya bertahan selama pemindaiannya diulang tangan (ADR-024). Itu keputusan tersendiri.
- **Riset Bulan 4 (arus arXiv)** selesai sebagai riset kemarin, tapi **tidak dituangkan ke ADR**: kritiknya sendiri membantah klaim terkuat rancangannya — jalur produksi pertamanya tetap butuh *Rilis citra* dan *Migrasi produksi*, dua workflow Actions yang mati di bawah #57.
- **Uji Railway (R1–R7) tidak dijalankan, dan tidak satu akun pun dibuat.** Pendaftarannya atas nama pemilik — dan R1 justru menguji akun GitHub pemilik sendiri.
- **Komentar kode dan teks workflow yang menyebut Koyeb dibiarkan**, begitu pula `Dockerfile.vercel` yang tidak dibuat. Yang pertama menunggu host dipilih, yang kedua hanya berguna kalau Railway gagal (ADR-025, *Konsekuensi*).

### 🏁 Keadaan akhir sesi — tempat sesi berikutnya mulai

Pemilik menutup sesi: *"simpan, commit dan push akan saya akhiri sesi hari ini"*. Semua cabang sudah di-push dan sama dengan `origin`; pohon kerja bersih, nol stash.

**Rantai PR, belum satu pun di-merge — urutan merge wajib dari bawah:**

| PR | Cabang | Isi | Base |
|---|---|---|---|
| [#56](https://github.com/xtheoputra/techverse-x/pull/56) | `fase-2/pencarian-teks-penuh` | pencarian teks penuh (ADR-022) | `main` |
| [#58](https://github.com/xtheoputra/techverse-x/pull/58) | `fase-2/knowledge-graph-dasar` | Knowledge Graph dasar + navigasi V1 (ADR-023/024) | #56 |
| [#62](https://github.com/xtheoputra/techverse-x/pull/62) | `fase-1/adr-025-host-api` | pengganti Koyeb (ADR-025, Diusulkan) — dokumen saja | #58 |
| [#63](https://github.com/xtheoputra/techverse-x/pull/63) | `fase-1/muatan-cacat-60-61` | perbaikan #60 dan #61 | #62 |

CI keempatnya merah dalam 2–4 detik dengan **nol langkah** — #57, bukan isinya.

**Menunggu pemilik:** #57 (kuota menit Actions se-akun); uji Railway R1–R6 menurut ADR-025 (#39) → proyek Neon sungguhan di region yang mengikutinya (#38) → Vercel (#40); merge rantai di atas.

**Menunggu pemicu tertulis, bukan macet:** #42 (jalan `tinjau` ADR-021, dibangun sesudah #40) · #54 (isi halaman sungguhan) · #59 (#42 tutup dan satu roadmap terisi di produksi) · #55 tutup saat #58 masuk · #60 dan #61 tutup saat #63 sampai `main`.

**Sengaja ditunda, dengan syaratnya:** mengulang sekali di `getJson` untuk 502 sesudah tidur (hanya kalau R6 melihat 502) · komentar kode Koyeb (sesudah host dipilih) · `Dockerfile.vercel` (hanya kalau Railway gagal).

**Belum terverifikasi:** tautan issue di `docs/*.md` ditulis `../../issues/N`. Menurut resolusi URL biasa, dari `blob/<cabang>/docs/` tautan itu jatuh ke `blob/issues/N`, bukan ke issue. Tautan baru sesi ini memakai `../../../`. Peramban mesin ini belum masuk GitHub, jadi belum diklik — satu klik di GitHub menjawabnya.

**Lingkungan pengembangan:** `techversex-postgres` dan `techversex-redis` tetap menyala seperti saat sesi mulai; basis data berisi 6 topik contoh dan 1 sisi, nol sisa uji. Tidak ada proses API yang ditinggalkan sesi ini. Satu `npx next start -p 3311` yang menyala sejak 12:14 **bukan** dari sesi ini — commit terakhir sesi ini 11:54, dan proses itu tidak menjawab di portnya — jadi sengaja tidak dihentikan.

---

## 2026-09-16 — Sesi 14: CI terbukti bisa ditiru lokal, Koyeb berhenti gratis, dan Knowledge Graph dirancang tiga kali sebelum satu baris ditulis

Permintaan pemilik: *"lanjutkan semua fase dan tugas yang belum selesai sesuai dokumen"*.

### 📏 CI masih mati — dan sebabnya kini terukur, bukan ditebak dari anotasinya

Jalan-ulang run `34582888286` (PR #56): merah dalam **3 detik**, `steps: []`. Sebabnya diambil dari API billing ([komentar #57](https://github.com/xtheoputra/techverse-x/issues/57#issuecomment-5695424090)):

| Repo (privat kecuali disebut) | Menit Actions September |
|---|---|
| `wellsy` | 1.053 |
| `techverse-x` | 558 |
| `zarastrade` | 527 |
| `think-mate` | 4 |
| `jago-bahasa` (**publik**, tidak masuk kuota, tidak terdampak) | 43 |

**Menit repo privat 2.142 > kuota 2.000 akun Free** — pemblokiran se-AKUN, bukan repo ini. 🔑 *"recent account payments have failed"* **menyesatkan**: akunnya tidak punya kartu; yang terjadi kuota gratis habis dengan anggaran $0. Perkiraan pulih tanpa membayar **1 Oktober 2026** — kesimpulan dari bentuk API-nya, **belum terbukti**. `push` CI di cabang `fase-*` makan ±26% menit repo ini, tapi **sengaja tidak diubah**: perubahan pemicu CI tidak bisa dibuktikan selama CI tidak berjalan, dan selisihnya terhadap batas kuota terlalu tipis untuk diklaim sebagai obat.

### 🧪 Premis Sesi 13 keliru: kelima gerbang "yang tidak bisa ditiru lokal" cuma butuh Docker

Keenam job `ci.yml` **dijalankan** terhadap `739bbd8` ([komentar PR #56](https://github.com/xtheoputra/techverse-x/pull/56#issuecomment-5695423679)):

| Job | Hasil |
|---|---|
| Backend — host Windows **dan** peti kemas `dotnet/sdk:10.0` (Linux) | 0 peringatan · **95 unit + 32 integrasi** di kedua OS · migrasi dari nol ke Postgres **tanpa skrip init**: skema `technology` 0 → 9 tabel, 14 bidang, 0 topik |
| Frontend — host Windows **dan** `node:22` | `npm ci` · lint · build |
| Pindai rahasia | gitleaks atas **seluruh riwayat** (54 commit non-merge) · no leaks |
| Citra | build ketiganya · gerbang ADR-020 `health/live=200 POST=405` · Trivy CRITICAL+HIGH **0/0/0** |

🐞 **Gerbang ADR-020 sempat merah duluan, dan salahnya di alat ukur saya.** `health/live=000` padahal peti kemasnya `Now listening`. Skrip tiruan saya mengekspor `MSYS_NO_PATHCONV=1` secara global, dan itu merusak `curl -o /dev/null` di skrip gerbangnya. Dibuktikan dengan skrip yang sama, satu-satunya beda variabel itu: tanpa → 200/405; dengan → 000/000. Kendali `/health/live` **bekerja** — ia menolak meluluskan apa pun saat alat ukurnya buta; hanya diagnosisnya yang salah alamat.

⚖️ **Merge #56 DICOBA dan DITOLAK pengaman izin sesi** (*merge tanpa review*). **Tidak diakali.** Bukti dan pilihannya diserahkan ke pemilik di PR #56, dan pekerjaan Bulan 3 ditumpuk di cabang `fase-2/knowledge-graph-dasar` di atas `fase-2/pencarian-teks-penuh`.

### 🔴 Kembaran `make seed` sudah menyimpang — dibuktikan dengan dijalankan

Dua basis data kosong, dua API tulis hidup. `seed.sh` → `ada AI Agents`, `ada Edge AI`, `ada Quantum Computing`, `dibuat Digital Twin`, `dibuat Cybersecurity Berbasis AI`: **2 topik, 0 bagian**, dan ketiga `ada` itu **bohong** — 409 slug-milik-bidang dibaca "sudah ada". `run.ps1 seed` ke basis data kosong yang lain → 5 topik, 1 draf. README berbunyi *"isi run.ps1 dan Makefile sengaja dijaga sama"*. Diperbaiki di sesi lanjutan.

### 🔴 Koyeb berhenti gratis untuk akun BARU — tujuh bulan sebelum ADR-019 ditulis

Ditemukan saat meriset Bulan 4, lalu diperiksa ke sumber primer: pengumuman Koyeb **17 Februari 2026** — *"new users will only be able to sign up for those plans"* (Pro USD 29/bulan ke atas), sedangkan organisasi lama tidak berubah. Tabel survei ADR-019 dan badan #39 (*"gratis, biasanya tanpa kartu"*) ditulis 8 September — **klaimnya basi sejak hari ditulis**. Diposting ke [#39](https://github.com/xtheoputra/techverse-x/issues/39#issuecomment-5695998488): **jangan mendaftar dan jangan memasukkan kartu dulu**. Riset penggantinya dijalankan hari itu juga, tapi **belum selesai** saat sesi berakhir.

### Knowledge Graph (#55): tiga rancangan, tiga penilai, satu sintesis

Tiga rancangan independen (*minimal dan jujur* · *integritas domain* · *pembaca*) dinilai tiga penilai (*konsistensi ADR* · *bisa dibangun dan diuji* · *jujur, tidak setengah jadi*), lalu disintesis jadi satu spesifikasi yang diperiksa ulang ke kode. Dasarnya rancangan *minimal dan jujur*, dicangkok dari dua lainnya — sebab **tidak satu rancangan pun benar di semua butir**:

- 🔴 **`TechnologyRelationship.Id` bertanda `ValueGeneratedOnAdd` padahal domain mengisinya** — sisi ke topik yang sudah tersimpan jadi `UPDATE` lalu 500 di tulis pertama. **Hanya satu dari tiga rancangan (*integritas domain*) yang menangkapnya**; penilai memverifikasi dua lainnya akan membalas 500.
- 🔴 **Ujung tujuan tanpa kunci asing** sejak `InitialTechnologySchema` — dan salah satu rancangan yang menambahkannya memilih `CASCADE`, melanggar kebiasaan repo untuk tautan ke agregat lain (Restrict).
- Baris header "Pelajari dulu" (dua rancangan) bertabrakan dengan ADR-012 bagian 1; tautan di dalam bagian roadmap (rancangan ketiga) mematahkan kontrak "Belum diisi." dan menyembunyikan sisi di halaman kurasi.

### Yang dibangun hari itu, dan dibuktikan merah

**`6ebc906` — domain.** `RelationshipKind` tinggal `Requires`; buktinya bukan grep: uji lama yang masih menyebut `Uses` gagal `CS0117` di **satu** baris, proyek layanan lolos. `Technology.RequireTopic(topicId, topicRequires)` dengan parameter kedua wajib. U1–U7 masing-masing dibuktikan merah (tukar From/To, buang return awal → 2 sisi, buang cek diri sendiri → ParamName `toTechnologyId`, buang cek dua arah → tidak melempar, sisipkan `MachineDrafted`, tambah `Uses = 1`, `MissingSections` menuntut "Relations"). ⚠️ `ArgumentException(…, "topicSlug")` ditolak **CA2208** — dimatikan untuk satu baris itu, alasannya tertulis.

**`388e054` — persistensi.** Migrasi `RelasiAntarTopik`: FK tujuan `RESTRICT` + indeks, dua CHECK (SQL `Kind` dibangkitkan dari enum), `ValueGeneratedNever`. Up/Down tepat empat operasi, nol churn.
- `ModelMigrasiTests` ditulis **dulu** dan hijau di model lama; **merah** sesudah konfigurasi berubah sebelum scaffold dan saat `Uses = 1`; dan **tetap hijau** saat `ValueGeneratedNever` dibuang — jadi ia bukan penjaga baris itu, dan komentarnya bilang begitu. `dotnet ef database update` terhadap model yang berbeda: exit 1 `PendingModelChangesWarning`, 0 tabel.
- P1–P4 **merah semua lewat migrasi Down sungguhan** ("No exception was thrown"), hijau lagi sesudah `run.ps1 migrate` — sekaligus bukti Down bekerja. P2 juga merah saat FK tujuan ditukar `CASCADE` di basis data.
- `ValueGeneratedNever` dikomentari → P2 `DbUpdateConcurrencyException`.
- Baris `Uses` yang disisipkan SQL ke skema lama **meledak saat dibaca**: `Cannot convert string value 'Uses' … to any value in the mapped 'RelationshipKind' enum` — alasan CHECK dari enum.
- Dari nol (`gladi_s14`): 5 migrasi, 9 tabel, 14 bidang, 0 topik. `technology.sql` ternyata baru memuat 3 migrasi; dibangkitkan ulang ke 5, dijalankan dua kali ke basis data kosong, exit 0.
- 🐞 `ExecuteSqlRawAsync(sql, slugs.ToArray())` mengikat `{0}` ke slug **pertama** (`string[]` kovarian ke `params object[]`); `DisposeAsync` gagal dan meninggalkan lima topik + satu sisi di dev DB — dibersihkan, diganti `ExecuteSqlAsync` berinterpolasi.

**`9b23d4a` — kontrak.** `Requires`/`RequiredBy` bertipe `TechnologySummaryResponse`, pemuat `TopikTerhubung`, dan jalur mutasi pindah dari irisan template ke `TopicMutation`. Parameter `related` tanpa bawaan dibuktikan: membuang argumennya → `CS7036`, menaruhnya sesudah parameter opsional → `CS1737`.

**Langkah 4 — selesai dan terbukti hari itu, di-commit keesokan harinya (`35ca3a3`).**
- `ValueGeneratedNever` dikomentari → 5 dari 10 uji endpoint **500**. Include `Relationships` dibuang, dan terpisah return awal domain dibuang → pengulangan 500 (`23505 ix_technology_relationships_edge`). Handler memberi `[]` → arah berlawanan 200, dan lewat API hidup halaman A berbunyi `requires=[B]` sekaligus `requiredBy=[B]`.
- `MapRequireTopic` di atas batas → uji ADR-020 gagal *".../requires membalas 400"*; kendali alamat salah → 404 dan merah.
- Kontrak lewat `curl`: idempoten, 400 berpesan untuk tiap tujuan keliru, 404 hanya untuk sumber, `DELETE` topik tujuan ditolak FK. `"type":"0"` di `POST …/resources` tersimpan sebagai `OfficialDocs` — celah `Enum.TryParse` yang disebut komentar kode memang nyata.
- ⚠️ **Temuan yang tidak diramal:** cabang tujuan-tak-ada dibuang dengan `topicId!.Value` **di dalam** lambda mutasi menjawab **400 `{"body":["Nullable object must have a value."]}`**, bukan 500 — `TopicMutation` menerjemahkan `InvalidOperationException` apa pun dari lambda. Aturannya kini tertulis di sana: pencarian selesai sebelum `RunAsync`.
- 🐞 **Dan alat ukurnya bohong sekali lagi:** pemulih sabotase memakai `copyFileSync`, yang di Windows mempertahankan mtime cadangan. MSBuild melewati kompilasi, DLL masih memuat sabotase, dan uji tetap merah sesudah "dipulihkan". 💡 *"Hijau lagi" hanya berarti sesuatu kalau yang dijalankan sungguh berubah.*

### 💡 Pola yang berulang, dan kali ini menutup #55

Sapuan *"penegak/entitas ini punya berapa pemanggil di kode produksi?"* — yang menemukan `MarkReviewed()`, `Publish()`, lalu tabel relasi — kini **menutup** temuan ketiganya: tabel relasi punya produsen, pembaca, dan tampilan. Tapi membangun produsennya membongkar bahwa tabel yang dianggap "sudah ada" pun tidak utuh. 💡 *Entitas tanpa pemanggil bukan cuma belum dipakai — ia belum pernah diuji oleh satu penulis sungguhan.*

---

## 2026-09-11 — Sesi 13: papan kerja buntu, jadi Bulan 3 dikerjakan duluan — dan pengukurannya yang memilih tiap keputusan

Permintaan pemilik: *"lanjutkan semua tugas dan fase"*.

Papan kerjanya sama persis dengan yang ditinggalkan Sesi 12: empat issue terbuka, **nol yang bisa dikerjakan tanpa pemilik** — #38/#39/#40 menuntut pendaftaran akun, #42 terkunci di belakang ADR-021 yang sengaja menunggu #40 tutup. Diperiksa lagi ke GitHub, bukan diingat: masih empat, masih sama.

🔑 **Yang membuka jalan bukan menemukan celah di keempat issue itu, melainkan membaca `RENCANA-V1.md` sampai baris berikutnya.** Bulan 1 dan 2 memang terhalang. **Bulan 3 tidak terhalang apa pun** — *"Explore + Learn + pencarian"*, dan pencariannya murni kerja kode. Urutan bulan di rencana itu urutan **prioritas**, bukan kunci.

Ini pengulangan pelajaran Sesi 10 dalam bentuk lain. Di sana pertanyaannya *"bagian mana yang bisa DILATIH sekarang?"*; di sini *"fase mana yang tidak menunggu siapa pun?"*

### Yang diukur sebelum satu baris pun ditulis

Tiap keputusan di [ADR-022](adr/ADR-022-pencarian-teks-penuh.md) lahir dari menjalankan kueri terhadap isi sungguhan — keempat belas bidang `FieldCatalog` plus lima contoh `run.ps1 seed` — bukan dari kebiasaan.

**1. Apa yang sebenarnya rusak dengan `ILIKE`?** Sepuluh kata kunci dicoba. **Empat menjawab nol semata-mata karena urutan katanya dibalik** (`protokol model`, `kuantum sirkuit`, `digital kembaran`, `model bahasa alat`). Itu angka yang membuat pekerjaan ini layak, dan sebelum diukur ia cuma dugaan.

**2. `english` atau `simple`?** Dugaan awal — *kamus Inggris akan merusak teks Indonesia* — **diperiksa dan ternyata salah**:

| Yang diperiksa | Hasil |
|---|---|
| Kata **Indonesia** yang hilang karena stopword Inggris | **nol** (yang hilang cuma `in`, `on`, `the` — memang kata Inggris) |
| Tabrakan stemmer di seluruh korpus | **satu**: `computer` + `computing` → `comput`, dan itu memang diinginkan |
| Yang dibeli | `agent` menemukan "AI Agents", `container` menemukan "containers" — dengan `simple` keduanya nol |

Kata Indonesia memang dipotong (`akses` → `aks`), tapi **simetris** — potongan yang sama berlaku di kuerinya. 💡 *Dugaan yang terdengar masuk akal tentang bahasa ternyata paling murah diperiksa dengan satu kueri.*

**3. `to_tsquery` atau `websearch_to_tsquery`?** Yang pertama — fungsi yang muncul di hampir semua contoh FTS — **melempar**:

```
to_tsquery('english', 'quantum &')  ->  ERROR: no operand in tsquery
```

Di kotak pencarian publik itu **HTTP 500 yang dipicu satu karakter yang diketik pengunjung**. Dijaga dua arah: empat masukan kotor dituntut menjawab 200, **dan** sebuah kendali menuntut `to_tsquery` benar-benar meledak untuk masukan yang sama. Tanpa kendali itu, "200" cuma berarti *hari ini tidak meledak*.

### 🔴 Dua kata kunci yang FTS-nya sendiri TIDAK selesaikan — dan itu yang paling berguna

Dari sepuluh kata kunci percobaan, dua menjawab nol di **ketiga** kolom. Keduanya membongkar batas yang lebih besar daripada pilihan fungsi mana pun:

- **`quantum` → nol.** Padahal situs ini punya bidang bernama **Quantum Computing**; ringkasan Qiskit menulis "kuantum", ejaan Indonesia. **Pencarian tidak menyentuh bidang sama sekali** — dan hari ini ada 14 bidang berbanding nol sampai lima topik, jadi pencarian yang cuma menjawab topik akan membalas "tidak ada hasil" untuk hampir setiap kata yang tercetak di halaman muka. Diperbaiki: `fields` punya `tsvector`-nya sendiri, dan sebuah topik **ikut terjaring oleh bidangnya**.
- **`json-rpc` → nol, dan tetap nol sesudahnya.** Frasa *"Dasar HTTP dan JSON-RPC"* benar-benar tercetak di situs ini, sebagai prasyarat roadmap Model Context Protocol. **Kolom terhitung hanya boleh menyebut kolom di barisnya sendiri**, jadi keempat tabel anak tidak bisa ikut tanpa mengembalikan sifat yang justru dibuang (sesuatu yang bisa lupa diperbarui). Ditagih di [#54](https://github.com/xtheoputra/techverse-x/issues/54), lengkap dengan kenapa keputusannya belum layak diambil: hari ini baru **satu** topik yang kelima bagiannya terisi, dan itu pun isi contoh.

💡💡 **Pola yang berulang: kolom "sesudah" di tabel pengukuran berguna, tapi baris yang tetap NOL di kedua kolom jauh lebih berguna.** Ia menunjuk batas yang tidak akan pernah muncul dari memperbaiki yang sudah hampir benar.

### 🔴 Sapuan "berapa pemanggilnya?" berbuah untuk KETIGA kalinya

Sasaran Bulan 3 yang lain berbunyi *"Knowledge Graph dasar **(relasi sudah ada di skema)**"*. Frasa dalam kurung itu benar tentang skema dan **menyesatkan tentang pekerjaannya**:

| Yang diperiksa | Keadaan |
|---|---|
| Tabel + entitas + enum + uji unit | ✅ ada |
| `TechnologyRelationship.Create()` dipanggil kode produksi | 🛑 **nol** |
| Metode di agregat untuk menambah relasi | 🛑 **tidak ada sama sekali** |
| Endpoint · medan di kontrak · tempat menampilkannya · baris di DB | 🛑 tidak ada · tidak ada · tidak ada · **0** |

Bentuknya: **tabel tanpa produsen, tanpa pembaca, dan tanpa isi.** Ini kembaran persis `MarkReviewed()` dan `Publish()` di Sesi 11 — pola **ketiga**. Ditagih di [#55](https://github.com/xtheoputra/techverse-x/issues/55) berikut empat keputusan yang harus diambil lebih dulu.

💡 Sapuan yang menemukan ketiganya sama dan murah: **"penegak/entitas ini punya berapa pemanggil di kode produksi?"** Layak dijalankan tiap sesi.

### 🔴 Dan kendali "matikan API-nya" menemukan klaim basi yang ditanam PERBAIKAN Sesi 12 sendiri

Halaman `/cari` dijalankan dari **build produksi** dengan API dimatikan — kendali yang sama yang menemukan dua kebocoran di Sesi 12. Halaman barunya bersih (tak ada alamat internal, tak ada `run.ps1`; sebabnya masuk log server lewat `gagal()`, terbukti di stdout). Tapi beranda di sebelahnya tidak:

> **Daftar topik belum bisa dimuat.**
> Coba muat ulang beberapa saat lagi. **Keempat belas bidang di bawah tetap bisa dibuka.**

Kalimat kedua keliru **dua kali**:

1. **Bidangnya ada DI ATAS, bukan di bawah.** `FieldGrid` dirender lebih dulu di `page.tsx` — diukur di posisi HTML jadinya (offset 11.033 vs 11.432), bukan ditebak.
2. **Janjinya tidak bisa ditepati.** Daftar bidang datang dari API yang sama; kalau ia tak terjangkau, kartu bidang di atas **juga** sedang mencetak galat, dan `/teknologi/<slug>` ikut gagal karena ia pun memanggil `listFields`.

🔴 Kalimat itu lahir di **PR #50** — yaitu di perbaikan yang dibuat justru untuk membuang bahan pengembang dari halaman itu. **Pola yang sama persis dengan PR #51: perbaikannya sendiri menanam klaim yang berhenti benar.** Dua sesi berturut-turut, dua kali, di berkas yang sama.

💡 Aturannya sekarang tertulis di tempat kejadiannya: **jangan menjanjikan apa pun yang datang dari API yang sama dengan yang barusan gagal.**

### Yang dibangun

| | |
|---|---|
| Migrasi `PencarianTeksPenuh` | kolom `search_vector` **`GENERATED ALWAYS … STORED`** di `technologies` **dan** `fields`, dua indeks **GIN** |
| Bobot | `A` untuk nama, `B` untuk ringkasan — terukur: `ts_rank` 0,61 vs 0,24 |
| `GET /api/v1/search?q=` | bidang **dan** topik sekaligus; sudah ada di daftar rute publik `KERANGKA.md` 4.9 |
| `/cari?q=…` + kotak cari di **layout** | `<form method="get">` biasa — bekerja sebelum satu baris JavaScript pun dimuat, dan hasilnya URL yang bisa ditautkan |
| Ukuran | **95 uji unit + 32 uji integrasi** (dari 87 + 18), `run.ps1 verify` hijau |

🔑 **`SearchHandler` tidak punya kueri sendiri.** Ia memanggil `ListFieldsHandler` dan `SearchTechnologyHandler` yang sudah dipakai halaman lain. Begitu ada kueri kedua yang menjawab *"apakah baris ini cocok?"*, pertanyaan yang sama punya dua jawaban — dan yang satu akan menyimpang tanpa ada yang merah.

🔑 **Pencocokan sebagian TIDAK dibuang**, dan itu keputusan yang diukur juga: FTS mencocokkan kata utuh, jadi `kubern` tidak menemukan "Kubernetes".

### 🔴 Dan cadangannya sendiri harus diperbaiki — lagi-lagi karena DIJALANKAN

Cadangan itu mula-mula `ILIKE '%kata%'`, dan ia lolos seluruh uji. Yang membongkarnya sekadar mencoba kata kunci pendek terhadap API yang hidup:

```
q=iot   ->  iot, edge-ai, BIOTECHNOLOGY      (b-iot-echnology)
q=a     ->  14 dari 14 bidang                (seluruh situs)
q=ai    ->  8 bidang, termasuk Blockchain    (bloc-k-ch-ai-n)
```

**`iot` bukan kata kunci aneh — ia nama salah satu dari empat belas bidang.** Dan sebabnya bukan "ambangnya kurang" melainkan **bentuk pencocokannya salah**: cadangan itu ada untuk *pengetikan sebagian*, dan orang mengetik **awal** kata, bukan tengahnya. Diganti jangkar awal-kata POSIX (`~* '\m…'`), lalu diukur ulang: `iot` → `iot`, `edge-ai` saja; `ai` → 6 bidang (Blockchain hilang, Robotics **tetap** — ringkasannya memang menyebut "Robotics AI"); `kubern` → `cloud-infrastructure` tetap ketemu.

💡 **Pola yang berulang seharian ini: yang dijaga uji adalah kasus yang saya PILIH, dan saya memilih kata kunci yang panjang dan khas.** Kata kunci pendek — yang justru paling sering diketik orang — tidak pernah masuk daftar. Sekarang ada ujinya.

🔴 **Dan perbaikan itu MEMBATALKAN salah satu klaim saya sendiri, di hari yang sama saya menuliskannya.** ADR-022 sempat berbunyi *"tidak ada satu pun kata kunci yang tadinya ketemu lalu berhenti ketemu — murni menambah"*. Sesudah jangkar awal-kata dipasang itu **tidak lagi benar**: kecocokan di TENGAH kata memang sengaja berhenti ketemu. Yang benar: **hampir** murni menambah, dan satu-satunya yang hilang persis yang diperbaiki.

💡💡 **Jadi pola PR #50/#51 terulang untuk KETIGA kalinya — kali ini di kalimat saya sendiri, dalam satu sesi, berjarak sekitar satu jam dari saat saya menuliskannya sebagai pelajaran.** Aturannya jelas: **sesudah mengubah perilaku, sapu kalimat yang MENJANJIKAN perilaku lama — termasuk kalimat yang baru saja Anda tulis.**

### 🛑 Dan CI-nya sendiri mati — bukan karena kodenya

PR [#56](https://github.com/xtheoputra/techverse-x/pull/56) dibuka, dan **keenam job merah dalam 2–4 detik dengan daftar langkah KOSONG.** Tidak satu pun pernah dijadwalkan. Anotasi GitHub:

> The job was not started because recent account payments have failed or your spending limit needs to be increased.

**Ini pelajaran repo ini persis, untuk kesekian kali: kalau gerbang merah, curigai alat ukurnya dulu.** Yang membuktikannya bukan tebakan melainkan tiga angka — durasi 2–4 detik (termasuk `Frontend (Next.js)` yang biasanya menit-menitan), `steps: []` kosong, dan **`Pindai rahasia` ikut merah** padahal ia tidak menyentuh kode ini sama sekali. Jalan CI terakhir yang hijau: 2026-09-10 09:05 di `main`.

⚠️ **Lokal hijau BUKAN pengganti, dan di sini bedanya nyata.** `run.ps1 verify` menjalankan gerbang yang sama, tapi empat gerbang CI tidak bisa ditiru lokal — build ketiga citra, **gerbang ADR-020 di tingkat CITRA**, Trivy, gitleaks — dan yang paling relevan hari ini: **`Migrasi bisa dijalankan dari nol`** terhadap PostgreSQL yang benar-benar kosong. PR ini **menambah migrasi**, jadi justru gerbang itulah yang paling ingin dijalankan. Karena itu **#56 sengaja TIDAK di-merge**.

🔴 **Akibatnya lebih besar daripada satu PR: `rilis-citra.yml` juga tidak bisa jalan.** Jadi SHA citra tetap beku di `e1871bb1…`, dan **#39 (Koyeb) ikut tertahan** — tidak ada citra baru untuk ditarik. Ditagih di [#57](https://github.com/xtheoputra/techverse-x/issues/57), berlabel `blocker`; deskripsi milestone Fase 1 ikut diperbarui, sebab sampai hari ini ia menulis *"yang tersisa cuma dua hal"*.

### 🧹 Higiene GitHub: audit Sesi 12 sendiri punya kolom yang tak pernah diukur

Butir 5 [[simpan-dan-commit-workflow]] dijalankan sebagai pemeriksaan, bukan asumsi — dan hasilnya persis pola repo ini lagi:

- 🔴 **`pr_tanpa_milestone = 9`.** Sembilan PR ter-merge (#23 · #35 · #36 · #37 · #44 · #45 · #46 · #47 · #49) tidak pernah punya milestone. **Sesi 12 memperbaiki LABEL PR dan menyatakan audit akhirnya nol** — dan memang nol, sebab auditnya cuma menghitung `issue_tanpa_label`, `issue_tanpa_milestone`, dan `pr_tanpa_label`. **Kolom keempat tidak pernah dihitung, jadi ia tidak bisa merah.** 💡 *Audit yang menyatakan "nol" hanya sekuat daftar kolom yang diperiksanya — periksa dulu ia mengukur berapa kolom.* Kesembilannya sudah masuk milestone Fase 1.
- 🔴 **Deskripsi repo basi — tapi BUKAN pada angka ujinya.** Ia berbunyi *"sisa tinggal ketiga akunnya"*, dan itu berhenti benar hari ini karena #57 penahan baru yang bukan salah satu dari ketiganya. Sudah diperbarui (339 dari 350 karakter).
- ⚖️ **Angka uji di deskripsi repo SENGAJA dibiarkan `87 + 18`.** Deskripsi repo menggambarkan `main`, dan #56 belum masuk `main`. Menulis `95 + 32` sekarang adalah klaim yang **mendahului** kenyataan — bentuk basi yang sama, cuma arah waktunya terbalik. Bentuk penggantinya sudah ditulis di komentar #57 supaya tidak perlu disusun ulang saat merge.
- ✅ Deskripsi ketiga milestone diperiksa satu per satu ke keadaan hari ini; Fase 1 diperbarui (menyebut #57), `Bulan 3` baru dibuat sesi ini. Topics ditambah `postgresql`. Audit akhir: **issue tanpa label 0 · issue tanpa milestone 0 · PR tanpa label 0 · PR tanpa milestone 0**.

### Yang TIDAK dikerjakan, dan kenapa

**ADR-021 tetap tidak dibangun.** Pemicunya tertulis jelas — #40 tutup — dan #40 masih terbuka. Membangunnya sekarang berarti menulis mesin peninjauan untuk isi yang belum ada, di situs yang belum tayang. Kalau pemilik ingin urutan itu dibalik, itu keputusan pemilik, bukan kesimpulan asisten.

**Milestone baru: `Bulan 3 — Explore, Learn & Pencarian`.** Namanya mengikuti `RENCANA-V1.md` — rencana yang berlaku — bukan meneruskan penomoran `Fase` dari `KERANGKA.md` 2.9 yang sudah digantikan. ⚠️ Nama **cabang** tetap `fase-2/...` karena `ci.yml` hanya memicu pada `main` dan `fase-*/**`; mengganti pola pemicu CI demi kerapian nama bukan pertukaran yang sepadan.

---

## 2026-09-10 — Sesi 12: janji `docker-compose.prod.yml` dibuktikan dengan dijalankan, dan sapuannya menemukan tiga kebocoran lagi

Permintaan pemilik: *"kerjakan semua tugas dan fase yang tersisa"*.

Papan kerjanya jelas dan sempit. Dari lima issue terbuka, **#38 · #39 · #40 menuntut akun atas nama pemilik** dan **#42 terkunci di belakang ADR-021** yang sengaja belum dibangun sampai #40 tutup. Yang tersisa untuk asisten: **#48** — dan issue itu sendiri melarang jalan pintasnya, dengan kalimat *"langkah pertama issue ini adalah membuktikannya dengan menjalankan, bukan langsung menyunting dokumen."*

### Yang dibuktikan, bukan disimpulkan

`docker compose -f docker-compose.prod.yml up --build` dijalankan dari volume yang baru dibuat. Hasilnya:

| Yang diperiksa | Hasil |
|---|---|
| `migrate` selesai lebih dulu, `api` menunggunya | ✅ exit 0, tiga migrasi terpasang |
| `/health/ready` | ✅ **200**, badannya **hanya** menyebut `postgres` |
| Kedelapan endpoint tulis | ✅ **tidak satu pun dipasang** |
| Ketiga endpoint baca | ✅ hidup — kendalinya: yang tertutup MENULIS, bukan seluruh API |
| `run.ps1 seed` terhadapnya | ✅ berhenti, **exit 1**, menyebut ADR-020 |
| Halaman muka | ⚠️ **14 bidang, NOL topik** |

**Klaim `PENYEBARAN.md` karena itu memang sudah berhenti benar**, persis seperti dugaan #48 — tapi baru sekarang ia punya angka.

🔑 **Kendali yang membuat angka di atas berarti:** citra `api` **yang sama persis** dijalankan sekali lagi dengan `Editorial__WritesEnabled=true`. Ketujuh dari delapan endpoint tulis berpindah dari 404/405 ke **400**. Tanpa kendali ini, "404" tidak membuktikan apa-apa — ia sama saja bunyinya dengan alamat yang salah ketik.

🐞 **Dan endpoint kedelapan membongkar alat ukurnya sendiri.** `POST …/draf` tetap **404** walau sakelarnya hidup, sebab `MarkDrafted` **tidak menerima badan permintaan** — muatan sengaja-tak-sah yang jadi dasar seluruh probe tidak pernah dibaca, jadi yang menjawab adalah slug yang memang tidak ada. Diskriminator yang benar untuk rute ini bukan muatan melainkan **metode**: `GET` ke alamat itu menjawab **404 di produksi** dan **405 saat sakelarnya hidup**. 💡 *Satu alat ukur jarang sah untuk seluruh permukaan — periksa tiap kolom yang jawabannya seragam.*

### Keputusan #48: sakelarnya dibiarkan MATI

Pilihannya dua — nyalakan `Editorial__WritesEnabled` di compose supaya tumpukan itu bisa di-seed lagi, atau biarkan mati dan koreksi dokumennya. **Jawabannya ternyata sudah tertulis di berkas itu sendiri, sejak sebelum ADR-020 ada:**

> *"Kalau berkas ini menyalakan Redis, ia justru menyembunyikan hal yang paling perlu dibuktikan."*

Kalimat itu dibuat untuk Redis dan berlaku sama persis untuk permukaan tulis. Terbitan yang hanya bisa dibaca **adalah** bentuk produksi V1; tumpukan yang berisi topik justru menampilkan **lebih** banyak daripada yang akan dilihat pengunjung. Yang hilang — melihat bentuk halaman topik yang sudah jadi — tidak hilang dari proyek, ia cuma tinggal di `docker-compose.yml` + `run.ps1 api` + `run.ps1 seed`. Dua berkas, dua tugas.

### 🔴 Kalimat yang sama ternyata memuat DUA klaim basi, bukan satu

`PENYEBARAN.md` menjanjikan tiga hal sekaligus: *"migrasi dari basis data kosong, kesiapan tanpa Redis, dan halaman yang benar-benar menampilkan datanya."* #48 menuduh yang ketiga. Yang **pertama** ternyata ikut basi, dan tidak ada yang menuduhnya.

`docker-compose.prod.yml` memasang `postgres-init/01-schemas.sql`, jadi skema `technology` **sudah ada sebelum `migrate` jalan**. Dibuktikan lewat pembeda yang tidak bisa dibantah: **komentar skema**. Skrip init menulisnya, `EnsureSchema` tidak.

```
DB techversex  (compose prod)   -> "Bounded context Technology. Source of truth …"
DB gladi_s12   (CREATE DATABASE) -> (tanpa komentar)
```

Neon tidak punya fasilitas seperti itu — ini pengulangan persis temuan Sesi 10: **cari keadaan yang dev/CI dapat GRATIS dan produksi TIDAK.** Pembuktian yang sah karena itu dijalankan terpisah, dan sekalian menutup butir kedua #48: **`/app/efbundle` di dalam citra `api`, terhadap basis data hasil `CREATE DATABASE`** — artefak yang dipakai *Migrasi produksi*, bukan `dotnet ef` dari sumber seperti di CI dan bukan citra `migrate` seperti di compose. Hasilnya **9 tabel, 14 bidang, 0 topik, exit 0**, sama persis dengan Sesi 10 tapi kini terhadap citra sesudah ADR-020 dan sesudah gerbang citra.

### 🔴🔴 Sapuan yang sama menemukan cacat yang jauh lebih dekat ke pengunjung

Halaman produksi yang dijalankan tadi mencetak ini di bawah judulnya:

> **Belum ada satu topik pun.**
> Isi contoh: `run.ps1 seed`

Halamannya **menyuruh pembacanya menjalankan perintah yang baru saja terbukti berhenti dengan exit 1.** Dan yang membuatnya serius bukan keberadaannya, melainkan **kapan** ia muncul — kedua keadaan yang mencetak bahan pengembang justru keadaan **biasa** dari situs yang sudah tayang:

- **kosong** — hari ini nol topik, dan #42 masih terkunci. Jadi begitu #40 selesai, **inilah tampilan pembukanya.**
- **galat** — instance gratis Koyeb tidur setelah satu jam (ADR-019). Bukan kecelakaan langka; itu cara kerja tier yang sengaja dipilih.

🔑 **Kendali "matikan API-nya" menemukan dua kebocoran lagi yang tidak terlihat dari membaca kode.** Sapuan `grep run.ps1` cuma menemukan satu berkas; menjalankan halamannya dengan API mati memperlihatkan `FieldGrid` — lalu `teknologi/[slug]` — mencetak **alamat API internal** (`Tidak bisa menghubungi API di http://api:8080`) lewat `result.reason`, jalur yang sama sekali tidak mengandung kata `run.ps1`. **Empat tempat, tiga berkas.**

Diperbaiki dengan satu sumbu di `lib/lingkungan.ts`: **bentuk bangunan**, bukan "apakah API-nya hidup". Build produksi yang disajikan di mana pun tidak mencetak bahan pengembang; `next dev` tetap mencetaknya lengkap.

🐞 **Alat ukur saya sendiri sempat mengukur keadaan yang salah, lagi.** Kendali "API mati" diulang lewat `compose up -d --build web` — yang **ikut menyalakan kembali `api`**, sebab api dependensi web. Laporannya berbunyi *"tidak ada kebocoran"* padahal yang terukur jalur SUKSES, bukan jalur galat. Ketahuan hanya karena keluarannya diperiksa, bukan angkanya. 💡 *Sesudah "gerbangnya hijau", tanyakan dulu: keadaan yang mana yang barusan diukur?*

### Yang ikut disapu

Pola terbesar Sesi 11 — *klaim berhenti benar tanpa memberi tahu pembacanya* — dipakai sebagai daftar periksa, bukan cuma dicatat:

- **`README.md`**: "Sembilan belas ADR" → **21**; "basis data kosong" (2026-09-07) → *"volume basis data baru"*, sebab yang itu ternyata bukan basis data kosong; komentar `seed` dipertegas bahwa API-nya harus yang dari baris di atasnya.
- **Issue #40**: ia mengutip frasa **"API belum bisa dihubungi"** sebagai gejala yang harus dicari — dan sesudah perubahan hari ini frasa itu **tidak muncul lagi di build produksi**, yaitu justru di tempat issue itu menyuruh mencarinya. Badannya diperbarui: gejala yang baru, plus tempat sebabnya sekarang tinggal. ⚠️ Kalimat ini semula berbunyi *"#39 & #40, keduanya"* — keliru, dan ketahuan hanya karena badan keempat issue benar-benar di-`grep`, bukan diingat. **Klaim tentang sapuan pun perlu diperiksa seperti klaim lain.**
- **`docker-compose.prod.yml`**: butir "tiga hal yang sengaja berbeda" jadi **empat**, dengan alasan keputusan #48 ditulis di tempat orang berikutnya akan bertanya.

### Ukuran

**87 uji unit + 18 uji integrasi**, `run.ps1 verify` hijau. Nol uji ditambahkan: yang berubah teks yang dicetak ke pembaca, dan repo ini belum punya harness uji JS sama sekali — memasangnya keputusan tersendiri, bukan sisipan. Sesuai kebiasaan repo ini, buktinya **halaman sungguhan dari peti kemas**, dua arah: build produksi bersih dari `run.ps1` maupun alamat internal, `next dev` tetap mencetak keduanya berikut frasa yang dikutip #39/#40.


### Lanjutan: perbaikannya sendiri kena pola yang sama (PR #51)

🔴 **PR #50 berhenti mencetak sebab kegagalan ke pembaca — dan tidak menaruhnya di tempat lain.** Itu menghapus satu-satunya diagnosis yang dipakai #40 untuk membedakan `API_BASE_URL` yang salah dari #39 yang belum selesai. **Persis pola yang #48 keluhkan, dilakukan oleh perbaikannya sendiri**, dalam hari yang sama.

Sebabnya kini dicatat lewat satu tempat keluar `gagal()` di `lib/api.ts`; `server-only` menjamin ia berjalan di server, jadi keluarannya masuk ke log fungsi Vercel atau stdout peti kemas. **404 sengaja tidak dicatat** — itu jawaban yang sah untuk slug yang memang tidak ada, dan mencatatnya menjadikan tiap perayap yang menebak URL sebagai baris log palsu.

⚠️ **Alat ukurnya berbohong dua kali berturut-turut, dan keduanya soal JENDELA, bukan kode.** `docker compose logs --since <penanda>` tetap memulangkan baris lama, jadi kendali "404 tidak dicatat" terbaca **gagal** dua kali. Yang menyelesaikannya bukan menambah percobaan melainkan **membuang filternya**: dump penuh memperlihatkan peti kemas itu hanya pernah punya dua baris, keduanya bertanggal sebelum penanda. 💡 *Kalau kendali gagal, curigai jendela pengukurannya sebelum menyalahkan yang diukur.*

### Lanjutan: dokumen rencana ikut disapu (PR #52)

Pertanyaan *"fase apa yang tersisa?"* dijawab dengan membaca `RENCANA-V1.md` — dan halaman itu sendiri ternyata memuat **tiga klaim basi**, satu di antaranya bisa membuat pembaca mendaftar ke platform yang salah:

1. *"Yang belum ada adalah **keputusan platform hosting**"* + arahan ke usul **Render** — padahal ADR-019 sudah memilih **Vercel + Koyeb + Neon** beberapa jam sesudah ADR-017 diterima.
2. *"terbukti melayani situsnya **dari basis data kosong**"* — kembaran persis kalimat yang baru saja runtuh di #48, hidup di halaman lain. **Memperbaiki satu tempat saja akan meninggalkannya.**
3. `README.md` menyebut **#41** sebagai pekerjaan *sesudah* tayang, padahal #41 sudah mendarat lewat PR #44. Premis #42 ikut dibetulkan di sana.

### Keadaan akhir Sesi 12

| | |
|---|---|
| `main` | **`d5d4cb3`** — push & bersih, **satu-satunya cabang** (lokal & remote) |
| PR | **#50 · #51 · #52** ketiganya merge; **#48 DITUTUP** |
| Uji | **87 unit + 18 integrasi**, `run.ps1 verify` hijau |
| CI | keempat job hijau di ketiga PR, termasuk gerbang citra ADR-020 |
| 📦 SHA citra | **`e1871bb1127868aee8c0baf3880affbe1a04badf`** |

📦 **Perhatikan SHA citra ≠ `main` lagi, dan itu ADR-018 membuktikan diri untuk kedua kalinya:** commit terakhir (PR #52) murni dokumen, jadi `Rilis citra` memang tidak berjalan untuknya. **`git rev-parse HEAD` tetap bukan jawaban "tag apa yang dipasang".**

### Yang tersisa, dan kenapa bukan asisten yang mengerjakannya

**Empat issue terbuka, nol di antaranya bisa diselesaikan tanpa pemilik.**

- **#38 → #39 → #40** menuntut pendaftaran akun (Neon · Koyeb · Vercel). Gratis, tanpa kartu, tapi tetap atas nama pemilik. Segala yang bisa **dilatih** tanpa akun sudah dilatih, dan latihannya memang berbuah — `channel_binding`, SHA pendek, bentuk *Root Directory* `apps/web`, dan hari ini bentuk produksi tanpa permukaan tulis.
- **#42** menunggu ADR-021 dibangun, dan **ADR-021 sengaja menunggu #40 tutup.** Membangunnya sekarang melanggar aturan yang dipilih sendiri: *Bulan 1 mengejar URL, bukan fitur.*

⚠️ **Dan pertanyaan "bagian mana yang bisa dilatih sekarang?" tetap ditanyakan, bukan dilewati** — itu yang membuka Sesi 10 dan Sesi 11 saat papannya juga tampak buntu. Jawaban hari ini: yang tersisa hanya **transport TLS** (`sslmode=require` dan `channel_binding=require` menuntut TLS sungguhan, dan Postgres lokal tidak punya). Sengaja **tidak** dilatih: sertifikat swa-tanda-tangan menuntut `Trust Server Certificate=true`, yang **bukan** keadaan Neon — hasilnya akan berbicara tentang keadaan lain, dan godaan menambal string koneksi produksi supaya "hijau" justru penurunan keamanan. Ini ditulis di sini supaya tidak dicoba diam-diam nanti.

---

## 2026-09-09 — Sesi 11: Isi halaman mendarat, lalu satu pertanyaan membuka lubang yang jauh lebih besar

Permintaan pemilik: *"lanjutkan"*.

Sesi ini membuka dengan dua cabang selesai tapi menggantung: **#43 terbuka dan hijau**, dan `fase-1/isi-halaman` sudah dikerjakan penuh tapi **tidak pernah dibuatkan PR**. Keduanya digabungkan, diperiksa ulang dengan dijalankan, lalu didaratkan. Yang mendarat: endpoint kelima bagian isi + `/teknologi/<slug>` (#41), perbaikan seed, dan **ADR-020**.

### Pertanyaan yang membuka lubangnya

Sapuan rutin *"penegak ini punya berapa pemanggil?"* menemukan **`MarkReviewed()` dan `Publish()` nol pemanggil di kode produksi** — bukan endpoint, bukan skrip, bukan UI; hanya uji unit. Konsekuensinya bukan soal kerapian:

🔴 **Target Bulan 2 (#42) hari ini tidak bisa dicapai SIAPA PUN, bukan hanya oleh asisten.** Catatan Sesi 6 dan Sesi 9 benar bahwa `tinjau` menuntut manusia — tapi keduanya berhenti di situ, dan yang tak pernah ditanyakan adalah *manusianya menekan apa?* Angka "N topik sudah diperiksa manusia" di halaman muka adalah pencacah **tanpa produsen**: ia terpaku di nol karena tidak ada kode yang bisa menaikkannya.

Menarik benang itu ke pertanyaan berikutnya — *kalau pemilik mau menandai halaman, lewat jalan mana?* — sampai pada `apps/api` yang **memasang seluruh permukaan tulis tanpa syarat: tanpa autentikasi, tanpa rate limit, tanpa CORS.**

🔴🔴 **Keputusan yang sudah diambil ternyata menjawab pertanyaan yang lain.** ADR-013 memutuskan "V1 tanpa autentikasi", dan ketiga alasannya menimbang identitas **PEMBACA** — Progress Tracker, Badge, AI Mentor. Permukaan tulis **REDAKSI** tidak disebut satu baris pun, sementara kalimat "V1 tanpa autentikasi" terlanjur terbaca seolah sudah menutup soalnya. Begitu #39 dan #40 selesai — dua langkah berikutnya, bukan sesuatu yang jauh — `https://….koyeb.app` menerima delapan endpoint tulis dari siapa pun yang menemukan alamatnya.

**Keputusan pemilik: tutup.** Bentuknya di [ADR-020](adr/ADR-020-permukaan-tulis-api.md) — endpoint tulis **tidak dipasang** kecuali diminta, bawaannya mati, dan parameternya tanpa nilai bawaan supaya host yang lupa menyatakan pilihannya gagal *dikompilasi*. Yang **tidak** diputuskan, dan sengaja dibiarkan terbuka: bagaimana isi sungguhan nanti masuk ke produksi.

### Yang ditemukan dari MENJALANKAN, lagi

- **`run.ps1 seed` menggandakan halaman yang dibuatnya untuk dilihat.** Dijalankan dua kali ia meninggalkan langkah roadmap, proyek, dan sumber yang kembar — di halaman contoh yang justru ada supaya bentuk halaman yang sudah jadi bisa dilihat orang. Lolos karena **`MarkDrafted()` memeriksa bagian yang KOSONG, bukan yang KEMBAR**, jadi seed tetap mencetak *"kelima bagian terisi"* dan `missingSections` tetap kosong di atas halaman yang sudah rusak.
- **Sabotase gerbang baru menemukan cacat di uji yang menjaganya.** Saat gerbangnya sengaja dijebolkan untuk dibuktikan merah, uji yang seharusnya tidak membuat apa pun justru membuat topik, lalu gagal sebelum sempat mendaftarkannya untuk dibersihkan — satu baris tertinggal. 🔑 **Uji yang bersih-bersih hanya di jalur suksesnya sama saja dengan tidak bersih-bersih**, dan justru di hari gerbangnya rusak basis data pengembang paling mudah ikut kotor.
- **Dua emoji di komentar `run.ps1` nyaris mengulang jebakan em dash.** Keduanya memuat byte `0x94` — yang dibaca PowerShell 5.1 sebagai tanda kutip penutup, sebab yang sama persis dengan em dash yang pernah merusak skrip ini. Ketahuan dari sapuan non-ASCII sebelum di-commit, bukan dari skrip yang gagal parse.

### Lanjutan: gerbang ADR-020 turun ke tingkat CITRA (PR #45)

Ketiga uji ADR-020 membuktikannya di tingkat **kode** — tapi semuanya merakit host sendiri lewat `WebApplicationFactory`, jadi buta terhadap kelas regresi yang paling mahal: **artefak yang membukanya kembali** (sebuah `ENV` di Dockerfile, default citra dasar, atau `launchSettings.json` yang suatu hari ikut terbawa publish). Ketiganya lolos seluruh uji .NET dan baru terlihat di produksi.

`.github/scripts/periksa-permukaan-tulis.sh` kini menjalankan **citra yang sudah jadi**, dipasang di `ci.yml` (PR) dan `rilis-citra.yml` (**sebelum push**) — dua tempat yang sama dengan Container Scan, satu berkas dipakai berdua supaya keduanya tidak bisa berbeda diam-diam. Terbukti berjalan di runner GitHub: `health/live=200  POST /api/v1/technologies=405`.

- ✅ **Klaim runbook kemarin akhirnya DIBUKTIKAN.** "Langkah 7" ditulis sebagai contoh peti kemas tapi hanya pernah dijalankan lewat `dotnet run --no-launch-profile`. Kini dijalankan di peti kemas sungguhan — dan sekalian ketahuan **`launchSettings.json` tidak ikut ke citra sama sekali**, jadi sakelar dev itu bukan cuma "tidak dibaca produksi", ia memang tidak ada di sana.
- 🔑 **Alat ukur yang jawabannya bergantung pada dependensi yang sengaja dimatikan bukan alat ukur.** Muatan probe **sengaja tidak sah**: validator menjawabnya sebelum satu baris pun dibaca, jadi **405 = rute tidak ada**, **400 = rute ada**, keduanya tanpa Postgres. Terukur di citra yang dijebolkan: tak sah **400 dalam 0,0 dtk**, sah **500 dalam 2,3 dtk** — dan pada batas waktu lebih pendek ia menggantung sampai `curl` menyerah, sempat mencetak `000000` (curl mencetak `000` sendiri lewat `%{http_code}`, lalu `|| echo 000` menambahkan yang kedua).
- 🔴 **Komentar saya sendiri melebih-lebihkan, dan kasus yang terasa paling sepele yang membongkarnya.** Versi pertama skrip menulis bahwa tanpa kendali `/health/live`, *"405 juga akan muncul dari peti kemas yang mati"*. **Keliru** — peti kemas mati menjawab `000`, jadi gerbangnya tetap merah tanpa kendali apa pun. Yang benar-benar dibeli kendali itu dua: **diagnosis yang benar** (tanpa itu, peti kemas gagal-menyala dituduh *"rutenya masih dipasang"* — salah alamat) dan **perlindungan dari proses LAIN yang menjawab di port itu**. Komentarnya ditulis ulang apa adanya. 💡 **Jalankan juga kasus ketiga yang terasa sepele: itu yang menguji ALASAN Anda, bukan cuma kodenya.**

### Lanjutan 2: `Migrasi produksi` dijalankan untuk PERTAMA KALINYA (PR #46)

Pemilik menyerahkan pengambilan keputusan: *"saya berikan anda kewenangan untuk mengambil alih decision making sesuai aturan"*.

Workflow ini ditulis di Sesi 9 dan **tidak pernah sekali pun dijalankan**. Aman dijalankan tanpa akun mana pun: repo ini **nol secret** (diperiksa lebih dulu), jadi ia berhenti di penjaganya sendiri sebelum menyentuh basis data apa pun.

- 🔴 **Jalan pertama GAGAL di `Tarik citra api` dengan `manifest unknown`** — sebabnya bentuk tulisan, bukan izin. `rilis-citra.yml` memberi tag `${GITHUB_SHA}` (**40 karakter**), sementara manusia menulis SHA tujuh karakter, dan badan #38/#39 mencontohkannya begitu. **Yang menabraknya adalah orang yang sedang memegang runbook itu sendiri.**
- ⚠️ **Galatnya berbohong tentang sebabnya.** *"manifest unknown"* terbaca seperti *"citranya belum terbit"*, padahal citranya ada dan sehat. 🔑 **Bedakan dua kegagalan yang mirip tapi obatnya berlawanan:** `manifest unknown` = kredensial DITERIMA, **tag**nya yang tidak ada · `denied`/`unauthorized` = soal **izin**, tagnya belum sempat dicari. Repo ini pernah salah membacanya ke arah sebaliknya (login sukses dikira membuktikan hak menarik). Pembedaan itu kini ditulis di dalam langkah `Tarik citra api`.
- ✅ **Yang sekalian TERBUKTI, dan ini lebih besar dari cacatnya: menarik citra privat dari Actions BEKERJA.** Selama ini asumsi — yang pernah dibuktikan justru kebalikannya (tarikan LOKAL 403, token `gh` tak bercakupan `read:packages`). Dengan SHA penuh, `Tarik citra api` **hijau**, lalu berhenti tepat di `Secret NEON_DATABASE_URL belum diisi`. Langkah tarik sengaja dipisah dari langkah menjalankan supaya beda ini kelihatan — hari ini pemisahan itu terbayar.
- ✅ **Perbaikannya dibuktikan sebelum di-merge**, dijalankan dari cabangnya: `Masukan 'b23650e' -> b23650e197954088c2fe46c3fbdce2c4d31e81cd` → `Status: Downloaded` → berhenti di penjaga secret.
- ⚖️ **Keputusan: dikanonikalkan di workflow, BUKAN dengan menambah tag pendek di `rilis-citra.yml`.** Dua nama untuk satu citra mengundang kesalahan yang lebih mahal — memigrasi tag yang satu lalu memasang tag yang lain di Koyeb. ⚠️ Kelonggaran ini **hanya** berlaku di workflow; **Koyeb tidak mengkanonikalkan apa pun**, jadi di sana ref-nya wajib lengkap.
- 📌 **Dua jalan MERAH di riwayat Actions itu disengaja**, bukan insiden: yang pertama menemukan cacatnya, yang kedua membuktikan tarikannya bekerja.

### Lanjutan 3: bentuk Vercel dilatih, dan #42 berhenti buntu (ADR-021)

- ✅ **Bentuk Vercel yang sesungguhnya (*Root Directory* `apps/web`) akhirnya dilatih**, kedua setelan lockfile-nya, di pohon `git archive` yang bersih. Keduanya hijau; yang ON memakai lockfile akar (**terpin**), yang OFF memasang **tanpa lockfile**. Hasil bangunannya **dijalankan**, bukan cuma dikompilasi — keempat rute benar terhadap peti kemas API yang permukaan tulisnya tertutup. Rekomendasi (sudah masuk #40): biarkan *Include files outside root* **ON**.
- 🔴 **Percobaan pertama saya mengukur kasus yang SEBALIKNYA dari yang saya kira.** Saya mengekspor seluruh repo lalu `npm install` di dalam `apps/web`, dan nyaris mencatatnya sebagai bukti *"apps/web berdiri sendiri"*. Ia bukan: npm menaiki pohon, menemukan workspace di akar, dan memasang di sana — **`apps/web/node_modules` bahkan tidak pernah dibuat**. Kasus terisolasi baru terukur sesudah `git archive HEAD apps/web` membuang akarnya. 💡 *Periksa dulu alat ukurnya sedang mengukur keadaan yang mana.*
- ⚖️ **[ADR-021](adr/ADR-021-jalan-menuju-tinjau.md) — pertanyaan yang ADR-020 biarkan terbuka, ditutup.** Jalan menuju `tinjau` = **workflow manual bergerbang** meniru *Migrasi produksi* (polanya kini terbukti), peti kemas `api` menyala **di dalam runner** dengan sakelar tulis hidup lalu dipanggil lewat `localhost` — **ADR-020 tidak dilonggarkan sedikit pun**. 🔑 **Nama pemeriksa diambil dari `github.actor`, bukan diketik**: nama yang sudah diautentikasi GitHub jauh lebih kuat daripada teks bebas, jadi ini **memperkuat** ADR-012 alih-alih melonggarkannya. Endpointnya nanti hidup di dalam grup tulis yang sudah ada, jadi ketiga uji ADR-020 langsung menjaganya tanpa satu pun uji baru.
- 🔴 **Sengaja TIDAK dibangun sekarang, dan itu bagian dari keputusannya.** `RENCANA-V1.md` menulis Bulan 1 mengejar URL bukan fitur, dan #41 memesan urutan itu eksplisit. Membangunnya sekarang = mesin peninjauan untuk isi yang belum ada, di situs yang belum tayang, terhadap basis data yang belum dibuat. **Pemicunya: #40 tutup.** Yang berlaku sejak hari ini cuma satu — soalnya tidak terbuka lagi, jadi tak perlu diperdebatkan ulang tiap sesi.

### Penutup: keadaan saat sesi diakhiri

`main` **`c1afd33`**, push & bersih, **satu-satunya cabang**. Lima PR mendarat hari ini (#43–#47), #41 tertutup. **87 uji unit + 18 integrasi**, `run.ps1 verify` hijau, CI hijau, `Rilis citra` hijau. 📦 **SHA citra `a213ce3`** — dan `main` justru `c1afd33`, sebab commit terakhir murni dokumen: ADR-018 membuktikan dirinya sendiri secara langsung.

**Jalur kritis kini sepenuhnya di tangan pemilik:** #38 Neon → #39 Koyeb → #40 Vercel. Itu **adalah** sasaran Bulan 1. Segala yang bisa dibuktikan tanpa akun sudah dibuktikan — tarikan citra privat dari Actions, workflow migrasi sampai penjaga secret, bentuk build Vercel, dan permukaan tulis tertutup di citra yang tayang.

**Sesudah #40 tutup, urutannya sudah tetap:** bangun jalan [ADR-021](adr/ADR-021-jalan-menuju-tinjau.md) → isi tujuh topik AI Agents sampai `draf` → pemilik menaikkannya ke `tinjau` (#42).

🔴 **Satu temuan ditinggalkan sebagai [#48](https://github.com/xtheoputra/techverse-x/issues/48), dan ia drift yang disebabkan perubahan sesi ini sendiri.** ADR-020 menutup permukaan tulis; `docker-compose.prod.yml` karenanya tidak punya lagi cara memasukkan data — sementara `PENYEBARAN.md` masih menjanjikan tumpukan itu membuktikan *"halaman yang benar-benar menampilkan datanya"*. ⚠️ Ditulis apa adanya di issue-nya: **disimpulkan dari membaca compose-nya, BELUM dijalankan** — langkah pertamanya membuktikan dulu, bukan langsung menyunting dokumen. Sekalian di sana: gladi bersih tujuh langkah yang lengkap terakhir dijalankan **sebelum** #41, ADR-020, dan gerbang citra, jadi `/app/efbundle` dari basis data kosong belum diuji ulang terhadap citra yang tayang hari ini.

💡 **Pola yang berulang tiga kali hari ini, dan layak dicari lagi besok: klaim berhenti benar tanpa ada yang memberi tahu pembacanya.** Komentar gerbang citra yang melebih-lebihkan kendalinya · badan #38/#39 yang mengunci SHA mati · dan sekarang `PENYEBARAN.md` soal compose. Ketiganya ditulis oleh orang yang sama yang mengubah keadaannya.

### Cara kerja yang layak diulang

🔑 **Sapuan "berapa pemanggilnya?" berbuah justru karena dijalankan atas penegak yang sudah DIPERCAYA.** `MarkReviewed()` adalah bagian yang paling sering dikutip sebagai bukti ADR-012 ditegakkan — dan itulah kenapa nol pemanggilnya tidak pernah terlihat selama tiga sesi. Pertanyaannya: *aturan ini ditegakkan oleh tipe — tapi tipe itu dipanggil dari mana?*

---

## 2026-09-09 — Sesi 10: Gladi bersih penyebaran, dan dua klaim yang runtuh karenanya

Permintaan pemilik: *"lanjutkan pekerjaan"*.

Papan kerjanya menunjukkan buntu: lima issue terbuka, **empat di antaranya menunggu akun pemilik** (#38 · #39 · #40 · #42), dan satu-satunya yang bisa dikerjakan asisten (#41) secara eksplisit dipesan **sesudah situs tayang**. Sesi 9 sendiri menyimpulkan *"tidak ada lagi yang bisa dikerjakan asisten sendiri untuk menayangkan situs."*

**Kesimpulan itu keliru,** dan sesi ini menunjukkan kenapa: yang belum dikerjakan bukan *langkahnya*, melainkan **latihannya**. Keenam langkah `PENYEBARAN.md` dilatih lengkap di mesin sendiri dengan tiruan lokal — dan langkah 3 gagal.

### 1. Gladi bersih: yang membuatnya berarti adalah basis data yang dibuat SATU CARA saja

Kuncinya satu detail: **`docker-compose.yml` memasang skrip init yang sudah membuat skema `technology`, dan Neon tidak punya fasilitas seperti itu.** Migrasi yang hijau di atas basis data dev karena itu tidak membuktikan apa pun tentang Neon. `CREATE DATABASE` di peti kemas yang sama memberi basis data yang benar-benar kosong — tiruan Neon yang sah — sebab skrip init hanya berjalan sekali untuk basis data bawaan.

Di atas tiruan itu, urutan `migrate → api → web` dijalankan dari **citra `api` yang dibangun dari Dockerfile yang sama**, lalu web **dibangun dari sumber** seperti Vercel, bukan dari citra `web`.

| Langkah | Hasil |
|---|---|
| `/app/efbundle` atas basis data kosong | **9 tabel + 14 bidang** — `EnsureSchema` memang membuat skemanya sendiri |
| `/health/ready` citra `api` | **200**, badannya **hanya** menyebut `postgres` |
| `next build` dari sumber + `API_BASE_URL` | halaman menyajikan **14 bidang** |

### 2. 🔴 Cacat yang menghentikan langkah 3: satu garis bawah

Dasbor Neon menyerahkan `?sslmode=require&channel_binding=require`, dan langkah 1 **melarang merakit ulang string itu dengan tangan**. Mematuhi larangan itu menggagalkan migrasinya: **Npgsql mencocokkan nama parameter dengan mengabaikan huruf besar-kecil dan spasi, tapi TIDAK garis bawah.** `sslmode` lolos karena tak punya pemisah sama sekali; `channel_binding` ditolak — padahal Npgsql punya properti `Channel Binding` untuk persis parameter itu.

🔑 **Kenapa uji yang ada tidak melihatnya:** `Parameter_kueri_seperti_sslmode_tidak_dibuang` memilih contoh yang, secara kebetulan, satu-satunya bentuk yang **tidak** bisa gagal. Ia menguji *ada tidaknya* pass-through parameter, bukan *ejaan* parameternya.

⚠️ Gejalanya menyesatkan: EF membungkusnya jadi *"Continuing without the application service provider"* diikuti galat DI yang terdengar seperti masalah lain. Kalimat yang menyebut sebabnya terjepit di tengah.

Diperbaiki di `PostgresConnectionString` (garis bawah → spasi). Yang benar-benar asing **tetap ditolak**, tapi pesannya kini menyebut nama parameternya. **Empat uji baru; dua di antaranya dibuktikan MERAH** dengan mematikan terjemahannya, dan uji `Npgsql_MEMANG_menolak_parameter_bergaris_bawah` menjaga alasan keberadaannya — kalau Npgsql suatu hari ikut mengabaikan garis bawah, uji itu yang pertama memerah.

**Kendalinya:** dengan string Neon apa adanya (`require`/`require`), galatnya **berpindah** dari *"Couldn't set channel_binding"* ke *"SSL connection requested. No SSL enabled connection from this host is configured"* — dari "stringmu tidak dimengerti" menjadi "server INI tidak punya TLS", dan Neon punya.

### 3. 🔴 Cacat kedua, dan ia hanya terlihat dari MENJALANKAN halamannya

Kartu bidang menampilkan `FieldPriority` dengan kata-kata kematangan isi. Halaman sungguhan berbunyi:

> **AI Agents — Ditulis manusia** · … · **0 topik · 0 diperiksa manusia**

Dua kalimat yang saling membantah di satu kartu, dan **enam kartu Core memasang klaim "ditulis manusia" sementara nol topik pernah diperiksa siapa pun** — persis klaim yang dilarang ADR-012, di halaman yang paragrafnya sendiri mengutip larangan itu.

Sebabnya bukan salah sumbu: `FieldPriority` memang menyatakan kematangan **tertinggi yang DIJANJIKAN**. Yang salah **tensesnya** — janji ditulis seolah pencapaian. Label kini berbunyi *"Target: ditulis manusia"*.

🔑 **Nol uji membaca teks kartu itu**, dan tidak ada gerbang yang bisa melihatnya. Ia muncul hanya karena halamannya benar-benar dibuka — pengulangan pelajaran yang sudah tercatat di repo ini: *bukti terbaik bukan uji, melainkan halaman sungguhan.*

### Keadaan akhir sesi

`run.ps1 verify` hijau: **87 uji unit** (83 → 87) + **6 uji integrasi**. Kelima issue tetap terbuka — **tidak satu pun ditutup sesi ini**, sebab keempatnya memang menunggu akun pemilik dan #41 masih menunggu situsnya tayang. Yang berubah: ketika pemilik akhirnya membuat ketiga akun itu, langkah 3 tidak lagi gagal.

---

## 2026-09-08 — Sesi 9: Dari dua PR menunggu sampai nol issue terbuka

Sesi terpanjang sejauh ini. Permintaan pemilik berturut-turut: *"lanjutkan semua tugas kemarin"* → *"lanjutkan secara berurutan tugas fasenya"* → *"#33 gunakan apapun… #32 cari semua rilis citra di internet… #17 kerjakan"* → *"jangan render, akun gratis pilihannya pada apa"*.

Keadaan akhir: `main` = `8be066d`, **nol PR dan nol issue terbuka**, 83 uji unit + 6 uji integrasi, CI dan Rilis citra hijau.

### 🔜 Yang menunggu besok

**Ketiganya menuntut akun atas nama pemilik. Tidak ada lagi yang bisa dikerjakan asisten sendiri untuk menayangkan situs.**

> ⛔ **Sudah terjawab — kalimat kedua itu keliru (Sesi 10).** Akunnya memang tetap
> menuntut pemilik, tapi *melatih* keenam langkahnya tidak. Gladi bersih dengan
> tiruan lokal menemukan cacat yang menghentikan langkah 3 (`channel_binding`).
> Kalimat aslinya dibiarkan utuh sebagai catatan sejarah.

| Menunggu | Butuh apa |
|---|---|
| [#38](https://github.com/xtheoputra/techverse-x/issues/38) | Buat akun **Neon**, salin URI-nya ke secret `NEON_DATABASE_URL` |
| [#39](https://github.com/xtheoputra/techverse-x/issues/39) | Buat akun **Koyeb**, service dari `…/api:8be066d`, token GHCR `read:packages` |
| [#40](https://github.com/xtheoputra/techverse-x/issues/40) | Buat akun **Vercel**, impor repo, root `apps/web`, isi `API_BASE_URL` |

Sesudah tayang, pekerjaan berikutnya sudah tercatat sebagai issue juga: endpoint isi halaman + `/teknologi/<slug>` ([#41](https://github.com/xtheoputra/techverse-x/issues/41)), dan isi AI Agents menuju Bulan 2 ([#42](https://github.com/xtheoputra/techverse-x/issues/42)).

⚠️ **Ukuran Bulan 2 tidak bisa dicapai asisten sendiri.** Targetnya "7 topik berstatus `tinjau`", dan satu-satunya jalan ke `tinjau` adalah `MarkReviewed(reviewer)` yang menuntut nama pemeriksanya — ADR-012 sengaja menjadikannya pekerjaan manusia.

---

### 1. Dua PR bertumpuk mendarat, dan citra pertama benar-benar terbit

`#29` (Redis opsional) → `ee6f836`, lalu `#31` (citra produksi) → `4bfbfff`. Merge commit, bukan squash.

Diperiksa **sebelum** merge: kedua cabang digabung ke salinan `main` lokal dan `run.ps1 verify` dijalankan di atas hasilnya. Sesudah merge, log CI dibaca per-baris untuk memastikan keempat uji integrasi **benar-benar berjalan** — `Total tests: 4 / Passed: 4` — bukan sekadar "Test Run Successful" atas nol uji.

📦 **`rilis-citra.yml` yang belum pernah berjalan itu akhirnya menyala, dan hijau di percobaan pertama:** tiga citra dibangun → dipindai Trivy (nol CRITICAL/HIGH) → didorong ke GHCR dengan tag SHA dan `:main`. Satu kekhawatiran terjawab sendiri: setelan repo `default_workflow_permissions = read` ternyata **tidak** menghalangi — blok `permissions:` tingkat workflow memang menaikkan izin.

🔴 **Yang ditemukan justru sesudah citranya terbit: `docker login` yang berhasil tidak membuktikan apa pun tentang menarik.** Login ke `ghcr.io` sukses; `docker pull` membalas **403** karena tokennya tidak bercakupan `read:packages`. Menarik citra adalah langkah pertama platform hosting mana pun, dan `PENYEBARAN.md` — runbook yang ditulis sebelum satu citra pun terbit — tidak menyebutnya sama sekali.

⚠️ **Dua jebakan GitHub yang memakan waktu:**

- **`gh pr merge --delete-branch` MENUTUP PR yang bertumpuk di atasnya**, bukan mengarahkannya ulang. Lalu kebuntuan: tidak bisa dibuka lagi (cabang base hilang) dan tidak bisa dipindah base-nya (PR tertutup). Jalan keluarnya **mendorong balik cabang yang baru dihapus**, buka lagi, pindahkan base, baru hapus.
- **`Closes #30` di JUDUL PR tidak menutup apa pun.** Dua sebab menumpuk, dan hanya satu yang terbukti di sini: selama base PR bukan cabang default, `closingIssuesReferences` **tetap kosong walaupun badan PR sudah memuat kata kuncinya**. Ini varian ketiga dari jebakan yang sama dengan Sesi 7 — **selalu periksa `closingIssuesReferences` sebelum merge, jangan percaya kalimatnya.**

### 2. Empat bagian isi halaman — dan `MarkDrafted()` berhenti mengaku

`RoadmapStep`, `Tool` + `TechnologyTool`, `Project`, `Resource` mendarat berikut migrasi lima tabel (murni penambahan). Uji **53 → 73** unit, **4 → 6** integrasi. Sisa entitas ADR-015 tinggal `Article` dan `Chunk`.

🔑 **Nilai utamanya bukan empat tabel itu.** `MarkDrafted()` sudah lama menulis di ringkasannya sendiri bahwa ia berarti *"kelima bagian terisi"* — sementara ia meluluskan halaman yang **sama sekali kosong**, sebab bagian-bagian itu belum punya entitas untuk diperiksa. **Enam uji lama memerah** saat aturan itu akhirnya dipasang.

Dua bentuk yang kini tidak punya jalan masuk untuk dilanggar: **nomor langkah roadmap tidak pernah dikirim pemanggil** (roadmap berlubang jadi mustahil, dan *"langkah 0 = prasyarat"* berhenti bergantung pada ketelitian penulis), dan **`Technology.Tools` bertipe `IReadOnlyList<TechnologyTool>`** — tidak ada tempat untuk menyalin alat ke dalam halaman.

### 3. Tiga issue ditutup: platform, rilis citra, hukum

**#33 — platform.** Diputuskan Render berbayar, lalu **dibatalkan pemilik** beberapa jam kemudian. Lihat bagian 4.

**#32 — rilis citra.** Diukur: commit dokumen (`aa78079`, 199 detik) memakan waktu praktis sama dengan commit kode (`4bfbfff`, 206 detik), dan menghasilkan tiga citra baru yang isinya tidak mungkin berbeda. Riset reproducible build lengkap dan seluruh jalurnya bisa dikerjakan — `SOURCE_DATE_EPOCH`, `rewrite-timestamp=true` (**opsi eksportir, bukan flag build**), matikan provenance, `DotNet.ReproducibleBuilds`, verifikasi bangun-dua-kali.

🔑 **Yang membatalkannya adalah syarat yang tidak disebut panduan mana pun: citra dasar repo ini memakai tag BERGERAK, dan itu disengaja.** Memakunya ke digest **melemahkan Container Scan** — gerbang itu bernilai justru karena membangun ulang menarik lapisan dasar yang sudah ditambal. Trade-off sesungguhnya: **digest yang bisa dibandingkan ⟷ tambalan keamanan yang datang sendiri.** Untuk situs yang belum tayang dengan satu pengurus, tambalan otomatis menang. Yang dipasang cuma `paths-ignore` untuk dokumen — dan itu **diperiksa ke Dockerfile-nya**, bukan diduga.

**#17 — hukum.** *OpenAI Services Agreement* dan *Sharing & Publication Policy* dibaca di peramban sungguhan (pengambil otomatis tetap 403). **§3.3 memuat daftar larangan yang lengkap dan TIDAK ADA satu pun larangan menerbitkan ulang Output**; §4.1 menegaskan Output milik pelanggan, §4.2 menyatakan isi kita tidak dipakai melatih layanannya.

Dua yang mengikat, keduanya baru kelihatan setelah dibaca:

- ⚠️ **§10 "No Publicity"** melarang memasang nama/logo pihak lain di situs tanpa izin tertulis. Menempel *"Powered by OpenAI"* — hal yang terasa sopan dan jujur — **melanggarnya**.
- 🔴 **Satu kalimat *Sharing & Publication Policy* berbenturan dengan tingkat `draf` ADR-012:** isi tidak boleh direpresentasikan sebagai sepenuhnya buatan AI, dan **manusia harus memikul tanggung jawab akhir**. Label `draf` lahir demi kejujuran — dan justru karena jujur, ia menyatakan persis apa yang dilarang, pada saat belum ada manusia yang memikulnya. **Dua pembacaan sama masuk akalnya, ditulis apa adanya alih-alih dipilih diam-diam.** Belum menyala: V1 nol panggilan model.

**Batas kutipan ditetapkan:** arXiv metadata penuh (CC0); sumber lain hanya judul, tautan, nama sumber, tanggal — **nol kutipan**. Kenapa nol dan bukan "sekian kata": V1 hanya menayangkan arXiv, jadi menetapkan jatah sekarang berarti memutuskan sesuatu yang belum ada pemakainya.

### 4. Render dibatalkan, dan hosting gratis ternyata bisa

Pemilik: *"jangan render, akun gratis pilihannya pada apa"*. Hasilnya [ADR-019](adr/ADR-019-hosting-gratis-tanpa-kartu.md), yang **menggantikan** ADR-017.

🔑 **Yang keliru dari ADR-017 adalah cara memandang soalnya, bukan jawabannya.** Ia mencari **satu platform** untuk semuanya. Begitu soalnya dipecah tiga, keberatan terbesarnya lenyap: yang ditolak sebenarnya bukan "gratis", melainkan **Postgres gratis Render yang menghapus dirinya sendiri setelah 30 hari**. Neon tidak begitu.

| Bagian | Tempatnya | Kartu |
|---|---|---|
| **PostgreSQL** | Neon — pgvector, scale-to-zero, tanpa kedaluwarsa | tidak |
| **web** | Vercel Hobby — tanpa cold start | tidak |
| **api** | Koyeb Free — GHCR privat didukung resmi | tidak |

Survei menemukan dua perubahan setahun terakhir: **Docker Space Hugging Face kini menuntut plan berbayar**, dan **Oracle memangkas Always Free ARM 4→2 OCPU tanpa pengumuman apa pun**. Keduanya ditulis di ADR-nya sebagai pengingat bahwa ia berumur simpan lebih pendek daripada ADR lain di repo ini.

**Yang membuatnya bekerja — dibuktikan, bukan diharapkan.** Instance Koyeb gratis tidur setelah satu jam dan itu tidak bisa dimatikan. Jawabannya bukan mencegahnya tidur, melainkan membuat pembaca tidak bergantung padanya: panggilan API di web kini di-cache lima menit.

| Keadaan | Halaman menampilkan |
|---|---|
| Proses baru, cache kosong, **API mati** | ❌ *"API belum bisa dihubungi"* |
| API hidup (cache terisi) | ✅ 14 bidang · 5 topik |
| **API dimatikan lagi**, cache terisi | ✅ 14 bidang · 5 topik |

⚠️ `await connection()` **tetap ada**; yang di-cache panggilan API-nya, bukan halamannya. Batas waktu fetch dinaikkan **5 → 30 detik**, dan itu **konsekuensi** bukan kelonggaran: sesudah di-cache, batas pendek justru menggugurkan permintaan tepat saat instance yang tidur sedang bangun — lalu **galatnya** yang ter-cache.

**Migrasi tanpa pre-deploy:** Koyeb tidak punya *pre-deploy command*, tapi Neon terjangkau internet. `migrasi-produksi.yml` menjalankan **citra `api` yang sama persis** dan memanggil `/app/efbundle` — bukan `dotnet ef` dari kode sumber, sebab itu memigrasi dari bit yang berbeda dari yang tayang. Sengaja manual.

🔴 **Dua batas baru yang mengikat:** **Vercel Hobby hanya untuk pemakaian NON-KOMERSIAL** (kembaran §10 OpenAI — tidak terasa hari ini, mengikat di hari pertama komersialisasi), dan **citra `web` berhenti dipakai untuk penyebaran** karena Vercel membangun dari sumber.

### 5. Tiga pelajaran yang berlaku di luar sesi ini

Ketiganya bentuk yang sama: **sesuatu tampak menjaga, padahal tidak.**

1. **Penjaga yang tidak menjaga — klaim saya sendiri.** Docstring uji integrasi baru mengklaim ia menjaga `UsePropertyAccessMode(PropertyAccessMode.Field)`. Dibuktikan keliru dengan **mematikan barisnya** — keenam uji tetap hijau; keempat blok `HasMany` sekalian dimatikan, masih hijau. Konvensi EF Core sudah memetakan semuanya. Konfigurasinya dipertahankan sebagai penulisan **maksud**, dan **klaimnya yang diperbaiki, bukan kodenya**.

2. **Penjaga yang tidak menjaga — urutan langkah CI.** `run.ps1 verify` hijau lokal, CI merah: `relation "technology.technologies" does not exist`. Langkah migrasi berada **sesudah** langkah uji. Kenapa tak pernah ketahuan: satu-satunya uji integrasi sebelumnya cuma menyentuh `/health/ready`, dan **`AddDbContextCheck` hanya menguji KONEKSI, bukan keberadaan tabel**. Uji pertama yang benar-benar **menulis baris** langsung menabraknya. Urutannya ditukar, dan hasil sampingnya: **CI kini menjalankan urutan yang sama dengan produksi**.

3. **Kendali yang tidak mengendalikan.** Untuk membuktikan cache API bekerja, cache dihapus dari disk — **selagi proses Next masih hidup**. Halamannya tetap berisi, dan saya nyaris menyimpulkan cachenya bekerja padahal yang menjawab salinan di memori. **Kendali yang benar menuntut prosesnya dimatikan lebih dulu.**

🔑 Dan satu lagi yang bukan tentang penjaga, tapi tentang urutan memutuskan: **bentuk citra `migrate` dan pilihan platform ternyata TERIKAT, dan bentuk citranya sudah diputuskan lebih dulu tanpa tahu ia akan menyeleksi platform.** Pertanyaan yang layak diajukan lebih awal: *keputusan pengemasan ini diam-diam mempersempit pilihan apa nanti?*

---

## 2026-09-07 — Sesi 8 (lanjutan): Citra produksi yang terbukti melayani situsnya

Ditumpuk di atas cabang Redis, karena citra produksi tanpa Redis mustahil sebelum perubahan itu ada.

Tiga citra lahir: API, web, dan **bundel migrasi** — satu berkas yang bisa dijalankan tanpa SDK dan tanpa kode sumber di sisi produksi. Yang terakhir menjawab langsung salah satu kejutan yang [`RENCANA-V1.md`](RENCANA-V1.md) sebut menunggu di penyebaran: *"migrasi di lingkungan asing"*.

### Yang dibuktikan, bukan dirancang

`docker compose -f docker-compose.prod.yml up --build`, dari basis data **kosong**:

| Bukti | Hasil |
|---|---|
| Migrasi dari nol | 4 tabel, **14 bidang** tersemai — angka yang mengikat, bukan kira-kira |
| `/health/ready` di peti kemas produksi | **200**, hanya `postgres`; tidak ada Redis di mana pun di compose |
| Log startup API | "Redis tidak dikonfigurasi ... Ini bentuk produksi V1 menurut ADR-016" |
| `POST` muatan sampah (`name: "!!!"`) | **400** — perbaikan #27 ikut terbukti di dalam peti kemas |
| Halaman web di `:8080` | menampilkan kedua topik yang baru disemai, nama bidangnya, dan label **"Kurasi tautan"** |

Baris terakhir yang paling berarti: ia membuktikan rantai **web → api → postgres** menyala lintas tiga peti kemas, dan penegakan label ADR-012 ikut hidup di produksi.

### Dua hal yang hampir lolos

**Bundel migrasi gagal dibangun** dengan `NETSDK1047` — restore di tahap bersama sengaja RID-agnostik supaya lapisannya bisa dipakai ulang, sedangkan bundel self-contained menuntut aset `linux-x64`. Butuh restore kedua yang ber-RID.

**Frasa uji saya sendiri yang usang, bukan aplikasinya.** Mencari "2 teknologi tercatat" di HTML dan tidak menemukannya; teks sebenarnya **"2 topik tercatat"** — kata itu berubah di #25. Nyaris jadi tuduhan palsu terhadap kode yang benar.

### Yang ikut dijaga

`.dockerignore` menolak `appsettings.Development.json` secara eksplisit. Berkas itu memang ditelan `.gitignore`, tapi **konteks Docker membaca disk, bukan git** — tanpa baris itu, sandi pengembangan ikut masuk citra produksi.

CI kini membangun ketiga citra. Dockerfile yang tidak pernah dibangun CI akan membusuk diam-diam, dan ketahuannya justru saat menyebarkan.

### Pemilik memilih: terbitkan dulu, putuskan hosting belakangan

Ditanya platform mana, pemilik menjawab **belum** — dorong citranya ke GitHub Container Registry lebih dulu.

Itu memisahkan dua hal yang memang terpisah: **membangun** artefak, dan **memilih** tempat ia berjalan. Nol biaya, nol akun baru, dan keputusan hostingnya tetap terbuka.

`rilis-citra.yml` menerbitkan ketiga citra setiap `main` bergerak, dengan dua tag yang bedanya disengaja: `:<sha>` yang tidak pernah berubah — **ini yang dipasang di produksi** — dan `:main` yang bergerak, untuk melihat-lihat saja. Men-deploy tag bergerak berarti melepas kendali atas apa yang sebenarnya sedang berjalan.

Gerbang `citra` di `ci.yml` dipersempit jadi PR saja, karena di `main` workflow rilis sudah membangun ketiganya sebelum mendorong.

[`docs/PENYEBARAN.md`](PENYEBARAN.md) lahir sebagai runbook netral platform: urutan `postgres → migrate → api → web` yang mengikat, variabel lingkungan tiap citra, kenapa `/health/ready` dan `/health/live` tidak boleh tertukar, dan kenapa mengisi `ConnectionStrings__Redis` dengan alamat yang tidak ada jauh lebih buruk daripada mengosongkannya.

### Gerbang kedelapan: Container Scan — dan ia berbuah di menit pertama

`ci.yml` sendiri mencatat Container Scan sebagai gerbang yang belum menyala. Sekarang menyala, dengan ambang **CRITICAL + HIGH** yang **menggagalkan build**.

Ambang itu dipilih dengan **mengukur lebih dulu, bukan menebak**. Hasil ukurnya mengejutkan:

| Citra | CRITICAL/HIGH sebelum |
|---|---|
| API | 0 |
| Bundel migrasi | 0 |
| **web** | **13, satu di antaranya CRITICAL** |

Sebelas dari tiga belas berasal dari **`npm` yang ikut terbawa citra dasar `node:22-alpine`** — dan komentar di Dockerfile saya sendiri menulis *"Tidak ada npm"*. Itu salah, dan pemindaianlah yang membuktikannya: `which npm` di dalam citra menjawab `/usr/local/bin/npm`, 17,2 MB.

`node apps/web/server.js` tidak pernah memanggil npm. Membuangnya plus menambal openssl citra dasar: **13 → 0**, dan citranya diuji tetap melayani halaman (HTTP 200, judul dan tagline terender).

⚠️ Satu hal yang **tidak** terjadi: citranya tidak mengecil (326 → 335 MB). `rm` di lapisan belakangan tidak mengembalikan byte lapisan dasar. Yang hilang adalah kerentanannya dan kemampuan menjalankan npm di dalam peti kemas — bukan ukurannya. Komentar di Dockerfile menyebutkan itu apa adanya.

**Gerbangnya dibuktikan MERAH lebih dulu**: dijalankan terhadap citra web versi lama yang masih ber-npm, ia keluar dengan kode bukan-nol. Terhadap ketiga citra yang sekarang, hijau.

### Yang masih milik pemilik

**Platform hosting, domain, dan sertifikat.** Azure muncul di ADR-016 hanya sebagai tabel perkiraan biaya, bukan keputusan. Citra ini tidak mengunci pilihan apa pun — ia jalan di Container Apps, Fly.io, Render, maupun Railway.

⚠️ **`rilis-citra.yml` belum pernah berjalan** — ia baru menyala saat mendarat di `main`. Yang sudah terbukti separuhnya: membangun ketiga citra sudah hijau di CI. Yang belum: login dan dorong ke GHCR.

> ✅ **Terjawab 2026-09-08 (Sesi 9): ia berjalan dan hijau di percobaan pertama**, ketiga citra terbit ke GHCR. Kalimat di atas dibiarkan sebagai rekaman keadaan hari itu.

---

## 2026-09-07 — Sesi 8: Redis berhenti menghalangi penyebaran

Permintaan pemilik: *"lanjutkan."* Sasarannya Bulan 1 dari [`RENCANA-V1.md`](RENCANA-V1.md) — **URL publik yang bisa dibuka orang lain.**

Langkah pertama menuju sana bukan Dockerfile, melainkan satu kalimat yang sudah ditulis [ADR-016](adr/ADR-016-pagu-biaya.md) tentang dirinya sendiri: mematikan Redis di produksi menuntut *"perubahan kode yang belum ditulis dan belum diuji"*. Selama itu belum ada, penyebaran tidak akan pernah selesai — dan gejalanya menyamar jadi "aplikasinya lambat menyala".

### Yang ditemukan dengan menjalankan, dua-duanya lebih buruk dari dugaan ADR

`ConnectionMultiplexer.Connect` melempar **di dalam pabrik DI**, yaitu saat `RedisHealthCheck` sedang dibangun — sebelum `CheckHealthAsync` sempat berjalan. Jadi `try/catch` di dalam health check itu, yang ditulis persis untuk kasus Redis tidak terhubung, **tidak pernah dijalankan**. `/health/ready` membalas `500`, bukan `503`, dan pemanggil tidak pernah tahu dependensi mana yang jatuh.

Dan `appsettings.json` memanggang `"Redis": "localhost:6379"`. Selama baris itu ada, produksi selalu mengira Redis terpasang, apa pun yang diputuskan ADR-016.

### Percobaan pertama memindahkannya ke tempat yang salah

Baris itu mula-mula dipindahkan ke `appsettings.Development.json` — tempat yang tampak paling wajar. Ia **diabaikan `.gitignore` baris 31 dan tidak pernah dilacak**, jadi isinya tidak akan pernah sampai ke CI maupun ke klona baru. Ketahuan hanya karena berkasnya **tidak muncul di `git status`** setelah ditulisi.

Berkas lokalnya lalu dikembalikan ke isi semula, bukan sekadar ditinggalkan: kalau tidak, mesin ini akan hijau karena alasan yang tidak dimiliki CI. Tempat yang benar ternyata `Properties/launchSettings.json` — tracked, dan hanya berlaku saat `dotnet run`, sehingga produksi yang menjalankan DLL-nya langsung tidak pernah melihatnya.

### Uji integrasi pertama repo ini

Komentar di `ci.yml` sudah menyiapkan tempatnya: gerbang Integration Test sengaja belum menyala *"karena belum ada yang bisa diperiksa"*. Sekarang ada, dan CI-nya memang sudah menjalankan Postgres dan Redis sebagai service container.

Empat uji, dua di antaranya **merah lebih dulu**. Yang dijaga bukan gejalanya melainkan bentuk jawabannya: kesiapan hijau tanpa Redis, `503` bukan `500` saat Redis dikonfigurasi tapi mati, dan liveness tetap hidup apa pun keadaan dependensinya.

Satu aturan yang ikut ditegakkan: **check yang tidak dipasang tidak boleh muncul sebagai "sehat"** — kalau tidak ada string koneksi, `redis` hilang sama sekali dari `/health/ready`. Yang kedua akan menyembunyikan salah konfigurasi.

### Kendali yang membuat buktinya berarti

Bukti akhir diambil dengan menjalankan API berlingkungan `Production` **selagi Redis pengembangan tetap menyala di 6379**. Tanpa kendali itu, hijaunya tidak membuktikan apa pun — bisa saja Redis-nya kebetulan memang mati.

Percobaan pertama gagal justru karena ini: `dotnet run` memakai `launchSettings.json`, yang memaksa `ASPNETCORE_ENVIRONMENT=Development` dan menimpa env. Hasilnya `redis` dilaporkan **sehat** di lingkungan yang seharusnya produksi. Butuh `--no-launch-profile`.

---

## 2026-09-07 — Sesi 7: Dua PR mendarat, dan satu cacat yang lolos keduanya

Permintaan pemilik: *"apakah masih ada PR?"* lalu *"lanjutkan secara berurutan."*

### Kedua PR di-merge, berurutan

`fase-1/skeleton` (#23) lebih dulu ke `main`, baru `fase-1/skema-konten` (#25) yang ditumpuk di atasnya. **Merge commit, bukan squash** — squash pada #23 akan menulis ulang commit yang jadi tumpuan #25, dan diff #25 jadi bentrok.

Yang diperiksa setelah merge, bukan sebelum: CI hijau di `main` untuk **kedua** merge commit, dan 35 uji lulus di atas hasil penggabungan. `main` = `eb394a0`.

### Tinjauan menemukan cacat yang tidak ditutup PR mana pun

Angka +11.097 di #23 menyesatkan: kode aplikasi sesungguhnya ~1.900 baris, sisanya `package-lock.json` (6.803) dan dokumen (1.584). Setelah dibaca, satu cacat tersisa di `POST /api/v1/technologies`:

**`name` atau `slug` berisi `"!!!"` membalas 500, padahal seharusnya 400.** `CreateTechnologyValidator` meluluskannya — ia hanya memeriksa `NotEmpty` dan `MaximumLength` — lalu `Technology.Create` melempar `ArgumentException`, dan tidak ada satu pun `try/catch` sampai `app.UseExceptionHandler()`.

Ironisnya obatnya sudah ditulis di #25. `Slugs.TryFrom` lahir persis untuk ini, lengkap dengan komentar *"'!!!' adalah permintaan yang keliru (400), bukan galat server (500)"* — tapi ia hanya dipasang ke `FieldSlug`. Jalur `name`→slug, yang justru dicontohkan komentarnya sendiri, tetap memakai `Slugs.From` yang melempar.

### Instans kedua dari kelas cacat yang sama

Ditemukan saat memperbaiki yang pertama: `Name` boleh 200 karakter, kolom `technologies.slug` hanya `varchar(160)`, dan **tidak ada yang memeriksa panjang slug yang DITURUNKAN**. Nama 200 karakter lolos validator, lalu ditolak basis data — 500 lagi, sebab yang berbeda.

Akarnya satu: angka 160 hidup di dua tempat yang tidak saling tahu. Sekarang ia `Technology.MaxSlugLength`, dipakai penjaga masukan maupun lebar kolom. `dotnet ef migrations has-pending-model-changes` memastikan penggantian angka literal jadi konstanta tidak mengubah model.

### Uji dibuktikan MERAH lebih dulu

11 kasus gagal terhadap kode sebelum perbaikan, termasuk uji yang menyatakan invarian sesungguhnya: **apa pun yang diluluskan validator harus bisa dikerjakan sampai selesai.** Sesudah perbaikan 53 lulus, nol gagal — naik dari 35. `run.ps1 verify` hijau seluruhnya.

---

## 2026-09-04 — Sesi 6: Taksonomi berhenti jadi teks bebas

Permintaan pemilik: *"lanjutkan."* Yang dikerjakan: pekerjaan Bulan 1 pertama dari [`RENCANA-V1.md`](../docs/RENCANA-V1.md) — keputusan Sesi 5 mendarat jadi kode.

Cabang **`fase-1/skema-konten`**, ditumpuk di atas `fase-1/skeleton`. PR #23 karenanya tetap satu unit review yang utuh.

### Yang berubah

`Technology.Category` — teks bebas yang boleh berisi apa saja, termasuk `"Belum diputuskan"` — naik jadi kunci asing ke tabel `fields` berisi **14 baris yang disemai migrasi**. Bidang bukan data pengguna; ia bagian dari keputusan struktur, jadi menambah atau membuang satu bidang harus lewat ADR dan migrasi, bukan lewat `POST` yang tidak meninggalkan jejak keputusan.

Bersamanya, `ContentMaturity` mendapat kolomnya sendiri, dan `GET /api/v1/fields` lahir.

### Aturan ADR-012 berhenti jadi kalimat

Tiga tempat menegakkannya sekaligus, dan ketiganya tipe — bukan komentar:

1. **`MarkReviewed(reviewer)` adalah satu-satunya jalan ke `HumanReviewed`,** dan ia menuntut nama pemeriksanya. Tidak ada cara menandai halaman "sudah diperiksa manusia" tanpa menyebut manusianya.
2. **`Update()` menurunkan kembali halaman yang sudah diperiksa.** Pemeriksaan berlaku atas teks yang diperiksa, bukan atas nama halamannya — kalau teksnya berubah, labelnya tidak boleh ikut bertahan.
3. **`Maturity` ada di kontrak API bentuk RINGKAS**, bukan cuma bentuk lengkap. Kalau daftar boleh tidak membawa kematangan, ada jalan menampilkan judul tanpa label, dan aturan itu jadi bergantung pada ingatan penulis UI.

Satu penjaga lagi ditulis sebagai uji: **`Publish()` sengaja TIDAK memeriksa `Maturity`.** Halaman kurasi yang terbit adalah bentuk akhir yang benar untuk bidang prioritas 3, jadi menambahkan larangan di sana adalah regresi — dan sekarang ada uji yang akan memerah kalau seseorang mencobanya.

### Migrasi ditulis tangan, dan penjaganya dibuktikan MERAH lebih dulu

Hasil scaffold EF membuang kolom `Category` **lebih dulu** lalu mengisi `FieldId` dengan Guid kosong — setiap baris lama kehilangan kategorinya dan langsung melanggar kunci asing. Urutannya dibalik: semai bidang → tambah kolom yang boleh kosong → isi dari kategori lama → **berhenti dengan pesan yang bisa ditindaklanjuti** → baru wajibkan dan buang kolom lama.

Penjaganya tidak diterima begitu saja. Satu baris berkategori `"Belum diputuskan"` **sengaja disisipkan** ke basis data pengembangan, lalu migrasi dijalankan:

```
P0001: Migrasi berhenti: kategori lama berikut bukan salah satu dari 14 bidang
ADR-010: Belum diputuskan. Perbaiki kategorinya dulu, atau kalau ini basis data
pengembangan berisi contoh saja, jalankan `run.ps1 reset` lalu migrate ulang.
```

Diperiksa juga bahwa transaksinya benar-benar berguling balik — tabel `fields` nol baris, kolom `Category` utuh. Baris perusaknya lalu dibuang dan migrasi dijalankan sungguhan: **14 bidang tersemai, kelima topik lama terpetakan, nol yatim.**

Alasan langkah ini dikerjakan: gerbang yang belum pernah terlihat merah belum terbukti menjaga apa pun.

### Dibuktikan jalan, bukan cuma dikompilasi

| Gerbang | Hasil |
|---|---|
| `run.ps1 verify` | Hijau — 0 peringatan, **35 uji** (naik dari 15) |
| Migrasi dari basis data berisi | 14 bidang, 5 topik terpetakan, 0 yatim |
| `GET /api/v1/fields` | 14 bidang, masing-masing dengan jumlah topik **dan** jumlah yang sudah ditinjau |
| `POST` dengan bidang karangan | **400**, bukan 500, dengan pesan yang menunjuk `GET /api/v1/fields` |
| Beranda web | 14 kartu bidang tampil, tiap topik membawa label kematangannya |

### Catatan

- Penyimpangan dari rencana sendiri, dicatat apa adanya: ADR-015 menulis implementasi menunggu PR #23 di-merge. Yang dikerjakan cabang bertumpuk. Alasan aslinya tetap dipenuhi — review #23 tidak jadi dua pekerjaan — tanpa mengunci pekerjaan di belakang review yang belum tentu segera datang.
- **`ContentMaturity.MachineDrafted`, bukan `Drafted`.** `TechnologyStatus` sudah punya `Draft`, dan dua sumbu yang ADR-015 susah-payah pisahkan tidak boleh memakai kata yang sama — itu jalan tercepat menuju penggabungan yang justru dilarang.
- `Slugify` pindah ke `Slugs` supaya `Field` dan `Technology` memakai aturan alamat yang sama. Dua entitas yang membuat slug dengan cara berbeda berarti dua alamat berbeda untuk nama yang sama.

### Satu lubang gerbang yang ketahuan karena PR ini sendiri

`ci.yml` memicu `push` untuk `main` **dan** `fase-*/**`, tapi `pull_request` hanya untuk `main`. Begitu PR #25 dibuka menyasar `fase-1/skeleton`, akibatnya kelihatan: PR bertumpuk cuma kebagian jalan `push` — yang menguji **ujung cabangnya**, bukan **hasil penggabungannya**. Jalur pindai-rahasia untuk kejadian `pull_request`, yang justru baru diperbaiki di #24, juga tidak pernah terlewati.

Diperbaiki jadi `branches: [main, "fase-*/**"]`, dan run `pull_request` pertamanya langsung hijau — jadi perbaikannya terbukti, bukan diasumsikan.

---

### Yang ditunggu Sesi 6 — semuanya sudah lewat

> 📌 **Snapshot akhir Sesi 6, bukan daftar yang berlaku.** Ketiga barisnya sudah
> terjawab: **#23 dan #25 di-merge di Sesi 7**, dan **ToS OpenAI & Microsoft
> dibaca 7 September 2026** (#17 tetap terbuka, tapi bukan lagi karena "belum
> ada yang membacanya" — lihat `AUDIT-KELAYAKAN.md`). Daftar yang berlaku ada
> di entri sesi paling atas.

| Ditunggu waktu itu | Catatan hari itu |
|---|---|
| **Review PR [#23](https://github.com/xtheoputra/techverse-x/pull/23)** | Kerangka Fase 1. 3/3 gerbang hijau. |
| **Review PR [#25](https://github.com/xtheoputra/techverse-x/pull/25)** | Bidang + kematangan konten, ditumpuk di atas #23. 6/6 gerbang hijau. |
| **Issue [#17](https://github.com/xtheoputra/techverse-x/issues/17)** | Baca ToS OpenAI dan Microsoft di peramban sungguhan. Tidak bisa diwakilkan ke agen mana pun — halaman OpenAI membalas 403 ke pengambil otomatis. |

**Begitu #23 dan #25 masuk, urutan kerjanya sudah ditetapkan** [`RENCANA-V1.md`](RENCANA-V1.md):

1. **Deploy ke produksi** — ini Bulan 1, bukan Bulan 6. Yang tayang lebih awal punya lima bulan untuk memperbaiki kejutannya.
2. Entitas isi halaman berikutnya: `RoadmapStep`, `Tool`, `Project`, `Resource`.
3. `Article` dan `Chunk` paling belakang — `Chunk` menuntut ekstensi pgvector dipasang di image Postgres, dan itu perubahan infrastruktur yang berdiri sendiri.

**Ukuran kemajuan tetap satu angka: berapa topik berstatus `tinjau`.** Hari ini **nol**. Target akhir Bulan 2 adalah 7, seluruhnya dari satu bidang: AI Agents.

---

## 2026-09-04 — Sesi 5: Dua puluh dua keputusan diambil sekaligus

Permintaan pemilik, setelah ke-22 issue diperlihatkan padanya: *"saya serahkan ke Anda saja, anggap Anda sebagai senior proyek… lakukan yang menurut Anda benar."*

Jadi keputusannya diambil, bukan ditanyakan balik. Semuanya tercatat di [`KEPUTUSAN.md`](../docs/KEPUTUSAN.md) berikut **cara membatalkan tiap keputusan** — pendelegasian bukan alasan untuk membuat sesuatu jadi sulit dibantah.

### Benang merah yang dipakai untuk memutuskan

> **Proyek ini mati karena empat puluh halaman setengah jadi, bukan karena kurang fitur.**

Ke-22 jawaban dipilih supaya konsekuensinya menuju satu arah: sedikit yang tuntas, bukan banyak yang menggantung. Empat keputusan yang paling menentukan, dan alasannya:

1. **Deployment naik dari Bulan 6 ke Bulan 1.** Ini perubahan paling penting di seluruh sesi. Rencana lama menaruh deployment di bulan terakhir — artinya sertifikat, variabel lingkungan, migrasi di lingkungan asing, CORS, dan tagihan pertama semuanya menunggu di garis finis, setelah seluruh biaya dikeluarkan.
2. **Template Versi B menang bukan karena lebih ringkas, tapi karena melepas kelengkapan halaman dari pipeline berita.** Versi A mewajibkan "Berita terbaru" dan "Paper terbaru" di tiap halaman — artinya satu blocker hukum ([#17](https://github.com/xtheoputra/techverse-x/issues/17)) akan membuat 82 halaman berstatus "belum lengkap" selamanya.
3. **Tiga tingkat kematangan (`kurasi` · `draf` · `tinjau`) menjawab "siapa menulis 287 blok" dengan ketiga opsinya sekaligus, berurutan.** Yang ditolak bukan salah satu opsinya, melainkan memilih satu opsi untuk semua halaman sekaligus.
4. **LangGraph dibuang.** Untuk pengembang tunggal, dua runtime adalah kesalahan termahal yang bisa diambil tanpa terasa. Keputusan ini juga yang membuat tiga pos biaya jadi nol dan abstraksi penyedia AI jadi gratis.

### Satu temuan yang datang dari kode, bukan dari dokumen

Saat merancang skema ([ADR-015](adr/ADR-015-skema-data-v1.md)), `TechnologyStatus` yang **sudah ada di kode Fase 1** ternyata bukan tingkat kematangan konten, melainkan daur hidup redaksi (Draft · Discovered · Verified · Published · Archived, dari `KERANGKA.md` 4.6 Research Pipeline).

Keduanya sumbu berbeda dan **tidak boleh digabung**: halaman `Kurasi` yang `Published` adalah bentuk akhir yang sah, dan halaman `Draf` yang `Published` adalah keadaan normal — asalkan berlabel. Kalau digabung, yang hilang adalah kemampuan menerbitkan halaman kurasi yang jujur. Dicatat eksplisit di ADR supaya tidak "disederhanakan" nanti.

Ini menegaskan kebiasaan yang sudah beberapa kali berbuah di repo ini: **baca kodenya, jangan hanya dokumennya.**

### Angka yang berubah, dan angka yang sengaja tidak dibesarkan

| Dulu | Sekarang |
|---|---|
| 12 bidang, 41 submenu, ±287 blok | **14 bidang, 82 topik** ([ADR-010](adr/ADR-010-taksonomi-bidang.md)) |
| 7 bagian template × tiap halaman | **5 bagian** — 164 blok tidak perlu ditulis |
| — | **42 topik** dijadwalkan ditulis manusia; **22** di antaranya dalam enam bulan |

Angka Bulan 6 sengaja **tidak** dinaikkan ke 42. Enam bulan hanya sanggup menuntaskan tiga dari enam bidang prioritas 1, dan menuliskan 42 di sana akan membuat rencana baru ini persis seperti rencana yang digantikannya.

### Yang TIDAK diputuskan, dan kenapa

[Issue #17](https://github.com/xtheoputra/techverse-x/issues/17) dibiarkan terbuka. Ketentuan layanan OpenAI membalas **403** ke pengambil otomatis — sampai sekarang belum dibaca siapa pun, termasuk saya. Yang bisa dikerjakan hanya **memundurkan blocker-nya**: V1 hanya menerbitkan ulang arXiv (metadata CC0, satu-satunya yang jelas boleh), dan sumber lain hanya ditampilkan judul + tautan + sumber. Dengan itu #17 berhenti menghalangi V1 dan tetap menghalangi yang lain — sebagaimana mestinya.

Ini pengurang paparan, bukan nasihat hukum.

### Aturan pemilik tetap dipegang

`KERANGKA.md` **tidak diubah satu kalimat pun**. Yang ditambahkan hanya blok penunjuk di kepalanya: kalau ia berbeda dengan `KEPUTUSAN.md`, yang berlaku `KEPUTUSAN.md`. Berikut tabel lima tempat yang paling sering salah dibaca.

### Hasil

8 ADR baru (009–016), 5 ADR lama dinaikkan dari "ditunda" jadi keputusan, `KEPUTUSAN.md`, `RENCANA-V1.md`, README dan ADR README diperbarui. **21 dari 22 issue ditutup**; butir E10 — satu-satunya dari 30 butir yang tidak pernah punya issue — ikut terjawab (ia bagian `/future`, bukan bidang ke-15).

---

## 2026-09-04 — Sesi 4: CI dari merah jadi hijau

Permintaan pemilik: *"lanjutkan kerjakan proyek."* Dari 23 issue terbuka, hanya [#24](https://github.com/xtheoputra/techverse-x/issues/24) yang bisa dikerjakan tanpa keputusan pemilik lebih dulu — 22 sisanya memang meminta pemilik memutuskan. Jadi itu yang dikerjakan.

### Dua sebab, dua-duanya tidak terlihat dari mesin pemilik

**1. `global.json` menulis versi runtime, bukan versi SDK.** `10.0.0` itu versi runtime; begitu `rollForward` disebut, `setup-dotnet` menuntut versi SDK utuh berikut pita fiturnya. Diubah jadi `10.0.100`. Yang penting dicatat: `dotnet` di mesin yang SDK-nya sudah terpasang menerima `10.0.0` **diam-diam** — dicoba, `dotnet --version` di folder repo tetap menjawab `10.0.302` tanpa keluhan. Karena itu lubang ini mustahil terlihat secara lokal.

**2. `gitleaks-action` mati 403 — dan sebabnya BUKAN penulisan komentar.** Diagnosis awal di issue #24 menduga aksi itu tersandung saat menulis komentar ringkasan ke PR. Pembacaan `dist/index.js` pada SHA yang benar-benar dijalankan CI (`ff98106`) menunjukkan hal lain: `ScanPullRequest` memanggil `GET /repos/{owner}/{repo}/pulls/{pull_number}/commits` **sebelum memindai apa pun**, sekadar untuk menentukan rentang commit. Itu panggilan **baca**, dan itu sebabnya jobnya mati dalam 6 detik padahal tidak ada satu pun rahasia di repo.

Konsekuensinya izin yang benar `pull-requests: read`, bukan `write`. Komentar PR — satu-satunya yang menuntut `write` — dimatikan lewat `GITLEAKS_ENABLE_COMMENTS`. Tidak ada yang hilang: kebocoran tetap menggagalkan job lewat exit code `2` → `process.exit(1)`, dan rinciannya (aturan · commit · berkas · baris · penulis) tetap ditulis **utuh** ke job summary lewat `core.summary`, yang tidak menyentuh API sama sekali. Aksi pihak ketiga jadi tidak perlu dipercayai memegang izin tulis.

### Penjaga baru: `verify` memeriksa `global.json` lebih dulu

Pelajaran sesi ini bukan "ada dua bug", melainkan **gerbang lokal dan gerbang CI menilai benda yang berbeda**. `verify` sekarang memeriksa bentuk `sdk.version` sebelum gerbang lain — di `Makefile` maupun `run.ps1`, keduanya diuji menolak `10.0.0` dan menerima `10.0.100`.

### Cacat sampingan: `run.ps1` sekarang murni ASCII

Windows PowerShell 5.1 membaca berkas `.ps1` tanpa BOM sebagai ANSI. Akibatnya panah `U+2192` di `Invoke-Step` selama ini tercetak sebagai `â†’` di layar pemilik. Yang jauh lebih berbahaya ditemukan saat menulis pesan galat penjaga di atas: em dash `U+2014` terbaca sebagai `”`, dan bagi PowerShell karakter itu **sah sebagai penutup string** — satu em dash saja menggagalkan parse seluruh skrip. `run.ps1` kini tidak memuat satu pun karakter di atas ASCII.

### Terbukti

| Gerbang | Hasil |
|---|---|
| `run.ps1 verify` lokal | Hijau — build Release 0 peringatan, 15 uji lulus, lint + build web lolos |
| CI `push` [33845918673](https://github.com/xtheoputra/techverse-x/actions/runs/33845918673) | 3/3 hijau |
| CI `pull_request` [33845921352](https://github.com/xtheoputra/techverse-x/actions/runs/33845921352) | 3/3 hijau |
| gitleaks benar-benar memindai | `2 commits scanned`, bukan dilewati |

### Selisih yang MASIH tersisa antara lokal dan CI

Log `setup-dotnet` menunjukkan `rollForward: latestFeature` memang dihormati: runner memakai SDK **10.0.400**, sementara mesin pemilik memakai **10.0.302**. Keduanya di atas lantai `10.0.100` dan keduanya hijau hari ini. Tapi dengan `TreatWarningsAsErrors` menyala, SDK yang lebih baru bisa membawa analyzer baru — artinya CI masih bisa merah pada perubahan yang hijau di mesin pemilik.

Bukti angka itu sekaligus **menggugurkan satu kalimat di [ADR-002](adr/ADR-002-dotnet.md)** yang berbunyi CI memakai `global-json-file` "supaya keduanya tidak bisa berbeda". Nyatanya bisa, dan `rollForward` sendiri sebabnya: `global.json` menyamakan **lantai** versi, bukan versinya. ADR-002 sudah dikoreksi berikut bukti nomor runnya.

Yang **tidak** diubah: pin ketat tetap ditolak. Itu keputusan ADR-002 yang sudah diambil — audit menyebut `global.json` yang mem-pin terlalu sempit sebagai "bom waktu senyap". Jadi selisih di atas adalah harga yang sengaja dibayar, bukan pekerjaan yang tertinggal.

---

## 2026-09-03 — Sesi 3: Baris kode pertama (Fase 1 skeleton)

Permintaan pemilik: *"eksekusi dokumen menjadi realita."*

### Benturan yang harus diselesaikan lebih dulu

Repo ini punya aturan buatan pemilik sendiri: **jangan menulis kode sebelum milestone Fase 0 selesai.** Saat permintaan ini masuk, **seluruh 22 issue masih terbuka**, empat di antaranya berlabel `blocker`.

Jalan tengah yang diambil: bangun **hanya Phase 1 — Skeleton** dari `KERANGKA.md` 4.17, karena tidak ada satu pun butirnya yang jawabannya berubah tergantung keempat blocker itu. Alasan lengkap dan daftar apa yang sengaja TIDAK dibangun ada di [ADR-008](adr/ADR-008-batas-fase-1.md).

### Yang dibangun

| Bagian | Isi |
|---|---|
| `apps/api` | Host ASP.NET Core — health check `live`/`ready`, correlation id, OpenAPI, ProblemDetails |
| `services/technology` | Bounded context pertama: agregat + 3 irisan vertikal (Create/Get/Search) |
| `packages/contracts` | Kontrak API bersama |
| `apps/web` | Next.js 16 App Router, satu halaman yang membaca API sungguhan |
| `tests/unit` | 15 uji domain |
| Infrastruktur | `docker-compose.yml` (Postgres 17 + Redis 8), migrasi EF Core, CI, `Makefile` + `run.ps1` |

### Dibuktikan jalan, bukan cuma dikompilasi

Diverifikasi ujung ke ujung pada 2026-09-03: container sehat, migrasi membuat tabel sungguhan, `/health/ready` melaporkan Postgres **dan** Redis sehat, keempat endpoint menjawab benar (201 · 200 · 404 · 409 · 400 validasi), dan halaman web menampilkan lima entri yang benar-benar datang dari PostgreSQL.

### Empat hal yang ditemukan justru karena setelan dibuat ketat

Peringatan diperlakukan sebagai galat sejak commit pertama. Hasilnya langsung terasa:

1. **Kerentanan tingkat tinggi di dependensi transitif.** `Microsoft.AspNetCore.OpenApi` 10.0.4 menarik `Microsoft.OpenApi` 2.0.0 yang punya advisory GHSA-v5pm-xwqc-g5wc. Build gagal, versinya disematkan naik ke 2.12.2. Ini persis gerbang "Dependency Scan" di 4.12, dan ia menangkap sesuatu di hari pertama.
2. **Konflik versi paket** — EF Core dipin 10.0.0 padahal Npgsql menuntut ≥ 10.0.4. Dijawab dengan manajemen versi terpusat (`Directory.Packages.props`), sejalan dengan peringatan audit B1 soal angka versi yang tersebar.
3. **Next.js 16 memanggang halaman jadi statis** — yang ikut terpanggang adalah galat "API tidak bisa dihubungi", karena saat build API memang mati. Diperbaiki dengan `connection()`. Rinciannya di [ADR-001](adr/ADR-001-nextjs.md).
4. **Analyzer menolak kode yang ditulis mesin dan nama uji.** Migrasi EF dan pola nama `Metode_Skenario` dikecualikan lewat `.editorconfig` — dikecualikan setepat mungkin, bukan dengan mematikan aturannya secara global.

### Catatan lingkungan

- `make` **tidak ada** di mesin Windows pemilik. Karena itu `Makefile` (untuk CI dan Linux) dikembari `run.ps1` dengan perintah yang sama persis.
- Next.js 16 menaruh `AGENTS.md` di `apps/web` yang isinya memperingatkan bahwa API-nya berbeda dari ingatan model, dan menyuruh membaca `node_modules/next/dist/docs/`. Peringatan itu terbukti benar — lihat temuan nomor 3 di atas.

### Yang TIDAK dikerjakan

Autentikasi, kode AI, vector DB, Neo4j, event bus, pengambilan berita, Dockerfile, Terraform, dan isi kurikulum. Semuanya bergantung pada issue yang masih terbuka. Daftar lengkap berikut alasan per butir ada di [ADR-008](adr/ADR-008-batas-fase-1.md).

**Satu-satunya keputusan Fase 0 yang praktis ikut terambil: `.NET 10`** (Issue [#12](https://github.com/xtheoputra/techverse-x/issues/12)) — butir yang tenggatnya nyata dan vonis auditnya berkeyakinan tinggi.

---

## 2026-09-03 — Sesi 2: Perapian seluruh dokumen

Permintaan pemilik: rapikan semua dokumen `.md`, termasuk yang duplikat. Tidak ada isi temuan maupun kata pemilik yang diubah — yang dirapikan struktur, urutan, dan pengulangan.

### Struktur

- **Satu `# H1` per berkas.** Ketiga dokumen besar sebelumnya memakai beberapa H1 dalam satu berkas, sehingga daftar isi otomatis GitHub tidak bersarang benar. Semua judul di bawahnya diturunkan satu tingkat.
- **`AUDIT-KESEGARAN.md` ternyata dua dokumen yang ditempel jadi satu** — masing-masing punya H1, blok metadata, dan bagian peringatannya sendiri. Kini jadi satu berkas dengan `Bagian A` (peta dua belas teknologi) dan `Bagian B` (empat bidang baru/berubah), mengikuti pola `AUDIT-KELAYAKAN.md`.
- **Dua bagian peringatan digabung jadi satu** di kaki berkas, sesuai janji di kepalanya sendiri (*"baca peringatan di bagian paling bawah"*).
- **`KERANGKA.md` dapat Daftar Isi.** Berkas 2.000 baris tanpa peta arah.

### Pengulangan yang dibuang

- **Blok metadata ganda di `AUDIT-KESEGARAN.md`** — dua audit, dua kepala berkas. Jadi satu tabel ringkas di kepala.
- **Dua bagian peringatan di berkas yang sama** — digabung, dan yang khusus Bagian B jadi butir di dalamnya.
- **Daftar temuan audit di berkas catatan sesi ini** yang mengulang isi kedua berkas audit hampir kata per kata — diganti tabel penunjuk.
- **Daftar temuan di `README.md`** — tetap ada karena halaman muka memang harus berdiri sendiri, tapi tiap butir kini menaut ke bagian kanonisnya di berkas audit, bukan jadi salinan keempat yang bisa hanyut sendiri.

### Pengulangan yang sengaja DIBIARKAN

Tidak semua pengulangan layak dibuang. Tiga ini dipertahankan, dengan alasan:

- **Deskripsi produk di `README.md` dan `KERANGKA.md`.** Sempat dihapus dari `KERANGKA.md` dan diganti penunjuk, lalu dikembalikan — itu kalimat pemilik, dan `KERANGKA.md` adalah rekaman kata pemilik. Halaman muka yang menunjuk ke sumber lebih sehat daripada sumber yang menunjuk ke halaman muka.
- **Empat vonis arsitektur** yang muncul di `KERANGKA.md` 2.6, 4.20, dan butir D4. Ketiganya beda peran: 2.6 tabel keputusan, 4.20 rekonsiliasi, D4 butir yang menunggu. Rumah kanonisnya tetap `AUDIT-KELAYAKAN.md` Bagian B.
- **Vonis Edge AI/IoT** di butir E11, E12, dan `AUDIT-KESEGARAN.md` Bagian B. E12 adalah keputusan yang menunggu; sisanya rujukan.

### Yang ditemukan saat merapikan

Lima hal yang bukan sekadar rapi-rapi, semuanya sudah diperbaiki atau ditandai di tempatnya:

1. **Dua berkas audit sama-sama mengaku "belum ada koreksi yang diterapkan ke `KERANGKA.md`"** — pernyataan itu sudah kedaluwarsa. Tabel arsitektur 2.6 sudah ditulis ulang (mengikuti Bagian 4, bukan mengikuti audit), dan butir E5/E11 sudah memuat penunjuk hasil audit. Kedua pernyataan diperbaiki agar menyebut persis apa yang berubah dan apa yang tidak.
2. **Tanggal GA Microsoft Agent Framework berbeda antar-berkas** — `AUDIT-KESEGARAN.md` menulis 3 April 2026, `AUDIT-KELAYAKAN.md` menulis 2 April 2026, untuk peristiwa yang sama. Keduanya keluaran agen berbeda. Tidak dipilih salah satu; perbedaannya ditandai di kedua berkas supaya dicek ke NuGet sebelum masuk halaman publik.
3. **Butir D6 dan catatan pengantar bagian D tercecer di bawah bagian E** — dikembalikan ke bagian D.
4. **Butir E10 tidak pernah jadi issue.** Dari 30 butir keputusan, 29 terpetakan ke 22 issue; E10 ("teknologi baru yang belum muncul" — bidang ke-13 atau bagian Future?) terlewat. Ditandai di peta butir, belum diputuskan.
5. **`README.md` tidak menyebut Bagian 4 sama sekali**, padahal Engineering Blueprint v1 kini tiga perempat isi `KERANGKA.md`. Ditambahkan.

**Masih nol baris kode.** Perapian ini tidak menyentuh milestone Fase 0.

---

## 2026-09-02 — Sesi 1: Kerangka awal + tiga audit

**Titik mulai:** folder kosong total. Nol berkas, nol subfolder, belum ada `.git`.

**Cara kerja yang disepakati pemilik:** pemilik mendiktekan kerangka bertahap dengan kata-kata yang belum tertata; tugas asisten merapikan, menandai benturan dan lubang, lalu menyimpannya ke `KERANGKA.md`. **Koreksi tidak boleh diterapkan diam-diam** — kata-kata pemilik direkam apa adanya, hasil pemeriksaan fakta ditaruh di berkas audit terpisah.

### Empat gelombang dikte

1. **Peta teknologi 2026** — dua belas teknologi, lima pilar, tabel prioritas pribadi. Jadi Bagian 1.
2. **Produk & dashboard** — tujuan, struktur menu 8 bidang / 41 halaman, template halaman, empat fitur AI, arsitektur teknologi, konsep basis data, Visi 2.0, rencana 6 bulan. Jadi Bagian 2.
3. **Identitas & struktur aplikasi** — revisi jadi 12 bidang, plus tujuh bagian aplikasi (Explore, Learn, Labs, AI, Intelligence, Knowledge Graph, Future). Jadi Bagian 3.
4. **Engineering Blueprint v1** — prinsip arsitektur, C4, bounded context, AI platform, event-driven, keamanan, observability, produksi, backlog 120 task. Jadi Bagian 4. Tabel arsitektur 2.6 ikut ditulis ulang agar sejalan, dan deviasinya direkonsiliasi di 4.20.

### Tiga audit dijalankan

Total **29 agen paralel**, nol gagal. Temuannya ada di berkas auditnya — tidak diulang di sini.

| Audit | Agen | Hasil ringkas | Berkas |
|---|---|---|---|
| Kesegaran peta 12 teknologi | 15 | Kedua belas entri berstatus `sebagian-usang` | [`AUDIT-KESEGARAN.md` Bagian A](../AUDIT-KESEGARAN.md#bagian-a--peta-dua-belas-teknologi) |
| Kelayakan sumber berita + tumpukan | 10 | Keempat pilihan arsitektur bermasalah | [`AUDIT-KELAYAKAN.md`](../AUDIT-KELAYAKAN.md) |
| Empat bidang baru/berubah | 4 | Renewable Energy kehilangan 2 dari 3 isinya | [`AUDIT-KESEGARAN.md` Bagian B](../AUDIT-KESEGARAN.md#bagian-b--empat-bidang-baruberubah) |

### Koreksi yang dilakukan asisten atas dirinya sendiri

Asisten sempat menduga **Edge AI wajar ditempatkan di bawah IoT**. Audit menolaknya dengan tiga alasan. Dicatat sebagai butir E12 — Edge AI perlu bidangnya sendiri.

### Batasan mutu yang tercatat pada hari itu

- **Kuota WebSearch keempat agen audit ketiga habis** sebelum sempat dipakai, sehingga verifikasi terpaksa lewat WebFetch ke URL tebakan. Bagian taksonominya tetap kuat; bagian "pemain terkini" lemah dan sebagian bersandar Wikipedia.
- Beberapa klaim luar biasa **belum diverifikasi independen** — sudah ditandai di kaki berkas audit.
- **Ketentuan layanan OpenAI dan Microsoft belum dibaca siapa pun** — halamannya memblokir bot. Ini pekerjaan manusia dan harus selesai sebelum tayang (Issue [#17](https://github.com/xtheoputra/techverse-x/issues/17)).

### Keadaan akhir sesi

Repositori dibuat dan didorong ke GitHub sebagai **privat**. Seluruh keputusan yang menunggu dipindahkan menjadi GitHub Issues berlabel dan bermilestone.

**Belum ada satu baris kode pun.** Itu disengaja — milestone *Fase 0 — Kunci Kerangka* harus selesai lebih dulu.
