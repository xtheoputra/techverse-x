# Catatan Sesi Kerja

Urutan terbaru di atas.

---

## 2026-09-02 — Sesi 1: Kerangka awal + tiga audit

### Yang dikerjakan

**Titik mulai:** folder kosong total. Nol berkas, nol subfolder, belum ada `.git`.

**Cara kerja yang disepakati pemilik:** pemilik mendiktekan kerangka bertahap dengan kata-kata yang belum tertata; tugas asisten merapikan, menandai benturan dan lubang, lalu menyimpannya ke `KERANGKA.md`. **Koreksi tidak boleh diterapkan diam-diam** — kata-kata pemilik direkam apa adanya, hasil pemeriksaan fakta ditaruh di berkas audit terpisah.

**Tiga gelombang dikte:**

1. **Peta teknologi 2026** — dua belas teknologi, lima pilar, tabel prioritas pribadi. Tercatat sebagai Bagian 1.
2. **Produk & dashboard** — tujuan, struktur menu 8 bidang / 41 halaman, template halaman, empat fitur AI, arsitektur teknologi, konsep basis data, Visi 2.0, rencana 6 bulan. Tercatat sebagai Bagian 2.
3. **Identitas & struktur aplikasi** — revisi jadi 12 bidang, plus tujuh bagian aplikasi (Explore, Learn, Labs, AI, Intelligence, Knowledge Graph, Future). Tercatat sebagai Bagian 3.

### Tiga audit dijalankan

Total **29 agen paralel**, nol gagal.

| Audit | Agen | Hasil |
|---|---|---|
| Kesegaran peta 12 teknologi | 15 | Kedua belas entri berstatus `sebagian-usang` |
| Kelayakan sumber berita + tumpukan | 10 | Keempat pilihan arsitektur bermasalah |
| Empat bidang baru/berubah | 4 | Renewable Energy kehilangan 2 dari 3 isinya |

### Temuan yang mengubah keputusan

- **`.NET 9` berhenti didukung 10 November 2026** — dan `.NET 8` berakhir di tanggal yang sama, jadi "turun ke LTS" tidak menolong. Yang benar `.NET 10`.
- **LangGraph tidak punya SDK .NET** — rancangan awal diam-diam berarti dua runtime. Alternatif asli .NET: Microsoft Agent Framework (`Microsoft.Agents.AI` 1.0.0 GA 2 April 2026).
- **Auth.js bukan penerbit token untuk API di luar aplikasi Next.js-nya** — cookie-nya JWE terenkripsi.
- **Qdrant belum diperlukan** pada skala ini; pgvector memadai.
- **AutoGen, Sora, dan GPT-5 semuanya sudah mati atau pensiun** — kurikulum akan lahir usang kalau dipakai apa adanya.
- **GitHub Trending tidak punya API** — harus dibangun sendiri dari REST API + delta bintang harian.
- **Hanya arXiv yang jelas boleh dipublikasi ulang** (metadata CC0). Lima sumber lain bersyarat.
- **Renewable Energy:** Fusion Energy dan Solid-State Battery keduanya tidak sah di bawah label itu; hanya Perovskite yang bertahan.

### Koreksi yang dilakukan asisten atas dirinya sendiri

Asisten sempat menduga **Edge AI wajar ditempatkan di bawah IoT**. Audit menolaknya: beban belajar Edge AI adalah ML bukan jaringan, permukaan terbesarnya bukan perangkat IoT, dan tinyML Foundation sudah berganti nama jadi **Edge AI Foundation**. Dicatat sebagai E12 — Edge AI perlu bidangnya sendiri.

### Batasan mutu yang perlu diingat

- Audit ketiga: **kuota WebSearch keempat agen habis** sebelum sempat dipakai, sehingga verifikasi terpaksa lewat WebFetch ke URL tebakan. Bagian taksonomi tetap kuat; bagian "pemain terkini" lemah dan sebagian bersandar Wikipedia.
- Beberapa klaim luar biasa **belum diverifikasi independen** — misalnya akuisisi Cursor oleh SpaceX. Sudah ditandai di berkas audit.
- **Ketentuan layanan OpenAI dan Microsoft belum dibaca siapa pun** — halamannya memblokir bot. Ini pekerjaan manusia dan harus selesai sebelum tayang.

### Keadaan akhir sesi

Repositori dibuat dan didorong ke GitHub sebagai **privat**. Seluruh keputusan yang menunggu dipindahkan menjadi GitHub Issues berlabel dan bermilestone.

**Belum ada satu baris kode pun.** Itu disengaja — milestone *Fase 0 — Kunci Kerangka* harus selesai lebih dulu.
