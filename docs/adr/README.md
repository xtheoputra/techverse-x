# Architecture Decision Record

Satu berkas per keputusan. Nama berkasnya mengikuti daftar yang sudah ditetapkan
`KERANGKA.md` 4.14, supaya rencana dan kenyataan memakai nomor yang sama.

| ADR | Keputusan | Status |
|---|---|---|
| [001](ADR-001-nextjs.md) | Next.js 16 App Router untuk `apps/web` | Diterima |
| [002](ADR-002-dotnet.md) | Menargetkan `.NET 10` | Diterima sementara — Issue [#12](../../../../issues/12) |
| [003](ADR-003-postgresql.md) | PostgreSQL + EF Core, schema per bounded context | Diterima |
| [004](ADR-004-event-driven.md) | Event dikumpulkan di agregat, bus belum dipasang | Diterima |
| [005](ADR-005-qdrant.md) | Qdrant **belum** dipakai | Ditunda — Issue [#15](../../../../issues/15) |
| [006](ADR-006-neo4j.md) | Neo4j **belum** dipakai; sisi graf di PostgreSQL | Ditunda |
| [007](ADR-007-agent-platform.md) | Orkestrasi agen **belum** dipilih | Ditunda — Issue [#13](../../../../issues/13) |
| [008](ADR-008-batas-fase-1.md) | Batas Fase 1: apa yang sengaja TIDAK dibangun | Diterima |

## Bentuk yang dipakai

Konteks · Keputusan · Konsekuensi · Status. Kalau sebuah ADR menyatakan
"ditunda", ia tetap ditulis — keputusan untuk **belum** memutuskan juga
keputusan, dan alasannya sama pentingnya untuk dicatat.
