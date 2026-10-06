# ADR-005 — Qdrant belum dipakai; pgvector pun belum dipasang

**Status:** Diterima. Issue [#15](https://github.com/xtheoputra/techverse-x/issues/15) ditutup 2026-09-04: **pgvector, tanpa Qdrant**.
**Tanggal:** 2026-09-03

## Konteks

`KERANGKA.md` 2.6 mencantumkan Qdrant sebagai Vector DB. `AUDIT-KELAYAKAN.md` B4 membantahnya untuk skala ini: tier **gratis** Qdrant Cloud saja sanggup menampung sekitar 1 juta vektor 768 dimensi, sedangkan beban TechVerse ada di kisaran ratusan ribu potongan — satu tingkat di bawah kelas terkecil yang Qdrant jual.

Audit juga mencatat satu alasan sah untuk tetap memilih Qdrant: **pencarian hibrida**. Untuk berita dan paper, pencocokan kata kunci (nama model, nomor CVE, judul paper) sering meleset kalau hanya mengandalkan embedding.

Keyakinan audit di butir ini **sedang**, bukan tinggi.

## Keputusan

Belum memasang keduanya. Tidak Qdrant, tidak pgvector.

Alasannya bukan bahwa pgvector menang — melainkan **belum ada satu embedding pun yang perlu disimpan**. RAG ada di EPIC 07, dan Fase 1 tidak menyentuhnya.

## Konsekuensi

- `docker-compose.yml` tidak memuat Qdrant, dan image Postgres yang dipakai `postgres:17-alpine` polos — bukan `pgvector/pgvector`. Memilih image ber-pgvector sekarang pun sudah setengah mengambil keputusan.
- **Satu hal yang harus diputuskan SEBELUM embedding pertama dibuat**, bukan sesudah: batas 2.000 dimensi untuk indeks HNSW pgvector. `text-embedding-3-large` (3.072 dimensi) tidak bisa diindeks langsung. Audit B4 menandainya sebagai jebakan yang "menggigit SETELAH embedding terlanjur dibuat".
- Berpindah antar-keduanya tetap murah: audit mencatat keduanya punya konektor .NET di bawah abstraksi `Microsoft.Extensions.VectorData` yang sama.

---

## Pembaruan 2026-09-04 - keputusan diambil

**pgvector saja. Qdrant tidak dipasang.**

Angka pembanding paling jujur datang dari Qdrant sendiri: tier gratisnya disebut
sanggup menampung sekitar 1 juta vektor 768 dimensi, sementara beban TechVerse -
82 topik plus arus arXiv harian - ada di kisaran ratusan ribu potongan. Kita satu
tingkat di bawah kelas terkecil yang Qdrant jual.

**Satu alasan sah memilih Qdrant - pencarian hibrida - diterima sebagai
kebutuhan, tapi dijawab tanpa menambah basis data.** PostgreSQL sudah membawa
`tsvector`; peringkat leksikal dan vektor digabung di satu kueri. Rinciannya, dan
ketiga jebakan pgvector yang wajib diputus sebelum embedding pertama dibuat, ada
di [ADR-015](ADR-015-skema-data-v1.md).

Keyakinan audit di butir ini **sedang**, bukan tinggi - jadi keputusan ini ditulis
sebagai yang paling mudah dibalik dari seluruh rangkaian: yang berubah kalau
dibalik hanyalah tempat vektor disimpan, karena kedua backend duduk di bawah
abstraksi `Microsoft.Extensions.VectorData` yang sama.

⚠️ Konektor lama `Microsoft.SemanticKernel.Connectors.PgVector` berstatus
**DEPRECATED**; penggantinya `CommunityToolkit.VectorData.PgVector`.
