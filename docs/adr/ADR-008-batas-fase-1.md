# ADR-008 — Batas Fase 1: apa yang sengaja TIDAK dibangun

**Status:** Diterima
**Tanggal:** 2026-09-03

## Konteks

`README.md` dan `KERANGKA.md` sama-sama menyatakan aturan yang dibuat pemilik sendiri:

> Selesaikan milestone **Fase 0 — Kunci Kerangka** sebelum menulis kode apa pun.

Pemilik kemudian meminta dokumen dieksekusi menjadi kode. Saat itu **seluruh 22 issue masih terbuka**, termasuk empat berlabel `blocker`: [#1](https://github.com/xtheoputra/techverse-x/issues/1) tulang punggung navigasi, [#3](https://github.com/xtheoputra/techverse-x/issues/3) nasib 41 submenu, [#10](https://github.com/xtheoputra/techverse-x/issues/10) siapa menulis 287 blok konten, dan [#17](https://github.com/xtheoputra/techverse-x/issues/17) ketentuan layanan sumber berita.

Dua hal itu bertabrakan. ADR ini mencatat bagaimana tabrakan itu diselesaikan.

## Keputusan

Yang dibangun **hanya Phase 1 — Skeleton** dari `KERANGKA.md` 4.17:

> Repository · Docker · .NET · Next.js · PostgreSQL · Redis · CI

Alasannya satu dan bisa diuji: **tidak ada satu pun butir Phase 1 yang jawabannya berubah tergantung keempat blocker itu.** Postgres tetap jalan dengan cara yang sama entah navigasinya berbasis bidang atau berbasis fungsi. Health check, correlation id, migrasi, dan CI tidak peduli siapa yang menulis isi kurikulum.

Karena itu Phase 1 tidak berjudi atas keputusan yang belum diambil — ia menyiapkan tanah yang sama untuk semua kemungkinan hasilnya.

## Yang sengaja TIDAK dibangun

| Tidak dibangun | Alasan |
|---|---|
| Navigasi, menu, taksonomi bidang | Issue [#1](https://github.com/xtheoputra/techverse-x/issues/1) dan [#3](https://github.com/xtheoputra/techverse-x/issues/3) — ini justru keputusan strukturalnya |
| Tabel `categories` / `concepts` / `tools` / `skills` | Skema penuh masih Issue [#20](https://github.com/xtheoputra/techverse-x/issues/20) |
| Autentikasi, `apps/admin` | Pola auth masih Issue [#14](https://github.com/xtheoputra/techverse-x/issues/14); audit B3 membuktikan Auth.js tidak bisa mengerjakannya |
| Kode AI apa pun, `agents/`, `apps/ai-gateway` | Issue [#13](https://github.com/xtheoputra/techverse-x/issues/13) dan [#16](https://github.com/xtheoputra/techverse-x/issues/16) — lihat [ADR-007](ADR-007-agent-platform.md) |
| Qdrant / pgvector / RAG | Issue [#15](https://github.com/xtheoputra/techverse-x/issues/15) — lihat [ADR-005](ADR-005-qdrant.md) |
| Neo4j | EPIC 10 — lihat [ADR-006](ADR-006-neo4j.md) |
| Pengambilan berita, Research Pipeline | Issue [#17](https://github.com/xtheoputra/techverse-x/issues/17): ketentuan layanan OpenAI dan Microsoft **belum dibaca siapa pun**. Mengambil sebelum itu dibaca adalah risiko hukum, bukan risiko teknis |
| NATS / Kafka / Temporal / OpenSearch | Belum ada yang perlu diangkut, dicari, atau dijalankan lama |
| Dockerfile, Terraform, Kubernetes | EPIC 14. Belum ada yang layak di-deploy |
| Isi kurikulum sungguhan | Issue [#10](https://github.com/xtheoputra/techverse-x/issues/10) dan [#18](https://github.com/xtheoputra/techverse-x/issues/18). Peta 12 teknologinya sendiri masih menunggu koreksi audit — mengisinya sekarang berarti menyemai data yang sudah usang sejak hari pertama |

## Konsekuensi

- Repositori sekarang punya kode yang **jalan dan terbukti jalan**, tanpa satu pun blocker Fase 0 ikut terputuskan diam-diam.
- Aturan "jangan menulis kode sebelum Fase 0 selesai" **sudah dilanggar sebagian**, dan ini catatannya. Yang dilanggar batas waktunya, bukan substansinya: tidak ada keputusan Fase 0 yang jadi terkunci karena kode ini ada.
- Satu-satunya keputusan Fase 0 yang praktis sudah diambil di kode adalah **`.NET 10`** (Issue [#12](https://github.com/xtheoputra/techverse-x/issues/12)) — lihat [ADR-002](ADR-002-dotnet.md). Itu butir yang punya tenggat dunia nyata kurang dari sepuluh minggu dan vonis auditnya berkeyakinan tinggi, jadi menundanya lebih mahal daripada mengambilnya.
- Begitu Issue #1 ditutup, yang perlu ditulis adalah layar dan taksonomi — bukan membongkar fondasinya.

---

## Pembaruan 2026-09-04 - gerbangnya sebagian besar sudah terangkat

Pemilik mendelegasikan ke-22 keputusan yang tersisa, dan semuanya sudah diambil -
lihat [`KEPUTUSAN.md`](../KEPUTUSAN.md). **Tabel "Yang sengaja TIDAK dibangun" di
atas karenanya tidak lagi berlaku seluruhnya.** Yang berlaku sekarang:

| Dulu ditahan karena | Sekarang |
|---|---|
| Navigasi & taksonomi (#1, #3) | **Terbuka** - [ADR-009](ADR-009-tulang-punggung-navigasi.md), [ADR-010](ADR-010-taksonomi-bidang.md) |
| Skema penuh (#20) | **Terbuka** - [ADR-015](ADR-015-skema-data-v1.md). ~~menunggu PR #23~~ - ✅ #23 mendarat 7 Sep 2026; lihat Pembaruan di kaki berkas |
| Kode AI (#13, #16) | **Terbuka** - Microsoft Agent Framework, [ADR-007](ADR-007-agent-platform.md) |
| Vektor / RAG (#15) | **Terbuka** - pgvector, [ADR-005](ADR-005-qdrant.md) |
| Neo4j | **Ditutup untuk V1** - graf tetap di PostgreSQL, [ADR-006](ADR-006-neo4j.md) |
| Autentikasi (#14) | **Tetap TIDAK dibangun di V1** - itu keputusannya sendiri, [ADR-013](ADR-013-autentikasi.md) |
| Isi kurikulum (#10, #18) | **Terbuka** - [ADR-012](ADR-012-template-halaman.md) |
| Dockerfile / deployment | **Naik ke Bulan 1**, bukan Bulan 6 - [RENCANA-V1](../RENCANA-V1.md) |
| Pengambilan berita (#17) | **Sebagian saja: arXiv boleh, sisanya tetap ditahan** |

**Satu gerbang yang masih berdiri, dan sengaja dibiarkan berdiri: Issue
[#17](https://github.com/xtheoputra/techverse-x/issues/17).** Ketentuan layanan OpenAI dan Microsoft tetap belum
dibaca siapa pun - halaman OpenAI membalas 403 ke pengambil otomatis. Yang
berubah cuma cakupannya: V1 hanya menerbitkan ulang **arXiv** (metadata CC0),
jadi #17 tidak lagi menghalangi V1 tapi tetap menghalangi sumber lain. Aturan
lengkapnya di [ADR-016](ADR-016-pagu-biaya.md).

Kalimat penutup ADR ini - *"begitu Issue #1 ditutup, yang perlu ditulis adalah
layar dan taksonomi, bukan membongkar fondasinya"* - kini bisa diuji. Issue #1
sudah ditutup.


---

## Pembaruan 2026-09-07 - ketiga PR yang ditunggu sudah mendarat

Tabel di atas menyisakan satu baris yang menunggu: skema penuh **"menunggu PR
#23 di-merge dulu"**. Penantian itu selesai - **#23, #25, dan #27 semuanya
sudah mendarat di `main`**, dan `main` tidak lagi murni dokumen.

Yang tersisa dari tabel itu sekarang cuma satu baris yang benar-benar belum
bergerak: **Dockerfile / deployment**. Citranya sendiri sudah ada dan terbukti
melayani situsnya dari basis data kosong; yang belum ada adalah **keputusan
platform hosting**, dan itu bukan pekerjaan teknis. Lihat
[`RENCANA-V1.md`](../RENCANA-V1.md#keadaan-bulan-1-per-7-september-2026).
