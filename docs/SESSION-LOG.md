# Catatan Sesi Kerja

Urutan terbaru di atas. Berkas ini mencatat **apa yang terjadi dan kapan** — bukan isi temuannya. Temuan tinggal di berkas auditnya masing-masing.

---

## 2026-09-03 — Sesi 2: Perapian seluruh dokumen

Permintaan pemilik: rapikan semua dokumen `.md`, termasuk yang duplikat. Tidak ada isi temuan maupun kata pemilik yang diubah — yang dirapikan struktur, urutan, dan pengulangan.

### Struktur

- **Satu `# H1` per berkas.** Ketiga dokumen besar sebelumnya memakai beberapa H1 dalam satu berkas, sehingga daftar isi otomatis GitHub tidak bersarang benar. Semua judul di bawahnya diturunkan satu tingkat.
- **`AUDIT-KESEGARAN.md` ternyata dua dokumen yang ditempel jadi satu** — masing-masing punya H1, blok metadata, dan bagian peringatannya sendiri. Kini jadi satu berkas dengan `Bagian A` (peta dua belas teknologi) dan `Bagian B` (empat bidang baru/berubah), mengikuti pola `AUDIT-KELAYAKAN.md`.
- **Dua bagian peringatan digabung jadi satu** di kaki berkas, sesuai janji di kepalanya sendiri (*"baca peringatan di bagian paling bawah"*).
- **`KERANGKA.md` dapat Daftar Isi.** Berkas 2.000 baris tanpa peta arah.

### Pengulangan yang dibuang

- **Blok metadata ganda di `AUDIT-KESEGARAN.md`** — dua audit, dua kepala berkas. Jadi satu tabel ringkas di kepala.
- **Dua bagian peringatan di berkas yang sama** — digabung, dan yang khusus Bagian B jadi butir di dalamnya.
- **Daftar temuan audit di berkas catatan sesi ini** yang mengulang isi kedua berkas audit hampir kata per kata — diganti tabel penunjuk.
- **Daftar temuan di `README.md`** — tetap ada karena halaman muka memang harus berdiri sendiri, tapi tiap butir kini menaut ke bagian kanonisnya di berkas audit, bukan jadi salinan keempat yang bisa hanyut sendiri.

### Pengulangan yang sengaja DIBIARKAN

Tidak semua pengulangan layak dibuang. Tiga ini dipertahankan, dengan alasan:

- **Deskripsi produk di `README.md` dan `KERANGKA.md`.** Sempat dihapus dari `KERANGKA.md` dan diganti penunjuk, lalu dikembalikan — itu kalimat pemilik, dan `KERANGKA.md` adalah rekaman kata pemilik. Halaman muka yang menunjuk ke sumber lebih sehat daripada sumber yang menunjuk ke halaman muka.
- **Empat vonis arsitektur** yang muncul di `KERANGKA.md` 2.6, 4.20, dan butir D4. Ketiganya beda peran: 2.6 tabel keputusan, 4.20 rekonsiliasi, D4 butir yang menunggu. Rumah kanonisnya tetap `AUDIT-KELAYAKAN.md` Bagian B.
- **Vonis Edge AI/IoT** di butir E11, E12, dan `AUDIT-KESEGARAN.md` Bagian B. E12 adalah keputusan yang menunggu; sisanya rujukan.

### Yang ditemukan saat merapikan

Lima hal yang bukan sekadar rapi-rapi, semuanya sudah diperbaiki atau ditandai di tempatnya:

1. **Dua berkas audit sama-sama mengaku "belum ada koreksi yang diterapkan ke `KERANGKA.md`"** — pernyataan itu sudah kedaluwarsa. Tabel arsitektur 2.6 sudah ditulis ulang (mengikuti Bagian 4, bukan mengikuti audit), dan butir E5/E11 sudah memuat penunjuk hasil audit. Kedua pernyataan diperbaiki agar menyebut persis apa yang berubah dan apa yang tidak.
2. **Tanggal GA Microsoft Agent Framework berbeda antar-berkas** — `AUDIT-KESEGARAN.md` menulis 3 April 2026, `AUDIT-KELAYAKAN.md` menulis 2 April 2026, untuk peristiwa yang sama. Keduanya keluaran agen berbeda. Tidak dipilih salah satu; perbedaannya ditandai di kedua berkas supaya dicek ke NuGet sebelum masuk halaman publik.
3. **Butir D6 dan catatan pengantar bagian D tercecer di bawah bagian E** — dikembalikan ke bagian D.
4. **Butir E10 tidak pernah jadi issue.** Dari 30 butir keputusan, 29 terpetakan ke 22 issue; E10 ("teknologi baru yang belum muncul" — bidang ke-13 atau bagian Future?) terlewat. Ditandai di peta butir, belum diputuskan.
5. **`README.md` tidak menyebut Bagian 4 sama sekali**, padahal Engineering Blueprint v1 kini tiga perempat isi `KERANGKA.md`. Ditambahkan.

**Masih nol baris kode.** Perapian ini tidak menyentuh milestone Fase 0.

---

## 2026-09-02 — Sesi 1: Kerangka awal + tiga audit

**Titik mulai:** folder kosong total. Nol berkas, nol subfolder, belum ada `.git`.

**Cara kerja yang disepakati pemilik:** pemilik mendiktekan kerangka bertahap dengan kata-kata yang belum tertata; tugas asisten merapikan, menandai benturan dan lubang, lalu menyimpannya ke `KERANGKA.md`. **Koreksi tidak boleh diterapkan diam-diam** — kata-kata pemilik direkam apa adanya, hasil pemeriksaan fakta ditaruh di berkas audit terpisah.

### Empat gelombang dikte

1. **Peta teknologi 2026** — dua belas teknologi, lima pilar, tabel prioritas pribadi. Jadi Bagian 1.
2. **Produk & dashboard** — tujuan, struktur menu 8 bidang / 41 halaman, template halaman, empat fitur AI, arsitektur teknologi, konsep basis data, Visi 2.0, rencana 6 bulan. Jadi Bagian 2.
3. **Identitas & struktur aplikasi** — revisi jadi 12 bidang, plus tujuh bagian aplikasi (Explore, Learn, Labs, AI, Intelligence, Knowledge Graph, Future). Jadi Bagian 3.
4. **Engineering Blueprint v1** — prinsip arsitektur, C4, bounded context, AI platform, event-driven, keamanan, observability, produksi, backlog 120 task. Jadi Bagian 4. Tabel arsitektur 2.6 ikut ditulis ulang agar sejalan, dan deviasinya direkonsiliasi di 4.20.

### Tiga audit dijalankan

Total **29 agen paralel**, nol gagal. Temuannya ada di berkas auditnya — tidak diulang di sini.

| Audit | Agen | Hasil ringkas | Berkas |
|---|---|---|---|
| Kesegaran peta 12 teknologi | 15 | Kedua belas entri berstatus `sebagian-usang` | [`AUDIT-KESEGARAN.md` Bagian A](../AUDIT-KESEGARAN.md#bagian-a--peta-dua-belas-teknologi) |
| Kelayakan sumber berita + tumpukan | 10 | Keempat pilihan arsitektur bermasalah | [`AUDIT-KELAYAKAN.md`](../AUDIT-KELAYAKAN.md) |
| Empat bidang baru/berubah | 4 | Renewable Energy kehilangan 2 dari 3 isinya | [`AUDIT-KESEGARAN.md` Bagian B](../AUDIT-KESEGARAN.md#bagian-b--empat-bidang-baruberubah) |

### Koreksi yang dilakukan asisten atas dirinya sendiri

Asisten sempat menduga **Edge AI wajar ditempatkan di bawah IoT**. Audit menolaknya dengan tiga alasan. Dicatat sebagai butir E12 — Edge AI perlu bidangnya sendiri.

### Batasan mutu yang tercatat pada hari itu

- **Kuota WebSearch keempat agen audit ketiga habis** sebelum sempat dipakai, sehingga verifikasi terpaksa lewat WebFetch ke URL tebakan. Bagian taksonominya tetap kuat; bagian "pemain terkini" lemah dan sebagian bersandar Wikipedia.
- Beberapa klaim luar biasa **belum diverifikasi independen** — sudah ditandai di kaki berkas audit.
- **Ketentuan layanan OpenAI dan Microsoft belum dibaca siapa pun** — halamannya memblokir bot. Ini pekerjaan manusia dan harus selesai sebelum tayang (Issue [#17](../../issues/17)).

### Keadaan akhir sesi

Repositori dibuat dan didorong ke GitHub sebagai **privat**. Seluruh keputusan yang menunggu dipindahkan menjadi GitHub Issues berlabel dan bermilestone.

**Belum ada satu baris kode pun.** Itu disengaja — milestone *Fase 0 — Kunci Kerangka* harus selesai lebih dulu.
