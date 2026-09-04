# ADR-002 — Menargetkan `.NET 10`

**Status:** Diterima. Issue [#12](../../../../issues/12) ditutup 2026-09-04.
**Tanggal:** 2026-09-03

## Konteks

`AUDIT-KELAYAKAN.md` B1 menemukan bahwa `.NET 9` berhenti didukung
**10 November 2026**, dan `.NET 8` berakhir pada tanggal yang sama persis —
sehingga "turun ke LTS" bukan jaring pengaman. `.NET 10` LTS sampai
**14 November 2028**.

Ini satu-satunya butir Fase 0 yang punya tenggat dunia nyata, dan tenggatnya
kurang dari sepuluh minggu dari hari ADR ini ditulis.

## Keputusan

Seluruh proyek menargetkan `net10.0`.

Angka versinya ditulis **satu kali saja**, di `Directory.Build.props`. Audit
memperingatkan bahwa "angka 9 akan tersebar di banyak tempat" — `TargetFramework`
tiap csproj, dua baris Dockerfile, `global.json`, workflow CI, konfigurasi Azure.
Di repositori ini yang tersisa dua: `Directory.Build.props` dan `global.json`.

`global.json` memakai `rollForward: latestFeature`, bukan pin ketat. Audit
menyebut `global.json` yang mem-pin SDK terlalu sempit sebagai "bom waktu
senyap" — runner CI dengan SDK lebih baru akan gagal build dengan pesan yang
menyesatkan.

Angka yang ditulis di situ harus versi **SDK** utuh berikut pita fiturnya
(`10.0.100`), bukan versi **runtime** (`10.0.0`). Begitu `rollForward` disebut,
`setup-dotnet` menolak bentuk runtime — sementara `dotnet` di mesin yang SDK-nya
sudah terpasang menerimanya diam-diam. Kekeliruan ini pernah memerahkan CI dan
hanya terlihat di runner bersih; lihat Issue [#24](../../../../issues/24).

## Konsekuensi

- Butuh SDK .NET 10 di mesin pengembang dan runner CI. CI memakai
  `global-json-file: global.json`, jadi keduanya memakai **lantai** versi yang
  sama.
- **Koreksi 2026-09-04 (bukti, bukan dugaan):** kalimat sebelumnya di baris ini
  berbunyi "supaya keduanya tidak bisa berbeda". Itu tidak benar, dan justru
  `rollForward: latestFeature` sendiri sebabnya — ia diselesaikan sendiri-sendiri
  di tiap mesin. Log
  [33845921352](https://github.com/xtheoputra/techverse-x/actions/runs/33845921352)
  menunjukkan runner memakai SDK **10.0.400** sementara mesin pemilik memakai
  **10.0.302**. `global.json` menyamakan lantainya, bukan versinya.
- Konsekuensi lanjutan dari koreksi itu: dengan `TreatWarningsAsErrors` menyala,
  SDK runner yang lebih baru bisa membawa analyzer baru — jadi CI tetap bisa
  merah pada perubahan yang hijau di mesin pemilik. Ini harga yang **sengaja
  dibayar**: keputusan di atas sudah menolak pin ketat karena bom waktu senyapnya
  dinilai lebih mahal. Dicatat supaya sesi berikutnya tidak "memperbaikinya"
  tanpa membuka ulang keputusan ini.
- Kalau keputusan ini nanti dibalik, yang berubah satu baris (`Directory.Build.props`) plus lantai di `global.json`.
- Dockerfile belum ada, jadi belum ada baris versi ketiga yang perlu dijaga.

---

## Pembaruan 2026-09-04 - keputusan disahkan

Issue #12 ditutup. `.NET 10` bukan lagi "diterima sementara": seluruh solusi sudah
menargetkan `net10.0`, CI hijau di atasnya, dan `KERANGKA.md` 2.6 yang masih
menulis ".NET 9" dinyatakan tergantikan oleh ADR ini - lihat
[KEPUTUSAN.md](../KEPUTUSAN.md).

Yang membuat butir ini paling mudah disahkan di antara 22 keputusan lain: ia
satu-satunya yang tenggatnya bukan pendapat. `.NET 9` dan `.NET 8` sama-sama
berhenti didukung **10 November 2026**.
