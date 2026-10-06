# ADR-009 — Tulang punggung navigasi: fungsi, dengan URL kanonik untuk tiap bidang

**Status:** Diterima. Menutup Issue [#1](https://github.com/xtheoputra/techverse-x/issues/1) dan [#9](https://github.com/xtheoputra/techverse-x/issues/9).
**Tanggal:** 2026-09-04

## Konteks

`KERANGKA.md` 2.2 menetapkan menu kiri berisi **bidang teknologi**. `KERANGKA.md`
3.3 menetapkan navigasi utama berisi **fungsi** — Explore, Learn, Labs, AI,
Intelligence, Knowledge Graph, Future. Keduanya tidak bisa sama-sama jadi
navigasi utama.

Enam fitur lain juga belum punya rumah sama sekali (`KERANGKA.md` E7): AI Roadmap
Generator, AI Project Generator, Progress Tracker, Timeline Teknologi, Career
Mode, Badge & Achievement.

Dan satu tabrakan yang lebih halus: submenu **"AI Engineer"** di 2.3 duduk di
antara LLM dan Computer Vision. Itu **peran kerja** yang diletakkan di antara
teknologi.

## Keputusan

**Fungsi jadi tulang punggung, bidang punya URL kanoniknya sendiri.** Ini opsi 3
di Issue #1.

```
/                      beranda
/explore               peta 14 bidang, filter per bidang
/learn                 roadmap; Career Mode di dalamnya
/labs                  proyek & eksperimen
/ai                    AI Mentor
/intelligence          arus artikel & paper
/graph                 Knowledge Graph
/future                emerging tech
/teknologi/<slug>      URL KANONIK tiap bidang dan tiap topik
```

Alasan memilih hibrida, bukan salah satu ekstremnya:

- **Opsi 2 (bidang jadi nav utama) menggandakan tujuh fungsi empat belas kali.**
  Tujuh tab × 14 bidang = 98 permukaan yang harus diisi. Untuk pengembang
  tunggal itu bukan struktur, itu utang.
- **Opsi 1 (fungsi murni) membuang pintu masuk pencarian.** Orang tidak mencari
  "Explore"; mereka mencari "belajar AI agents". Kalau bidang cuma jadi
  parameter filter, halaman yang paling dicari justru tidak punya alamat.
- Hibrida menyimpan keduanya: yang dinavigasi manusia adalah kata kerja, yang
  ditautkan dan diindeks adalah kata benda.

**Rumah untuk enam fitur yang menggantung:**

| Fitur | Rumah | Kapan |
|---|---|---|
| AI Roadmap Generator | `/learn` | V1.1 |
| AI Project Generator | `/labs` | V1.1 |
| Timeline Teknologi | `/future` | V1.1 |
| Career Mode | `/learn/karier` | V1.1 |
| Progress Tracker | lintas-bagian | **V1.1 — butuh autentikasi** |
| Badge & Achievement | lintas-bagian | **dicoret dari 6 bulan pertama** |

**Peran kerja hanya ditangani Career Mode.** Submenu "AI Engineer" dicabut dari
taksonomi teknologi (lihat [ADR-010](ADR-010-taksonomi-bidang.md)). Peran adalah
lintasan yang menyusun teknologi, bukan salah satu teknologinya. Mencampur
keduanya membuat "AI Engineer" tampak setara dengan "Computer Vision", padahal
yang satu berisi yang lain.

**"Teknologi baru yang belum muncul" (`KERANGKA.md` 3.2, butir E10) adalah bagian
`/future`, bukan bidang ke-15.** Ia tidak punya kurikulum, tidak punya roadmap,
dan tidak akan pernah punya — isinya justru hal yang belum cukup matang untuk
diajarkan. Menjadikannya bidang memaksa kita menulis roadmap untuk sesuatu yang
menurut definisinya belum ada.

## Konsekuensi

- `/teknologi/<slug>` adalah satu-satunya URL yang dijanjikan stabil. Yang lain
  boleh berubah bentuk tanpa memutus tautan orang lain.
- Knowledge Graph naik jadi bagian inti (bukan lagi "Visi 2.0"), dan hibrida ini
  membuatnya masuk akal: graf menghubungkan **entitas ber-URL**, dan entitas itu
  ada karena keputusan ini.
- Badge & Achievement adalah satu-satunya fitur yang benar-benar dibuang dari
  rencana enam bulan. Alasannya di [RENCANA-V1](../RENCANA-V1.md): ia menuntut
  autentikasi, penyimpanan progres, dan aturan permainan — tiga pekerjaan penuh
  demi lencana, sementara belum ada satu pun halaman yang tuntas.

---

## Pembaruan 2026-09-17 - tabel rutenya tetap, cara menggelarnya diputuskan

Skema URL di atas **tidak berubah**, dan `/teknologi/<slug>` tetap satu-satunya
URL yang dijanjikan stabil. Yang diputuskan [ADR-024](ADR-024-explore-learn-navigasi-v1.md)
adalah kapan tiap bagian muncul:

- **Explore V1** dilayani `/`, `/teknologi/<bidang>`, `/cari`, dan tautan
  antar-topik - diukur dengan penelusuran dari `/`, bukan diklaim. `/explore`
  menunggu PR pertama yang harus menaruh sesuatu selain peta bidang di `/`.
- **`/learn`** menunggu [#42](https://github.com/xtheoputra/techverse-x/issues/42) tutup DAN satu roadmap terisi
  di produksi.
- **`/graph`** menunggu EPIC 10. Knowledge Graph V1 adalah daftar tautan di
  halaman `/teknologi/<slug>` ([ADR-023](ADR-023-knowledge-graph-dasar.md)).
- **Sebuah bagian masuk menu hanya di PR yang membuktikan halamannya berisi**
  dalam bentuk produksi. Menu tujuh butir yang enam di antaranya kosong adalah
  persis kegagalan yang ditulis RENCANA-V1.
