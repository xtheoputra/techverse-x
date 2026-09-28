# ADR-016 — Pagu biaya bulanan, dan aturan penerbitan ulang sumber luar

**Status:** Diterima. Menutup Issue [#22](https://github.com/xtheoputra/techverse-x/issues/22) dan — sejak 8 Sep 2026 —
Issue [#17](https://github.com/xtheoputra/techverse-x/issues/17): **keempat dokumen ketentuan layanan sudah dibaca**
dan **batas kutipan sumber non-arXiv sudah ditetapkan**. Lihat butir 4.
**Tanggal:** 2026-09-04

## Konteks

Biaya jalan belum pernah dibahas sama sekali, padahal ini proyek pribadi dengan
satu orang yang membayarnya. Dan fitur yang paling mudah meledak biayanya —
News Curator harian — adalah fitur yang sama yang terganjal pertanyaan hukum
yang belum terjawab. Keduanya karenanya diputuskan bersama.

## Keputusan

### 1. Pagu ditetapkan lebih dulu, arsitektur menyesuaikan

Usul audit diterima: tetapkan batas yang bisa diterima, lalu pilih arsitektur
yang muat di dalamnya — bukan sebaliknya.

**Pagu V1: USD 60 per bulan.** Alarm di 80% (USD 48).

| Pos | V1 | Kenapa |
|---|---|---|
| PostgreSQL (Azure Flexible Server, tier terkecil) | ~$15–30 | wajib |
| Container Apps / App Service | ~$0–15 | scale-to-zero kalau bisa |
| **OpenAI** | **pagu keras $20** | disetel di dashboard penyedia, bukan cuma di kode |
| Qdrant | **$0** | pgvector — [ADR-005](ADR-005-qdrant.md) |
| Clerk | **$0** | tidak dipasang di V1 — [ADR-013](ADR-013-autentikasi.md) |
| LangGraph Platform | **$0** | LangGraph dibuang — [ADR-007](ADR-007-agent-platform.md) |
| Redis | **$0 di produksi V1** | lihat butir 2 |
| Domain | ~$1–2/bln | belum dibeli |

Tiga dari delapan pos jadi nol **karena keputusan arsitektur di ADR lain**, bukan
karena dihemat belakangan. Itu urutan yang benar.

### 2. Redis harus membuktikan dirinya dulu

Redis tetap ada di `docker-compose.yml` untuk pengembangan, tapi **tidak
di-provision di cloud untuk V1** sampai ada beban yang benar-benar
membutuhkannya. Issue #22 sendiri menandainya dengan "perlu diputuskan apakah
benar-benar dibutuhkan di awal".

Konten V1 nyaris seluruhnya statis dan jarang berubah — itu beban yang dilayani
cache HTTP dan pembangkitan statis Next.js, bukan cache berbagi. Menyalakan Redis
"karena arsitekturnya begitu" adalah tagihan bulanan untuk masalah yang belum
ada.

⚠️ `/health/ready` sekarang melaporkan Redis. Mematikannya di produksi berarti
kesiapan (readiness) harus bisa dikonfigurasi — **itu perubahan kode yang belum
ditulis dan belum diuji**, jadi dicatat sebagai pekerjaan, bukan diasumsikan
gratis.

### 3. Tiga sumber ledakan biaya, ketiganya sudah dipadamkan di rancangan

Issue #22 menandai tiga hal yang paling mudah meledak tanpa disadari. Ketiganya
sudah tidak berlaku:

| Ancaman | Kenapa sudah padam |
|---|---|
| **Retry berlapis** (Polly + LangGraph + SDK = 1 kegagalan jadi 8 panggilan berbayar) | Hanya ada satu runtime dan satu lapisan retry — LangGraph dibuang di ADR-007 |
| **Harga Clerk per pengguna aktif** | Tidak dipasang di V1; syarat keluarnya sudah ditulis di ADR-013 |
| **News Curator harian** | Lihat butir 4 — V1 tidak memanggil model untuk berita sama sekali |

### 4. Intelligence V1: arXiv saja, dan tanpa ringkasan AI

Ini keputusan yang sekaligus menjawab biaya **dan** memundurkan blocker hukum.

- **V1 hanya menerbitkan ulang metadata arXiv.** Menurut tabel di Issue #17,
  arXiv satu-satunya sumber yang **jelas boleh** — metadatanya berlisensi CC0.
- **Tanpa ringkasan AI.** Metadata arXiv sudah memuat abstrak; menuliskan ulang
  dengan model berarti biaya token berulang **setiap hari** untuk nilai tambah
  yang tipis. Ringkasan menyusul hanya setelah pagu terbukti aman selama satu
  bulan penuh.

Untuk sumber selain arXiv — **aturan tetap sejak 8 September 2026, bukan lagi
sementara**: boleh disimpan, tapi yang ditampilkan hanya **judul, tautan
kanonik, nama sumber, dan tanggal terbit**. **Nol kutipan, nol ringkasan, nol
abstrak.** Prinsip yang dipegang datang dari issue itu sendiri: robots.txt hanya
mengizinkan **mengambil**, ia tidak pernah berbicara soal **menerbitkan ulang**.

**Kenapa nol dan bukan "sekian kata":** V1 hanya menayangkan arXiv, jadi
menetapkan jatah kutipan sekarang berarti memutuskan sesuatu yang belum ada
pemakainya. Angkanya ditetapkan **per sumber** saat sumber non-arXiv pertama
benar-benar mau ditayangkan — syaratnya memang berbeda per sumber: Microsoft
menuntut *"tanpa modifikasi"*, arXiv tidak menuntut apa pun.

⚠️ **Aturan di atas adalah pengurang paparan, bukan nasihat hukum, dan bukan
pengganti #17.**

📌 **Diperbarui 7 September 2026 — kedua ToS sudah dibaca**, di peramban
sungguhan; blokir 403 hanya menyasar pengambil otomatis. Rekaman lengkapnya di
[`AUDIT-KELAYAKAN.md`](../../AUDIT-KELAYAKAN.md#pembaruan-7-september-2026--ketentuan-layanan-sudah-dibaca)
— **jangan disalin ke sini**, supaya tidak ada dua versi yang bisa hanyut
sendiri. Dua hal yang mengubah ADR ini:

- 🔴 **Membacanya TIDAK menutup #17.** ToS OpenAI mengatur pemakaian *Services*
  (ChatGPT, DALL·E), bukan penerbitan ulang isi editorial `openai.com`; untuk
  API yang berlaku **Business Terms yang terpisah, dan itu belum dibaca.**
  Asumsi lama bahwa membaca satu halaman akan menyelesaikan pertanyaannya
  ternyata salah.
  > ✅ **Dibaca 8 September 2026** — *OpenAI Services Agreement* (berlaku 1 Jan
  > 2026) dan *Sharing & Publication Policy*. Hasilnya melegakan: **§3.3 tidak
  > memuat satu pun larangan menerbitkan ulang Output**, dan **§4.1 menegaskan
  > Output milik pelanggan**. ⚠️ Tapi **§10 No Publicity** melarang memasang nama
  > atau logo OpenAI di situs tanpa izin tertulis — *"Powered by OpenAI"* di
  > halaman muka melanggarnya. Dan satu kalimat di *Sharing & Publication Policy*
  > **berbenturan dengan tingkat `draf` ADR-012**; ia belum menyala karena V1
  > tidak memanggil model sama sekali. Detailnya di `AUDIT-KELAYAKAN.md`.
- **Microsoft ternyata tiga lapis**, dan kalimat yang selama ini dikutip berkas
  ini sebenarnya **syarat dari sebuah izin**, bukan larangan telanjang. Efek
  praktisnya untuk aturan di atas tetap sama, dan malah menajamkannya: syarat
  *"tanpa modifikasi"* berarti **judul + tautan aman, ringkasan tidak** —
  persis batas yang sudah dipasang di paragraf sebelumnya.

Yang berubah dengan keputusan ini: **#17 berhenti menghalangi V1.** Ia tetap
menghalangi apa pun di luar arXiv, dan itu memang seharusnya.

### 5. AI Mentor dibatasi pagu harian global, bukan per pengguna

V1 tidak punya login ([ADR-013](ADR-013-autentikasi.md)), jadi tidak ada
pengguna untuk dibatasi. Gantinya: **batas panggilan harian global.** Setelah
terlampaui, AI Mentor menjawab bahwa kuota harian habis.

Batas yang keras dan terlihat lebih baik daripada tagihan yang mengejutkan. Angka
tepatnya ditetapkan saat fitur dibangun, diturunkan dari pagu $20 — bukan
ditebak sekarang.

## Konsekuensi

- Pagu ini **belum diuji dengan tagihan sungguhan**. Ia perkiraan dari daftar pos
  di Issue #22, bukan angka yang pernah dibayar. Tinjau setelah bulan pertama
  berjalan.
- Kalau salah satu keputusan arsitektur dibalik — LangGraph dipakai, Qdrant
  dipasang, Clerk dinyalakan lebih awal — **pagu $60 batal dengan sendirinya**.
  Ketiga nol di tabel itu bukan penghematan, melainkan konsekuensi.
- Ollama disebut di [ADR-014](ADR-014-mcp-dan-penyedia-ai.md) sebagai penyedia
  yang didukung Agent Framework. Kalau pagu OpenAI tetap terlampaui, jalan
  menurunkan biaya sudah terbuka tanpa perubahan arsitektur.

---

## Pembaruan 2026-09-07 — pekerjaan yang ditandai di butir 2 sudah ditulis

Butir 2 di atas menutup dirinya dengan peringatan bahwa mematikan Redis di
produksi menuntut **"perubahan kode yang belum ditulis dan belum diuji"**.
Perubahan itu sekarang ada, berikut gerbangnya.

**Keputusannya tidak berubah** — Redis tetap tidak di-provision di V1. Yang
berubah hanya: keputusan itu kini benar-benar bisa dijalankan.

Tiga hal yang ditemukan saat mengerjakannya, dan dua di antaranya lebih buruk
daripada yang diduga halaman ini:

1. **Kesiapan tidak sekadar merah — ia membalas `500`, bukan `503`.**
   `ConnectionMultiplexer.Connect` melempar di dalam pabrik DI, yaitu saat
   `RedisHealthCheck` sedang **dibangun**, sebelum `CheckHealthAsync` sempat
   berjalan. Akibatnya `try/catch` di dalam health check itu — yang ditulis
   persis untuk kasus ini — tidak pernah dijalankan, dan pemanggil tidak pernah
   tahu dependensi mana yang jatuh. Diperbaiki dengan `AbortOnConnectFail =
   false`, sehingga kegagalannya jatuh ke `PingAsync` dan tertangkap.

2. **`appsettings.json` memanggang `"Redis": "localhost:6379"`.** Selama baris
   itu ada, produksi selalu mengira Redis terpasang, apa pun yang diputuskan di
   halaman ini. Baris itu pindah ke `Properties/launchSettings.json`, yang hanya
   berlaku saat `dotnet run` — jadi pengembangan tetap menguji jalur Redis, dan
   produksi (yang menjalankan DLL-nya langsung) tidak pernah melihatnya.

   ⚠️ Tempat yang tampak paling wajar, `appsettings.Development.json`, **salah**:
   `.gitignore` menelannya, jadi isinya tidak akan pernah sampai ke CI maupun ke
   klona baru. Percobaan pertama memakai berkas itu dan baru ketahuan karena
   `git status` tidak menampilkannya.

3. **Check yang tidak dipasang tidak boleh muncul sebagai "sehat".** Kalau tidak
   ada string koneksi, `redis` hilang sama sekali dari `/health/ready`, bukan
   dilaporkan hijau. Perbedaannya penting: yang kedua menyembunyikan salah
   konfigurasi.

Dijaga oleh uji integrasi pertama repo ini
(`tests/integration/HealthEndpointTests.cs`), dan dibuktikan **merah lebih
dulu**. Bukti akhirnya diambil dengan menjalankan API sungguhan berlingkungan
`Production` **selagi Redis pengembangan tetap menyala di 6379** — tanpa kendali
itu, hijaunya tidak membuktikan apa pun.

## Pembaruan 2026-09-28 - pagu turun ke nol

Pagu **USD 60 per bulan** di butir 1 **dibatalkan** oleh
[ADR-026](ADR-026-nol-biaya-gratis-mandiri.md), atas arahan pemilik: tidak ada pos
berbayar, dan yang mandiri didahulukan. Pos-pos di tabel butir 1 kini:
PostgreSQL → Neon Free; OpenAI → tier gratis Groq dan Cloudflare Workers AI;
domain → `*.vercel.app`. Butir 2–5 tetap — terutama butir 4 (arXiv saja, tanpa ringkasan
AI), yang alasan hukumnya tidak bergantung pada biaya. Rinciannya hanya di ADR-026,
supaya tidak ada dua versi yang hanyut sendiri.
