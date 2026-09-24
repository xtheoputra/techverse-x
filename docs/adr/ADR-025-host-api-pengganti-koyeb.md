# ADR-025 — Host API pengganti Koyeb: belum dipilih, diuji berurutan dengan syarat lulus tertulis

**Status:** Diusulkan. **Host API-nya BELUM dipilih.** Yang diusulkan adalah
urutan uji dan syarat lulusnya; pilihannya diambil pemilik sesudah uji dan dicatat
sebagai Pembaruan di sini. Yang **bukan** usulan melainkan temuan: kaki `api`
[ADR-019](ADR-019-hosting-gratis-tanpa-kartu.md) (Koyeb Free) sudah gugur untuk
akun baru. Kaki web (Vercel Hobby) dan PostgreSQL (Neon Free) tetap, dengan
koreksi di bawah. Issue [#39](https://github.com/xtheoputra/techverse-x/issues/39).
**Tanggal:** 2026-09-17 (riset 2026-09-16 dan 17)

## Konteks

### Koyeb Free tidak ada lagi untuk akun baru — tujuh bulan sebelum ADR-019 ditulis

Pengumuman Koyeb bertanggal **17 Februari 2026**
([*Koyeb is Joining Mistral AI…*](https://www.koyeb.com/blog/koyeb-is-joining-mistral-ai-to-build-the-future-of-ai-infrastructure)):

> *"The Starter plan will soon be removed and new users will instead need to
> subscribe to the Pro, Scale, or Enterprise plan."*
>
> *"You'll need a valid payment method and must subscribe to one of our paid
> plans to get started."*
>
> *"If you have an existing organization on an existing plan, nothing will change
> for you."*

[Halaman harganya](https://www.koyeb.com/pricing), diambil ulang 2026-09-17:
**Pro $29/bulan + pemakaian**, Scale $299, Enterprise. Tidak ada paket gratis untuk
layanan web. Temuan ini sudah diposting ke [#39](https://github.com/xtheoputra/techverse-x/issues/39) pada
2026-09-16, berikut larangan mendaftar dan memasukkan kartu.

ADR-019 ditulis 8 September 2026, dan tabel surveinya mencatat Koyeb *"biasanya
tidak"* menuntut kartu. Klaim itu basi sejak hari ditulis.

### Yang tidak berubah

- **Syarat pemilik:** *"jangan render, akun gratis pilihannya pada apa"* — gratis
  permanen, tanpa kartu, dan bukan Render.
- **Beban kerjanya:** citra .NET 10 dari `apps/api/Dockerfile` dengan konteks build
  akar repo, port `8080`, `ConnectionStrings__Postgres` ke Neon (keluar lewat TCP
  5432), `/health/live` dan `/health/ready`, **hanya membaca** di produksi
  ([ADR-020](ADR-020-permukaan-tulis-api.md)), dan `/app/efbundle` wajib selesai
  sebelum API menyala. Web di Vercel meng-cache panggilan API lima menit.
- 🔴 **GitHub Actions menolak menjalankan job** karena urusan tagihan akun
  ([#57](https://github.com/xtheoputra/techverse-x/issues/57)): `rilis-citra.yml` dan `migrasi-produksi.yml`
  tidak bisa jalan. Host yang hanya
  bisa menarik citra GHCR yang sudah terbit terkunci di SHA lama sampai itu
  selesai.

### Cara riset ini dilakukan, dan kenapa tidak seperti survei ADR-019

Tabel survei ADR-019 ternyata keliru di **dua** baris sejak hari ditulis: Koyeb
(gratisnya sudah hilang) dan Railway (gratisnya sudah kembali — lihat di bawah).
Kekeliruannya berlawanan arah, dan tak satu pun tertangkap dengan membaca
ringkasan. Jadi kali ini:

1. **Empat penyapuan, 2026-09-16:** 21 PaaS kontainer; awan besar dan jalan tak
   lazim (13 pilihan plus dua arsitektur tanpa host API); pemeriksaan ulang kaki
   Neon dan Vercel; lalu peringkat tiga teratas.
2. **Tiap kandidat teratas diserahkan ke pemeriksa terpisah yang tugasnya
   MEMBANTAH**, bukan mengonfirmasi (2026-09-17).
3. **Kutipan penentu diambil ulang dari halaman yang hidup dan dicocokkan kata per
   kata** (2026-09-17): tiga belas halaman Railway, Vercel, dan Koyeb, plus bundel
   dasbor Back4App. Semuanya cocok.

⚠️ **Tidak ada akun yang dibuat dan tidak ada yang disebarkan.** Segala yang
tertulis di bawah tentang perilaku platform berasal dari dokumentasi vendor,
bukan pengukuran. Itulah sebabnya keputusan ADR ini berupa **urutan uji**, bukan
nama vendor.

## Hasil pemeriksa: satu terbantah, dua bertahan bersyarat

| Kandidat | Hasil | Penentunya |
|---|---|---|
| **Back4App Containers** | ❌ **terbantah** | Kode dasbornya sendiri (bundel yang dilayani 2026-09-17, `Last-Modified` 11 Sep 2026): URL publik app gratis *"is temporary and will be live for 60 minutes"*, dan server bisa menuntut validasi kartu (`CreditCardValidationRequiredError`, tagihan $1 yang dikembalikan). Halaman harganya menulis *"no credit card required"*. |
| **Railway Free** | ✅ bertahan, **bersyarat** | Gratis dan tanpa kartu benar menurut sumber primer 2026. Syaratnya verifikasi akun otomatis. |
| **Vercel Functions dari citra kontainer (Hobby)** | ✅ bertahan, **Beta** | *"Container Images are available in Beta on all plans"* — dan Vercel sendiri: *"Vercel does not recommend using Beta products in a full production environment."* |

### Railway Free

**Gratis dan tanpa kartu: terbukti.** [Halaman harga](https://railway.com/pricing):
*"No credit card required"*, uji coba 30 hari berkredit $5 *"then $1 per month"*,
*"0.5 GB RAM"* per layanan. [Dokumen uji coba](https://docs.railway.com/pricing/free-trial):
*"After 30 days passes or $5 is spent, the free trial reverts to the Free plan,
which provides $1 of free credit per month."* Paket ini diluncurkan ulang
**29 Agustus 2025** (changelog Railway: *"The. Free. Plan. Is. Back."*) — lebih
dari setahun sebelum ADR-019 menulis Railway *"tak ada tier gratis untuk
pendaftar baru"*.

**🔴 Syaratnya: verifikasi otomatis, dan jalan keluarnya kartu.** Dokumen yang sama:

> *"Your verification status depends on a number of factors, including the age
> and activity of your GitHub account."*
>
> *"…services on the Limited Trial have restricted outbound network access and
> only a limited set of ports are available."*
>
> *"This is a fully automated process, and Railway does not respond to requests
> for verification. If your account is not verified, you can upgrade to the Hobby
> plan…"*

Akun GitHub pemilik dibuat **15 Juni 2026**: tiga bulan, nol pengikut
(`api.github.com`). Di forum Railway (Maret 2024), staf menjawab pemilik akun
berumur 3–4 bulan: *"your github account is too new or doesn't have enough
activity… the only way around this would be to upgrade to hobby"*. Daftar port
Limited Trial tidak dipublikasikan, sementara Npgsql ke Neon butuh 5432. **Jadi
kemungkinan gagal di menit pertama itu nyata**, dan jalan keluar yang ditawarkan
vendor — kartu atau paket berbayar — justru yang dilarang syarat pemilik.

**Cocok dengan repo ini, lebih dari yang diduga:**

- Citra GHCR privat **tidak** bisa ditarik: *"Private registry credentials are
  available on the Pro plan"*. Railway membangun sendiri dari repo.
- `apps/api/Dockerfile` dipakai **apa adanya**. Variabel `RAILWAY_DOCKERFILE_PATH`
  memilih Dockerfile di subdirektori, sementara direktori akar layanan tetap `/` —
  konteks build yang sama dengan CI. Tanpa perubahan repo.
- *Pre-deploy command* berjalan di *"separate container"*, dan kalau gagal ia
  *"will not be retried"* dan penyebarannya tidak dilanjutkan. **Itulah bentuk yang
  membuat `/app/efbundle` dimasukkan ke citra `api` sejak
  [ADR-017](ADR-017-platform-hosting.md)** — komentar di `apps/api/Dockerfile`
  menyebut Railway namanya. Urutan `migrate → api` kembali ditegakkan platform,
  tanpa Actions.
- ⚠️ **Tidak terdokumentasi:** apakah perintah itu **menggantikan** `ENTRYPOINT`
  citra atau ditempelkan sebagai argumen. `ENTRYPOINT` citra `api` adalah
  `["dotnet", "TechVerseX.Api.dll"]`; kalau perintahnya ditempelkan, yang berjalan
  adalah API, dan ia tidak pernah selesai. Dokumennya sendiri memperingatkan:
  *"By default a pre-deploy command has no time limit"* — penyebaran akan
  menggantung, bukan gagal. Karena itu uji di bawah memasang batas waktu.

**Batas yang membentuk situsnya:**

| Batas | Sumber | Artinya di sini |
|---|---|---|
| **Tidur wajib** di Free | pesan galat platform *"Free plan deployments must be serverless"*, dikonfirmasi staf di forum — **tidak** ada di dokumen | API tidur 5–10 menit sesudah lalu lintas keluarnya yang terakhir |
| *"The first request sent to a slept service may return a 502 Bad Gateway response."* | [dokumen](https://docs.railway.com/deployments/serverless) | `getJson` di web tidak mengulang. Yang terlindungi cache hanya yang pernah diambil; `/cari?q=` dengan kata kunci baru bisa menjawab *"belum bisa diambil"* |
| Deploy free-tier **ditolak** pukul 8 pagi–8 malam waktu setempat region | [dokumen](https://docs.railway.com/deployments/reference) | Dalam WIB, per September: Singapura 07–19, US West 22–10, US East 19–07, EU West 13–01. Jamnya bergeser begitu musim panas di sana berakhir |
| *"Railway may temporarily suspend resource allocation, including builds, to Free…"* | dokumen yang sama | penyebaran bisa tertahan tanpa sebab di repo ini |
| *"Global regions — Trial only, unavailable on the free plan"* | halaman harga | region layanan Free **baru diketahui sesudah dicoba** |
| Kredit $1/bulan; memori $10 per GB-bulan | halaman harga, dokumen paket | aritmetika: 150 MB menyala sebulan penuh ≈ $1,5. **Hanya muat kalau API tidur sebagian besar hari** |
| Uji coba habis → penyebaran dijeda sampai pindah ke Free, yang menurut laporan pengguna harus diklik | forum | hari ke-30 adalah gangguan kalau belum pindah |
| 1 proyek, 3 layanan, log 3 hari, domain kustom 0 (domain Railway tetap ada), build maks 10 menit | halaman harga | cukup untuk satu API |

### Vercel Functions dari citra kontainer

**Yang terbukti:** fiturnya tersedia di semua paket termasuk Hobby, sebagai *Beta*
(kutipan di atas). Port diatur lewat `PORT`. Instans yang *"not receiving any
traffic for 5 minutes in production environments … will automatically scale
down"*. Hobby tanpa kartu — **disimpulkan** dari alur pendaftaran; tidak ada
kalimat Vercel yang menyatakannya langsung.

**Yang membuatnya kedua, bukan pertama:**

- 🔴 **Kuotanya dibagi dengan web.** Active CPU 4 jam, Provisioned Memory 360
  GB-jam, satu juta invocation — untuk kedua proyek bersama. Lewat batas:
  *"you will have to wait until 30 days have passed before you can use the feature
  again"*. **Kuota yang jebol mematikan web dan API bersama-sama selama 30 hari.**
  Menurut pemeriksa, dokumen Vercel sendiri bertentangan soal apakah memori
  kontainer yang menganggur ditagih.
- **Butuh perubahan repo.** `Dockerfile.vercel` harus ada di akar proyek, dan
  pembangunnya memakai direktori Dockerfile itu sebagai konteks build (dibaca
  pemeriksa dari kode `@vercel/container`). Jadinya proyek Vercel kedua dengan
  *Root Directory* akar repo dan **salinan** `apps/api/Dockerfile` yang bisa
  menyimpang dari aslinya.
- **Tidak ada *pre-deploy*.** Migrasi dijalankan dari luar — dan karena Actions
  mati, dari mesin pemilik.
- Laporan pihak ketiga, bukan dokumen: *cold boot* 18–29 detik; satu proyek
  kontainer Hobby membalas 500 di semua rute sesudah build ulang commit yang sama,
  tanpa balasan staf.

**Yang membuatnya tetap layak:** tidak menambah vendor maupun akun (Vercel memang
dibutuhkan web), tidak ada lotere verifikasi, 2 GB / 1 vCPU, dan region Singapura
ada untuk fungsi biasa.

### Yang gugur sebelum sampai ke pemeriksa

Dari penyapuan 2026-09-16 ke halaman vendor. **Tidak diulang hari ini**, kecuali
Koyeb.

| Sebab gugur | Host |
|---|---|
| **Menuntut kartu** | Google Cloud Run, Azure Container Apps dan App Service (akun biasa), AWS Lambda, Oracle Always Free, IBM Code Engine, Northflank (*"regardless of plan selection"*), Heroku, DigitalOcean App Platform, Fly.io |
| **Tidak ada komputasi gratis** | Koyeb (Pro $29), Hugging Face Docker Spaces (PRO — dan keluar hanya lewat port 80, 443, 8080, jadi Postgres terblokir), Cloudflare Containers (Workers Paid $5), Zeabur (klaster gratis ditutup Maret–April 2026), Sevalla · Clever Cloud · Choreo (uji coba saja), Qovery (awan milik sendiri) |
| **Tidak menerima pelanggan baru, atau hilang** | AWS App Runner (*"no longer open to new customers"*), ClawCloud Run (pendaftaran ditutup 23 April 2026, produk mati 11 Mei — sumber sekunder; domainnya tidak lagi menjawab) |
| **Tidak bisa menjalankan bebannya** | Replit (app gratis turun sesudah 30 hari), Leapcell (tanpa Docker maupun .NET), Alwaysdata (kartu saat daftar; Docker hanya di awan privat), Red Hat Developer Sandbox (pod dihapus sesudah 12 jam), GitHub Codespaces (ketentuannya melarang hosting produksi) |
| **Tidak dilanjutkan** | SnapDeploy — vendor sangat kecil, halamannya sendiri bertentangan soal batas deploy, dan tarikan GHCR privat tidak terkonfirmasi. Nasib ClawCloud Run adalah alasannya. |
| **Ditolak pemilik** | **Render** — di atas kertas justru yang paling lengkap: menarik citra GHCR privat per SHA, region Singapura, 512 MB, 750 jam per bulan, tanpa kartu. Dicatat sebagai fakta, **bukan** usulan. |

## Keputusan yang diusulkan

### 1. Kaki `api` ADR-019 gugur untuk akun baru

Kecuali satu cabang yang hanya pemilik bisa jawab: **kalau sudah ada organisasi
Koyeb yang dibuat sebelum 17 Februari 2026**, *"nothing will change"* — ADR-019
berlaku apa adanya, dan ADR ini tidak dibutuhkan.

### 2. Vendornya belum dipilih; yang dipilih urutan uji dan syarat lulusnya

Setiap hal yang menentukan — status verifikasi, 5432 dari dalam platform, region
paket gratis, *cold start* .NET, pemakaian memori dan kuota yang sungguhan —
**hanya terlihat dari dalam akun.** Memilih vendor dari dokumentasi adalah persis
cara ADR-019 memilih Koyeb. Dan dua kandidat yang bertahan gagal dengan cara yang
berbeda, jadi yang perlu diputuskan adalah urutannya.

### 3. Railway Free diuji lebih dulu, Vercel kedua

Empat alasan, dari yang paling berat:

1. **Kegagalannya tinggal di satu kaki.** Kalau kredit Railway habis atau
   layanannya dijeda, web tetap menyajikan halaman yang sudah pernah diambil dari
   cache — perilaku yang dibuktikan ADR-019 dengan API dimatikan total. Kalau kuota
   Hobby jebol, web dan API berhenti **bersama** selama 30 hari.
2. **Ketidakpastian terbesarnya terjawab dalam hitungan menit, sebelum satu baris
   repo berubah.** Status verifikasi terlihat saat mendaftar, dan satu penyebaran
   menjawab apakah 5432 ke Neon terbuka. Risiko terbesar Vercel — kuota bersama
   yang jebol — **tidak bisa diuji sama sekali**; ia baru terlihat di bawah lalu
   lintas sungguhan.
3. **Tanpa perubahan repo, dan cocok dengan bentuk migrasi yang sudah dibangun.**
   `RAILWAY_DOCKERFILE_PATH` memakai `apps/api/Dockerfile` apa adanya, dan
   *pre-deploy* adalah kait yang untuknya `/app/efbundle` ditaruh di citra `api`.
   Vercel menuntut Dockerfile kedua di akar repo dan migrasi tangan dari luar.
4. **Paket yang umum lawan fitur Beta.** Batas Railway Free keras, tapi tertulis.
   Vercel sendiri tidak menyarankan Beta untuk produksi.

### 4. Uji Railway

🔴 **Aturan di setiap langkah: kalau layar mana pun meminta kartu atau paket
berbayar, BERHENTI.** Itu hasil uji — gagal — bukan rintangan untuk dilewati.

| # | Langkah | Lulus kalau |
|---|---|---|
| R1 | Daftar Railway **dengan akun GitHub**, lalu buka `railway.com/verify` | tertulis **Full Trial**. *Limited Trial* → lanjut ke R3 hanya untuk melihat apakah 5432 diizinkan; kalau tidak, **gagal** |
| R2 | Buat proyek Neon **untuk uji**, region mana saja. Salin string koneksi dengan sakelar *Connection pooling* **MATI** (string *direct*) | — |
| R3 | Service baru dari repo. Variabel: `RAILWAY_DOCKERFILE_PATH=/apps/api/Dockerfile`, `PORT=8080`, `ConnectionStrings__Postgres` = string direct. **Tidak ada yang lain** — `ConnectionStrings__Redis` dan `Editorial__WritesEnabled` tetap kosong. *Pre-deploy command* `/app/efbundle` dengan **Pre-deploy Timeout 300 detik**. *Healthcheck path* `/health/ready` | build selesai; *pre-deploy* keluar 0 dan Neon berisi **9 tabel, 14 bidang**; `/health/ready` **200**, badannya hanya menyebut `postgres`. Catat lama build: batas Free 10 menit, uji coba 20 |
| R4 | Tiga `curl` di [`PENYEBARAN.md`](../PENYEBARAN.md#variabel-lingkungan) terhadap domain Railway | **405**, **200**, dan pencarian `quantum` memuat `quantum-computing` |
| R5 | **Pindah ke paket Free sekarang, jangan menunggu hari ke-30.** Nyalakan *serverless*, sebarkan ulang | Free aktif **tanpa kartu**, dan penyebaran ulang dari repo privat berhasil. Catat **region** tempat layanannya berakhir, dan apakah penyebarannya kena larangan jam sibuk |
| R6 | Biarkan menganggur ≥ 15 menit, lalu minta `/api/v1/fields`. Ulangi sepuluh kali sepanjang hari | selalu bangun. Catat status dan lama permintaan pertama. **Satu 502 saja sudah berarti** `getJson` wajib mengulang sekali sebelum tayang |
| R7 | Tujuh hari di Free, dengan web ([#40](https://github.com/xtheoputra/techverse-x/issues/40)) menunjuk ke layanan ini | proyeksi tagihan sebulan **≤ $0,70** — sisa 30% untuk lalu lintas yang tumbuh |

**Kenapa R5 tidak ditunda:** batas uji coba lebih longgar daripada Free — RAM 1 GB
lawan 0,5 GB, build 20 menit lawan 10, pilihan region. Lulus di uji coba **belum**
berarti lulus di Free. Dan kalau pindah paketnya ternyata menuntut kartu, itu harus
ketahuan di hari pertama, bukan di hari ke-30 saat situsnya sudah tayang.

**Region dan Neon:** proyek Neon yang sungguhan ([#38](https://github.com/xtheoputra/techverse-x/issues/38))
dibuat **sesudah R5**, di region AWS terdekat dengan layanan Railway — region Neon
tidak bisa diubah sesudah proyeknya ada. Proyek uji dari R2 boleh langsung dipakai
kalau kebetulan regionnya sudah benar.

**Lulus R1–R6 cukup untuk tayang.** R7 berjalan di minggu pertama sesudahnya;
kalau R7 gagal, uji Vercel dimulai selagi situsnya tetap tayang di Railway.

**Satu keputusan yang ikut terbawa kalau Railway dipilih: migrasi berjalan di
setiap penyebaran.** ADR-019 sengaja membuat migrasi tombol manual
(`workflow_dispatch`) dengan alasan *"gerbang yang dilewati manusia lebih murah
daripada migrasi salah yang mendarat sendiri"*. Dengan *pre-deploy*, gerbang
manusianya pindah ke **merge ke `main`** — dan yang me-merge memang hanya pemilik.
Diusulkan diterima: `efbundle` hanya menerapkan migrasi yang belum tercatat, dan
jalur manualnya bergantung pada Actions yang sedang mati. Syaratnya dua:
penyebaran otomatis hanya dari `main`, dan *watch paths* layanan mencakup persis
yang disalin `apps/api/Dockerfile` — supaya commit dokumen tidak membangun ulang,
tidak memigrasi, dan tidak menabrak larangan jam sibuk.

### 5. Uji Vercel — hanya kalau Railway gagal

Perubahan repo yang dibutuhkannya (`Dockerfile.vercel` di akar) **sengaja belum
dibuat**: ia hanya berguna kalau Railway gagal.

| # | Langkah | Lulus kalau |
|---|---|---|
| V1 | Proyek Vercel **kedua**, *Root Directory* akar repo, `Dockerfile.vercel` di akar. `PORT=8080`, `ConnectionStrings__Postgres` string direct. Penyebaran otomatis dari Git dimatikan, supaya urutan `migrate → api` tetap dijaga tangan | build hijau di Hobby; `/health/ready` 200 hanya menyebut `postgres`; tiga `curl` permukaan tulis benar. ⚠️ Menurut pemeriksa, variabel lingkungan ikut diteruskan ke build sebagai `--build-arg` — pastikan log build tidak mencetak string koneksinya |
| V2 | Sebelum tiap penyebaran API: bangun citra dari commit yang sama di mesin pemilik, lalu `docker run --entrypoint /app/efbundle` dengan string direct | keluar 0 |
| V3 | Menganggur ≥ 10 menit, lalu satu permintaan; sepuluh kali | nol galat, dan **tidak satu pun lebih dari 15 detik** — separuh batas fetch web |
| V4 | Tujuh hari; baca pemakaian **kedua** proyek | proyeksi sebulan **≤ 50%** tiap jatah Hobby, web dan API dijumlah. Ambangnya lebih ketat daripada R7 karena yang dipertaruhkan seluruh situs |
| V5 | Region tempat fungsi kontainer berjalan | tercatat — Neon mengikutinya |

### 6. Kalau keduanya gagal

Tidak ada lagi host yang gratis, tanpa kartu, dan menjalankan API ini **tanpa
mengorbankan sesuatu yang lain**. Yang tersisa semuanya keputusan pemilik; tidak
ada yang diusulkan di sini.

| Jalan | Yang dikorbankan |
|---|---|
| Mesin pemilik di belakang **Cloudflare Tunnel** | ketersediaan setara PC rumah. *Named tunnel* butuh domain (~$1–2/bulan, [ADR-016](ADR-016-pagu-biaya.md)) dan orientasi Zero Trust yang mungkin meminta data pembayaran; *quick tunnel* ditulis Cloudflare sebagai *"for testing and development only"* |
| **Tanpa host API saat berjalan**: halaman dibangun dari API lokal saat build | ADR tersendiri yang membalik [ADR-001](ADR-001-nextjs.md) #2 dan mekanisme cache ADR-019; `/cari` hilang dari produksi; AI Mentor (Bulan 5) tetap butuh API yang berjalan |
| **Melonggarkan "tanpa kartu"** | Railway Hobby $5/bulan (termasuk $5 pemakaian) atau Koyeb Pro $29/bulan + pemakaian — keduanya di dalam pagu $60 ADR-016 |
| **Azure for Students Starter** | hanya kalau pemilik mahasiswa penuh waktu yang bisa diverifikasi; pemakaiannya dibatasi untuk pendidikan; belum diperiksa ulang |
| **Menimbang ulang Render** | keputusan pemilik yang sudah diambil |

❌ **Yang tidak ditawarkan sama sekali: web membaca Neon langsung.** Itu bukan
pilihan hosting, melainkan membalik KERANGKA 4.9 dan ADR-001 — web berhenti
menjadi klien API — sambil menyalin logika domain (`MissingSections`, peringkat
pencarian [ADR-022](ADR-022-pencarian-teks-penuh.md)) ke dua tempat.

## Koreksi untuk dua kaki yang tetap

Diperiksa ulang ke sumber primer 2026-09-17, kecuali yang ditandai. **Keduanya masih
gratis dan tanpa kartu** — tapi ADR-019 dan langkah-langkahnya melewatkan ini.

**Neon Free**

- *"The Free plan is permanent (not a trial); no credit card required."* 100 CU-jam
  dan 0,5 GB per proyek.
- 🔴 **Migrasi wajib memakai string DIRECT.** *"Migrations must use a direct
  (non-pooled) Neon connection string; PgBouncer pooled strings are not supported
  for dotnet-ef operations."* Sakelar pooling *"is on by default for new
  projects"* — jadi mengikuti #38 apa adanya (salin string koneksinya, jangan
  dirakit ulang) menghasilkan string yang **salah** untuk `NEON_DATABASE_URL`.
- 🔴 **Region tidak bisa diubah sesudah proyek dibuat** (diperiksa 2026-09-16).
  Proyek sungguhannya dibuat sesudah region host API diketahui.
- *Scale to zero* sesudah lima menit, **tidak bisa dimatikan**. CU-jam habis →
  *"your compute is suspended until the next billing period"*. Aritmetika: 0,25 CU
  × 720 jam = 180 CU-jam, sedangkan jatahnya 100. **Apa pun yang membuat Postgres
  terjaga terus — pemantau yang memanggil `/health/ready` lebih sering dari lima
  menit sekali, atau pool koneksi yang tak pernah menganggur — menangguhkan basis
  datanya sekitar hari ke-16.** Pemantau dari luar memanggil `/health/live`.
- Riwayat pemulihan 6 jam, tanpa cadangan terjadwal di Free → `pg_dump` lewat
  string direct begitu ada topik yang masuk lewat [ADR-021](ADR-021-jalan-menuju-tinjau.md).

**Vercel Hobby**

- Masih gratis, dan tetap *"non-commercial, personal use only"*. Pedoman *fair use*
  (diperiksa 2026-09-16) mendefinisikan komersial lebih luas daripada ADR-019:
  termasuk proyek yang kodenya ditulis *"a paid employee or consultant"*.
- Lewat batas → menunggu 30 hari.
- **Log fungsi disimpan satu jam.** Pemeriksaan log di #40 harus dilakukan dalam
  satu jam sesudah kegagalannya.
- Setelan monorepo bernama *"Include source files outside of the Root Directory in
  the Build Step"*, dan *"Vercel projects created after August 27th 2020 23:50 UTC
  have this option enabled by default."* **#40 benar** — riset 2026-09-16 yang
  menyatakan sebaliknya keliru.
- "Tanpa kartu" disimpulkan dari alur pendaftaran; tidak ada kalimat Vercel yang
  menyatakannya langsung.

## Konsekuensi

- 🛑 **Bulan 1 menunggu satu langkah lagi:** uji Railway R1–R6, satu duduk, oleh
  pemilik. Urutan akun berubah dari *Neon → Koyeb → Vercel* menjadi *Railway (uji,
  dengan proyek Neon uji) → Neon sungguhan di region yang mengikutinya → Vercel*.
- ✅ **Jalur Railway tidak tertahan [#57](https://github.com/xtheoputra/techverse-x/issues/57).** Railway membangun
  lewat aplikasi GitHub-nya sendiri, dan migrasinya lewat *pre-deploy*. Jalur Koyeb
  tertahan justru karena ia hanya bisa menarik citra yang diterbitkan Actions.
- **Kalau Railway dipilih, *"pasang citra GHCR per SHA"*
  ([ADR-018](ADR-018-rilis-citra-dan-reproducibility.md)) berhenti menggambarkan
  produksi.** Railway membangun dari commit, jadi yang berjalan bukan citra yang
  dipindai Trivy dan diperiksa gerbang permukaan tulis di `rilis-citra.yml`. Yang
  menjaga apa yang benar-benar berjalan tinggal tiga `curl` sesudah menyebar — yang
  memang sudah wajib ([ADR-020](ADR-020-permukaan-tulis-api.md)). Gerbang CI —
  begitu #57 selesai — memeriksa Dockerfile yang sama, tapi bukan bit yang tayang.
- **Komentar kode dan teks workflow yang menyebut Koyeb** (`apps/web/src/lib/api.ts`,
  `apps/web/src/lib/lingkungan.ts`, `apps/web/src/app/teknologi/[slug]/page.tsx`,
  `.github/workflows/migrasi-produksi.yml`) **dibiarkan** sampai host dipilih.
  Mengubahnya sekarang berarti menulis nama host yang belum ada; ia diperbarui di
  PR penyebarannya.
- **Umur simpan ADR ini sama pendeknya dengan ADR-019.** Tahun 2026 saja sudah
  menghapus tiga tier kontainer gratis — Koyeb (Februari), Zeabur (Maret–April),
  ClawCloud Run (Mei) — dan menurut riwayat dokumennya Railway mengubah batas Free
  berkali-kali (larangan jam sibuk April, catatan 502 sesudah tidur Mei). **Kutipan
  penentu di atas diperiksa ulang di hari pendaftaran.**

## Pelajaran untuk survei berikutnya

Tabel survei ADR-019 keliru di dua baris **dengan arah berlawanan**: tier gratis
yang sudah hilang tercatat ada, dan tier gratis yang sudah kembali tercatat tidak
ada. Keduanya lolos karena satu baris tabel tidak membawa kutipan primer maupun
tanggal pengambilannya. Aturan yang dipakai ADR ini, dan diusulkan untuk survei
host berikutnya:

1. Setiap klaim "gratis" dan "tanpa kartu" membawa **kutipan vendor dan tanggal
   diambil**.
2. Kandidat teratas diserahkan ke pemeriksa yang tugasnya **membantah**.
3. Kutipan penentu dicocokkan ulang ke halaman yang **hidup** sebelum ditulis.

💡 Back4App menunjukkan kenapa butir 2 tidak bisa diganti dengan membaca halaman
harga lebih teliti: halaman pemasarannya menulis *"no credit card required"* dan
*"no time limits"*. Yang membantahnya adalah kode dasbornya sendiri.

## Cara membatalkan keputusan ini

- **Railway → host lain:** tidak ada yang khas Railway di repo — hanya variabel
  layanan di dasbornya. `apps/api/Dockerfile` tidak berubah.
- **Vercel kontainer → host lain:** hapus `Dockerfile.vercel` dan proyek keduanya.
- **Neon dan Vercel web** tidak disentuh keputusan ini.
