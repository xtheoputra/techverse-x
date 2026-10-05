# ADR-013 — Autentikasi: Clerk sebagai Identity Provider, .NET memverifikasi sendiri

**Status:** Diterima sebagai pola. **Implementasi ditunda ke V1.1** — V1 tidak punya login.
Menutup Issue [#14](https://github.com/xtheoputra/techverse-x/issues/14).
**Tanggal:** 2026-09-04

## Konteks

Tabel arsitektur `KERANGKA.md` 2.6 menulis "Auth.js / Clerk" seolah dua
alternatif setara. Audit membuktikan keduanya tidak setara: **cookie sesi Auth.js
adalah JWE terenkripsi** (A256CBC-HS512) dengan kunci turunan `AUTH_SECRET`,
bukan JWT bertanda tangan. Dokumentasi Auth.js sendiri menyatakan JWT-nya
ditujukan untuk aplikasi yang menerbitkannya, dan menyarankan mengandalkan
Identity Provider untuk API pihak ketiga. **Backend .NET adalah "pihak ketiga"
dalam arti teknis itu.**

## Keputusan

### V1 tidak punya autentikasi sama sekali

Ini keputusan yang paling banyak menghemat, jadi ditulis lebih dulu. Satu-satunya
fitur V1 yang membutuhkan identitas adalah Progress Tracker dan Badge — dan
keduanya sudah dijadwalkan V1.1 atau dicoret di
[ADR-009](ADR-009-tulang-punggung-navigasi.md).

Membangun autentikasi untuk produk yang belum punya satu halaman pun berstatus
`tinjau` berarti memasang permukaan serangan terbesar sebelum ada yang perlu
dilindungi.

### Kalau nanti dipasang: Clerk sebagai IdP, verifikasi RS256 via JWKS di .NET

Opsi C. Bentuknya:

- **Clerk menerbitkan token**, bukan Next.js.
- **`apps/api` memverifikasi sendiri** lewat JWKS Clerk (RS256), memakai SDK .NET
  resmi `Clerk.BackendAPI`.
- **Setiap endpoint .NET wajib punya `[Authorize]` sendiri.** Middleware Next.js
  hanya untuk pengalaman pengguna, tidak pernah untuk otorisasi.

Alasan memilih Clerk di atas tiga opsi lain:

- **Opsi A (BFF murni)** menaruh seluruh beban otorisasi di lapisan yang kita
  tulis sendiri. Untuk pengembang tunggal, itu justru yang paling mudah salah.
- **Opsi B (Auth.js sebagai klien OIDC Entra)** menambah satu lapisan yang tidak
  menambah kemampuan apa pun di atas D, sambil menyeret ketergantungan pada
  `next-auth` v5 yang **masih beta** (`5.0.0-beta.32`). Kalau ia patah, yang
  macet adalah login — bukan fitur pinggiran.
- **Opsi D (Entra External ID langsung)** lebih murah pada skala besar tapi jauh
  lebih berat dikonfigurasi, dan tenant-nya salah pilih tidak bisa diperbaiki
  dengan ganti pengaturan.

### Tiga hal yang dikunci bersamaan supaya tidak terlupa

1. **CVE-2025-29927 (CVSS 9.1 Critical)** — otorisasi yang ditaruh di middleware
   Next.js bisa dilewati lewat satu header. Karena itu `[Authorize]` di .NET
   bukan pelengkap, melainkan satu-satunya penegak yang dihitung.
2. **Menandatangani JWT sendiri dengan HS256 + secret bersama dilarang.** Ia
   jalan di hari pertama, lalu tidak ada JWKS, tidak ada rotasi kunci, tidak ada
   revocation, dan logout di Next.js tidak mencabut apa pun.
3. **Kalau nanti pindah ke Entra, yang benar adalah Entra External ID**, bukan
   tenant karyawan. Azure AD B2C sudah tidak dijual ke pelanggan baru sejak
   1 Mei 2025.

### Syarat keluar dari Clerk sudah ditetapkan sekarang

Harga Clerk berbasis **pengguna aktif**: gratis sampai 50.000, lalu
$0,02/pengguna/bulan. Pendaftaran kelas massal bisa melipatgandakan tagihan dalam
satu bulan.

**Pemicu pindah: MAU menyentuh 40.000** (80% dari ambang gratis), bukan 50.000.
Tujuannya pindah sebelum tagihan pertama, bukan sesudah.

Pindahnya murah **justru karena .NET memverifikasi sendiri lewat JWKS standar**:
yang berubah adalah issuer dan alamat JWKS, bukan kode otorisasi. Ini alasan
kedua memilih opsi C di atas A — opsi A mengikat kita pada lapisan buatan sendiri
yang tidak punya jalan keluar sestandar itu.

## Konsekuensi

- `apps/api` **tidak** memasang autentikasi apa pun sekarang, konsisten dengan
  [ADR-008](ADR-008-batas-fase-1.md).
- 🔴 **Ditambahkan 2026-09-09 — lihat [ADR-020](ADR-020-permukaan-tulis-api.md).**
  Ketiga alasan di atas semuanya menimbang identitas **PEMBACA** (Progress
  Tracker, Badge, AI Mentor). ADR ini tidak pernah menimbang permukaan tulis
  **REDAKSI** — delapan endpoint yang mengubah isi situs — dan kalimat "V1 tanpa
  autentikasi" terlanjur dibaca seolah sudah menjawabnya. Ia tidak: keputusan
  yang sudah diambil itu menjawab pertanyaan yang lain. Kesimpulan ADR ini tetap berlaku
  apa adanya; yang ditambahkan ADR-020 adalah bahwa produksi **tidak memasang
  endpoint tulisnya sama sekali** selama belum ada cara sah mengenali penulisnya.
- Karena V1 tanpa login, AI Mentor tidak bisa dibatasi per pengguna. Batasnya
  jadi **pagu pemakaian harian global**, ditetapkan di
  [ADR-016](ADR-016-pagu-biaya.md).
- Pilihan ini menyimpan satu ketergantungan komersial di jalur login. Itu diambil
  sadar: risiko vendor yang punya syarat keluar tertulis lebih kecil daripada
  risiko lapisan autentikasi buatan sendiri yang tidak punya siapa-siapa untuk
  meninjaunya.
