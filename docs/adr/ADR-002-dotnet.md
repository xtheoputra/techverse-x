# ADR-002 — Menargetkan `.NET 10`

**Status:** Diterima sementara. Issue [#12](../../../../issues/12) masih terbuka.
**Tanggal:** 2026-09-03

## Konteks

`AUDIT-KELAYAKAN.md` B1 menemukan bahwa `.NET 9` berhenti didukung
**10 November 2026**, dan `.NET 8` berakhir pada tanggal yang sama persis —
sehingga "turun ke LTS" bukan jaring pengaman. `.NET 10` LTS sampai
**14 November 2028**.

Ini satu-satunya butir Fase 0 yang punya tenggat dunia nyata, dan tenggatnya
kurang dari sepuluh minggu dari hari ADR ini ditulis.

## Keputusan

Seluruh proyek menargetkan `net10.0`.

Angka versinya ditulis **satu kali saja**, di `Directory.Build.props`. Audit
memperingatkan bahwa "angka 9 akan tersebar di banyak tempat" — `TargetFramework`
tiap csproj, dua baris Dockerfile, `global.json`, workflow CI, konfigurasi Azure.
Di repositori ini yang tersisa dua: `Directory.Build.props` dan `global.json`.

`global.json` memakai `rollForward: latestFeature`, bukan pin ketat. Audit
menyebut `global.json` yang mem-pin SDK terlalu sempit sebagai "bom waktu
senyap" — runner CI dengan SDK lebih baru akan gagal build dengan pesan yang
menyesatkan.

## Konsekuensi

- Butuh SDK .NET 10 di mesin pengembang dan runner CI. CI memakai
  `global-json-file: global.json` supaya keduanya tidak bisa berbeda.
- Kalau pemilik menutup Issue #12 dengan pilihan lain, yang berubah satu baris.
- Dockerfile belum ada, jadi belum ada baris versi ketiga yang perlu dijaga.
