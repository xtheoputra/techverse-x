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

## Menyebarkan ke Render

Platformnya sudah diputuskan di [ADR-017](adr/ADR-017-platform-hosting.md):
**Render, tier berbayar.** Bagian ini contoh konkretnya. Segala yang di atas
tetap berlaku untuk platform mana pun — bagian ini tidak menggantikannya.

Infrastrukturnya ditulis sebagai berkas: [`render.yaml`](../render.yaml) di akar
repo. Empat nilai di dalamnya bertanda `GANTI` dan hanya bisa diisi pemilik.

### Enam langkah

1. **Buat akun Render** dan sebuah Workspace.
2. **Tambahkan kredensial registry** (Workspace Settings → Registry Credentials):
   registry `ghcr.io`, pengguna = nama akun GitHub, sandi = token bercakupan
   **`read:packages`**. Catat **id kredensialnya** → itu nilai `<CRED-ID>`.
   ⚠️ Lihat bagian *Kredensial untuk MENARIK citra* di atas: `docker login` yang
   berhasil **tidak** membuktikan token itu boleh menarik.
3. **Isi `<SHA>`** di `render.yaml` dengan tag citra yang mau dipasang — ambil
   dari ringkasan workflow *Rilis citra* di Actions. Tag SHA, bukan `:main`.
4. **Blueprint → New Blueprint Instance**, arahkan ke repo ini. Render membaca
   `render.yaml` dan membuat ketiga sumber daya sekaligus.
5. **Tunggu penyebaran pertama.** Urutannya ditegakkan `preDeployCommand`:
   bundel migrasi berjalan sampai selesai sebelum API menyala. Kalau ia gagal,
   penyebarannya berhenti dan **versi lama tetap melayani**.
6. **Periksa `/health/ready` di host API.** Ia harus **200** dan badannya hanya
   menyebut `postgres` — kalau `redis` muncul di sana, berarti
   `ConnectionStrings__Redis` terisi dan itu salah (lihat *Redis: jangan
   dipasang*).

### Tiga hal yang khas Render dan sudah diketahui

**1. Postgres-nya diserahkan sebagai URI, bukan sebagai `Host=...;Port=...`.**
Aplikasi menerjemahkannya sendiri (`PostgresConnectionString`) karena Npgsql
melempar untuk bentuk URI. Jadi `fromDatabase` di `render.yaml` bisa dipakai apa
adanya — tidak perlu merakit string koneksi dengan tangan.

**2. Pre-deploy berjalan di citra layanan itu SENDIRI.** Karena itu bundel
migrasi ikut dimasukkan ke citra `api` (`/app/efbundle`). Citra `migrate` yang
berdiri sendiri tetap terbit dan tetap dipakai compose, VPS, dan
`initContainers` — dua jalan, satu migrasi EF yang sama.

**3. ⚠️ Render TIDAK punya urutan antar-layanan.** `web` bisa menyala sebelum
`api` siap, jadi di penyebaran pertama halaman ber-data bisa gagal beberapa saat.
`docker-compose.prod.yml` menegakkan urutan itu; Render tidak.

### 🔴 Jangan pakai tier gratisnya

| Batas | Akibatnya |
|---|---|
| Web service **tidur setelah 15 menit** menganggur | Pengunjung pertama menunggu **30–60 detik** |
| Postgres gratis **kedaluwarsa 30 hari**, dihapus setelah tenggang 14 hari | Bukan basis data, melainkan hitungan mundur |

Gratis di sini memindahkan bebannya dari uang ke pengalaman pembaca dan ke risiko
kehilangan data. ADR-017 menolaknya justru atas dasar kriteria *"mudah dibuka dan
kuat"*, bukan atas dasar biaya.

---

## Yang belum ada

- **Pilihan platform.** ✅ **ADR hosting sekarang ADA** —
  [ADR-017](adr/ADR-017-platform-hosting.md) memeriksa lima kandidat terhadap
  empat syarat dan mengusulkan **Render**, tapi statusnya masih **Diusulkan**:
  keputusannya milik pemilik ([Issue #33](../../issues/33)). Azure di ADR-016
  tetap cuma tabel perkiraan biaya, bukan keputusan.
- 🔴 **Konsekuensi yang paling mengejutkan dari ADR itu, dan yang paling
  menyentuh halaman ini:** bagian *Urutan yang mengikat* di atas menuntut
  **citra `migrate` yang BERBEDA** berjalan sampai selesai — dan *pre-deploy
  command* milik Render, Railway, maupun Fly.io menjalankan perintah di dalam
  **citra layanan itu sendiri**. Hanya `initContainers` Azure Container Apps
  (dan `docker-compose` di halaman ini) yang memetakan langsung. Di platform
  lain, bentuk bundel migrasinya harus berubah.
- **Domain.** Belum dibeli.
- **HTTPS/sertifikat.** Umumnya urusan platform, tapi tetap harus dibuktikan.
