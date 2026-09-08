# ADR-017 — Platform hosting V1, dan kenapa bentuk citra `migrate` ikut diputuskan di sini

**Status:** 🟡 **DIUSULKAN — menunggu keputusan pemilik.** Belum menutup issue apa pun.
Pemilihan platform menyangkut akun dan uang, jadi ADR ini menyiapkan keputusannya, bukan mengambilnya.
**Tanggal:** 2026-09-08

## Konteks

[`RENCANA-V1.md`](../RENCANA-V1.md) menetapkan sasaran Bulan 1 sebagai **URL publik
yang bisa dibuka orang lain**. Per 8 September 2026 semua yang bisa dikerjakan tanpa
akun sudah selesai: ketiga citra terbit ke GHCR, dipindai, dan
[`PENYEBARAN.md`](../PENYEBARAN.md) sudah menuliskan urutan yang mengikat. Yang
menahan tinggal **platform, domain, dan sertifikat**.

[ADR-016](ADR-016-pagu-biaya.md) menyebut Azure PostgreSQL Flexible Server dan
Container Apps **hanya di dalam tabel perkiraan biaya**. Itu bukan keputusan, dan
ADR ini tidak mewarisinya sebagai keputusan.

## Anggaran yang benar-benar tersedia

Pagu ADR-016 adalah **USD 60/bulan**, alarm di $48.

| Pos | Nilai tunak | Bulan 1 |
|---|---|---|
| Pagu keras OpenAI | $20 | **$0** — belum ada satu baris kode AI pun |
| Domain | ~$1,5 | ~$1,5 |
| **Sisa untuk infrastruktur** | **≈ $38** | ≈ $58 |

⚠️ **Yang mengikat adalah kolom "nilai tunak", bukan kolom "Bulan 1".** Memilih
platform yang muat di $58 lalu kehabisan ruang begitu AI Mentor menyala berarti
pindah platform di tengah jalan — pekerjaan yang tidak ada di rencana enam bulan.

## Empat syarat — dan dua di antaranya menggugurkan kandidat tanpa menyentuh harga

Ini bagian terpenting ADR ini. Pemeriksaan ini dimulai dari harga, lalu **harga
ternyata bukan penyeleksi yang paling tajam.**

### Syarat 1 — bisa menarik citra privat dari GHCR

Sudah dibuktikan 8 September, bukan diduga: `docker login ghcr.io` **berhasil**,
lalu `docker pull` membalas **403** karena tokennya tidak bercakupan
`read:packages`. Platform yang tidak punya tempat menyimpan kredensial registry
luar akan berhenti di langkah pertama — dan gejalanya menyamar jadi "login-nya
sudah benar kok".

### Syarat 2 — menjalankan citra `migrate` YANG BERBEDA sampai selesai sebelum `api` menyala

🔴 **Inilah syarat yang mengubah bentuk keputusan.**

`PENYEBARAN.md` menuntut `postgres → migrate → api → web`, dan
`docker-compose.prod.yml` menegakkannya dengan
`condition: service_completed_successfully`. Yang dijalankan bukan sebuah
*perintah*, melainkan **citra yang berbeda** — bundel migrasi EF yang sengaja
dibangun tanpa SDK dan tanpa kode sumber.

Hampir semua PaaS menyebut fasilitas ini "pre-deploy command" atau
"release command", dan **hampir semuanya menjalankan perintah itu di dalam citra
LAYANAN ITU SENDIRI**, bukan citra lain:

| Platform | Kata dokumentasinya |
|---|---|
| **Railway** | Pre-deploy jalan di container terpisah tapi **image yang sama**; "dependencies needed for the command must be installed in the application image" |
| **Render** | Pre-deploy jalan di instance terpisah; deploy ditolak kalau `imgURL`-nya berbeda dari citra layanan **selain pada tag/digest** |
| **Fly.io** | `release_command` jalan "in a temporary Machine using **the newly built image**" |
| **Azure Container Apps** | `initContainers` punya medan `image` **sendiri**, dan "must complete successfully before the primary app container starts" |

Artinya: **hanya Azure Container Apps yang memetakan langsung ke bentuk yang
sudah dibangun dan dibuktikan.** Tiga lainnya menuntut bentuk migrasinya berubah.

### Syarat 3 — PostgreSQL terkelola, dan pgvector saat `Chunk` tiba

[ADR-005](ADR-005-qdrant.md) memilih pgvector, jadi Postgres yang dipakai V1
harus bisa `CREATE EXTENSION vector` nanti tanpa pindah basis data.
[`RENCANA-V1.md`](../RENCANA-V1.md) menaruh `Chunk` paling belakang justru karena
ini perubahan infrastruktur.

"Terkelola" di sini berarti **cadangan dan pemulihan titik-waktu dikerjakan
platform**, bukan oleh pemilik. Satu orang yang mengelola basis datanya sendiri
adalah risiko kehilangan data yang paling murah dihindari di awal.

### Syarat 4 — dua layanan HTTP + TLS

`api` (:8080) dan `web` (:3000), masing-masing dapat nama host, sertifikat
diurus platform.

## Kandidat diperiksa terhadap keempat syarat

| | GHCR privat | Citra `migrate` terpisah | Postgres terkelola + pgvector | Perkiraan $/bln |
|---|---|---|---|---|
| **Render** | ✅ terdokumentasi, menuntut `read:packages` | ❌ pre-deploy pakai citra layanan sendiri | ✅ terkelola, PITR, pgvector | **≈ $20** |
| **Azure Container Apps** | ✅ `registries` + secret | ✅ **`initContainers`, cocok persis** | ✅ Flexible Server + pgvector | **≈ $45–50** |
| **Railway** | ✅ token `read:packages` | ❌ citra yang sama | ⚠️ tanpa PITR | ≈ $10–20 |
| **Fly.io** | ❌ **tidak ada dukungan bawaan** | ❌ citra aplikasi sendiri | ❌ **Postgres-nya tidak terkelola** | ≈ $10–15 |
| **VPS + `docker-compose.prod.yml`** | ✅ `docker login` biasa | ✅ **berkasnya jalan apa adanya** | ❌ pemilik jadi DBA-nya | ≈ $5 |

**Fly.io gugur pada tiga syarat sekaligus, dan tak satu pun soal harga.** Yang
paling menentukan: tanpa dukungan registry privat luar, citra harus didorong
ulang ke `registry.fly.io` atau paketnya dibuat publik — keduanya membatalkan
keputusan "terbitkan ke GHCR dulu, pilih hosting belakangan" yang sudah diambil.

## Temuan yang membuat keputusan ini bukan sekadar memilih vendor

**Bentuk citra `migrate` dan pilihan platform ternyata terikat satu sama lain, dan
bentuk citranya sudah diputuskan lebih dulu tanpa tahu ia akan menyeleksi
platform.** Memisahkan bundel migrasi jadi citra sendiri adalah keputusan yang
benar untuk `docker-compose` — dan diam-diam mempersempit pilihan PaaS jadi satu.

Ada tiga jalan keluar, dan memilih platform berarti memilih salah satunya:

**A. Ikuti bentuknya — pakai Azure Container Apps.** Nol perubahan kode.
Bayarannya sekitar dua kali lipat per bulan, dengan tagihan variabel di pagu yang
**belum pernah diuji tagihan sungguhan**.

**B. Jalankan `migrate` sebagai tugas sekali-jalan.** Semua platform bisa
menjalankan satu container lepas. Bayarannya: **urutan yang mengikat berhenti
ditegakkan platform** dan berpindah ke tangan manusia atau ke langkah CI. Penjaga
yang dipindahkan ke manusia adalah penjaga yang suatu hari terlewat.

**C. Pindahkan bundel migrasi ke dalam citra `api`**, lalu pakai pre-deploy bawaan
platform. Ini yang diusulkan, dan lebih murah daripada kelihatannya:

- Bundel dibuat `--self-contained` **hanya karena** citra `migrator` berbasis
  `runtime-deps` yang tidak punya runtime .NET. Citra `api` berbasis
  `aspnet:10.0` **sudah punya runtime itu**, jadi di sana bundelnya bisa
  *framework-dependent* — jauh lebih kecil daripada varian self-contained.
- Yang dikhawatirkan `PENYEBARAN.md` — dua instans berlomba mengubah skema —
  **tidak muncul di sini.** Yang dilarang adalah migrasi di **startup API**;
  pre-deploy jalan **sekali per penyebaran**, bukan sekali per instans. API tetap
  tidak pernah memanggil bundelnya sendiri.
- Citra `migrate` yang berdiri sendiri **tetap diterbitkan**, karena
  `docker-compose.prod.yml`, VPS, dan Azure masih memakainya. Usul ini menambah
  jalan, bukan membuang jalan — dan karena itu ia tidak mengunci platform.

## Usulan: **Render**, dengan bundel migrasi ikut masuk citra `api`

Alasannya, berurutan dari yang paling menentukan:

1. **Harga tetap, di pagu yang belum pernah diuji.** ≈$20/bln melawan jatah $38
   menyisakan ruang untuk kejutan pertama. Azure menyisakan hampir nol, dan
   tagihannya variabel — gabungan terburuk untuk pagu yang belum pernah dibuktikan.
2. **Ketiga syarat lain terpenuhi secara bawaan**, termasuk yang sudah terbukti
   menggigit (`read:packages` disebut eksplisit di dokumentasi Render — persis
   galat 403 yang saya dapat).
3. **Satu-satunya syarat yang meleset ditutup perubahan sekali jalan yang kita
   kendalikan sendiri**, bukan biaya bulanan atau batas platform. Membayar premi
   permanen ~2× untuk menghindari satu perubahan sekali jalan adalah pertukaran
   yang salah arah.
4. **Postgres-nya terkelola berikut PITR di tier termurahnya**, dan pgvector
   tersedia — jadi `Chunk` nanti tidak menuntut pindah basis data.

**Pilihan kedua: VPS + `docker-compose.prod.yml`** (≈$5/bln). Ia satu-satunya
yang menjalankan **artefak yang sudah dibuktikan, tanpa terjemahan sama sekali**.
Ambil ini kalau pemilik lebih menghargai "jalankan persis yang sudah terbukti"
daripada "jangan jadi DBA". Yang dibayar: cadangan, pemulihan, penambalan OS, dan
TLS jadi pekerjaan pemilik, di satu mesin tanpa cadangan.

## Kalau usul ini disetujui, pekerjaan yang mengikuti

1. Tambah tahap bundel *framework-dependent* ke citra `api`; **buktikan MERAH
   dulu** — jalankan `api` terhadap basis data yang belum dimigrasi dan pastikan
   ia gagal dengan cara yang bisa dibaca.
2. Buat layanan `api` dan `web` di Render dari tag SHA, isi kredensial GHCR
   (`read:packages`), set pre-deploy `api` ke bundelnya.
3. Sediakan Render Postgres, isi `ConnectionStrings__Postgres`.
   **`ConnectionStrings__Redis` dikosongkan** — ADR-016.
4. Beli domain, arahkan, biarkan platform menerbitkan sertifikat.
5. Tambahkan bagian **Render** ke `PENYEBARAN.md` sebagai contoh konkret; bagian
   netral platform tetap jadi bagian utamanya.
6. Ukur tagihan bulan pertama terhadap pagu, lalu perbarui tabel ADR-016 dengan
   **angka sungguhan** — pagu $60 baru berarti setelah sekali diuji.

## Konsekuensi

- ⚠️ **Render tidak punya urutan antar-layanan.** `web` bisa menyala sebelum
  `api` siap, dan halaman ber-data akan gagal beberapa saat di penyebaran
  pertama sampai `api` hidup. Compose menegakkan urutan itu; Render tidak.
  Ini regresi kecil yang nyata dan harus ditulis, bukan ditemukan nanti.
- Citra `api` membesar sedikit karena membawa bundel yang tidak pernah ia
  jalankan sendiri.
- Dua tempat kini bisa menjalankan migrasi (citra `migrate` dan bundel di dalam
  `api`). Keduanya memanggil migrasi EF yang sama, tapi **dua jalan berarti dua
  yang harus tetap sejalan** — gerbang CI `citra` sudah membangun keduanya.
- Kredensial GHCR jadi rahasia yang harus dirotasi. Kalau memakai PAT
  *fine-grained*, catat tanggal kedaluwarsanya — habis masa berlaku muncul
  sebagai kegagalan tarik citra, bukan sebagai peringatan.

## Cara membatalkan keputusan ini

**Murah dibatalkan, dan itu disengaja.** Citra tetap di GHCR dan tidak memuat
apa pun yang khas Render. Untuk pindah: sediakan Postgres di tempat baru,
`pg_dump`/`pg_restore`, arahkan platform baru ke tag SHA yang sama, arahkan
ulang DNS. Yang khas Render cuma setelan dashboard-nya.

**Kalau yang dibatalkan justru bagian migrasinya** — misalnya nanti pindah ke
Azure Container Apps — bundel di dalam citra `api` cukup ditinggalkan tak
terpakai; `initContainers` kembali memakai citra `migrate` yang tetap terbit.

## Yang TIDAK diputuskan di sini

- **Domain dan namanya.** Belum dibeli.
- **Wilayah/region.** Menyangkut latensi ke pembaca Indonesia dan harus diukur,
  bukan ditebak.
- **CDN dan cache.** V1 nyaris statis; ini optimasi setelah ada trafik nyata.
- **Kapan Redis dinyalakan.** ADR-016 masih berlaku: ia harus membuktikan diri
  dengan beban yang benar-benar ada.

## Angka mana yang sudah diverifikasi, dan mana yang belum

Bagian ini ada supaya tidak ada yang mengutip angka di atas seolah semuanya
setara kualitasnya.

| Klaim | Dari mana | Kualitas |
|---|---|---|
| `docker pull` GHCR 403 tanpa `read:packages` | **dijalankan sendiri 2026-09-08** | ✅ terbukti |
| Render menuntut PAT `read:packages` untuk citra privat | dokumentasi Render | ✅ vendor |
| Render menolak `imgURL` pre-deploy yang berbeda | dokumentasi Render | ✅ vendor |
| Railway pre-deploy memakai citra yang sama | dokumentasi Railway | ✅ vendor |
| Fly.io `release_command` memakai citra aplikasi | dokumentasi Fly.io | ✅ vendor |
| ACA `initContainers` punya `image` sendiri & harus selesai lebih dulu | dokumentasi Microsoft | ✅ vendor |
| ACA gratis 180.000 vCPU-detik + 360.000 GiB-detik + 2 juta permintaan/bln | dokumentasi Azure | ✅ vendor |
| pgvector tersedia di Render Postgres | dokumentasi Render | ✅ vendor |
| **Semua angka dolar per bulan** | artikel banding pihak ketiga | 🔴 **PERKIRAAN — wajib dicek ulang di halaman harga vendor sebelum mendaftar** |
| Fly.io tanpa dukungan registry privat luar | dokumentasi + diskusi komunitas | 🟡 cukup untuk menggugurkan, periksa lagi kalau Fly dipertimbangkan serius |

🔴 **Jangan mendaftar berdasarkan kolom harga di ADR ini.** Yang layak dipercaya
dari dokumen ini adalah kolom **kemampuan** — dan kebetulan justru kolom itulah
yang menentukan hasilnya.
