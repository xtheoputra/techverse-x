# ADR-011 — Nama produk: TechVerse X

**Status:** Diterima. Menutup Issue [#2](../../../../issues/2).
**Tanggal:** 2026-09-04

## Konteks

Tiga nama bertabrakan: `KERANGKA.md` 3.1 menulis **TechVerse X**, nama folder
proyek **TechVerse X Labs**, dan salah satu dari tujuh bagian aplikasi bernama
**Labs**.

Kalau produknya bernama "TechVerse X Labs" sekaligus punya bagian bernama
"Labs", kalimat seperti "buka Labs" jadi ambigu di dalam produk sendiri —
ambiguitas yang muncul setiap hari, bukan sekali.

## Keputusan

Nama produk **TechVerse X**.

- Repositori `techverse-x` **tidak perlu diganti** — sudah cocok.
- Nama folder di mesin pemilik boleh tetap "TechVerse X Labs". Nama folder bukan
  nama produk, dan mengubahnya cuma memutus jalur di skrip tanpa manfaat.
- **"Labs" tetap dipakai, tapi hanya sebagai nama bagian aplikasi** (`/labs`),
  bukan bagian dari nama produk.

Ikutan yang terkunci bersama keputusan ini: judul halaman `TechVerse X`, nama
paket npm/NuGet berawalan `techverse-x` / `TechVerseX` (sudah dipakai di kode
Fase 1), dan metadata Open Graph memakai `TechVerse X`.

## Konsekuensi

- Domain belum dibeli dan **belum diperiksa ketersediaannya** — itu pekerjaan
  pemilik, bukan keputusan arsitektur. Dicatat di
  [ADR-016](ADR-016-pagu-biaya.md) sebagai pos biaya.
- Kalau nanti nama produk berubah, yang ikut berubah cuma teks — bukan struktur,
  karena tidak ada satu pun URL di [ADR-009](ADR-009-tulang-punggung-navigasi.md)
  yang memuat nama produk.
