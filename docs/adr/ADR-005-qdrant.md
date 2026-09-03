# ADR-005 — Qdrant belum dipakai; pgvector pun belum dipasang

**Status:** Ditunda. Issue [#15](../../../../issues/15) masih terbuka.
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
