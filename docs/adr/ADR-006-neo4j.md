# ADR-006 — Neo4j belum dipakai; sisi graf disimpan di PostgreSQL

**Status:** Ditunda sampai EPIC 10.
**Tanggal:** 2026-09-03

## Konteks

`KERANGKA.md` 4.6 menetapkan pembagian yang jelas: PostgreSQL sebagai *source of truth*, Neo4j sebagai *derived knowledge*.

## Keputusan

Sisi graf (`technology_relationships`) disimpan di PostgreSQL sejak sekarang; Neo4j belum dipasang.

Ini bukan penyimpangan dari 4.6, justru penerapannya: kalau PostgreSQL memang sumber kebenaran, maka daftar sisi graf memang lahir di sana. Neo4j nanti diisi dari sana, bukan sebaliknya.

Tabelnya sudah memuat penjaga yang tidak datang gratis di graph database:

- Kunci unik `(From, To, Kind)` — sisi yang sama tidak bisa tercatat dua kali.
- Penjaga domain yang menolak teknologi berhubungan dengan dirinya sendiri.

## Konsekuensi

- Kueri graf yang dalam (jalur, tetangga berjarak-n) belum bisa dilakukan efisien. Itu memang belum dibutuhkan — belum ada layar yang memintanya.
- Saat EPIC 10 tiba, yang dibangun adalah proyeksi satu arah dari PostgreSQL ke Neo4j. Tidak ada data yang perlu dipindahkan, hanya disalin.
- `AUDIT-KELAYAKAN.md` menandai Neo4j sebagai pos biaya produksi baru yang belum masuk hitungan. Menundanya ikut menunda biayanya (Issue [#22](../../../../issues/22)).
