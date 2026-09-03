# ADR-003 — PostgreSQL + EF Core, schema per bounded context

**Status:** Diterima
**Tanggal:** 2026-09-03

## Konteks

`KERANGKA.md` 4.7 (Database Ownership) menetapkan tiap bounded context punya
ownership atas datanya, tetapi pada fase awal semuanya tinggal di **satu**
PostgreSQL dengan **schema per context** — lalu dipisah kalau skalanya menuntut.

Rancangan skema penuhnya sendiri masih Issue [#20](../../../../issues/20).

## Keputusan

- Satu PostgreSQL, schema `technology` untuk bounded context pertama.
- EF Core dengan migrasi yang tinggal di dalam proyek layanan, bukan di folder
  pusat. Migrasi adalah milik konteks yang memilikinya; menaruhnya terpusat
  membuat pemisahan basis data nanti jadi mahal.
- Tabel riwayat migrasi ikut masuk ke schema-nya sendiri
  (`technology.__ef_migrations_history`), bukan `public`. Kalau tidak, dua
  konteks akan berebut satu tabel riwayat dan pemisahannya nanti berantakan.
- Enum disimpan sebagai **teks**, bukan angka: dump basis data harus terbaca
  manusia, dan menyisipkan anggota enum baru tidak boleh diam-diam mengubah
  arti baris lama.
- Kunci primer memakai **UUID v7** (`Guid.CreateVersion7`) — berurut menurut
  waktu, jadi indeks primernya tidak terfragmentasi seperti UUID acak.

`database/migrations/` tetap dipakai seperti di 4.7, tapi isinya **SQL hasil
generate** (`make db-script`) — bukan sumber kebenarannya. Gunanya supaya
perubahan skema bisa dibaca sebagai SQL sebelum menyentuh produksi.

## Konsekuensi

- Menambah bounded context berarti menambah `DbContext` dan schema-nya sendiri.
- `Technology.Category` sengaja **teks bebas, bukan kunci asing**. Taksonomi
  bidang masih Issue [#1](../../../../issues/1) dan [#3](../../../../issues/3);
  membuat tabel kategori sekarang berarti menebak keputusan yang belum diambil.
- pgvector **tidak** dipasang — lihat [ADR-005](ADR-005-qdrant.md).
