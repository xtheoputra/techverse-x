# ADR-018 — Rilis citra: berhenti membangun ulang untuk commit dokumen, dan **tidak** mengejar reproducibility

**Status:** Diterima. Menutup Issue [#32](https://github.com/xtheoputra/techverse-x/issues/32).
**Tanggal:** 2026-09-08

## Konteks

[Issue #32](https://github.com/xtheoputra/techverse-x/issues/32) lahir dari dua pengukuran, bukan dari dugaan.

`rilis-citra.yml` menyala pada **setiap** push ke `main` — tidak ada penyaring
`paths`. Dua jalannya yang pertama:

| Jalan | Isi commit | Durasi |
|---|---|---|
| `4bfbfff` | kode: dua PR mendarat | **206 detik** |
| `aa78079` | **dokumen saja**, nol baris kode | **199 detik** |

Dan ketiga citranya punya **digest yang berbeda** meski sumbernya identik:

| Citra | `4bfbfff` | `aa78079` (dokumen saja) |
|---|---|---|
| `api` | `sha256:bd13e2d1…` | `sha256:0f38bdb3…` |
| `migrate` | `sha256:881f624c…` | `sha256:71635ed9…` |
| `web` | `sha256:e9941bc4…` | `sha256:0f549cc6…` |

Dua masalah, dan issue itu mengusulkan tiga arah: biarkan · saring `paths` ·
bikin buildnya reproducible.

## Yang ditemukan setelah menggali, dan yang mengubah jawabannya

### Reproducibility punya jalur yang jelas — dan syarat yang tidak disebut siapa pun

| Bahan | Keadaannya |
|---|---|
| `SOURCE_DATE_EPOCH` | Standar lintas-alat (reproducible-builds.org); dihormati `tar`, `gzip`, `zip`, kompiler Go. Buildx ≥ 0.10 **mempropagasikannya otomatis** dari env host ke build arg |
| `rewrite-timestamp=true` | **Opsi EKSPORTER, bukan flag build** — `--output type=image,name=…,push=true,rewrite-timestamp=true`. BuildKit ≥ 0.13. Tanpa ini, `SOURCE_DATE_EPOCH` hanya menyentuh config citra, **bukan stempel waktu berkas di dalam lapisan**. Tidak menyala bawaan karena menulis ulang lapisan itu mahal |
| Attestation / provenance | Mengubah media type dan jadi sumber ketidakpastian; umumnya dimatikan saat mengejar digest yang sama |
| Sisi .NET | `Deterministic` + `ContinuousIntegrationBuild` (paket `DotNet.ReproducibleBuilds`) membuat keluaran kompiler identik byte lepas dari path dan waktu |
| Cara memverifikasi | **Bangun dua kali, banding digest**; `diffoscope` untuk melihat bedanya. Satu-satunya cara memastikan ia tetap reproducible seiring proyek berubah |

Semuanya bisa dikerjakan. **Tapi ada satu syarat yang tidak muncul di panduan
mana pun, dan justru syarat itu yang menentukan di repo ini:**

### 🔴 Citra dasar repo ini memakai tag BERGERAK — dan itu disengaja

`mcr.microsoft.com/dotnet/aspnet:10.0`, `sdk:10.0`, `runtime-deps:10.0`,
`node:22-alpine`. Semuanya tag yang **bergerak**.

Artinya: begitu Microsoft menerbitkan `aspnet:10.0` yang baru, membangun ulang
sumber yang sama **wajib** menghasilkan citra yang berbeda — dan itu **benar**,
bukan cacat. Bit-for-bit reproducibility **mustahil** tanpa memaku citra dasar ke
digest lebih dulu.

Dan memakunya menabrak gerbang yang sudah kita pasang:

> **Gerbang kedelapan (Container Scan) bernilai justru karena membangun ulang
> menarik lapisan dasar yang SUDAH DITAMBAL.** Memaku digest membekukan
> kerentanan lapisan dasar sampai ada yang ingat memperbaruinya — dan itu
> mengubah Trivy dari penjaga jadi pengingat.

Jadi trade-off yang sebenarnya bukan *"boros vs rapi"*, melainkan:

**digest yang bisa dibandingkan ⟷ tambalan keamanan yang datang sendiri**

Untuk situs yang belum tayang, dengan satu orang yang mengurusnya, **tambalan
otomatis jauh lebih berharga daripada digest yang bisa dibandingkan.**

## Keputusan

### 1. TIDAK mengejar reproducible bit-for-bit

Ia menuntut memaku citra dasar ke digest, dan itu melemahkan Container Scan yang
sudah terbukti berbuah (`npm` yang ikut terbawa `node:22-alpine`, 13 → 0 temuan).
Ketidakpastian digest **diterima dan dicatat**, bukan dibiarkan tanpa penjelasan.

Konsekuensi yang harus disadari: **digest BUKAN pembanding.** Pertanyaan *"versi
berapa yang tayang?"* tetap terjawab oleh tag SHA; pertanyaan *"apakah ada yang
berubah di antara dua tag ini?"* dijawab dengan **membandingkan commit, bukan
digest.** Kalau suatu saat pertanyaan kedua jadi mahal — misalnya audit rilis —
ADR ini yang dibatalkan.

### 2. Rilis TIDAK lagi berjalan untuk commit yang murni dokumen

`rilis-citra.yml` mendapat `paths-ignore` untuk `**.md`, `docs/**`, dan
`LICENSE`.

**Ini aman, dan dibuktikan bukan diduga:** tidak satu pun `COPY` di kedua
Dockerfile menyalin dokumen ke citra akhir — `apps/api/Dockerfile` menyalin
keluaran `dotnet publish`, `apps/web/Dockerfile` menyalin keluaran `standalone`.
`.dockerignore` juga sudah membuang `docs` dan `*.md` dari konteks build.
**Commit dokumen tidak punya jalan untuk mengubah isi citra.**

### 3. Invariannya berubah, dan yang baru lebih jujur

| Sebelum | Sesudah |
|---|---|
| Setiap commit `main` punya citra | Setiap commit `main` **yang bisa mengubah citra** punya citra |

Yang lama terdengar lebih rapi, tapi ia membeli kerapian itu dengan membangun
ulang sesuatu yang terbukti tidak berubah.

**Lubang yang ditinggalkannya ditutup dengan menjawab pertanyaannya, bukan
dengan menghindarinya:** *"SHA mana yang harus dipasang?"* dijawab
`PENYEBARAN.md` — **SHA yang disebut di ringkasan jalan _Rilis citra_ terakhir
yang hijau**, bukan `git rev-parse HEAD`. `workflow_dispatch` tetap ada, jadi
membangun ulang kapan saja tetap satu klik.

## Konsekuensi

- Commit dokumen berhenti membayar ~3,3 menit runner. Di repo yang sepadat
  dokumen ini, itu bagian terbesar dari tagihan Actions-nya.
- ⚠️ **`git rev-parse HEAD` berhenti jadi jawaban yang benar** untuk "tag apa
  yang dipasang". Ini harga yang nyata, dan satu-satunya alasan butir 3 di atas
  ditulis panjang.
- Trivy tetap memindai dengan basis data terbaru **setiap kali citra dibangun
  ulang** — yaitu setiap kali kode berubah. Ia tidak lagi memindai ulang di hari
  yang tidak ada perubahan kode; kalau nanti pemindaian terjadwal diinginkan, itu
  `schedule:` tersendiri, bukan alasan membangun ulang citra.
- Ketidakpastian digest tetap ada dan sekarang **tertulis**, jadi orang
  berikutnya tidak menghabiskan waktu mengejar sebabnya.

## Cara membatalkan keputusan ini

**Butir 2 murah dibatalkan:** buang blok `paths-ignore`, dan invarian lama
kembali di push berikutnya.

**Butir 1 lebih mahal, dan urutannya mengikat:** paku dulu keempat citra dasar ke
digest, tambahkan `DotNet.ReproducibleBuilds`, pindah dari `docker build` ke
`docker buildx build --output type=image,…,rewrite-timestamp=true` dengan
`SOURCE_DATE_EPOCH` dari stempel waktu commit, matikan provenance, lalu **bangun
dua kali di CI dan bandingkan digest** — tanpa langkah terakhir itu, klaim
reproducible tidak pernah terbukti. Dan siapkan jawaban untuk pertanyaan yang
ditinggalkannya: **siapa yang memperbarui digest citra dasar, dan seberapa
sering.**
