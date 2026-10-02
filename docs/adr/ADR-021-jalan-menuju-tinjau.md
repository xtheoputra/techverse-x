# ADR-021 — Jalan sah menuju `tinjau`: workflow bergerbang, bukan endpoint publik

**Status:** Diterima sebagai **pola** (2026-09-09); **diimplementasikan 2026-10-02
MENDAHULUI [#40](https://github.com/xtheoputra/techverse-x/issues/40)** atas keputusan
pemilik — lihat [Pembaruan 2026-10-02](#pembaruan-2026-10-02--dibangun-mendahului-40-atas-keputusan-pemilik)
di kaki. **Menjawab pertanyaan yang dibiarkan terbuka
[ADR-020](ADR-020-permukaan-tulis-api.md).**

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

> 🟢 **Disuperseding 2026-10-02 oleh keputusan pemilik — bagian ini kini REKAMAN.**
> Alasan menunda di bawah tetap ditulis apa adanya sebagai rekaman keadaan
> 2026-09-09; yang berubah hanya satu premisnya (lihat
> [Pembaruan 2026-10-02](#pembaruan-2026-10-02--dibangun-mendahului-40-atas-keputusan-pemilik)).

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

---

## Pembaruan 2026-10-02 — dibangun mendahului #40 atas keputusan pemilik

Pemilik meminta *"kerjakan secara bertahap deliverable rencananya"* sesudah
diberi tahu bahwa seluruh deliverable berkode sudah selesai-menunggu-merge atau
digerbang di belakang #40. Itu **melonggarkan pemicu "#40 dulu"** yang ditulis ADR
ini, dan alur ini — deliverable kode paling hulu yang membuat pencacah resmi
RENCANA-V1 bisa bergerak — dibangun hari ini. Keputusan menimpa urutan tertulis,
jadi ia dicatat di sini, bukan dijalankan diam-diam.

**Satu premis "jangan bangun sekarang" sudah gugur, dua masih berdiri — dan itu
membentuk apa yang dibangun vs tidak.** #57 beres (repo publik, CI & artefak
hidup), jadi keberatan "workflow-nya mati di bawah #57" tidak berlaku lagi. Yang
MASIH berdiri: belum ada URL publik (#40) dan belum ada DB produksi (#38). Karena
itu yang dibangun adalah **mesin dan penjaganya, diuji di dev** — bukan
menjalankannya di produksi, yang memang belum ada.

**Ketiga keputusan ADR ini diimplementasikan apa adanya:**

1. **Workflow manual bergerbang** — `.github/workflows/tinjau.yml`, meniru
   `migrasi-produksi.yml`: `workflow_dispatch` + `environment: produksi` +
   `packages: read`, menjalankan citra `api` yang sama dengan
   `Editorial__WritesEnabled=true` terikat ke `127.0.0.1` (tak pernah publik), lalu
   memanggil `/tinjau` lewat `localhost` dan membuang peti kemasnya.
2. **Nama dari `github.actor`, tanpa masukan penimpa** — workflow mengikat
   `PEMERIKSA: ${{ github.actor }}` dan hanya punya dua input (`sha`, `slug`).
   Aturan "tanpa medan atas-nama" yang ADR ini sebut *"belum punya penjaga sama
   sekali"* kini **punya penjaga**: `tests/unit/Workflows/TinjauWorkflowGuardTests.cs`
   menolak input ketiga apa pun dan menuntut pemeriksa datang dari `github.actor`.
3. **Endpoint di dalam grup TULIS** — `POST /api/v1/technologies/{slug}/tinjau`
   (`services/technology/Features/MarkReviewed/`), memakai ulang `TopicMutation`
   persis seperti yang komentar berkas itu ramalkan. Ia masuk daftar
   `SemuaEndpointTulis`, jadi ketiga uji ADR-020 langsung menuntutnya hilang di
   produksi dan ada saat sakelar hidup — tanpa uji baru untuk itu.

**Yang SENGAJA tetap di luar lingkup:** `ReviewedBy` masih tidak ada di
`TechnologyResponse` (butir 2 [#68](https://github.com/xtheoputra/techverse-x/issues/68)).
Memunculkan identitas pemeriksa di tiap halaman topik publik adalah keputusan
produk/privasi tersendiri; ADR ini hanya membangun jalan yang MENULIS nama itu,
dan uji membacanya langsung dari DB. Jalan membuang sisi/bagian (Pembaruan
2026-09-17) juga belum dibangun — belum ditagih permintaan ini.

**Terukur, bukan diklaim:** 106 uji unit + 69 uji integrasi hijau. Satu topik
dibawa dari kosong sampai `tinjau` lewat HTTP (`TinjauEndpointTests`), dan nama
pemeriksanya terbukti tersimpan di DB. 🔴 **Yang BELUM terjadi:** workflow-nya
belum pernah DIJALANKAN — ia butuh secret `NEON_DATABASE_URL` dan citra yang
terbit, keduanya menunggu #38/#40. Jadi di produksi pencacah `tinjau` **tetap 0**;
yang berubah hanya ini: begitu #40 tutup, produsennya sudah ada dan terbukti.

**Cara membalikkan tidak lagi berbiaya nol.** Paragraf "Konsekuensi" di atas benar
saat ditulis; kini membalikkan berarti membuang kode yang sudah mendarat berikut
ujinya dan workflow-nya, bukan sekadar menghapus ADR.
