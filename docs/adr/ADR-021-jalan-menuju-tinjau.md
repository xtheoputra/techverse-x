# ADR-021 — Jalan sah menuju `tinjau`: workflow bergerbang, bukan endpoint publik

**Status:** Diterima sebagai **pola**; implementasinya **sengaja ditunda** sampai
[#40](https://github.com/xtheoputra/techverse-x/issues/40) tutup. **Menjawab pertanyaan yang dibiarkan terbuka
[ADR-020](ADR-020-permukaan-tulis-api.md).** **Tanggal:** 2026-09-09

## Konteks

Dua temuan bertemu di satu titik:

1. **`MarkReviewed()` dan `Publish()` nol pemanggil di kode produksi** — bukan
   endpoint, bukan skrip, bukan UI; hanya uji unit. Jadi sasaran Bulan 2
   ([#42](https://github.com/xtheoputra/techverse-x/issues/42): tujuh topik AI Agents berstatus `tinjau`)
   bukan sekadar "menunggu manusia" — **hari ini ia mustahil bagi siapa pun.**
   Angka *"N topik sudah diperiksa manusia"* di halaman muka adalah pencacah
   **tanpa produsen**.
2. **[ADR-020](ADR-020-permukaan-tulis-api.md) menutup permukaan tulis di
   produksi**, dan sengaja **tidak** memutuskan bagaimana isi sungguhan nanti
   masuk. Pertanyaan itu ditagih di sini.

Yang membuat soal ini tidak sepele: `tinjau` adalah **satu-satunya klaim
kepercayaan yang dipasang produk ini**. ADR-012 menuntut manusia yang menyebut
namanya; *Sharing & Publication Policy* OpenAI menuntut manusia memikul tanggung
jawab akhir. Sebuah endpoint publik tanpa autentikasi (V1 nol login, ADR-013)
akan membuat klaim itu **bisa dipalsukan siapa saja** — kerusakan yang jauh lebih
besar daripada topik palsu, karena yang dipalsukan adalah *lencana kejujurannya*.

## Keputusan

### 1. Jalannya adalah workflow manual bergerbang, meniru `Migrasi produksi`

Polanya sudah ada, sudah dipakai, dan **tarikan citranya baru saja terbukti
bekerja** (2026-09-09): `workflow_dispatch` + `environment: produksi` +
`packages: read`, menjalankan **citra `api` yang sama persis** dengan yang tayang.

Bentuknya untuk kasus ini:

- Runner menyalakan peti kemas `api` **dengan `Editorial__WritesEnabled=true`**,
  terhubung ke Neon, lalu memanggilnya lewat **`localhost` di dalam runner itu**.
- Peti kemas itu **tidak pernah punya alamat publik** dan dibuang di akhir jalan.
- Karena itu **ADR-020 tidak dilonggarkan sedikit pun**: yang tayang di Koyeb
  tetap tidak memasang satu pun endpoint tulis.

🔑 **Tidak ada artefak baru, tidak ada mekanisme auth baru, tidak ada saklar
baru di produksi.** Yang dipakai ulang: citranya, endpoint-nya, validatornya,
handler-nya, dan sakelar yang memang sudah ada.

### 2. Nama pemeriksanya diambil dari `github.actor`, bukan diketik

Ini bagian yang paling penting, dan ia **memperkuat ADR-012, bukan
melonggarkannya**.

`MarkReviewed(reviewer)` menuntut nama. Kalau namanya **teks bebas**, ia
sekadar pernyataan — siapa pun yang bisa memanggilnya bisa mengarang siapa pun.
Diambil dari `github.actor`, namanya **sudah diautentikasi GitHub** sebelum
sampai ke kode kita, dan jalannya tercatat permanen di riwayat Actions.

⚠️ **Tidak boleh ada masukan untuk menimpa nama itu.** Kalau yang memeriksa
orang lain, orang itu yang menekan tombolnya. Menambahkan medan "atas nama"
mengembalikan persis sifat yang dibuang di sini.

### 3. Endpointnya hidup di dalam grup TULIS yang sudah ada

`POST /api/v1/technologies/{slug}/tinjau` ditempatkan bersama keenam endpoint
bagian isi, **di bawah cabang `editorialWrites`** milik ADR-020. Konsekuensinya
otomatis dan itulah maksudnya: ia **ikut hilang di produksi** tanpa aturan
tambahan, dan uji `PermukaanTulisTests` yang sudah ada akan menuntutnya hilang
juga begitu ia masuk ke daftar.

## Yang TIDAK diputuskan di sini

- **Dari mana isi sungguhan datang** (berkas di repo, atau ditulis di lingkungan
  pengembangan lalu dipindahkan). Itu keputusan tersendiri yang **belum layak
  diambil sekarang**, sebab belum ada satu pun isi sungguhan untuk diuji
  bentuknya. Yang diputuskan di sini hanya **jalan terakhirnya**, yang berlaku
  apa pun sumbernya.
- **`Publish()`**, yang juga nol pemanggil. `TechnologyStatus` sumbu yang
  berbeda (ADR-015) dan tidak ada satu pun halaman yang menyaring atasnya hari
  ini; menyeretnya ke sini akan menggabungkan dua sumbu yang ADR-015 pisahkan.

## 🔴 Kenapa TIDAK dibangun sekarang

`RENCANA-V1.md` menulisnya tanpa ambiguitas: **Bulan 1 mengejar URL, bukan
fitur**, dan pekerjaan sisi isi adalah pekerjaan **sesudah** situs tayang.
[#41](https://github.com/xtheoputra/techverse-x/issues/41) bahkan memesan urutan itu secara eksplisit.

Membangun mesin peninjauan sekarang berarti: menulis mesin untuk **isi yang
belum ada**, di **situs yang belum tayang**, untuk **basis data yang belum
dibuat** (#38). Itu persis bentuk kegagalan yang jadi benang merah seluruh
proyek ini — *mati karena 40 halaman setengah jadi, bukan karena kurang fitur*.

**Pemicu membangunnya: [#40](https://github.com/xtheoputra/techverse-x/issues/40) tutup** — yaitu saat URL
publiknya ada. Sebelum itu, yang berlaku dari ADR ini cuma satu hal: **soalnya
sudah tidak terbuka lagi**, jadi tidak perlu diperdebatkan ulang tiap sesi.

## Alternatif yang ditolak, berikut alasannya

| Alternatif | Kenapa ditolak |
|---|---|
| **Endpoint publik biasa** (`POST …/tinjau` tayang di Koyeb) | V1 nol autentikasi. Siapa pun yang menemukan URL bisa menandai halaman *"diperiksa manusia"* — memalsukan satu-satunya klaim kepercayaan produk ini. |
| **Header rahasia bersama** | Mengarang mekanisme auth sendiri, yang ADR-013 justru hati-hati soal itu. Menambah rahasia yang harus dijaga dan dirotasi, demi satu operasi yang dipakai beberapa kali sebulan. |
| **Menyalakan `Editorial__WritesEnabled` sementara di Koyeb** | Selama saklarnya menyala, **dunia ikut bisa menulis**. Jendela balapan yang tidak perlu ada. |
| **SQL langsung ke Neon** | Melewati `MarkReviewed()` sepenuhnya — yaitu melewati justru penegak yang ADR-012 pasang. Kolom `ReviewedBy` terisi tanpa ada yang memikulnya. |
| **Alat CLI baru di dalam citra** | Jalur kode kedua yang menulis data, dengan uji sendiri, di samping jalur endpoint yang sudah ada dan sudah diuji. Lebih banyak kode untuk hasil yang sama. |
| **Status `tinjau` sebagai migrasi** | Menjadikan tiap peninjauan sebuah migrasi skema. ADR-015 memodelkan isi sebagai **data**, bukan kode. |

## Konsekuensi

- **#42 berhenti menjadi issue yang tidak bisa dikerjakan siapa pun.** Ia kini
  punya urutan yang jelas: #40 tutup → bangun jalan ini → baru isi dan tinjau.
- **Uji yang akan menjaganya sudah ada sebagian.** Begitu
  `POST …/tinjau` masuk ke daftar `SemuaEndpointTulis` di
  `tests/integration/PermukaanTulisTests.cs`, ketiga uji ADR-020 langsung
  menuntutnya hilang di produksi dan ada saat sakelarnya hidup — tanpa satu pun
  uji baru.
- ⚠️ **Yang belum punya penjaga sama sekali:** larangan menambahkan medan "atas
  nama" pada workflow-nya. Itu aturan yang hari ini cuma tertulis di sini, dan
  repo ini punya sejarah panjang soal *aturan yang dinyatakan tapi tidak
  dijaga*. Saat workflow-nya dibangun, penjaganya dibangun bersamaan.
- **Cara membalikkan:** hapus ADR ini dan pilih salah satu baris di tabel
  alternatif. Karena belum ada kode yang mendarat, membalikkannya hari ini
  berbiaya nol — dan itu memang salah satu alasan menundanya.

---

## Pembaruan 2026-09-17 - alur ini juga produsen relasi di produksi

Begitu dibangun, workflow ADR ini juga menjadi **satu-satunya produsen PRODUKSI
untuk relasi antar-topik** ([ADR-023](ADR-023-knowledge-graph-dasar.md)) - lewat
endpoint yang sudah ada, `POST …/requires`, dipanggil lewat `localhost` di dalam
runner. Tidak ada mekanisme baru.

- `POST …/tinjau` akan duduk di samping endpoint bagian isi dan `/requires`, dan
  bisa memakai ulang jalur `TopicMutation` alih-alih menyalinnya.
- ⚠️ **Yang ikut diputuskan saat alur ini dibangun:** jalan membuang sisi DAN jalan
  membuang bagian isi. Keduanya belum ada, jadi sisi atau bagian yang keliru di
  produksi hari ini tidak punya jalan perbaikan sama sekali.
