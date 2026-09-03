# TechVerse X

**Explore. Learn. Build. Innovate.**

Platform pembelajaran teknologi terdepan. Satu tempat berisi peta teknologi, roadmap belajar, tutorial, proyek praktik, tools, berita, dan AI mentor — dengan isi yang diperbarui secara otomatis.

Sasaran pemakai: pemilik sendiri, mahasiswa, engineer, sampai perusahaan yang butuh onboarding teknologi baru.

---

## ⚠️ Status: TAHAP KERANGKA — belum ada kode sama sekali

Repositori ini **belum berisi aplikasi**. Isinya murni dokumen perancangan. Jangan mencari `package.json` atau `.csproj` — keduanya memang belum ada, dan tidak akan dibuat sebelum keputusan-keputusan di [Issues](../../issues) selesai.

| Yang sudah ada | Yang belum ada |
|---|---|
| Peta 12 teknologi (diaudit) | Kode aplikasi |
| Struktur 12 bidang + 7 bagian aplikasi | Skema basis data |
| Arsitektur teknologi (diaudit) | Isi kurikulum |
| Engineering Blueprint v1 + backlog 120 task | Roadmap belajar |
| Rencana 6 bulan | Keputusan navigasi utama |

---

## Isi Repositori

| Berkas | Isi |
|---|---|
| **[KERANGKA.md](KERANGKA.md)** | Dokumen utama. Empat bagian dikte pemilik — peta teknologi, produk, identitas & struktur aplikasi, dan Engineering Blueprint v1 — ditutup daftar 30 butir yang menunggu keputusan. **Mulai dari sini.** |
| **[AUDIT-KESEGARAN.md](AUDIT-KESEGARAN.md)** | Dua audit isi. Bagian A memeriksa peta dua belas teknologi — seluruhnya berstatus sebagian usang. Bagian B memeriksa empat bidang baru/berubah di 3.2. |
| **[AUDIT-KELAYAKAN.md](AUDIT-KELAYAKAN.md)** | Dua audit kelayakan. Bagian A memeriksa enam sumber berita, Bagian B memeriksa empat pilihan arsitektur. |
| **[docs/SESSION-LOG.md](docs/SESSION-LOG.md)** | Catatan sesi kerja, urutan terbaru di atas. |

### Aturan pemisahan yang dipakai

`KERANGKA.md` merekam **apa yang dikatakan pemilik, apa adanya**. Hasil pemeriksaan fakta ditaruh di berkas audit terpisah, supaya kata-kata pemilik tidak tercampur dengan koreksi. Tidak ada koreksi audit yang diterapkan ke `KERANGKA.md` tanpa keputusan eksplisit — satu-satunya bagian tulisan asisten di sana adalah 4.20 *Rekonsiliasi*, dan itu sudah ditandai sebagai pengecualian.

---

## Dua Belas Bidang Teknologi

AI & Machine Learning · AI Agents · Cybersecurity · Robotics · Quantum Computing · Biotechnology · Cloud · Blockchain · Renewable Energy · Space Technology · IoT · AR/VR

Ditambah satu kategori terbuka: **teknologi baru yang belum muncul**.

## Tujuh Bagian Aplikasi

```
TechVerse X
│
├── 🌐 Explore          Jelajahi teknologi
├── 🧠 Learn            Roadmap & pembelajaran
├── 🧪 Labs             Eksperimen & project
├── 🤖 AI               AI Mentor
├── 📰 Intelligence     Berita & perkembangan terbaru
├── 🕸 Knowledge Graph  Hubungan antar teknologi
└── 🚀 Future           Emerging technologies
```

> ⚠️ Daftar bidang dan daftar bagian aplikasi di atas **belum bisa hidup berdua sebagai navigasi utama**. Itu keputusan struktural terbesar yang masih terbuka — Issue [#1](../../issues/1).

---

## Temuan Audit yang Paling Menentukan

Tiga audit dijalankan pada 2026-09-02 (total 29 agen, nol gagal). Yang paling perlu diketahui sebelum menulis baris kode pertama:

1. **⏰ `.NET 9` berhenti didukung 10 November 2026.** Rancangan awal memakainya. Yang benar `.NET 10` (LTS sampai 14 November 2028). Turun ke `.NET 8` tidak menolong — berakhir di tanggal yang sama persis. → [B1](AUDIT-KELAYAKAN.md#b1-net-9-sudah-kedaluwarsa--pakai-net-10)
2. **LangGraph tidak punya SDK .NET.** Memakainya diam-diam berarti dua runtime. Alternatif asli .NET: Microsoft Agent Framework. → [B2](AUDIT-KELAYAKAN.md#b2-langgraph-tidak-punya-sdk-net--ini-keputusan-besar-yang-belum-anda-ambil)
3. **Auth.js secara desain bukan penerbit token untuk API di luar aplikasi Next.js-nya.** Perlu pola pengganti. → [B3](AUDIT-KELAYAKAN.md#b3-authjs-bukan-penerbit-token-untuk-backend-net)
4. **Kurikulumnya akan lahir usang kalau dipakai apa adanya** — AutoGen sudah maintenance mode, Sora ditutup, GPT-5 dipensiunkan. → [Enam Kesalahan Paling Berbahaya](AUDIT-KESEGARAN.md#enam-kesalahan-paling-berbahaya)
5. **Otomatisasi berita layak, tapi GitHub Trending tidak punya API** dan hanya arXiv yang jelas boleh dipublikasi ulang. → [Bagian A](AUDIT-KELAYAKAN.md#bagian-a--enam-sumber-berita)

Kedua berkas audit membawa peringatan metodenya masing-masing. **Baca bagian peringatan di kaki tiap berkas sebelum mengutip angkanya** — sebagian temuan bersandar pada liputan sekunder, dan ketentuan layanan OpenAI serta Microsoft belum pernah dibaca siapa pun karena halamannya memblokir bot.

---

## Cara Membaca Repositori Ini

1. Buka **[KERANGKA.md](KERANGKA.md)** — bagian *[Yang Perlu Anda Putuskan atau Lengkapi](KERANGKA.md#yang-perlu-anda-putuskan-atau-lengkapi)* di bagian bawah adalah inti kerjanya.
2. Buka **[Issues](../../issues)** — 29 dari 30 butir keputusan sudah jadi issue tersendiri, berlabel dan bermilestone. Satu butir (E10) belum.
3. Selesaikan milestone **Fase 0 — Kunci Kerangka** sebelum menulis kode apa pun.

---

## Lisensi

Belum ditentukan. Repositori privat.
