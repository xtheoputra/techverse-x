-- Satu schema per bounded context di dalam SATU basis data.
-- Rujukan: KERANGKA.md 4.7 (Database Ownership) — tahap awal memakai
-- schema-per-context, pemisahan jadi basis data terpisah menyusul kalau perlu.
--
-- Hanya schema yang kodenya sudah ada yang dibuat di sini. Menambah schema
-- kosong untuk konteks yang belum ditulis cuma bikin ilusi kemajuan.

CREATE SCHEMA IF NOT EXISTS technology;

COMMENT ON SCHEMA technology IS
  'Bounded context Technology. Source of truth entri teknologi dan sisi grafnya.';
