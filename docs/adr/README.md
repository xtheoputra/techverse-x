# Architecture Decision Record

Satu berkas per keputusan. Nama berkasnya mengikuti daftar yang sudah ditetapkan
`KERANGKA.md` 4.14, supaya rencana dan kenyataan memakai nomor yang sama.

| ADR | Keputusan | Status |
|---|---|---|
| [001](ADR-001-nextjs.md) | Next.js 16 App Router untuk `apps/web` | Diterima |
| [002](ADR-002-dotnet.md) | Menargetkan `.NET 10` | Diterima - Issue [#12](https://github.com/xtheoputra/techverse-x/issues/12) ditutup |
| [003](ADR-003-postgresql.md) | PostgreSQL + EF Core, schema per bounded context | Diterima |
| [004](ADR-004-event-driven.md) | Event dikumpulkan di agregat, bus belum dipasang | Diterima |
| [005](ADR-005-qdrant.md) | **pgvector, tanpa Qdrant** | Diterima - Issue [#15](https://github.com/xtheoputra/techverse-x/issues/15) ditutup |
| [006](ADR-006-neo4j.md) | Graf di PostgreSQL, **Neo4j tidak dipasang di V1** | Diterima |
| [007](ADR-007-agent-platform.md) | **Microsoft Agent Framework**, LangGraph dibuang | Diterima - Issue [#13](https://github.com/xtheoputra/techverse-x/issues/13) ditutup |
| [008](ADR-008-batas-fase-1.md) | Batas Fase 1: apa yang sengaja TIDAK dibangun | Diterima |
| [009](ADR-009-tulang-punggung-navigasi.md) | Nav utama = fungsi, tiap bidang punya URL kanonik | Diterima - Issue [#1](https://github.com/xtheoputra/techverse-x/issues/1), [#9](https://github.com/xtheoputra/techverse-x/issues/9) |
| [010](ADR-010-taksonomi-bidang.md) | **14 bidang, 82 topik**, taksonomi 41 submenu disusun ulang | Diterima - Issue [#3](https://github.com/xtheoputra/techverse-x/issues/3)-[#8](https://github.com/xtheoputra/techverse-x/issues/8) |
| [011](ADR-011-nama-produk.md) | Nama produk **TechVerse X** | Diterima - Issue [#2](https://github.com/xtheoputra/techverse-x/issues/2) |
| [012](ADR-012-template-halaman.md) | Template 5 bagian + tiga tingkat kematangan konten | Diterima - Issue [#10](https://github.com/xtheoputra/techverse-x/issues/10), [#11](https://github.com/xtheoputra/techverse-x/issues/11), [#18](https://github.com/xtheoputra/techverse-x/issues/18), [#19](https://github.com/xtheoputra/techverse-x/issues/19) |
| [013](ADR-013-autentikasi.md) | **V1 tanpa login**; nanti Clerk sebagai IdP | Diterima sebagai pola - Issue [#14](https://github.com/xtheoputra/techverse-x/issues/14) |
| [014](ADR-014-mcp-dan-penyedia-ai.md) | MCP ke dalam dulu; penyedia AI tidak dikunci | Diterima - Issue [#16](https://github.com/xtheoputra/techverse-x/issues/16). Bawaan OpenAI **diganti Ollama lokal** oleh [026](ADR-026-nol-biaya-gratis-mandiri.md) |
| [015](ADR-015-skema-data-v1.md) | Skema data V1: 9 entitas, **dua sumbu status** | Diterima - Issue [#20](https://github.com/xtheoputra/techverse-x/issues/20); `Field` + `ContentMaturity` sudah mendarat |
| [016](ADR-016-pagu-biaya.md) | Pagu **USD 60/bulan** + aturan penerbitan ulang | Diterima - Issue [#22](https://github.com/xtheoputra/techverse-x/issues/22). 🔴 **Pagunya dibatalkan** oleh [026](ADR-026-nol-biaya-gratis-mandiri.md); aturan penerbitan ulang tetap |
| [017](ADR-017-platform-hosting.md) | Platform hosting: Render berbayar, dan bundel migrasi ikut ke citra `api` | ⛔ **DIGANTIKAN [019](ADR-019-hosting-gratis-tanpa-kartu.md)** - Issue [#33](https://github.com/xtheoputra/techverse-x/issues/33) |
| [018](ADR-018-rilis-citra-dan-reproducibility.md) | Rilis citra melewati commit dokumen; **reproducibility TIDAK dikejar** | Diterima - Issue [#32](https://github.com/xtheoputra/techverse-x/issues/32) |
| [019](ADR-019-hosting-gratis-tanpa-kartu.md) | **Vercel + Koyeb + Neon** - gratis, tanpa kartu. Menggantikan 017 | Diterima - 🔴 **kaki `api` (Koyeb) gugur untuk akun baru** sejak 17 Feb 2026, lihat [025](ADR-025-host-api-pengganti-koyeb.md) |
| [020](ADR-020-permukaan-tulis-api.md) | **Endpoint tulis tidak dipasang di produksi** - melengkapi 013, yang hanya menimbang login PEMBACA | Diterima |
| [021](ADR-021-jalan-menuju-tinjau.md) | Jalan sah menuju `tinjau`: **workflow bergerbang**, nama pemeriksa dari `github.actor` | Diterima sebagai pola - **dibangun sesudah [#40](https://github.com/xtheoputra/techverse-x/issues/40)** |
| [022](ADR-022-pencarian-teks-penuh.md) | **Pencarian teks penuh PostgreSQL**: kolom `tsvector` terhitung di `technologies` DAN `fields`, kamus `english`, `websearch_to_tsquery`, dan cadangan **awal kata** untuk pengetikan sebagian | Diterima - **sudah dibangun** |
| [023](ADR-023-knowledge-graph-dasar.md) | **Knowledge Graph dasar**: relasi antar-topik `Requires` saja, dijaga basis data (FK kedua ujung, dua CHECK), produsen di grup tulis ADR-020, tampil sebagai "Topik terhubung" di halaman topik | Diterima - **mekanisme sudah dibangun; produksi NOL sisi** sampai topik masuk lewat ADR-021. Menjawab [#55](https://github.com/xtheoputra/techverse-x/issues/55) |
| [024](ADR-024-explore-learn-navigasi-v1.md) | **Explore dilayani rute yang sudah ada** (diukur dengan penelusuran), `/learn` dan `/graph` menunggu pemicu, satu bagian masuk menu hanya kalau halamannya berisi, dan **aturan teks pembaca** | Diterima - `/explore`, `/learn`, `/graph` **sengaja ditunda** dengan pemicu tertulis |
| [025](ADR-025-host-api-pengganti-koyeb.md) | **Host API pengganti Koyeb belum dipilih**: diuji berurutan - **Railway Free lebih dulu**, Vercel kontainer kedua - dengan syarat lulus tertulis; Back4App terbantah pemeriksa. Berikut koreksi untuk kaki Neon dan Vercel | **Diusulkan** - menunggu uji pemilik, [#39](https://github.com/xtheoputra/techverse-x/issues/39) |
| [026](ADR-026-nol-biaya-gratis-mandiri.md) | **Nol biaya: mandiri lebih dulu**, gratis tanpa kartu kedua, berbayar tidak pernah. CI mandiri (`run.ps1 ci`), Ollama lokal, pemindai disematkan digest, jalan hibrida mandiri untuk host API | Diterima - arahan pemilik 2026-09-28, [#57](https://github.com/xtheoputra/techverse-x/issues/57) |

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
