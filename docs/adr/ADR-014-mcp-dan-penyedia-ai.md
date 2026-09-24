# ADR-014 — Peran MCP ke dalam dulu, dan penyedia AI tidak dikunci

**Status:** Diterima. Menutup Issue [#16](https://github.com/xtheoputra/techverse-x/issues/16).
**Tanggal:** 2026-09-04

## Konteks

Dua baris di `KERANGKA.md` 2.6 belum jelas maksudnya: **MCP** terdaftar sebagai
lapisan "Protocol" tanpa penjelasan untuk apa, dan lapisan AI hanya menulis
**"OpenAI API"** — padahal Bagian 1 dokumen yang sama menyebut Anthropic dan
Google DeepMind sebagai pemain utama.

MCP punya dua arah pakai yang konsekuensinya sangat berbeda: **ke dalam** (AI
Mentor mengakses Knowledge Graph, basis data konten, dan indeks vektor sebagai
tool) dan **ke luar** (aplikasi lain seperti Claude Code, Cursor, atau ChatGPT
mengakses TechVerse sebagai sumber pengetahuan).

## Keputusan

### MCP ke dalam untuk V1. Ke luar dicatat sebagai peluang, tidak dijadwalkan.

- **Ke dalam** dibangun di V1: AI Mentor memanggil konten TechVerse sebagai
  tool. Ini yang membuat AI Mentor menjawab dari bahan sendiri alih-alih
  mengarang.
- **Ke luar tidak dijadwalkan**, tapi ditulis di sini supaya tidak hilang: server
  MCP publik yang bisa dipasang orang lain adalah **peluang produk tersendiri**,
  dan bentuk distribusi yang jauh lebih murah daripada mengejar trafik web.

Syaratnya jelas dan sengaja tinggi: **ke luar baru masuk akal setelah ada minimal
satu bidang penuh berstatus `tinjau`.** Membuka server MCP di atas konten `draf`
berarti menyebarkan teks yang belum diperiksa ke dalam alat kerja orang lain —
persis kerusakan yang [ADR-012](ADR-012-template-halaman.md) berusaha cegah, tapi
dengan jangkauan lebih luas dan tanpa label yang ikut terbawa.

### Penyedia AI tidak dikunci

Penguncian ke satu penyedia **tidak disengaja** — itu penyederhanaan penulisan
tabel. Keputusan: kode bicara ke abstraksi, bukan ke penyedia.

Abstraksinya **datang gratis** dari [ADR-007](ADR-007-agent-platform.md):
Microsoft Agent Framework mendukung OpenAI, Anthropic, dan Ollama sekaligus. Jadi
tidak ada lapisan tambahan yang perlu ditulis untuk keputusan ini — yang perlu
dijaga hanya disiplin: **tidak memanggil SDK penyedia langsung dari kode fitur.**

Default V1 tetap OpenAI. Yang berubah bukan penyedianya, melainkan bahwa
menggantinya nanti adalah perubahan konfigurasi, bukan perubahan arsitektur.

Alasan ini bukan teoretis. Audit mencatat lini model berputar sangat cepat
sepanjang 2026 — **GPT-5 dipensiunkan dari ChatGPT 13 Februari 2026** dan
**Sora ditutup 26 April 2026**. Mengunci diri ke satu penyedia di pasar yang
mempensiunkan produknya dalam hitungan bulan adalah taruhan yang tidak perlu
diambil, apalagi ketika tidak mengambilnya gratis.

## Konsekuensi

- Nama model **tidak pernah ditulis di kode fitur**; ia konfigurasi.
- Ollama disebut di Agent Framework, jadi jalur "jalankan model lokal untuk
  meredam biaya" terbuka tanpa perubahan arsitektur — relevan langsung dengan
  pagu di [ADR-016](ADR-016-pagu-biaya.md).
- Server MCP ke luar, kalau nanti dibangun, **hanya menyajikan halaman berstatus
  `tinjau`**. Status halaman jadi penjaga distribusi, bukan sekadar label
  tampilan.
