# ADR-019 — Hosting gratis tanpa kartu: Vercel + Koyeb + Neon

**Status:** Diterima. **Menggantikan [ADR-017](ADR-017-platform-hosting.md).**
🔴 **Kaki `api` (Koyeb) gugur untuk akun baru** — lihat *Pembaruan 2026-09-17* di
bawah dan [ADR-025](ADR-025-host-api-pengganti-koyeb.md).
**Tanggal:** 2026-09-08

> 🔴 **Jangan mendaftar Koyeb dari ADR ini.** Sejak 17 Februari 2026 akun baru
> Koyeb wajib memilih paket berbayar dan memasukkan metode pembayaran. Kaki web
> (Vercel) dan PostgreSQL (Neon) tetap berlaku, dengan koreksi yang dirinci di
> ADR-025. Isi di bawah dibiarkan sebagai rekaman keputusan 8 September.

## Konteks

[ADR-017](ADR-017-platform-hosting.md) memilih **Render berbayar** (~$20/bln) dan
menolak tier gratis karena tidur 15 menit dan basis data yang kedaluwarsa 30
hari. Pemilik membatalkannya: *"jangan render, akun gratis pilihannya pada apa"*.

Syaratnya jadi lebih keras dari sebelumnya: **gratis permanen, dan sebisa mungkin
tanpa kartu sama sekali** — bukan sekadar "tidak ditagih".

## Yang berubah dari cara ADR-017 memandang soal ini

ADR-017 mencari **satu platform** untuk semuanya. Itu keliru. Soalnya pecah tiga,
dan **hanya satu yang sulit**:

| Bagian | Gratis? | Kenapa |
|---|---|---|
| **PostgreSQL** | ✅ mudah | Neon: 0,5 GB, pgvector, scale-to-zero, **tanpa kedaluwarsa**, tanpa kartu |
| **web (Next.js)** | ✅ mudah | Vercel Hobby: tanpa kartu, tanpa cold start, CDN global |
| **api (.NET)** | 🔴 sulit | hampir semua tier gratis cuma memberi **satu** layanan |

🔑 **Keberatan terbesar ADR-017 lenyap begitu Postgres dipisahkan dari
kontainer.** Yang ditolak di sana bukan "gratis", melainkan Postgres gratis
Render yang menghapus dirinya sendiri setelah 30 hari. Neon tidak melakukan itu.

## Survei tier gratis untuk API .NET, per 8 September 2026

| Host | Kartu? | Batas yang menentukan |
|---|---|---|
| **Koyeb** | biasanya **tidak** | **1 instance gratis** per organisasi · 512 MB / 0,1 vCPU · tidur setelah 1 jam **dan itu tidak bisa dimatikan** · hanya Frankfurt atau Washington · **GHCR privat didukung resmi** |
| **Google Cloud Run** | **ya** | region gratis hanya `us-*` |
| **Oracle Always Free** | **ya** (verifikasi) | VM sungguhan, selalu hidup — tapi Juni 2026 Oracle memangkas ARM 4→2 OCPU **tanpa pengumuman apa pun** |
| **Hugging Face Docker Space** | — | ❌ **kini menuntut plan berbayar (PRO)** |
| **Fly.io · Railway** | ya | ❌ tak ada tier gratis untuk pendaftar baru |

⚠️ **Dua baris di tabel itu adalah perubahan yang terjadi dalam setahun
terakhir** — Docker Space yang jadi berbayar dan Oracle yang memangkas diam-diam.
Keduanya pengingat bahwa **ADR ini punya umur simpan lebih pendek daripada ADR
lain di repo ini**, dan wajar kalau ia perlu ditinjau ulang.

## Keputusan

| Bagian | Tempatnya |
|---|---|
| **web** | **Vercel Hobby** — dibangun dari sumber |
| **api** | **Koyeb Free** — dari citra `ghcr.io/…/api:<sha>` |
| **PostgreSQL** | **Neon Free** |

Ketiganya gratis permanen dan tak satu pun menuntut kartu.

### Yang membuatnya bekerja, dan ini sudah DIBUKTIKAN bukan diharapkan

Instance Koyeb gratis **tidur setelah satu jam** dan itu tidak bisa dimatikan.
Kalau setiap pembaca memicu panggilan API, setiap pembaca pertama sesudah jeda
akan menunggu instance itu bangun.

Jawabannya bukan mencegah API tidur — melainkan **membuat pembaca tidak
bergantung padanya**: panggilan API di `apps/web/src/lib/api.ts` kini di-cache
lima menit (`next: { revalidate: 300 }`).

Dibuktikan dengan menjalankan, tiga keadaan:

| Keadaan | Halaman menampilkan |
|---|---|
| Proses baru, cache kosong, **API mati** | ❌ *"API belum bisa dihubungi"* |
| API hidup (cache terisi) | ✅ 14 bidang · 5 topik |
| **API dimatikan lagi**, cache terisi | ✅ 14 bidang · 5 topik |

Baris pertama itu **kendalinya** — tanpa ia, dua baris lain tidak membuktikan apa
pun. Percobaan pertama saya cacat: cache dihapus dari disk **selagi proses Next
masih hidup**, dan halamannya tetap berisi karena cachenya ada di memori. Kendali
yang benar menuntut **prosesnya dimatikan** lebih dulu.

⚠️ `await connection()` di `FieldGrid` dan `TechnologyList` **tetap ada.** Yang
di-cache bukan halamannya melainkan panggilan API-nya; keduanya tidak
bertabrakan. Membuangnya justru mengembalikan cacat lama: halaman dipanggang saat
build, dan yang tersimpan adalah *"API tidak bisa dihubungi"*.

### Migrasi: dijalankan dari GitHub Actions, bukan oleh platform

Koyeb tidak punya *pre-deploy command* seperti Render. Tapi **Neon terjangkau
dari internet**, jadi migrasi bisa dijalankan dari CI — memakai **citra `api`
yang sama persis** yang akan berjalan di produksi:

```
docker run --rm -e ConnectionStrings__Postgres=… ghcr.io/…/api:<sha> /app/efbundle
```

Sengaja **`workflow_dispatch`, bukan otomatis**: ia menyentuh data produksi, dan
gerbang yang dilewati manusia lebih murah daripada migrasi salah yang mendarat
sendiri. Urutan `migrate → api` tetap ditegakkan — oleh urutan tombol, bukan oleh
platform, dan itu ditulis apa adanya di `PENYEBARAN.md`.

## Konsekuensi

- 🔴 **Citra `web` berhenti dipakai untuk penyebaran.** Vercel membangun dari
  sumber. Citranya tetap diterbitkan dan tetap dipakai `docker-compose.prod.yml`
  serta pembuktian lokal — tapi *"pasang tag SHA"* kini hanya berlaku untuk API.
- 🔴 **Vercel Hobby hanya untuk pemakaian NON-KOMERSIAL.** Begitu TechVerse X
  memungut bayaran, plan ini dilanggar. Ini kembaran temuan §10 OpenAI di
  `AUDIT-KELAYAKAN.md`: batas yang tidak terasa hari ini dan mengikat di hari
  pertama komersialisasi.
- **Tiga dashboard**, bukan satu. Harga dari memakai tier gratis terbaik di
  masing-masing bagian alih-alih satu platform yang sedang-sedang saja.
- **API hidup di Frankfurt atau Washington** — Koyeb gratis tidak menawarkan Asia.
  Latensi ke pembaca Indonesia tinggi, dan itu **tidak terasa** justru karena
  cachenya: yang jauh cuma penyegaran di latar.
- ⚠️ **Pembaca pertama sesudah penyebaran baru tetap menunggu**, sebab cachenya
  kosong. Batas waktu fetch dinaikkan 5 → 30 detik khusus untuk itu. Fetch yang
  GAGAL tidak ikut ter-cache, jadi galatnya tidak bertahan lima menit.
- **Neon 0,5 GB** cukup jauh untuk V1 (82 topik teks), dan `Chunk` nanti tetap
  muat karena pgvector ada di tier gratisnya.

## Cara membatalkan keputusan ini

Ketiganya bisa dilepas satu per satu, dan itu disengaja:

- **Neon → Postgres lain:** `pg_dump`/`pg_restore`, ganti satu variabel
  lingkungan. Aplikasi sudah menerima bentuk URI maupun kunci-nilai
  (`PostgresConnectionString`), jadi tidak ada perubahan kode.
- **Koyeb → host lain:** citranya tetap di GHCR dan tidak memuat apa pun yang
  khas Koyeb.
- **Vercel → citra `web` lagi:** citranya masih diterbitkan tiap rilis, jadi
  kembali ke penyebaran berbasis citra tinggal mengarahkan host ke tagnya.
- **Kembali ke ADR-017 (Render berbayar):** seluruh pekerjaannya masih ada —
  `preDeployCommand` tetap bisa memanggil `/app/efbundle`, dan alasan
  penolakannya tercatat di sana apa adanya.

---

## Pembaruan 2026-09-17 — kaki `api` gugur untuk akun baru, dan tabel survei keliru di dua baris sejak hari ditulis

🔴 **Koyeb Free tidak ada lagi untuk akun baru sejak 17 Februari 2026** — tujuh
bulan sebelum ADR ini ditulis. Pengumuman Koyeb: *"You'll need a valid payment
method and must subscribe to one of our paid plans to get started."* Harga per
2026-09-17: Pro $29/bulan + pemakaian. Kecuali pemilik sudah punya organisasi
Koyeb dari sebelum tanggal itu, keputusan `api` di atas **tidak bisa dijalankan
tanpa kartu**. Penggantinya belum dipilih; ia diuji menurut
[ADR-025](ADR-025-host-api-pengganti-koyeb.md) — Railway Free lebih dulu.

Koreksi tabel survei, ke sumber primer 2026-09-16 dan 17:

| Baris | Tertulis | Kenyataannya |
|---|---|---|
| **Koyeb** | kartu *"biasanya tidak"*, 1 instance gratis | akun baru wajib paket berbayar dan metode pembayaran sejak 2026-02-17 |
| **Fly.io · Railway** | *"tak ada tier gratis untuk pendaftar baru"* | **benar untuk Fly.io, keliru untuk Railway.** Paket Free $0 berkredit $1/bulan, tanpa kartu, diluncurkan ulang 29 Agustus 2025 — bersyarat verifikasi akun otomatis (ADR-025) |
| **Hugging Face Docker Space** | berbayar | benar, dan lebih buruk dari yang tertulis: lalu lintas keluar hanya lewat port 80, 443, dan 8080, jadi Postgres di 5432 terblokir |

**Dua kaki lain masih berlaku** — Neon dan Vercel Hobby tetap gratis dan tanpa
kartu — dengan koreksi yang dirinci di ADR-025: migrasi ke Neon wajib memakai
string *direct* (sakelar pooling menyala secara bawaan), region Neon terkunci saat
proyek dibuat, CU-jam yang habis menangguhkan basis data sampai bulan berikutnya,
dan log fungsi Vercel Hobby hanya disimpan satu jam.

⚠️ **Butir *Koyeb → host lain* di bagian sebelumnya terlalu optimis.** *"Citranya
tetap di GHCR"* benar tentang citranya, tapi kedua kandidat gratis yang bertahan
**tidak bisa menarik citra GHCR privat**: Railway Free (*"Private registry
credentials are available on the Pro plan"*) maupun Vercel, yang membangun citranya
sendiri. Melepas Koyeb tanpa biaya berarti membangun dari sumber.

💡 **Peringatan di bagian survei — *"ADR ini punya umur simpan lebih pendek"* —
terbukti, tapi tidak dengan cara yang dibayangkannya.** Yang dibayangkan: tier
gratis berubah **sesudah** ADR ini ditulis. Yang terjadi: dua barisnya sudah basi
**pada hari** ditulis, ke arah yang berlawanan — tier yang sudah hilang tercatat
ada, dan tier yang sudah kembali tercatat tidak ada. Aturan survei yang dipakai
penggantinya — kutipan primer bertanggal, pemeriksa yang membantah, dan pencocokan
ulang ke halaman yang hidup — tertulis di ADR-025.
