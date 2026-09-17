# ADR-022 — Pencarian teks penuh: kolom terhitung PostgreSQL, kamus `english`, dan cadangan awal-kata yang sengaja dipertahankan

**Status:** Diterima, **sudah dibangun**. Melaksanakan
[ADR-015](ADR-015-skema-data-v1.md) bagian 5 dan tangga *Search Architecture*
[KERANGKA.md](../../KERANGKA.md) 4.6 (**V1 PostgreSQL FTS** → V2 OpenSearch →
V3 hybrid). Sasaran **Bulan 3** di [RENCANA-V1.md](../RENCANA-V1.md).
**Tanggal:** 2026-09-11

## Konteks

`SearchTechnologyHandler` sejak awal berbunyi `ILIKE '%kata%'` atas `Name` dan
`Summary`, dengan catatan di kodenya sendiri bahwa FTS dijadwalkan Bulan 3.
Batasnya bukan teoretis. Sepuluh kata kunci diuji terhadap isi sungguhan
(keempat belas bidang `FieldCatalog` + lima contoh `run.ps1 seed`):

| Kata kunci | `ILIKE` hari itu | FTS |
|---|---|---|
| `model context protocol` | 1 | 1 |
| **`protokol model`** | **0** | 1 |
| `sirkuit kuantum` | 1 | 1 |
| **`kuantum sirkuit`** | **0** | 1 |
| **`model bahasa alat`** | **0** | 1 |
| `kembaran digital` | 1 | 1 |
| **`digital kembaran`** | **0** | 1 |
| `ancaman` | 1 | 1 |
| `quantum` | 0 | **0** ← lihat di bawah |
| `json-rpc` | 0 | **0** ← lihat di bawah |

**Empat dari sepuluh menjawab nol semata-mata karena urutan katanya dibalik.**
`ILIKE` mencocokkan potongan huruf, bukan kata; ia tidak punya konsep "dua kata
ini muncul, di mana saja".

Dua baris terakhir tidak terselesaikan oleh FTS atas kolom yang sama, dan itu
justru yang paling berguna dari pengukuran ini — keduanya membongkar batas yang
lebih besar daripada pilihan fungsi. Keduanya dibahas di bawah.

## Keputusan

### 1. `tsvector` sebagai kolom `GENERATED ALWAYS … STORED`, bukan trigger dan bukan kolom biasa

```sql
setweight(to_tsvector('english', coalesce("Name",    '')), 'A') ||
setweight(to_tsvector('english', coalesce("Summary", '')), 'B')
```

Dipasang di **`technologies` dan `fields`**, keduanya dengan indeks **GIN**.

🔑 **Kolom biasa yang diisi kode aplikasi bisa hanyut dari teks sumbernya.**
Setiap jalan tulis baru — endpoint, migrasi data, perbaikan lewat SQL — wajib
ingat memperbaruinya, dan yang lupa **tidak menghasilkan galat**, cuma baris
yang tidak pernah ketemu lagi. Kolom terhitung memindahkan kewajiban itu ke
PostgreSQL, yang tidak bisa lupa. Ini dijaga uji: `Vektor_dihitung_PostgreSQL_
sendiri_bukan_oleh_kode` mengganti nama topik lewat **SQL mentah**, menembus
agregat dan EF sekaligus, lalu menuntut pencarian menemukan nama barunya dan
berhenti menemukan nama lamanya.

⚠️ **Ekspresinya wajib `IMMUTABLE`, dan itu yang memaksa bentuk dua argumen.**
`to_tsvector(x)` memakai `default_text_search_config` yang bisa berbeda per
sesi, jadi PostgreSQL menolaknya di kolom terhitung. `to_tsvector('english', x)`
diterima.

✅ **Baris lama ikut terisi tanpa migrasi data.** `ADD COLUMN … GENERATED …
STORED` menulis ulang tabelnya, jadi keempat belas bidang dan kelima topik
contoh punya vektornya begitu migrasi selesai — diperiksa, nol baris kosong.

**Bobot A untuk nama, B untuk ringkasan.** Terukur: `ts_rank` memberi **0,61**
untuk kecocokan di nama dan **0,24** untuk kecocokan di ringkasan. Itu yang
membuat mengetik nama halaman memunculkan halaman itu, bukan halaman lain yang
kebetulan menyebutnya di paragraf.

### 2. Kamus `english` — dan itu DIUKUR, bukan disimpulkan dari bahasa isinya

Isi TechVerse X campur: nama bidang hampir seluruhnya Inggris (ADR-010),
ringkasannya Indonesia. **PostgreSQL tidak membawa kamus bahasa Indonesia sama
sekali**, jadi pilihannya `simple` (tanpa stemming, tanpa stopword) atau kamus
bahasa lain. Dugaan awal — *`english` akan merusak teks Indonesia* — diperiksa
ke korpus sungguhan pada 2026-09-11:

| Yang diperiksa | Hasil |
|---|---|
| Kata **Indonesia** yang hilang karena dianggap stopword Inggris | **nol** (yang hilang cuma `in`, `on`, `the` — dari "on-device" dan "tinyML Foundation") |
| Kata korpus yang berbeda lexeme antara `english` dan `simple` | 44 dari 206 |
| **Tabrakan** stemmer: dua kata berbeda jadi satu lexeme | **satu** — `computer` + `computing` → `comput`, dan itu memang diinginkan |
| Yang dibeli | `agent` menemukan "AI Agents"; `container` menemukan "containers". Dengan `simple`: keduanya nol |

Kata Indonesia memang dipotong (`akses` → `aks`, `konektivitas` → `konektivita`),
tapi **simetris** — potongan yang sama berlaku di kuerinya, jadi pencocokan
bentuk kata yang sama tetap bekerja. Yang hilang adalah morfologi Indonesia
(`keamanan` ↔ `aman`), dan itu **juga hilang dengan `simple`**, jadi ia bukan
biaya dari pilihan ini.

⚠️ **Mengubah nilai ini wajib disertai migrasi.** Kolom terhitung menyimpan hasil
stemming; isi lama tidak ikut berubah sendiri.

### 3. `websearch_to_tsquery`, bukan `to_tsquery`

`to_tsquery` — fungsi yang muncul di hampir semua contoh FTS — **melempar** pada
masukan yang tidak berbentuk kueri:

```
to_tsquery('english', 'quantum &')  →  ERROR: no operand in tsquery
```

Di kotak pencarian publik itu berarti **HTTP 500 yang dipicu satu karakter yang
diketik pengunjung**. `websearch_to_tsquery` tidak pernah melempar, dan sekalian
memberi tanda kutip untuk frasa serta `-` untuk pengecualian.

Dijaga dua arah: empat masukan kotor (`zarqun &`, `& | !`, `(`, `'`) dituntut
menjawab **200**, dan **kendali** `Kendali_to_tsquery_MEMANG_meledak_pada_
masukan_yang_sama` menuntut `to_tsquery` benar-benar melempar untuk masukan yang
sama. Tanpa kendali itu, "200" cuma berarti *hari ini tidak meledak* — tidak ada
yang membuktikan pilihan fungsinyalah yang mencegahnya.

### 4. Pencocokan sebagian TIDAK dibuang — tapi ia AWAL KATA, bukan potongan huruf

FTS mencocokkan **kata utuh**. Terukur: `kubern` tidak menemukan "Kubernetes",
`kube` juga tidak, `orkes` juga tidak. Padahal orang mengetik sambil berpikir —
itu keadaan **normal** sebuah kotak pencarian, bukan kasus tepi.

Jadi FTS dipakai bersama sebuah cadangan, atas **teks yang sama persis** (nama
dan ringkasan, topik dan bidang):

- **FTS** menyumbang pencocokan lintas urutan kata, bentuk jamak Inggris, dan
  **peringkat**.
- **Cadangannya** menjaga pengetikan sebagian tetap bekerja.

🔴 **Cadangan itu mula-mula `ILIKE '%kata%'`, dan MENJALANKANNYA langsung
memperlihatkan kenapa itu salah:**

```
q=iot   ->  iot, edge-ai, BIOTECHNOLOGY      (b-iot-echnology)
q=a     ->  14 dari 14 bidang                (seluruh situs)
q=ai    ->  8 bidang, termasuk Blockchain    (bloc-k-ch-ai-n)
```

`iot` bukan kata kunci aneh — ia **nama salah satu dari empat belas bidang**.

Sebabnya bukan "ambangnya kurang" melainkan **bentuk pencocokannya salah**.
Cadangan ini ada untuk **pengetikan sebagian**, dan orang mengetik *awal* kata,
bukan tengahnya. Jadi polanya jadi jangkar awal-kata POSIX:

```sql
"Name" ~* '\m' || <kata kunci>
```

Sesudahnya, diukur ulang: `iot` → **`iot`, `edge-ai`** saja, `ai` → 6 bidang
(Blockchain hilang, Robotics **tetap ada** — ringkasannya memang menyebut
"Robotics AI"), `kubern` → `cloud-infrastructure` tetap ketemu.

💡 `\m` juga lebih tepat daripada mencocokkan spasi sendiri: ia mengenali batas
kata di `/` dan `-`, jadi `thread` tetap menemukan **Matter/Thread**.

🔑 `Regex.Escape` di sisi C# bukan kemewahan: tanpa itu, pengunjung yang
mengetik `(` mengirim pola yang tidak sah ke PostgreSQL — persis bentuk
kegagalan yang dihindari dengan memilih `websearch_to_tsquery` di poin 3.
Terukur sesudahnya: `?q=(` menjawab **200** dengan nol hasil.

🔴 **Dan di sinilah satu klaim saya sendiri berhenti benar, di hari yang sama
saya menuliskannya.** Sebelum cadangannya diganti, ADR ini berbunyi *"tidak ada
satu pun kata kunci yang tadinya ketemu lalu berhenti ketemu — perubahan ini
murni menambah"*. Sesudah jangkar awal-kata dipasang itu **tidak lagi benar**:
kecocokan di **tengah** kata memang sengaja berhenti ketemu — `iot` tidak lagi
memulangkan Biotechnology. Yang benar: **hampir** murni menambah, dan
satu-satunya yang hilang persis yang diperbaiki. (Pengulangan pola PR #50/#51,
kali ini di kalimat saya sendiri, dalam satu sesi.)

Konsekuensinya ditulis apa adanya: baris yang **hanya** cocok lewat pola ini
tidak bisa memakai indeks GIN (pemindaian berurutan) dan berperingkat 0, jadi ia
selalu muncul **sesudah** kecocokan FTS. Pada ukuran V1 — 14 bidang, target 22
topik di akhir Bulan 6, 82 halaman di seluruh taksonomi — pemindaian berurutan
atas puluhan baris tidak terukur. **Angka yang membuat keputusan ini perlu
ditinjau ulang: ribuan baris, bukan puluhan.**

⚠️ Alternatif `to_tsquery(kata || ':*')` **terukur bekerja** untuk awalan — dan
ditolak, sebab ia mengembalikan justru fungsi yang melempar di poin 3. Kalau
suatu saat pencarian awalan benar-benar dibutuhkan berikut indeksnya, jalannya
`pg_trgm` dengan indeks GIN trigram, bukan merakit `to_tsquery` dari masukan
pengunjung.

### 5. Bidang ikut dicari, dan bidang ikut menjaring topik di bawahnya

`quantum` adalah kasus yang membongkar ini: ia menjawab **nol** — dengan `ILIKE`
maupun dengan FTS atas kolom topik — padahal situs ini punya bidang bernama
**Quantum Computing**. Ringkasan Qiskit menulis "kuantum", ejaan Indonesia.

Hari ini ada **14 bidang dan nol sampai lima topik**. Pencarian yang cuma
menjawab topik akan membalas "tidak ada hasil" untuk hampir setiap kata —
termasuk kata yang tercetak di halaman muka. Karena itu:

- `fields` punya `tsvector`-nya sendiri dan ikut dicari;
- sebuah topik **ikut terjaring oleh bidangnya**. Mencari `cybersecurity` memang
  seharusnya memunculkan topik di bawah bidang itu, bukan hanya topik yang
  kebetulan menuliskan kata itu lagi;
- peringkat bidang **dibagi empat** sebelum dijumlahkan, supaya topik yang
  menyebut kata kuncinya di namanya sendiri tetap menang atas tetangga sebidang
  yang tidak menyebutnya sama sekali.

### 6. Satu endpoint baru: `GET /api/v1/search?q=`

Ia mengembalikan **bidang dan topik sekaligus**, sebab ADR-009 memberi URL
kanonik kepada keduanya dan `/teknologi/<slug>` melayani keduanya.

🔑 **Handler-nya tidak punya kueri sendiri.** Ia memanggil `ListFieldsHandler`
dan `SearchTechnologyHandler`, keduanya sudah dipakai halaman lain. Begitu ada
kueri kedua yang menjawab *"apakah baris ini cocok?"*, pertanyaan yang sama punya
dua jawaban — dan yang satu akan menyimpang dari yang lain pada perubahan
berikutnya tanpa ada yang merah. Itu sebabnya `ListFieldsHandler` yang tumbuh
satu parameter, bukan `SearchHandler` yang menulis proyeksi `FieldResponse`
kedua: proyeksi itu membawa dua angka kemajuan dari subkueri, dan menyalinnya
adalah cara paling mudah membuat halaman muka dan halaman pencarian memberi
**dua angka berbeda untuk satu bidang yang sama**.

`GET /api/v1/fields` sendiri **tetap tanpa penyaringan** — daftarnya tertutup
di angka 14 dan tidak bisa bertambah tanpa migrasi.

**Ini permukaan BACA.** Ia dipasang di atas batas ADR-020 dan ikut tayang di
produksi; tidak ada endpoint tulis baru.

### 7. Halaman `/cari` memakai `<form method="get">`, bukan state komponen

Tidak ada `'use client'`, tidak ada `useState`, tidak ada `onSubmit`. Peramban
sendiri yang menyusun `/cari?q=…`. Yang dibeli bukan sekadar bundel yang lebih
kecil:

- kotaknya **bekerja sebelum satu baris JavaScript pun dimuat**;
- hasilnya sebuah URL, jadi ia bisa **ditautkan, di-bookmark, dan dibuka lagi
  oleh tombol kembali** peramban.

Kotaknya duduk di **layout**, bukan di halaman muka: orang tidak selalu masuk
lewat beranda. Mesin pencari, tautan yang dibagikan, dan bookmark semuanya
mendarat langsung di halaman topik, dan kotak yang cuma ada di beranda menuntut
pengunjung kembali ke sana lebih dulu — persis menebak-nebak yang mau
dihilangkan.

Halaman hasilnya `robots: { index: false }`. Setiap kata kunci melahirkan URL
baru; membiarkannya diindeks berarti menyuruh mesin pencari memetakan ruang kata
kunci yang tak terbatas, dan yang dipetakan bukan halaman TechVerse X melainkan
bayangannya.

## 🔴 Yang TIDAK dicakup, dan kenapa itu penting

**Kelima bagian isi halaman (ADR-012) tidak ikut dicari.** Baris `json-rpc` di
tabel pengukuran adalah buktinya: teks *"Dasar HTTP dan JSON-RPC"* benar-benar
ada di situs ini — sebagai **prasyarat roadmap** topik Model Context Protocol —
dan mencarinya menjawab nol, sebelum maupun sesudah perubahan ini.

Sebabnya struktural, bukan kelalaian: **kolom terhitung hanya boleh menyebut
kolom di barisnya sendiri.** Langkah roadmap, proyek, dan sumber hidup di tabel
lain, jadi memasukkannya menuntut salah satu dari: trigger di empat tabel anak,
materialized view yang harus disegarkan, atau tabel dokumen pencarian yang
dipelihara aplikasi. **Ketiganya mengembalikan persis sifat yang dibuang di poin
1** — sesuatu yang bisa lupa diperbarui, dan lupanya tidak berbunyi.

Keputusan itu belum layak diambil sekarang: hari ini ada **satu** topik yang
kelima bagiannya terisi, dan itu pun isi contoh. Bentuk yang tepat baru bisa
dinilai setelah ada isi sungguhan. Ditagih di **[#54](../../../../issues/54)**.

⚠️ **Yang berlaku sekarang: jangan menulis di dokumen mana pun bahwa "pencarian
mencakup isi halaman".** Ia mencakup nama dan ringkasan — topik maupun bidang.

## Alternatif yang ditolak, berikut alasannya

| Alternatif | Kenapa ditolak |
|---|---|
| **Tetap `ILIKE` saja** | Empat dari sepuluh kata kunci percobaan menjawab nol hanya karena urutan katanya dibalik. Terukur, bukan diduga. |
| **`ILIKE '%kata%'` sebagai cadangan** | Dijalankan, lalu ditolak: `iot` memulangkan **Biotechnology**, `a` memulangkan seluruh situs. Bentuk pencocokannya yang salah, bukan ambangnya. |
| **Kamus `simple`** | Membuang `agent` → "AI Agents" dan `container` → "containers", padahal nama bidang ADR-010 hampir seluruhnya Inggris. Yang ditakutkan darinya — kata Indonesia hilang jadi stopword — terukur **nol**. |
| **Trigger yang mengisi `tsvector`** | Sesuatu yang bisa lupa diperbarui, dan lupanya tidak berbunyi. Kolom terhitung memindahkan kewajiban itu ke PostgreSQL. |
| **`to_tsquery` dengan `:*` untuk awalan** | Mengembalikan fungsi yang melempar pada masukan pengunjung — HTTP 500 dari satu karakter. |
| **OpenSearch / Qdrant sekarang** | KERANGKA.md 4.6 menaruhnya di V2/V3, dan ADR-015 bagian 5 sudah menolak basis data kedua untuk kebutuhan ini: container kedua, konsistensi kedua, tagihan kedua. |
| **Menyaring dan memeringkat di klien** | Aturan "apa yang dianggap cocok" akan ditulis ulang di setiap klien lalu menyimpang di salah satunya — kembaran persis alasan `missingSections` dikirim dari server. |
| **`GET /api/v1/fields?q=`** | Berkasnya sendiri menulis bahwa daftar itu tertutup dan tanpa penyaringan. Pencarian pertanyaan yang berbeda dari "sebutkan taksonominya", jadi ia dapat alamat sendiri. |

## Konsekuensi

- **Ukuran selesai Bulan 3 — *"orang bisa menemukan halaman tanpa menebak
  URL"* — tercapai untuk bidang dan topik.** Dua sasaran Bulan 3 lainnya
  (Knowledge Graph dasar) tidak tersentuh ADR ini; keadaannya ada di
  [#55](../../../../issues/55).
- **14 uji integrasi baru + 8 uji unit baru** (87+18 → 95+32). Yang integrasi tidak bisa
  dipindahkan jadi uji unit: yang diuji hampir seluruhnya perilaku PostgreSQL.
- **Tidak ada permukaan tulis baru** (ADR-020 utuh), tidak ada dependensi baru,
  tidak ada peti kemas baru, tidak ada rahasia baru.
- **Cara membalikkan:** buang migrasi `PencarianTeksPenuh`, kembalikan blok
  `Where` di `SearchTechnologyHandler` ke `ILIKE` saja, hapus `Features/Search`,
  `apps/web/src/app/cari`, dan `KotakCari`. Tidak ada data yang hilang — kolomnya
  turunan, bukan sumber.

---

## Pembaruan 2026-09-17 - contoh keenam menggeser dua hitungan, dan relasi tidak ikut dicari

Tabel pengukuran di Konteks diambil dengan **lima** contoh seed.
[ADR-023](ADR-023-knowledge-graph-dasar.md) menambah contoh keenam, **Tool Use**
(bidang AI Agents), sebagai tujuan relasi contoh. Kelima belas kata kunci yang
dipakai saat itu diukur ulang di kedua endpoint, dan **hanya dua yang bergeser**:

| Kata kunci | Topik sebelum | Sesudah |
|---|---|---|
| `ai` | 3 | **4** (+`tool-use`) |
| `agent` | 1 | **2** (+`tool-use`) |

Sebabnya diperiksa lewat SQL, bukan diduga: `tool-use` cocok **hanya lewat
bidangnya** - FTS dan pola awal-kata atas teksnya sendiri `false`, atas teks
bidangnya `true`. Itu persis perilaku bagian 5. Kesepuluh baris tabel di Konteks
tidak memuat kedua kata kunci itu, dan tidak berubah.

Ringkasan tiga bidang ikut berubah pada hari yang sama (migrasi
`RingkasanBidangTanpaRujukanADR`, [ADR-024](ADR-024-explore-learn-navigasi-v1.md));
kelima belas kata kunci diukur lagi sesudahnya dan **identik**.

**Relasi antar-topik tidak ikut dicari.** Dan batas *"yang tercari nama dan
ringkasan"* di bagian *Yang TIDAK dicakup* kini juga tercetak untuk pembaca, di
halaman `/cari` yang nol hasil - kalimat yang wajib diubah #54.
