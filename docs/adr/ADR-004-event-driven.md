# ADR-004 — Event dikumpulkan di agregat, bus belum dipasang

**Status:** Diterima
**Tanggal:** 2026-09-03

## Konteks

`KERANGKA.md` 4.8 menetapkan arsitektur event-driven: katalog event, amplop berversi, NATS untuk dev dan Kafka untuk produksi.

Tetapi pada Fase 1 baru ada **satu** bounded context. Bus dengan satu penerbit dan nol pelanggan tidak mengangkut apa pun — yang ia tambahkan cuma satu container lagi untuk dinyalakan, dan satu lagi yang bisa mati.

## Keputusan

Bentuk event ditegakkan sekarang, pengangkutannya nanti.

- `DomainEvent` memuat `EventId`, `OccurredAt`, `EventType`, dan `Version` — persis bidang amplop di 4.8.
- Agregat mengumpulkan eventnya sendiri, dan handler memanggil `ClearEvents()` setelah menyimpan.
- **Tidak ada** NATS/Kafka di `docker-compose.yml`.

## Konsekuensi

- Saat bounded context kedua lahir, yang perlu ditambah hanya penerbitnya — bentuk eventnya sudah benar sejak awal, jadi tidak ada migrasi kontrak.
- Event saat ini **hilang** setelah `ClearEvents()`. Itu wajar selama belum ada pelanggan, tapi harus berubah jadi pola outbox begitu ada — kalau tidak, event akan hilang setiap kali transaksi berhasil tapi penerbitannya gagal.
- `Version` sengaja bisa di-`init`, bukan konstanta, supaya event yang bentuknya berubah bisa menaikkan versinya sendiri tanpa memecah yang lain.
