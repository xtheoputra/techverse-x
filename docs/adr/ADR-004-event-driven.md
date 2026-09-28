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

## Pembaruan 2026-09-28 - "persis" kurang satu medan, dan dua medan yang tampak mati memang belum boleh dibaca

Butir pertama bagian *Keputusan* berbunyi: `DomainEvent` memuat `EventId`,
`OccurredAt`, `EventType`, dan `Version` — *"persis bidang amplop di 4.8"*.
**Kata "persis" keliru.** Amplop di `KERANGKA.md` 4.8 punya enam medan:

| Medan 4.8 | Di `DomainEvent` |
|---|---|
| `eventId` | ✅ `EventId` |
| `eventType` | ✅ `EventType` |
| `version` | ✅ `Version` |
| `occurredAt` | ✅ `OccurredAt` |
| `correlationId` | ❌ **tidak ada** |
| `payload` | ✅ diwakili medan rekaman turunannya |

Akibatnya satu konsekuensi di atas ikut berlebihan: *"bentuk eventnya sudah benar
sejak awal, jadi tidak ada migrasi kontrak"*. Begitu bus dipasang, menambah
`correlationId` **adalah** perubahan kontrak — kecil, tapi bukan nol. Belum ada
yang dirugikan: nol penerbit, nol pelanggan, dan nol event yang pernah keluar dari
proses. Karena itu medannya sengaja **tidak** ditambahkan hari ini: menambahnya
berarti memutuskan dari mana nilainya datang (jejak permintaan HTTP? pemicu
terjadwal?) tanpa satu pun pembaca yang bisa membantah pilihannya.

**Yang BUKAN temuan, supaya sapuan berikutnya tidak memungutnya lagi.** Sapuan
pemanggil-nol keempat ([#68](https://github.com/xtheoputra/techverse-x/issues/68)) mencatat `EventId` dan `OccurredAt` sebagai "nol
pembaca produksi". Diukur ulang: benar, dan **keempat** medan amplop sama nolnya
— `EventType` dan `Version` hanya dibaca uji. Itu isi keputusan ini sendiri:
bentuk ditegakkan sekarang, pengangkutan nanti, dan event hilang di
`ClearEvents()`. Medan amplop tanpa pembaca adalah bentuk yang ADR ini pilih,
bukan kode mati.

Dua rujukan ke ADR ini juga salah nomor sejak awal — ringkasan `DomainEvent` dan
kepala `docker-compose.yml` sama-sama menunjuk *"ADR-0005"*, nomor yang tidak ada
(ADR-005 membahas Qdrant). Ringkasan `DomainEvent` bahkan menulis bahwa event
*"dibaca saat SaveChanges"*; tidak ada yang membacanya di sana. Keduanya
dibetulkan hari ini.
