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
bergerak. Citranya **privat** — dan visibilitas paket GHCR **tidak** ikut repo:
repo publik sejak 2026-09-28, sedangkan ketiga citra diukur 2026-09-29 tetap
menolak tarikan anonim (`UNAUTHORIZED`). Kredensial di bagian berikutnya karena
itu tetap wajib.

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

### 🔴 Panjangnya juga menentukan: tag di registry selalu SHA **40 karakter**

`rilis-citra.yml` memberi tag `${GITHUB_SHA}`, dan itu SHA **penuh**. Manusia
menulis SHA pendek. Akibatnya:

```
docker pull ghcr.io/<pemilik>/techverse-x/api:b23650e
Error response from daemon: manifest unknown
```

⚠️ **Galat itu berbohong tentang sebabnya.** *"manifest unknown"* terbaca seperti
*"citranya belum terbit"*, padahal citranya ada dan sehat — yang tidak ada cuma
tag sependek itu. Terbukti pada gladi bersih 2026-09-09, dan yang menabraknya
adalah orang yang sedang memegang runbook ini.

🔑 **Bedakan dua kegagalan yang terlihat mirip tapi obatnya berlawanan:**

| Balasan registry | Artinya | Yang diperbaiki |
|---|---|---|
| `manifest unknown` | kredensial **diterima**; **tag**nya tidak ada | SHA-nya — panjang atau memang belum pernah dibangun |
| `denied` / `unauthorized` | soal **izin**; tag belum sempat dicari | cakupan token (`read:packages`) |

**Siapa yang memaafkan SHA pendek, dan siapa yang tidak:**

| Tempat | SHA pendek? |
|---|---|
| Workflow *Migrasi produksi* | ✅ dikanonikalkan sendiri lewat API GitHub |
| **Koyeb** (dan `docker pull` mana pun) | 🔴 **TIDAK** — tulis 40 karakter |

Jalan teraman: salin ref citra **apa adanya** dari ringkasan *Migrasi produksi*,
yang memang mencetaknya lengkap.

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
| `Editorial__WritesEnabled` | **tidak** | 🔴 **JANGAN DIISI.** Lihat di bawah. |
| `ASPNETCORE_ENVIRONMENT` | tidak | Biarkan kosong (= `Production`). |

#### 🔴 `Editorial__WritesEnabled`: biarkan kosong

Bawaannya `false`, dan itulah bentuk produksi V1 — **terbitan yang hanya bisa
dibaca** ([ADR-020](adr/ADR-020-permukaan-tulis-api.md)). Mengisinya `true` di
lingkungan yang terjangkau internet berarti **siapa pun yang menemukan URL API
boleh mengubah isi situs**: membuat topik yang langsung tampil di halaman muka,
menyisipkan tautan asing di bagian Resources, dan menaikkan halaman ke `draf`.
V1 tidak punya autentikasi sama sekali ([ADR-013](adr/ADR-013-autentikasi.md)),
jadi tidak ada apa pun di belakangnya yang menyaring.

Tidak ada yang hilang karena dibiarkan kosong: keempat belas bidang datang dari
**migrasi**, bukan dari HTTP.

**Cara memeriksanya sesudah menyebar** — dua baris, dan keduanya harus benar:

```bash
curl -s -o /dev/null -w '%{http_code}\n' -X POST https://<api>/api/v1/technologies \
  -H 'Content-Type: application/json' -d '{"name":"Uji","summary":"Uji","fieldSlug":"ai-agents"}'
# harus 405   (404 untuk endpoint tulis yang lain)

curl -s -o /dev/null -w '%{http_code}\n' https://<api>/api/v1/fields
# harus 200   - kendalinya: yang tertutup MENULIS, bukan seluruh API

curl -s "https://<api>/api/v1/search?q=quantum" | head -c 200
# harus memuat "quantum-computing"   - pencarian permukaan BACA (ADR-022),
# jadi ia ikut tayang. Kata kunci ini dipilih karena jawabannya datang dari
# BIDANG, yang disemai migrasi - jadi ia benar bahkan saat belum ada satu
# topik pun.
```

Log startup juga menyebutkannya. `Editorial:WritesEnabled mati` adalah bentuk
yang benar; baris ber-level **`warn`** yang berbunyi `HIDUP` di produksi adalah
insiden, bukan catatan.

💡 **CITRA-nya sendiri sudah dijaga CI** (`.github/scripts/periksa-permukaan-tulis.sh`,
dipasang di `ci.yml` dan di `rilis-citra.yml` sebelum push), jadi citra yang
permukaan tulisnya terbuka tidak akan pernah terbit. Yang **tidak** bisa dijaga
dari sana adalah **konfigurasi di platformnya** — karena itu dua `curl` di atas
tetap dijalankan setelah menyebar. Yang dijaga CI adalah artefaknya; yang
diperiksa `curl` adalah apa yang benar-benar Anda pasang.

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

🔴 **Pemantau dari luar — *uptime monitor*, ping terjadwal — memanggil
`/health/live`, BUKAN `/health/ready`.** Yang kedua menyentuh Postgres, dan Neon
Free menjatah waktu basis data terjaga: **100 CU-jam per bulan**, sedangkan 0,25 CU
yang terjaga sebulan penuh memakan 180. Pemantau yang memanggil `/health/ready`
lebih sering dari lima menit sekali membuat Neon tidak pernah tidur, dan sekitar
hari ke-16 basis datanya *"suspended until the next billing period"* — halaman
ikut kosong sampai bulan berikutnya. Probe kesiapan yang hanya dijalankan platform
saat menyebar tidak kena. Diperiksa 2026-09-17,
[ADR-025](adr/ADR-025-host-api-pengganti-koyeb.md#koreksi-untuk-dua-kaki-yang-tetap).

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
keluar**.

Yang benar-benar dibuktikannya, diukur ulang 2026-09-10 (issue
[#48](https://github.com/xtheoputra/techverse-x/issues/48)):

| Klaim | Keadaan |
|---|---|
| **Kesiapan tanpa Redis** | ✅ `/health/ready` **200**, dan badannya hanya menyebut `postgres` |
| **Urutan `migrate → api` ditegakkan** | ✅ `migrate` keluar dengan kode 0 lebih dulu, `api` menunggunya |
| **Permukaan tulis tertutup** | ✅ kedelapan endpoint tulis **tidak dipasang** — ADR-020, dan ini yang paling mirip produksi |
| **Halaman menyajikan isinya** | ⚠️ **14 bidang, NOL topik** — dan itu memang yang akan dilihat pengunjung hari ini |
| **Migrasi dari basis data kosong** | ⚠️ **tidak di sini** — lihat peringatan di bawah |

🔴 **Tumpukan ini tidak punya satu pun cara memasukkan topik, dan itu disengaja.**
Sejak [ADR-020](adr/ADR-020-permukaan-tulis-api.md) permukaan tulis tidak dipasang
kecuali `Editorial:WritesEnabled` dinyalakan, dan berkas ini **tidak**
menyalakannya. `run.ps1 seed` terhadapnya berhenti dengan **exit 1** sambil
menyebut ADR-020. Sampai 2026-09-10 alinea ini masih menjanjikan *"halaman yang
benar-benar menampilkan datanya"* — janji yang berhenti bisa ditepati sehari
sebelumnya, tanpa ada yang memberi tahu pembacanya.

Kalau yang ingin dilihat adalah **bentuk halaman topik yang sudah jadi**, itu
pekerjaan `docker-compose.yml` + `run.ps1 api` + `run.ps1 seed` — bukan berkas
ini. Dua berkas, dua tugas.

⚠️ **Dan berkas ini bukan tiruan Neon yang sah untuk urusan migrasi.** Postgres di
dalamnya memasang `infrastructure/docker/postgres-init/01-schemas.sql`, jadi skema
`technology` sudah **ada sebelum `migrate` jalan** — terbukti dari komentar skema
yang hanya ditulis skrip itu, bukan oleh `EnsureSchema`. Neon tidak memberi
fasilitas seperti itu. Pembuktian migrasi dari basis data yang benar-benar kosong
ada di [*Gladi bersih tanpa satu pun akun*](#gladi-bersih-tanpa-satu-pun-akun) di
bawah, lewat `CREATE DATABASE`.

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

Menjalankan pemindaian yang sama di mesin sendiri — atau seluruh gerbang CI
sekaligus lewat `.\run.ps1 ci` / `make ci` ([ADR-026](adr/ADR-026-nol-biaya-gratis-mandiri.md)):

```
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy:0.74.0@sha256:62b1e65e8869bc4b4c6aa4fa2b21595256c7c2f6018a9d9ad61caf87187c1969 \
  image --scanners vuln --severity CRITICAL,HIGH --exit-code 1 <nama-citra>
```

🔴 **Per digest, bukan `:latest`.** Maret 2026 Trivy disusupi
(CVE-2026-33634): citra v0.69.4–v0.69.6 — dan `latest` selama jendela paparan —
membawa pencuri kredensial, dan perintah di atas memasang soket Docker ke
dalamnya. Sampai 2026-09-28 baris ini, `ci.yml`, dan `rilis-citra.yml` menarik
`:latest`.

⚠️ Kalau suatu hari gerbang ini merah karena kerentanan baru di citra dasar,
**perbaiki akarnya** — perbarui citra dasar atau tambal paketnya. Menurunkan
ambang adalah cara paling cepat membuat gerbang ini berhenti menjaga apa pun.

---

## Menyebarkan: Vercel + Koyeb + Neon

> 🔴 **Diperbarui 2026-09-17 — kaki Koyeb di bagian ini TIDAK berlaku untuk akun
> baru.** Sejak 17 Februari 2026 Koyeb mewajibkan paket berbayar dan metode
> pembayaran bagi pendaftar baru. Penggantinya **belum dipilih**; ia diuji menurut
> [ADR-025](adr/ADR-025-host-api-pengganti-koyeb.md) — Railway Free lebih dulu,
> lengkap dengan syarat lulus tiap langkahnya.
>
> Yang berubah di bawah: **langkah 1 dan 2 (Neon) dikoreksi di tempat**, dan
> proyek Neon yang sungguhan dibuat **sesudah** region host API diketahui. Langkah
> 3 (Actions) tidak bisa jalan selama [#57](https://github.com/xtheoputra/techverse-x/issues/57). Langkah 4 dan 6
> (Koyeb) dibiarkan sebagai rekaman; syarat yang dibawanya — SHA lengkap,
> `ConnectionStrings__Redis` dan `Editorial__WritesEnabled` kosong, `/health/ready`
> yang hanya menyebut `postgres`, tiga `curl` permukaan tulis — berlaku untuk host
> mana pun.
>
> ✅ **Diperbarui 2026-09-29 — langkah 3 bisa jalan lagi.** #57 terjawab: repo
> publik sejak 2026-09-28, dan Actions kembali menjalankan langkahnya. Yang kini
> ditunggu langkah 3 hanya secret `NEON_DATABASE_URL`
> ([#38](https://github.com/xtheoputra/techverse-x/issues/38)) — diukur hari ini,
> repo belum punya secret satu pun.

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

1. **Neon** — buat proyek **di region AWS terdekat dengan host API**; region itu
   tidak bisa diubah sesudah proyeknya ada. Salin *connection string* dengan
   sakelar **Connection pooling MATI**. 🔴 Sakelar itu menyala secara bawaan untuk
   proyek baru, sedangkan Neon sendiri menulis *"Migrations must use a direct
   (non-pooled) Neon connection string"* — string yang host-nya berakhiran
   `-pooler` adalah yang **salah** untuk langkah 3. Bentuknya URI
   (`postgresql://…`); aplikasi menerimanya apa adanya
   (`PostgresConnectionString`), jadi jangan dirakit ulang dengan tangan: **yang
   dipilih sakelarnya, bukan huruf di string-nya.**
2. **GitHub → Settings → Secrets** — isi `NEON_DATABASE_URL` dengan URI *direct*
   itu.
3. **Actions → *Migrasi produksi* → Run workflow**, isi SHA-nya (pendek maupun
   penuh — langkah ini mengkanonikalkannya sendiri). Ini langkah `migrate`. Ia
   harus **hijau** sebelum lanjut, dan ringkasannya mencetak **ref citra
   lengkap** yang dipakai langkah berikutnya.
4. **Koyeb** — buat Service dari ref citra **yang persis sama** dengan yang
   dicetak langkah 3. 🔴 **Salin lengkap; SHA 40 karakter, bukan tujuh** — Koyeb
   tidak mengkanonikalkan apa pun, dan SHA pendek di sini berujung
   `manifest unknown`. Registry secret: token GitHub bercakupan `read:packages`.
   Isi `ConnectionStrings__Postgres` dengan URI Neon. Port `8080`.
   ⚠️ **`ConnectionStrings__Redis` dikosongkan** — ADR-016.
   ⚠️ **`Editorial__WritesEnabled` juga dikosongkan** — ADR-020.
5. **Vercel** — impor repo, *Root Directory* `apps/web`. Isi `API_BASE_URL`
   dengan URL publik service Koyeb (`https://…koyeb.app`).
6. **Periksa** `/health/ready` di URL Koyeb: harus **200**, dan badannya hanya
   menyebut `postgres`. Kalau `redis` muncul, berarti langkah 4 salah.
7. **Periksa permukaan tulisnya tertutup** — dua `curl` di bagian
   *Variabel lingkungan* di atas. Ini langkah tersendiri dan bukan pelengkap:
   sampai ia dijalankan, yang terbukti baru bahwa situsnya **bisa dibaca**, dan
   API yang sehat tapi terbuka terlihat persis sama sehatnya.

🔴 **Memasang SHA yang berbeda dari yang baru dimigrasikan adalah cara paling
mudah membuat skema dan kode tidak sejalan.** Langkah 3 dan 4 harus menyebut SHA
yang sama.

### Gladi bersih tanpa satu pun akun

Ketujuh langkah di atas bisa **dilatih lengkap di mesin sendiri** sebelum akun
mana pun dibuat, dan sekali dijalankan ia langsung menemukan satu cacat yang
menghentikan langkah 3 (lihat bagian berikutnya). Yang menjadikannya latihan yang
berarti bukan menjalankan citranya, melainkan **satu detail yang mudah terlewat:**

> 🔑 **Basis data latihannya harus dibuat TANPA skrip init.**
> `docker-compose.yml` memasang `infrastructure/docker/postgres-init/01-schemas.sql`
> yang sudah membuat skema `technology` lebih dulu — **Neon tidak punya
> fasilitas seperti itu.** Migrasi yang hijau di atas basis data dev karena itu
> belum membuktikan apa pun tentang Neon. `CREATE DATABASE <nama>` di dalam
> peti kemas yang sama memberi basis data yang benar-benar kosong (skrip init
> hanya berjalan sekali, untuk basis data bawaan), dan itulah tiruan Neon yang
> sah.

```bash
docker exec techversex-postgres psql -U techversex -d techversex -c "CREATE DATABASE gladi;"
docker build -f apps/api/Dockerfile -t tvx/api:gladi .

# Langkah 3, persis seperti workflow "Migrasi produksi" menjalankannya.
docker run --rm --network techversex_default \
  -e "ConnectionStrings__Postgres=postgresql://techversex:techversex_dev@techversex-postgres:5432/gladi?sslmode=prefer&channel_binding=prefer" \
  --entrypoint /app/efbundle tvx/api:gladi

# Langkah 4 + 6.
docker run -d --name tvx-gladi --network techversex_default -p 5085:8080 \
  -e "ConnectionStrings__Postgres=<URI yang sama>" tvx/api:gladi
curl -s http://localhost:5085/health/ready

# Langkah 7 - permukaan tulis. Peti kemas ini TIDAK membaca launchSettings.json,
# jadi ia sudah berbentuk produksi: yang di bawah harus 405, lalu 200.
curl -s -o /dev/null -w '%{http_code}\n' -X POST http://localhost:5085/api/v1/technologies \
  -H 'Content-Type: application/json' -d '{"name":"Uji","summary":"Uji","fieldSlug":"ai-agents"}'
curl -s -o /dev/null -w '%{http_code}\n' http://localhost:5085/api/v1/fields

# Langkah 5, dibangun dari SUMBER seperti Vercel - bukan dari citra `web`.
npm run build --workspace web
cd apps/web && API_BASE_URL=http://localhost:5085 npx next start -p 3010
```

⚠️ `next start` akan memperingatkan *"does not work with output: standalone"*.
Halamannya tetap tersaji dan pemeriksaan ini tetap sah — ia menyajikan `.next`
biasa, bukan `.next/standalone`. **Vercel tidak memakai `standalone` sama
sekali**; setelan itu ada untuk citra `web`.

#### Bentuk Vercel yang sesungguhnya: *Root Directory* = `apps/web`

Perintah di atas dibangun dari **akar repo**. Vercel tidak begitu — ia memakai
`apps/web` sebagai *Root Directory*, dan itu bentuk yang berbeda karena
**`apps/web` tidak punya `package-lock.json` sendiri**; satu-satunya lockfile ada
di akar. Kedua kemungkinan sudah dilatih di pohon hasil `git archive` yang bersih:

| Setelan Vercel | Yang terjadi | Hasil |
|---|---|---|
| *Include files outside root* **ON** (bawaan monorepo) | npm menaiki pohon, menemukan akar workspace, memakai **lockfile akar** | ✅ build hijau, versi **terpin** |
| *Include files outside root* **OFF** | hanya `apps/web` yang ada; npm memasang **tanpa lockfile** | ✅ build hijau, tapi versi **tidak terpin** |

🔑 **Biarkan setelan itu ON.** Keduanya hijau hari ini — dan pada percobaan ini
keduanya bahkan memilih versi yang **sama persis** (`next` 16.3.4, `tailwindcss`
4.3.3, `eslint` 9.39.5, `typescript` 5.9.3) — tapi yang OFF hijau karena rentang
`^` kebetulan belum bergerak, bukan karena ada yang menahannya. Yang ON dijamin
lockfile.

⚠️ **Yang dilatih di sini BENTUKNYA, bukan platformnya.** Perilaku Vercel yang
sesungguhnya baru bisa dibuktikan sesudah akunnya ada (#40). Yang sudah
tersingkir adalah satu-satunya risiko yang bisa diperiksa tanpa akun: bahwa
`apps/web` tanpa lockfile gagal dipasang atau gagal dibangun. Ia tidak.

⚠️ Dua hal yang berbeda dari produksi, dan keduanya **memang tidak bisa
ditiru** di sini: Postgres lokal tidak punya TLS, jadi `sslmode` dan
`channel_binding` dipakai dengan nilai `prefer`, bukan `require`. Yang tetap
terbukti adalah **string koneksinya dimengerti**; yang tidak terbukti adalah
transportnya. Kalau Neon menolak, bacalah galatnya: *"Couldn't set …"* berarti
parameternya, *"SSL connection requested…"* berarti transport.

💡 Hasilnya sudah dicatat: 9 tabel + 14 bidang dari basis data kosong,
`/health/ready` **200 hanya menyebut `postgres`**, dan halaman dari sumber
menampilkan **14 bidang**.

✅ **Diulang 2026-09-10 terhadap citra hari ini** — yaitu sesudah ADR-020 dan
sesudah gerbang citra dipasang, dua hal yang belum ada saat angka di atas diambil.
Hasilnya sama persis: **9 tabel, 14 bidang, 0 topik**, exit code **0**. Yang
diperiksa di sini `/app/efbundle` **di dalam citra `api`** — bukan `dotnet ef`
dari sumber seperti di CI, dan bukan citra `migrate` seperti di
`docker-compose.prod.yml`. Ketiganya artefak yang berbeda; yang dipakai produksi
lewat *Migrasi produksi* adalah yang ini.

⚠️ **Dua baris pertama langkah 3 selalu terlihat seperti kegagalan, dan bukan:**

```
Cannot load library libgssapi_krb5.so.2
Error: libgssapi_krb5.so.2: cannot open shared object file: No such file or directory
```

Itu Npgsql mencari pustaka Kerberos yang memang sengaja tidak ada di citra dasar
.NET. Migrasinya berjalan terus sampai `Done.` — **yang menentukan adalah exit
code langkahnya, bukan kata "Error" di dalam lognya.**

### 🔴 `channel_binding`: satu garis bawah yang menghentikan langkah 3

Dasbor Neon menyerahkan string koneksinya lengkap dengan **dua** parameter:

```
postgresql://user:sandi@ep-….neon.tech/dbname?sslmode=require&channel_binding=require
```

Langkah 1 melarang merakit ulang string itu dengan tangan — dan sampai
2026-09-09, mematuhi larangan itu **menggagalkan migrasinya**. Sebabnya satu
karakter: **Npgsql mencocokkan nama parameter dengan mengabaikan huruf
besar-kecil dan spasi, tapi TIDAK garis bawah.** Karena itu `sslmode` lolos (ia
tak punya pemisah sama sekali) sementara `channel_binding` ditolak — walaupun
Npgsql punya properti `Channel Binding` untuk persis parameter itu.

Gejalanya menyesatkan, dan itu bagian terburuknya. EF membungkusnya jadi:

```
An error occurred while accessing the Microsoft.Extensions.Hosting services.
Continuing without the application service provider. Error: Couldn't set channel_binding
Unable to create a 'DbContext' of type '…'. The exception 'Unable to resolve service for type
'Microsoft.EntityFrameworkCore.DbContextOptions`1[…]' …' was thrown …
```

Kalimat yang menyebut sebabnya berada **di tengah**, diapit kata *"Continuing"*
dan sebuah galat DI yang terdengar seperti masalah lain sama sekali.

`PostgresConnectionString` kini menerjemahkan garis bawah jadi spasi, jadi
seluruh parameter gaya libpq (`channel_binding`, `application_name`, …) ikut
terbawa. Yang benar-benar tidak dikenali **tetap ditolak** — membuangnya
diam-diam persis kesalahan yang dihindari sepanjang berkas ini — tapi pesannya
kini menyebut nama parameternya.

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

⚠️ **Diperbarui 2026-09-17: alasan di atas ditulis untuk host yang tidur sesudah
SATU JAM.** Kedua kandidat pengganti Koyeb tidur jauh lebih cepat — Railway Free
5–10 menit, Vercel 5 menit — jadi penyegaran lima menit web hampir selalu mengenai
instans yang sedang tidur. Cache tetap melindungi halaman yang **sudah pernah**
diambil; yang tidak terlindungi adalah permintaan yang belum pernah ter-cache,
terutama `/cari?q=` dengan kata kunci baru. Railway menulisnya sendiri: *"The first
request sent to a slept service may return a 502 Bad Gateway response."* Seberapa
sering itu terjadi **diukur** di uji ADR-025 (langkah R6), bukan diandaikan.

### Tiga batas yang sudah diketahui

1. 🔴 **Vercel Hobby hanya untuk pemakaian NON-KOMERSIAL.** Begitu TechVerse X
   memungut bayaran, plan ini dilanggar.
2. **Citra `web` tidak dipakai untuk penyebaran** — Vercel membangun dari sumber.
   Citranya tetap terbit dan tetap dipakai `docker-compose.prod.yml`, tapi
   *"pasang tag SHA"* kini hanya berlaku untuk API.
3. **Koyeb gratis hanya Frankfurt atau Washington** — tidak ada Asia. Latensi ke
   pembaca Indonesia tinggi, dan itu tidak terasa justru karena cachenya: yang
   jauh cuma penyegaran di latar. *(2026-09-17: gugur bersama kaki Koyeb. Region
   host penggantinya baru diketahui sesudah diuji — paket Free Railway tidak
   memberi pilihan region — dan region Neon mengikutinya.)*

---


## Yang belum ada

- **Pilihan platform.** 🟡 **Dua dari tiga sudah diputuskan** —
  [ADR-019](adr/ADR-019-hosting-gratis-tanpa-kartu.md): Vercel (web) dan Neon
  (PostgreSQL), gratis dan tanpa kartu. 🔴 **Host API dibuka lagi:** Koyeb tidak
  gratis untuk akun baru sejak 17 Februari 2026, dan penggantinya diuji menurut
  [ADR-025](adr/ADR-025-host-api-pengganti-koyeb.md). ⛔
  [ADR-017](adr/ADR-017-platform-hosting.md) (Render berbayar) **sudah
  digantikan**; ia dibiarkan utuh karena pemeriksaan empat syaratnya masih benar
  dan masih dipakai.
- 🔴 **Temuan ADR-017 yang paling menyentuh halaman ini tetap berlaku:** bagian
  *Urutan yang mengikat* di atas menuntut **citra `migrate` yang BERBEDA**
  berjalan sampai selesai — dan *pre-deploy command* milik Render maupun Railway,
  serta `release_command` Fly.io, menjalankan perintah di dalam **citra layanan
  itu sendiri**. Hanya `initContainers` Azure Container Apps (dan
  `docker-compose` di halaman ini) yang memetakan langsung. Itulah yang
  melahirkan `/app/efbundle` di citra `api` — dan di Koyeb, yang tidak punya
  pre-deploy sama sekali, ia dipanggil dari GitHub Actions.
- **Domain.** Belum dibeli. Sementara ini alamatnya `*.vercel.app`.
- **HTTPS/sertifikat.** Diurus Vercel dan host API, tapi tetap harus dibuktikan.
