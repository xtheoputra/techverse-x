# ADR-020 — Permukaan tulis API ditutup di produksi

**Status:** Diterima. **Melengkapi [ADR-013](ADR-013-autentikasi.md)** (tidak
menggantikannya). **Tanggal:** 2026-09-09

## Konteks

[ADR-013](ADR-013-autentikasi.md) memutuskan **V1 tidak punya autentikasi sama
sekali**, dan alasannya masih benar hari ini:

> *"Membangun autentikasi untuk produk yang belum punya satu halaman pun berstatus
> `tinjau` berarti memasang permukaan serangan terbesar sebelum ada yang perlu
> dilindungi."*

🔴 **Tapi perhatikan APA yang ditimbangnya.** Ketiga alasannya — Progress Tracker,
Badge, AI Mentor — semuanya soal **identitas PEMBACA**. ADR-013 tidak pernah
menyebut, di satu baris pun, bahwa aplikasi yang sama juga memasang **permukaan
tulis REDAKSI**: delapan endpoint yang mengubah isi situs.

Sampai 2026-09-09 kedelapan endpoint itu dipasang **tanpa syarat**, di lingkungan
apa pun:

| Endpoint | Yang bisa dilakukan orang asing |
|---|---|
| `POST /api/v1/technologies` | membuat topik yang **langsung tampil di halaman muka** dan di halaman bidangnya |
| `POST /api/v1/tools` | menambah entri katalog alat |
| `PUT /api/v1/technologies/{slug}/roadmap/prasyarat` | menulis ulang langkah 0 topik mana pun |
| `POST …/roadmap` · `…/projects` | menyisipkan langkah dan proyek |
| `POST …/tools` | menautkan alat, berikut catatannya |
| `POST …/resources` | **menaruh tautan ke situs mana pun di bagian Resources** |
| `POST …/draf` | menaikkan halaman ke `draf` tanpa ada yang memintanya |

Ini bukan risiko teoretis yang menunggu fitur berikutnya. Ia menyala **tepat pada
langkah yang sedang dikerjakan**: issue [#39](../../../../issues/39) memasang API
di Koyeb dan [#40](../../../../issues/40) memberi situsnya URL publik. Sejak menit
itu, `https://….koyeb.app` menerima kedelapan permintaan di atas dari siapa pun.
Alamatnya tidak rahasia — ia tercatat di log Certificate Transparency, dan
`PENYEBARAN.md` sendiri menyuruh menaruhnya di `API_BASE_URL`.

🔑 **Yang membuatnya lolos begitu lama: "tanpa autentikasi" terdengar seperti
sebuah keputusan yang sudah diambil.** Keputusannya memang sudah diambil — untuk
pertanyaan yang lain.

## Keputusan

**Endpoint tulis tidak dipasang kecuali diminta.** Satu setelan,
`Editorial:WritesEnabled`, **bawaannya `false`**.

```csharp
var editorialWrites = builder.Configuration.GetValue("Editorial:WritesEnabled", false);
...
app.MapTechnologyEndpoints(editorialWrites);
```

Tiga hal yang dipilih sengaja, dan masing-masing punya alasannya:

1. **Tidak dipasang, bukan dipasang lalu ditolak.** Bentuknya meniru Redis di
   `Program.cs` — *tidak dikonfigurasi = tidak dipasang* ([ADR-016](ADR-016-pagu-biaya.md)).
   Rute yang tidak ada tidak punya rahasia untuk bocor, tidak perlu dirotasi,
   dan tidak bisa salah dibandingkan. Jawabannya jadi 405 untuk
   `POST /api/v1/technologies` (alamat itu masih dipegang GET pencarian) dan 404
   untuk sisanya.
2. **Bawaannya tertutup.** Lingkungan yang lupa dikonfigurasi mendapat bentuk
   yang **aman**, bukan bentuk yang terbuka. Ini kebalikan dari keadaan
   sebelumnya, yang bawaannya terbuka dan tidak punya cara untuk ditutup.
3. **Parameternya tidak punya nilai bawaan** di `MapTechnologyEndpoints(routes,
   editorialWrites)`. Host yang lupa menyatakan pilihannya **gagal dikompilasi**,
   bukan diam-diam membuka.

Dinyalakan hanya di dua tempat, dan **keduanya tidak pernah dilihat produksi**:

| Tempat | Kenapa aman |
|---|---|
| `apps/api/Properties/launchSettings.json` | hanya dibaca `dotnet run`; produksi menjalankan DLL-nya langsung |
| `builder.UseSetting(...)` di uji integrasi | `WebApplicationFactory` tidak pernah membaca `launchSettings.json` |

Mekanisme ini bukan hal baru — persis yang sudah dipakai `ConnectionStrings__Redis`
sejak ADR-016, di berkas yang sama, dengan alasan yang sama.

## Apa yang TIDAK diputuskan di sini

⚠️ **Ini bukan pengganti autentikasi, dan tidak boleh dibaca begitu.** Yang
diputuskan hanya: *selama V1 belum punya cara sah untuk mengenali penulisnya,
produksi tidak memasang jalan untuk menulis.*

Bentuk produksi V1 karenanya adalah **terbitan yang hanya bisa dibaca**, dan itu
bukan pengorbanan: keempat belas bidangnya datang dari **migrasi**
([ADR-010](ADR-010-taksonomi-bidang.md)), bukan dari HTTP. Produksi hari ini nol
topik, jadi tidak ada satu pun kemampuan yang hilang karena keputusan ini.

Yang **belum** diputuskan, dan sengaja dibiarkan terbuka:

- **Bagaimana isi sungguhan nanti masuk ke produksi.** Tiga arah yang masuk akal:
  menyalakan sakelar ini sementara di balik jaringan yang terkendali; workflow
  GitHub Actions bergerbang `environment:` seperti *Migrasi produksi*; atau
  autentikasi sungguhan sesuai ADR-013 begitu ada yang perlu dilindungi.
- **`MarkReviewed()` dan `Publish()` sampai hari ini nol pemanggil di kode
  produksi** — bukan endpoint, bukan skrip, bukan UI; hanya uji unit. Artinya
  target Bulan 2 ("tujuh topik AI Agents berstatus `tinjau`",
  issue [#42](../../../../issues/42)) hari ini **tidak bisa dicapai siapa pun**,
  bukan hanya oleh asisten. Itu soal terpisah, dan keputusan ini tidak
  memperburuknya: jalan yang ditutup di sini memang tidak pernah ada.

## Konsekuensi

- 🔴 **`run.ps1 seed` hanya bekerja terhadap API yang dijalankan `dotnet run`.**
  Diarahkan ke produksi ia akan membalas 404/405. Itu memang yang dimaksud.
- **Gerbangnya sudah dibuktikan MERAH**, bukan sekadar hijau: dengan sakelar
  dipaksa hidup di host bawaan, `POST /api/v1/technologies` membalas **201** dan
  ujinya gagal menyebut angka itu. Tiga uji integrasi menjaganya —
  kedelapan endpoint hilang saat bawaan, membaca tetap jalan (kendali), dan
  alamat yang sama persis terbuka lagi saat sakelarnya hidup (kendali arah
  berlawanan, supaya satu salah ketik alamat tidak meluluskan ujinya).
- 🔒 **Dijaga DUA KALI, di dua tingkat yang berbeda.** Uji integrasi menjaganya
  di tingkat **kode**, tapi ia merakit host-nya sendiri lewat
  `WebApplicationFactory` — jadi ia buta terhadap satu kelas regresi: **artefak
  yang membukanya kembali.** Sebuah `ENV Editorial__WritesEnabled=true` di
  Dockerfile, sebuah default dari citra dasar, atau `launchSettings.json` yang
  suatu hari ikut terbawa `dotnet publish` akan **lolos seluruh uji .NET** dan
  baru terlihat di produksi. Karena itu `.github/scripts/periksa-permukaan-tulis.sh`
  menjalankan **citra yang sudah jadi** dan menuntut `POST /api/v1/technologies`
  membalas 405 — dipasang di `ci.yml` (PR) dan di `rilis-citra.yml` **sebelum
  push**, dua tempat yang sama dengan Container Scan, dan karena alasan yang sama:
  yang sudah terbit bisa ditarik orang.
  - Pemeriksaannya **tidak butuh Postgres**, dan muatannya **sengaja tidak sah**:
    validator menjawab sebelum satu baris pun dibaca, jadi **405 = rutenya tidak
    ada** dan **400 = rutenya ada**, keduanya tanpa dependensi. Muatan yang sah
    justru menyeret basis data ke dalam alat ukurnya (terukur: 400 dalam 0,0
    detik vs 500 dalam 2,3 detik, dan menggantung pada batas waktu yang lebih
    pendek).
  - Ketiga keadaannya sudah dijalankan: citra benar **lulus**, citra yang
    sengaja diberi `ENV` pembuka **gagal** dengan diagnosis "rutenya ADA", dan
    peti kemas yang tidak menyala **gagal pada KENDALInya** — bukan lolos.
- **Log startup menyebut kedua keadaan**, bukan hanya yang tidak biasa — keadaan
  terbuka sebagai `Warning`, keadaan tertutup sebagai `Information`. Log produksi
  yang sehat karenanya tidak sunyi, dan sunyi tidak bisa disalahartikan sebagai
  "barisnya hilang saat refactor".
- **Yang perlu diubah kalau keputusan ini dibalik:** hapus cabangnya di
  `TechnologyModule.MapTechnologyEndpoints`, dan hapus ketiga uji di
  `tests/integration/PermukaanTulisTests.cs`. Membalikkannya sadar itu murah;
  yang mahal adalah membalikkannya tanpa sadar, dan itu yang dicegah parameter
  tanpa nilai bawaan.
