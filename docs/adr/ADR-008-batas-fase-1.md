# ADR-008 — Batas Fase 1: apa yang sengaja TIDAK dibangun

**Status:** Diterima
**Tanggal:** 2026-09-03

## Konteks

`README.md` dan `KERANGKA.md` sama-sama menyatakan aturan yang dibuat pemilik sendiri:

> Selesaikan milestone **Fase 0 — Kunci Kerangka** sebelum menulis kode apa pun.

Pemilik kemudian meminta dokumen dieksekusi menjadi kode. Saat itu **seluruh 22 issue masih terbuka**, termasuk empat berlabel `blocker`: [#1](../../../../issues/1) tulang punggung navigasi, [#3](../../../../issues/3) nasib 41 submenu, [#10](../../../../issues/10) siapa menulis 287 blok konten, dan [#17](../../../../issues/17) ketentuan layanan sumber berita.

Dua hal itu bertabrakan. ADR ini mencatat bagaimana tabrakan itu diselesaikan.

## Keputusan

Yang dibangun **hanya Phase 1 — Skeleton** dari `KERANGKA.md` 4.17:

> Repository · Docker · .NET · Next.js · PostgreSQL · Redis · CI

Alasannya satu dan bisa diuji: **tidak ada satu pun butir Phase 1 yang jawabannya berubah tergantung keempat blocker itu.** Postgres tetap jalan dengan cara yang sama entah navigasinya berbasis bidang atau berbasis fungsi. Health check, correlation id, migrasi, dan CI tidak peduli siapa yang menulis isi kurikulum.

Karena itu Phase 1 tidak berjudi atas keputusan yang belum diambil — ia menyiapkan tanah yang sama untuk semua kemungkinan hasilnya.

## Yang sengaja TIDAK dibangun

| Tidak dibangun | Alasan |
|---|---|
| Navigasi, menu, taksonomi bidang | Issue [#1](../../../../issues/1) dan [#3](../../../../issues/3) — ini justru keputusan strukturalnya |
| Tabel `categories` / `concepts` / `tools` / `skills` | Skema penuh masih Issue [#20](../../../../issues/20) |
| Autentikasi, `apps/admin` | Pola auth masih Issue [#14](../../../../issues/14); audit B3 membuktikan Auth.js tidak bisa mengerjakannya |
| Kode AI apa pun, `agents/`, `apps/ai-gateway` | Issue [#13](../../../../issues/13) dan [#16](../../../../issues/16) — lihat [ADR-007](ADR-007-agent-platform.md) |
| Qdrant / pgvector / RAG | Issue [#15](../../../../issues/15) — lihat [ADR-005](ADR-005-qdrant.md) |
| Neo4j | EPIC 10 — lihat [ADR-006](ADR-006-neo4j.md) |
| Pengambilan berita, Research Pipeline | Issue [#17](../../../../issues/17): ketentuan layanan OpenAI dan Microsoft **belum dibaca siapa pun**. Mengambil sebelum itu dibaca adalah risiko hukum, bukan risiko teknis |
| NATS / Kafka / Temporal / OpenSearch | Belum ada yang perlu diangkut, dicari, atau dijalankan lama |
| Dockerfile, Terraform, Kubernetes | EPIC 14. Belum ada yang layak di-deploy |
| Isi kurikulum sungguhan | Issue [#10](../../../../issues/10) dan [#18](../../../../issues/18). Peta 12 teknologinya sendiri masih menunggu koreksi audit — mengisinya sekarang berarti menyemai data yang sudah usang sejak hari pertama |

## Konsekuensi

- Repositori sekarang punya kode yang **jalan dan terbukti jalan**, tanpa satu pun blocker Fase 0 ikut terputuskan diam-diam.
- Aturan "jangan menulis kode sebelum Fase 0 selesai" **sudah dilanggar sebagian**, dan ini catatannya. Yang dilanggar batas waktunya, bukan substansinya: tidak ada keputusan Fase 0 yang jadi terkunci karena kode ini ada.
- Satu-satunya keputusan Fase 0 yang praktis sudah diambil di kode adalah **`.NET 10`** (Issue [#12](../../../../issues/12)) — lihat [ADR-002](ADR-002-dotnet.md). Itu butir yang punya tenggat dunia nyata kurang dari sepuluh minggu dan vonis auditnya berkeyakinan tinggi, jadi menundanya lebih mahal daripada mengambilnya.
- Begitu Issue #1 ditutup, yang perlu ditulis adalah layar dan taksonomi — bukan membongkar fondasinya.
