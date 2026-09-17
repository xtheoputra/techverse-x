# ADR-006 — Neo4j belum dipakai; sisi graf disimpan di PostgreSQL

**Status:** Diterima untuk V1 - Neo4j tidak dipasang. Ditegaskan 2026-09-04 lewat Issue [#20](../../../../issues/20).
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

---

## Pembaruan 2026-09-04 - naik dari "ditunda" jadi keputusan V1

Issue #20 menanyakan apakah relasi Knowledge Graph disimpan di tabel PostgreSQL
atau basis data graf tersendiri. Jawabannya menegaskan ADR ini: **PostgreSQL,
dan Neo4j tidak dipasang di V1.**

Yang berubah cuma statusnya - dari "belum sekarang" jadi "tidak untuk V1", dengan
alasan berangka: pada 14 bidang dan 82 topik
([ADR-010](ADR-010-taksonomi-bidang.md)), jumlah sisi graf ada di orde ratusan.
Itu skala yang membutuhkan satu `JOIN`, bukan mesin graf.

---

## Pembaruan 2026-09-17 - daftar penjaganya ternyata separuh, dan kini ada layarnya

Daftar di bagian Keputusan menulis tabelnya *"sudah memuat penjaga"*: kunci unik
`(From, To, Kind)` dan penjaga domain untuk sisi ke diri sendiri. Diukur saat
produsen pertamanya dibangun ([ADR-023](ADR-023-knowledge-graph-dasar.md)), daftar
itu kurang dua hal yang justru paling mendasar:

- **ujung tujuan sama sekali tidak berkunci asing** - `INSERT` ke
  `ToTechnologyId` acak masuk, dan menghapus topik yang masih dibutuhkan
  meninggalkan sisi yang menggantung;
- **penolakan sisi ke diri sendiri hanya hidup di domain**, tidak di basis data.

Penjaganya sekarang: indeks unik `(From, To, Kind)` + **kunci asing di KEDUA
ujung** (tujuan `RESTRICT`, migrasi `RelasiAntarTopik`) + CHECK sisi ke diri
sendiri + CHECK `Kind` yang SQL-nya dibangkitkan dari enum + aturan dua topik
tidak boleh saling mensyaratkan di agregat.

Kalimat Konsekuensi *"belum ada layar yang memintanya"* kini punya layar: blok
**"Topik terhubung"** di halaman topik. Ia tetap satu lompatan - satu `JOIN` per
arah - jadi kueri graf yang dalam masih belum dibutuhkan, dan EPIC 10 tetap
proyeksi satu arah dari tabel ini. Produksi berisi nol sisi.
