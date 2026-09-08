# Penyebaran

Halaman ini menjawab satu pertanyaan: **apa yang harus disiapkan platform mana
pun supaya TechVerse X tayang.** Ia sengaja tidak menyebut platform tertentu —
pilihannya belum diambil, dan tidak ada di halaman ini yang mengunci pilihan itu.

> **Sasaran Bulan 1 menurut [`RENCANA-V1.md`](RENCANA-V1.md): URL publik yang
> bisa dibuka orang lain.** Bukan fitur.

---

## Tiga citra

Diterbitkan ke GitHub Container Registry oleh
[`rilis-citra.yml`](../.github/workflows/rilis-citra.yml) setiap kali `main`
bergerak. Citranya **privat**, mengikuti repo ini.

| Citra | Peran |
|---|---|
| `ghcr.io/<pemilik>/techverse-x/migrate` | Bundel migrasi EF. Berjalan sampai selesai lalu berhenti. |
| `ghcr.io/<pemilik>/techverse-x/api` | API. Mendengar di `8080`. |
| `ghcr.io/<pemilik>/techverse-x/web` | Next.js. Mendengar di `3000`. |

**Pasang dengan tag SHA, bukan `:main`.** Tag `:main` bergerak; men-deploy tag
yang bergerak berarti melepas kendali atas apa yang sebenarnya sedang berjalan,
dan pertanyaan "versi berapa yang tayang?" kehilangan jawabannya tepat saat ia
paling dibutuhkan.

### 🔑 SHA yang mana?

> **SHA yang disebut di ringkasan jalan _Rilis citra_ terakhir yang hijau** —
> bukan `git rev-parse HEAD`.

Keduanya sering sama, tapi **tidak selalu**, dan itu disengaja:
[ADR-018](adr/ADR-018-rilis-citra-dan-reproducibility.md) membuat commit yang
murni dokumen **tidak** membangun citra baru. Jadi kalau commit terakhir di
`main` cuma menyentuh `docs/` atau `*.md`, citra untuk SHA itu **tidak ada** —
yang berlaku citra dari commit kode terakhir sebelumnya.

Membangun ulang untuk SHA mana pun tetap satu klik: Actions → *Rilis citra* →
**Run workflow**.

---

## Kredensial untuk MENARIK citra

Langkah pertama platform mana pun, dan satu-satunya yang sudah pasti dibutuhkan
sebelum apa pun di halaman ini berlaku. Citranya privat, jadi platform harus
diberi kredensial registry — biasanya bernama *registry secret*, *image pull
secret*, atau *private registry credentials*.

Isinya: **registry `ghcr.io`, pengguna = nama akun GitHub, sandi = token yang
punya cakupan `read:packages`.**

🔴 **`docker login` yang berhasil TIDAK membuktikan Anda bisa menarik.**
Dibuktikan 2026-09-08, tepat sesudah ketiga citra pertama terbit:

```
$ gh auth token | docker login ghcr.io -u <pemilik> --password-stdin
Login Succeeded

$ docker pull ghcr.io/<pemilik>/techverse-x/api:<sha>
Error response from daemon: unknown: failed to resolve reference ...: 403 Forbidden
```

Token OAuth bawaan `gh` bercakupan `gist`, `read:org`, `repo`, `workflow` —
**`read:packages` tidak termasuk**, dan ketiadaannya baru terasa saat menarik,
bukan saat masuk. Platform yang kredensialnya kurang cakupan akan gagal dengan
cara yang sama: masuk terlihat sehat, `ImagePullBackOff` menyusul kemudian.

Tiga cara memberi cakupan itu, dari yang paling sempit:

| Cara | Catatan |
|---|---|
| **Fine-grained PAT**, izin *Packages: read*, dibatasi ke repo ini | Paling sempit. Bisa kedaluwarsa — catat tanggalnya. |
| **Classic PAT** dengan `read:packages` | Berlaku untuk semua paket akun, tidak bisa dipersempit. |
| **Menjadikan paketnya publik** (Package settings → Change visibility) | Menghapus kebutuhan kredensial sama sekali. Ingat: citra `api` memuat kode aplikasi. |

Kalau yang dipakai token milik sendiri, cakupannya bisa ditambahkan dengan
`gh auth refresh -s read:packages` (membuka peramban).

---

## Urutan yang mengikat

```
postgres sehat  →  migrate (sampai selesai, keluar 0)  →  api  →  web
```

Migrasi **harus** selesai sebelum API menyala. Ia sengaja bukan bagian dari
startup API: kalau ia diselipkan ke sana, setiap instans yang menyala akan
mencoba bermigrasi, dan menambah instans kedua berarti dua proses berlomba
mengubah skema yang sama.

`docker-compose.prod.yml` menegakkan urutan ini lewat
`condition: service_completed_successfully`. Platform lain menyebutnya *release
command*, *pre-deploy job*, atau *init container* — namanya berbeda, syaratnya
sama.

---

## Variabel lingkungan

### `migrate` dan `api`

| Variabel | Wajib | Catatan |
|---|---|---|
| `ConnectionStrings__Postgres` | **ya** | Tanpa ini API berhenti saat menyala dengan pesan yang menyebutkan namanya. Sengaja keras. |
| `ConnectionStrings__Redis` | **tidak** | **Jangan diisi di V1.** Lihat di bawah. |
| `ASPNETCORE_ENVIRONMENT` | tidak | Biarkan kosong (= `Production`). |

### `web`

| Variabel | Wajib | Catatan |
|---|---|---|
| `API_BASE_URL` | **ya** | Alamat API dari **dalam** jaringan platform, bukan `localhost`. Tiap peti kemas punya `localhost`-nya sendiri. |

---

## Redis: jangan dipasang

[ADR-016](adr/ADR-016-pagu-biaya.md) memutuskan Redis **tidak di-provision di
V1** — ia harus membuktikan dirinya dulu dengan beban yang benar-benar ada.

Kalau `ConnectionStrings__Redis` tidak diisi, health check `redis` **tidak
dipasang sama sekali** dan hilang dari `/health/ready`. Ia tidak dilaporkan
"sehat" — melaporkannya hijau akan menyembunyikan salah konfigurasi.

Mengisinya dengan alamat yang tidak ada jauh lebih buruk daripada
mengosongkannya: kesiapan akan merah selamanya, dan penyebaran tidak akan pernah
selesai.

---

## Health check

| Jalur | Untuk | Menyentuh dependensi? |
|---|---|---|
| `/health/live` | Apakah prosesnya masih hidup | **Tidak** |
| `/health/ready` | Apakah siap menerima lalu lintas | Ya — Postgres |

Pasang **`/health/ready`** sebagai probe kesiapan dan **`/health/live`** sebagai
probe keaktifan. Menukarnya adalah kesalahan yang mahal: Postgres yang sedang
bermasalah akan terus-menerus me-restart aplikasi yang sebenarnya sehat.

Balasan `/health/ready` memuat nama tiap dependensi dan statusnya, jadi kegagalan
bisa dibaca tanpa masuk ke peti kemas.

---

## Mencobanya lebih dulu di mesin sendiri

```
docker compose -f docker-compose.prod.yml up --build
```

- web → <http://localhost:8080>
- API → <http://localhost:8081>

Ini bukan pengganti `docker-compose.yml` — yang itu infrastruktur pengembangan.
Yang ini ada supaya bentuk produksinya bisa dibuktikan **sebelum uang hosting
keluar**: migrasi dari basis data kosong, kesiapan tanpa Redis, dan halaman yang
benar-benar menampilkan datanya.

---

## Pemindaian citra

CI memindai ketiga citra dengan **Trivy**, ambang **CRITICAL + HIGH**, dan
pemindaian itu **menggagalkan build** (`--exit-code 1`). Di `main` ia berjalan
**sebelum** citranya didorong — yang sudah terbit di registry bisa ditarik orang,
jadi memeriksanya sesudah terbit terlambat.

Trivy dijalankan sebagai peti kemas, bukan lewat action pihak ketiga: gerbang ini
karena itu tidak menuntut satu pun izin tambahan.

**Gerbangnya langsung berbuah saat dipasang.** Citra web ternyata membawa `npm`
dari citra dasarnya — 17 MB yang tidak pernah dipanggil `node server.js`, tetapi
menyumbang **11 dari 13 temuan CRITICAL/HIGH**, termasuk satu-satunya CRITICAL.
Membuangnya, plus menambal openssl citra dasar, membawa hitungannya **13 → 0**.

Menjalankan pemindaian yang sama di mesin sendiri:

```
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy:latest image --scanners vuln \
  --severity CRITICAL,HIGH --exit-code 1 <nama-citra>
```

⚠️ Kalau suatu hari gerbang ini merah karena kerentanan baru di citra dasar,
**perbaiki akarnya** — perbarui citra dasar atau tambal paketnya. Menurunkan
ambang adalah cara paling cepat membuat gerbang ini berhenti menjaga apa pun.

---

## Menyebarkan: Vercel + Koyeb + Neon

Platformnya diputuskan di [ADR-019](adr/ADR-019-hosting-gratis-tanpa-kartu.md):
**tiga tempat gratis, tak satu pun menuntut kartu.** Segala yang di atas tetap
berlaku untuk platform mana pun — bagian ini contoh konkretnya.

| Bagian | Tempatnya | Dari apa |
|---|---|---|
| **web** | Vercel Hobby | dibangun dari **sumber** |
| **api** | Koyeb Free | citra `ghcr.io/<pemilik>/techverse-x/api:<sha>` |
| **PostgreSQL** | Neon Free | — |

### Urutannya mengikat, dan di sini ia dijaga TANGAN

`postgres → migrate → api → web`. Koyeb tidak punya *pre-deploy command*, jadi
langkah `migrate` dijalankan dari GitHub Actions memakai **citra `api` yang sama
persis** — bukan dari kode sumber, sebab itu akan memigrasi dari bit yang berbeda
dari yang tayang.

1. **Neon** — buat proyek, salin *connection string*. Bentuknya URI
   (`postgresql://…`); aplikasi menerimanya apa adanya
   (`PostgresConnectionString`), jadi jangan dirakit ulang dengan tangan.
2. **GitHub → Settings → Secrets** — isi `NEON_DATABASE_URL` dengan URI itu.
3. **Actions → *Migrasi produksi* → Run workflow**, isi SHA-nya. Ini langkah
   `migrate`. Ia harus **hijau** sebelum lanjut.
4. **Koyeb** — buat Service dari citra `…/api:<sha>` **yang SAMA** dengan langkah
   3. Registry secret: token GitHub bercakupan `read:packages`. Isi
   `ConnectionStrings__Postgres` dengan URI Neon. Port `8080`.
   ⚠️ **`ConnectionStrings__Redis` dikosongkan** — ADR-016.
5. **Vercel** — impor repo, *Root Directory* `apps/web`. Isi `API_BASE_URL`
   dengan URL publik service Koyeb (`https://…koyeb.app`).
6. **Periksa** `/health/ready` di URL Koyeb: harus **200**, dan badannya hanya
   menyebut `postgres`. Kalau `redis` muncul, berarti langkah 4 salah.

🔴 **Memasang SHA yang berbeda dari yang baru dimigrasikan adalah cara paling
mudah membuat skema dan kode tidak sejalan.** Langkah 3 dan 4 harus menyebut SHA
yang sama.

### Kenapa API yang tidur tidak menjadi masalah

Instance Koyeb gratis **tidur setelah satu jam** menganggur, dan itu tidak bisa
dimatikan. Yang membuatnya tidak terasa bukan mencegahnya tidur, melainkan
**panggilan API di web di-cache lima menit** (`REVALIDATE_SECONDS` di
`apps/web/src/lib/api.ts`).

Sudah dibuktikan dengan menjalankan: dengan **peti kemas API dimatikan total**,
halaman tetap menyajikan 14 bidang dan 5 topik. Kendalinya juga dibuktikan —
proses baru dengan cache kosong dan API mati memang menampilkan *"API belum bisa
dihubungi"*.

⚠️ **Pembaca pertama sesudah penyebaran baru tetap menunggu**, sebab cachenya
kosong. Batas waktu fetch sengaja 30 detik untuk itu. Fetch yang **gagal** tidak
ikut ter-cache, jadi galatnya tidak bertahan lima menit.

### Tiga batas yang sudah diketahui

1. 🔴 **Vercel Hobby hanya untuk pemakaian NON-KOMERSIAL.** Begitu TechVerse X
   memungut bayaran, plan ini dilanggar.
2. **Citra `web` tidak dipakai untuk penyebaran** — Vercel membangun dari sumber.
   Citranya tetap terbit dan tetap dipakai `docker-compose.prod.yml`, tapi
   *"pasang tag SHA"* kini hanya berlaku untuk API.
3. **Koyeb gratis hanya Frankfurt atau Washington** — tidak ada Asia. Latensi ke
   pembaca Indonesia tinggi, dan itu tidak terasa justru karena cachenya: yang
   jauh cuma penyegaran di latar.

---


## Yang belum ada

- **Pilihan platform.** ✅ **Sudah diputuskan** —
  [ADR-019](adr/ADR-019-hosting-gratis-tanpa-kartu.md): Vercel + Koyeb + Neon,
  gratis dan tanpa kartu. ⛔ [ADR-017](adr/ADR-017-platform-hosting.md) (Render
  berbayar) **sudah digantikan**; ia dibiarkan utuh karena pemeriksaan empat
  syaratnya masih benar dan masih dipakai.
- 🔴 **Temuan ADR-017 yang paling menyentuh halaman ini tetap berlaku:** bagian
  *Urutan yang mengikat* di atas menuntut **citra `migrate` yang BERBEDA**
  berjalan sampai selesai — dan *pre-deploy command* milik Render maupun Railway,
  serta `release_command` Fly.io, menjalankan perintah di dalam **citra layanan
  itu sendiri**. Hanya `initContainers` Azure Container Apps (dan
  `docker-compose` di halaman ini) yang memetakan langsung. Itulah yang
  melahirkan `/app/efbundle` di citra `api` — dan di Koyeb, yang tidak punya
  pre-deploy sama sekali, ia dipanggil dari GitHub Actions.
- **Domain.** Belum dibeli. Sementara ini alamatnya `*.vercel.app`.
- **HTTPS/sertifikat.** Diurus Vercel dan Koyeb, tapi tetap harus dibuktikan.
