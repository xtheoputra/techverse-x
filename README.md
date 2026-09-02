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
| Rencana 6 bulan | Roadmap belajar |

---

## Isi Repositori

| Berkas | Isi |
|---|---|
| **[KERANGKA.md](KERANGKA.md)** | Dokumen utama. Semua yang didiktekan pemilik, dirapikan, plus daftar keputusan yang menunggu. **Mulai dari sini.** |
| **[AUDIT-KESEGARAN.md](AUDIT-KESEGARAN.md)** | Pemeriksaan fakta atas isi peta teknologi. Menemukan seluruh dua belas entri berstatus sebagian usang. |
| **[AUDIT-KELAYAKAN.md](AUDIT-KELAYAKAN.md)** | Pemeriksaan enam sumber berita dan empat pilihan arsitektur. |
| **[docs/SESSION-LOG.md](docs/SESSION-LOG.md)** | Catatan sesi kerja. |

### Aturan pemisahan yang dipakai

`KERANGKA.md` merekam **apa yang dikatakan pemilik, apa adanya**. Hasil pemeriksaan fakta ditaruh di berkas audit terpisah, supaya kata-kata pemilik tidak tercampur dengan koreksi. Tidak ada koreksi audit yang diterapkan ke `KERANGKA.md` tanpa keputusan eksplisit.

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

---

## Temuan Audit yang Paling Menentukan

Tiga audit dijalankan pada 2026-09-02 (total 29 agen, nol gagal). Yang paling perlu diketahui sebelum menulis baris kode pertama:

1. **⏰ `.NET 9` berhenti didukung 10 November 2026.** Rancangan awal memakainya. Yang benar `.NET 10` (LTS sampai 14 November 2028). Turun ke `.NET 8` tidak menolong — berakhir di tanggal yang sama persis.
2. **LangGraph tidak punya SDK .NET.** Memakainya diam-diam berarti dua runtime. Alternatif asli .NET: Microsoft Agent Framework.
3. **Auth.js secara desain bukan penerbit token untuk API di luar aplikasi Next.js-nya.** Perlu pola pengganti.
4. **Kurikulumnya akan lahir usang kalau dipakai apa adanya** — AutoGen sudah maintenance mode, Sora ditutup, GPT-5 dipensiunkan.
5. **Otomatisasi berita layak, tapi GitHub Trending tidak punya API** dan hanya arXiv yang jelas boleh dipublikasi ulang.

Rincian lengkap ada di kedua berkas audit.

---

## Cara Membaca Repositori Ini

1. Buka **[KERANGKA.md](KERANGKA.md)** — bagian *"Yang Perlu Anda Putuskan atau Lengkapi"* di bagian bawah adalah inti kerjanya.
2. Buka **[Issues](../../issues)** — setiap keputusan yang menunggu sudah jadi issue tersendiri, berlabel dan bermilestone.
3. Selesaikan milestone **Fase 0 — Kunci Kerangka** sebelum menulis kode apa pun.

---

## Lisensi

Belum ditentukan. Repositori privat.
