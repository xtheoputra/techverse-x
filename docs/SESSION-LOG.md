# Catatan Sesi Kerja

Urutan terbaru di atas. Berkas ini mencatat **apa yang terjadi dan kapan** — bukan isi temuannya. Temuan tinggal di berkas auditnya masing-masing.

---

## 2026-09-08 — Sesi 9: Citra pertama benar-benar terbit

Permintaan pemilik: *"lanjutkan semua tugas kemarin."*

### Kedua PR bertumpuk mendarat

`#29` (Redis opsional) lebih dulu ke `main` — `ee6f836` — lalu `#31` (citra produksi) — `4bfbfff`. Merge commit, bukan squash, alasan yang sama seperti Sesi 7.

Diperiksa **sebelum** merge, bukan sesudah: kedua cabang digabung ke salinan `main` di mesin lokal lalu `run.ps1 verify` dijalankan di atas hasilnya. Hijau seluruhnya — 53 uji unit + 4 uji integrasi, 0 peringatan, lint dan build web bersih. `main` sudah bergerak 6 commit sejak kedua cabang dibuat, tapi berkas yang disentuh tidak beririsan sama sekali.

Sesudah merge, log CI `main` dibaca per-baris untuk memastikan keempat uji integrasi **benar-benar berjalan**, bukan sekadar "Test Run Successful" atas nol uji: `Total tests: 4 / Passed: 4`, dan yang paling lambat (`Ready_membalas_503...`, 6 detik) memang menunggu koneksi Redis mati seperti yang dimaksudkan.

### `rilis-citra.yml` akhirnya berjalan — dan hijau di percobaan pertama

Ketiga citra dibangun, dipindai Trivy (CRITICAL+HIGH, nol temuan), lalu didorong ke GHCR dengan dua tag masing-masing:

| Citra | Digest |
|---|---|
| `api` | `sha256:bd13e2d1fe5ca4ea75fbfc59512e35c5c0d421c0c6e1b4677dd92f7eaf66c896` |
| `migrate` | `sha256:881f624c1cd0baca419b18da2ee67e691234a6fb22bb1d93977b73e230d47437` |
| `web` | `sha256:e9941bc425a9ad7e5079a3aaeb465cf01662e5dcf67a3196e08230cd6f3f0641` |

Satu kekhawatiran sebelum merge terjawab sendiri: setelan repo ini `default_workflow_permissions = read`, dan sempat meragukan apakah `packages: write` di badan workflow cukup untuk masuk GHCR. **Cukup** — blok `permissions:` tingkat workflow memang menaikkan, bukan sekadar mempersempit.

### Yang ditemukan justru sesudah citranya terbit

🔴 **`docker login` yang berhasil tidak membuktikan apa pun tentang menarik.** Login ke `ghcr.io` dengan token `gh` berhasil; `docker pull` citra yang baru saja terbit membalas **403**. Token OAuth `gh` bercakupan `gist`, `read:org`, `repo`, `workflow` — **`read:packages` tidak ada**, dan ketiadaannya baru terasa satu langkah kemudian.

Ini bukan soal mesin ini saja: **menarik citra adalah langkah pertama platform hosting mana pun**, dan `PENYEBARAN.md` — runbook yang ditulis sebelum satu citra pun pernah terbit — tidak menyebutnya sama sekali. Sekarang ada bagian **Kredensial untuk MENARIK citra**, lengkap dengan galat 403 yang sesungguhnya dan tiga cara memberi cakupan itu.

### Dua jebakan GitHub yang memakan waktu

⚠️ **`gh pr merge --delete-branch` MENUTUP PR yang bertumpuk di atasnya, bukan mengarahkannya ulang.** Menghapus `fase-1/redis-opsional-di-produksi` sesudah #29 mendarat membuat #31 ikut **CLOSED**. Lalu kebuntuan: tidak bisa dibuka lagi karena cabang base-nya sudah tidak ada, dan tidak bisa dipindah base-nya karena PR-nya tertutup (*"Cannot change the base branch of a closed pull request"*).

Jalan keluarnya **mendorong balik cabang yang baru saja dihapus** (tip-nya masih ada di klona lokal), buka lagi PR-nya, pindahkan base ke `main`, baru hapus cabangnya. Untuk PR bertumpuk berikutnya: **jangan `--delete-branch` pada PR bawah** — arahkan ulang PR atasnya lebih dulu.

⚠️ **`Closes #30` di JUDUL PR tidak menutup apa pun**, dan ketahuannya cuma karena `closingIssuesReferences` #31 kosong sementara #29 berisi `[28]`. Dua sebab menumpuk, dan hanya satu yang bisa saya buktikan sendiri:

1. **Terbukti di sini:** selama base PR bukan cabang default, daftar itu **tetap kosong walaupun badan PR sudah memuat `Closes #30`**. Ia baru muncul sesudah base dipindah ke `main`.
2. **Dokumentasi, tidak saya uji terpisah:** kata kuncinya dibaca dari **badan** PR. Badan #29 memang membukanya dengan `Closes #28.`, badan #31 tidak punya sama sekali — judulnya saja yang menulisnya, dan itu tidak pernah cukup.

Keduanya diperbaiki bersamaan, jadi #30 tertutup otomatis saat #31 mendarat. Ini varian ketiga dari jebakan yang sama dengan Sesi 7 (*"Menutup #26"* berbahasa Indonesia): **selalu periksa `closingIssuesReferences` sebelum merge, jangan percaya kalimatnya.**

### ADR hosting akhirnya ditulis — dan harga bukan yang memutuskan

Pemilik memilih agar keputusan platform **disiapkan**, bukan diambil. Hasilnya [ADR-017](adr/ADR-017-platform-hosting.md), satu-satunya ADR berstatus **Diusulkan** ([Issue #33](../../issues/33)).

Pemeriksaannya dimulai dari harga dan berakhir di tempat lain. Empat syarat diturunkan dari apa yang repo ini sudah putuskan, dan **satu di antaranya menggugurkan kandidat tanpa menyentuh angka dolar**: `PENYEBARAN.md` menuntut **citra `migrate` yang BERBEDA** berjalan sampai selesai sebelum `api` menyala, sementara *pre-deploy command* milik Render, Railway, dan `release_command` Fly.io **semuanya menjalankan perintah di dalam citra layanan itu sendiri**. Hanya `initContainers` Azure Container Apps yang memetakan langsung ke bentuk yang sudah dibangun. Fly.io gugur pada tiga syarat sekaligus — termasuk tidak punya dukungan registry privat luar, yang akan membatalkan keputusan "terbitkan ke GHCR dulu" yang sudah diambil.

🔑 **Temuan yang paling layak diingat: bentuk citra `migrate` dan pilihan platform ternyata TERIKAT, dan bentuk citranya sudah diputuskan lebih dulu tanpa tahu ia akan menyeleksi platform.** Memisahkan bundel jadi citra sendiri benar untuk `docker-compose`, dan diam-diam mempersempit pilihan PaaS jadi satu.

Usulnya **Render**, dengan bundel migrasi pindah ke citra `api` — di sana ia bisa *framework-dependent* dan jauh lebih kecil, karena `aspnet:10.0` sudah punya runtime yang membuat `--self-contained` diperlukan di citra `migrator`. Alasan utamanya bukan Render paling murah, tapi **harganya tetap** di pagu yang belum pernah diuji tagihan sungguhan.

⚠️ Kolom **kemampuan** di ADR itu seluruhnya dari dokumentasi vendor; kolom **harga** dari artikel banding pihak ketiga dan ditandai wajib dicek ulang. Bagian "angka mana yang diverifikasi" ditulis eksplisit supaya tidak ada yang mengutip keduanya seolah sekualitas.

### Klaim basi yang ikut disapu

- **ADR-016 butir 4** masih menulis ToS OpenAI *"belum pernah dibaca siapa pun"* — sudah dibaca 7 September. Diperbaiki dengan penunjuk ke `AUDIT-KELAYAKAN.md`, bukan salinan keempat, plus temuan yang mengubah ADR itu sendiri: **membacanya tidak menutup #17**, karena ToS OpenAI mengatur *Services* dan bukan penerbitan ulang isi editorial.
- **Blok "Yang menunggu sesi berikutnya"** yang menggantung di tengah berkas ini masih mendaftar PR #23 dan #25 sebagai menunggu review — keduanya mendarat empat sesi lalu.

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
| **Review PR [#23](../../pull/23)** | Kerangka Fase 1. 3/3 gerbang hijau. |
| **Review PR [#25](../../pull/25)** | Bidang + kematangan konten, ditumpuk di atas #23. 6/6 gerbang hijau. |
| **Issue [#17](../../issues/17)** | Baca ToS OpenAI dan Microsoft di peramban sungguhan. Tidak bisa diwakilkan ke agen mana pun — halaman OpenAI membalas 403 ke pengambil otomatis. |

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
2. **Template Versi B menang bukan karena lebih ringkas, tapi karena melepas kelengkapan halaman dari pipeline berita.** Versi A mewajibkan "Berita terbaru" dan "Paper terbaru" di tiap halaman — artinya satu blocker hukum ([#17](../../issues/17)) akan membuat 82 halaman berstatus "belum lengkap" selamanya.
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

[Issue #17](../../issues/17) dibiarkan terbuka. Ketentuan layanan OpenAI membalas **403** ke pengambil otomatis — sampai sekarang belum dibaca siapa pun, termasuk saya. Yang bisa dikerjakan hanya **memundurkan blocker-nya**: V1 hanya menerbitkan ulang arXiv (metadata CC0, satu-satunya yang jelas boleh), dan sumber lain hanya ditampilkan judul + tautan + sumber. Dengan itu #17 berhenti menghalangi V1 dan tetap menghalangi yang lain — sebagaimana mestinya.

Ini pengurang paparan, bukan nasihat hukum.

### Aturan pemilik tetap dipegang

`KERANGKA.md` **tidak diubah satu kalimat pun**. Yang ditambahkan hanya blok penunjuk di kepalanya: kalau ia berbeda dengan `KEPUTUSAN.md`, yang berlaku `KEPUTUSAN.md`. Berikut tabel lima tempat yang paling sering salah dibaca.

### Hasil

8 ADR baru (009–016), 5 ADR lama dinaikkan dari "ditunda" jadi keputusan, `KEPUTUSAN.md`, `RENCANA-V1.md`, README dan ADR README diperbarui. **21 dari 22 issue ditutup**; butir E10 — satu-satunya dari 30 butir yang tidak pernah punya issue — ikut terjawab (ia bagian `/future`, bukan bidang ke-15).

---

## 2026-09-04 — Sesi 4: CI dari merah jadi hijau

Permintaan pemilik: *"lanjutkan kerjakan proyek."* Dari 23 issue terbuka, hanya [#24](../../issues/24) yang bisa dikerjakan tanpa keputusan pemilik lebih dulu — 22 sisanya memang meminta pemilik memutuskan. Jadi itu yang dikerjakan.

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

**Satu-satunya keputusan Fase 0 yang praktis ikut terambil: `.NET 10`** (Issue [#12](../../issues/12)) — butir yang tenggatnya nyata dan vonis auditnya berkeyakinan tinggi.

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
- **Ketentuan layanan OpenAI dan Microsoft belum dibaca siapa pun** — halamannya memblokir bot. Ini pekerjaan manusia dan harus selesai sebelum tayang (Issue [#17](../../issues/17)).

### Keadaan akhir sesi

Repositori dibuat dan didorong ke GitHub sebagai **privat**. Seluruh keputusan yang menunggu dipindahkan menjadi GitHub Issues berlabel dan bermilestone.

**Belum ada satu baris kode pun.** Itu disengaja — milestone *Fase 0 — Kunci Kerangka* harus selesai lebih dulu.
