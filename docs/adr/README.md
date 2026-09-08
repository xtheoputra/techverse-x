# Architecture Decision Record

Satu berkas per keputusan. Nama berkasnya mengikuti daftar yang sudah ditetapkan
`KERANGKA.md` 4.14, supaya rencana dan kenyataan memakai nomor yang sama.

| ADR | Keputusan | Status |
|---|---|---|
| [001](ADR-001-nextjs.md) | Next.js 16 App Router untuk `apps/web` | Diterima |
| [002](ADR-002-dotnet.md) | Menargetkan `.NET 10` | Diterima - Issue [#12](../../../../issues/12) ditutup |
| [003](ADR-003-postgresql.md) | PostgreSQL + EF Core, schema per bounded context | Diterima |
| [004](ADR-004-event-driven.md) | Event dikumpulkan di agregat, bus belum dipasang | Diterima |
| [005](ADR-005-qdrant.md) | **pgvector, tanpa Qdrant** | Diterima - Issue [#15](../../../../issues/15) ditutup |
| [006](ADR-006-neo4j.md) | Graf di PostgreSQL, **Neo4j tidak dipasang di V1** | Diterima |
| [007](ADR-007-agent-platform.md) | **Microsoft Agent Framework**, LangGraph dibuang | Diterima - Issue [#13](../../../../issues/13) ditutup |
| [008](ADR-008-batas-fase-1.md) | Batas Fase 1: apa yang sengaja TIDAK dibangun | Diterima |
| [009](ADR-009-tulang-punggung-navigasi.md) | Nav utama = fungsi, tiap bidang punya URL kanonik | Diterima - Issue [#1](../../../../issues/1), [#9](../../../../issues/9) |
| [010](ADR-010-taksonomi-bidang.md) | **14 bidang, 82 topik**, taksonomi 41 submenu disusun ulang | Diterima - Issue [#3](../../../../issues/3)-[#8](../../../../issues/8) |
| [011](ADR-011-nama-produk.md) | Nama produk **TechVerse X** | Diterima - Issue [#2](../../../../issues/2) |
| [012](ADR-012-template-halaman.md) | Template 5 bagian + tiga tingkat kematangan konten | Diterima - Issue [#10](../../../../issues/10), [#11](../../../../issues/11), [#18](../../../../issues/18), [#19](../../../../issues/19) |
| [013](ADR-013-autentikasi.md) | **V1 tanpa login**; nanti Clerk sebagai IdP | Diterima sebagai pola - Issue [#14](../../../../issues/14) |
| [014](ADR-014-mcp-dan-penyedia-ai.md) | MCP ke dalam dulu; penyedia AI tidak dikunci | Diterima - Issue [#16](../../../../issues/16) |
| [015](ADR-015-skema-data-v1.md) | Skema data V1: 9 entitas, **dua sumbu status** | Diterima - Issue [#20](../../../../issues/20); `Field` + `ContentMaturity` sudah mendarat |
| [016](ADR-016-pagu-biaya.md) | Pagu **USD 60/bulan** + aturan penerbitan ulang | Diterima - Issue [#22](../../../../issues/22) |
| [017](ADR-017-platform-hosting.md) | Platform hosting: Render berbayar, dan bundel migrasi ikut ke citra `api` | ⛔ **DIGANTIKAN [019](ADR-019-hosting-gratis-tanpa-kartu.md)** - Issue [#33](../../../../issues/33) |
| [018](ADR-018-rilis-citra-dan-reproducibility.md) | Rilis citra melewati commit dokumen; **reproducibility TIDAK dikejar** | Diterima - Issue [#32](../../../../issues/32) |
| [019](ADR-019-hosting-gratis-tanpa-kartu.md) | **Vercel + Koyeb + Neon** - gratis, tanpa kartu. Menggantikan 017 | Diterima |

⚠️ **ADR-017 dibiarkan utuh meski digantikan**, karena dua bagiannya masih benar
dan masih dipakai: pemeriksaan empat syarat yang menemukan **bentuk citra
`migrate` menyeleksi platform** (itulah asal-usul `/app/efbundle`), dan alasan
Fly.io gugur.

Ringkasan seluruh 22 keputusan dalam satu halaman:
[`docs/KEPUTUSAN.md`](../KEPUTUSAN.md). Rencana yang menggantikan `KERANGKA.md`
2.9: [`docs/RENCANA-V1.md`](../RENCANA-V1.md).

**Penomoran 001-007 mengikuti daftar `KERANGKA.md` 4.14.** Nomor 008 ke atas
lahir setelahnya dan tidak ada di daftar itu - wajar, karena daftar 4.14 ditulis
sebelum keputusannya ada.

## Bentuk yang dipakai

Konteks · Keputusan · Konsekuensi · Status. Kalau sebuah ADR menyatakan
"ditunda", ia tetap ditulis — keputusan untuk **belum** memutuskan juga
keputusan, dan alasannya sama pentingnya untuk dicatat.
