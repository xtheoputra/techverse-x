# ADR-023 — Knowledge Graph dasar: satu jenis relasi antar-topik, dijaga basis data, tampil sebagai tautan di halaman topik

**Status:** Diterima, **mekanismenya sudah dibangun**. **Produksi berisi NOL
sisi**, dan tetap nol sampai topik masuk lewat jalan
[ADR-021](ADR-021-jalan-menuju-tinjau.md). Menjawab
[#55](https://github.com/xtheoputra/techverse-x/issues/55). Sasaran Bulan 3 di [RENCANA-V1.md](../RENCANA-V1.md).
**Tanggal:** 2026-09-16 (diputuskan), dibangun sampai 2026-09-17; pemeriksaan siklus transitif 2026-09-24

## Konteks

### Tabel tanpa produsen, tanpa pembaca, tanpa isi

Sesi 13 memeriksa frasa RENCANA-V1 *"Knowledge Graph dasar (relasi sudah ada di
skema)"* dan menemukan bahwa frasa itu benar tentang skemanya — dan hanya
tentang skemanya:

| Yang diperiksa | Keadaan sebelum ADR ini |
|---|---|
| Tabel `technology_relationships`, entitas, enum, uji unit | ada |
| `TechnologyRelationship.Create()` dipanggil kode produksi | **nol** |
| Metode di agregat untuk menambah relasi | **tidak ada** |
| Endpoint · medan kontrak · tempat menampilkan · baris di basis data | tidak ada · tidak ada · tidak ada · **0** |

### Lima cacat integritas — diukur, bukan dibaca

Merancang produsennya membongkar lima hal di tabel yang dianggap "sudah ada":

1. **Ujung TUJUAN tidak punya kunci asing sejak `InitialTechnologySchema`.**
   Diukur lewat migrasi Down: `INSERT` dengan `ToTechnologyId` acak **masuk**, dan
   `DELETE` topik yang masih dibutuhkan **masuk** sambil meninggalkan sisi yang
   menggantung — yang lalu dijatuhkan diam-diam oleh `JOIN` pembaca mana pun.
2. **`Id` bertanda `ValueGeneratedOnAdd` padahal domain mengisinya sendiri.**
   Sisi yang ditambahkan ke topik yang SUDAH tersimpan berubah jadi `UPDATE`,
   lalu `DbUpdateConcurrencyException: ... expected to affect 1 row(s), but
   actually affected 0 row(s)` — **HTTP 500 di permintaan tulis pertama**.
   **Hanya satu dari tiga rancangan independen yang menangkapnya**; penilai
   memverifikasi dua lainnya akan membalas 500. Terbukti merah di dua lapis:
   persistensi dan endpoint (5 dari 10 uji 500).
3. **Penolakan sisi ke diri sendiri hanya ada di domain.** Kalimat
   [ADR-015](ADR-015-skema-data-v1.md) bagian 3 — *"kunci unik (From, To, Kind)
   dan penolakan simpul berelasi dengan dirinya sendiri"* — setengah benar.
4. **Indeks unik per arah tidak bisa menangkap kebalikan.** A→B dan B→A sama-sama
   lolos. Terukur saat penjaga aplikasinya disabotase: baris sepasang jadi 2, dan
   halaman A berbunyi `requires=[B]` sekaligus `requiredBy=[B]`.
5. **Empat dari lima anggota `RelationshipKind` menunjuk sesuatu yang bukan topik
   V1** (tabel di bawah). Dan baris berjenis yang kelak dibuang **meledak saat
   dibaca**, bukan saat ditulis — diukur: `Cannot convert string value 'Uses'
   from the database to any value in the mapped 'RelationshipKind' enum.`

### Kesembilan sisi di KERANGKA.md 4.6, dipetakan ke struktur V1

Enum lama berbunyi *"diambil apa adanya dari graph model di KERANGKA.md 4.6"*.
Diperiksa terhadap ADR yang sudah diterima:

| Sisi (`AI Agent` →) | Yang sebenarnya di V1 |
|---|---|
| `requires → LLM` | bidang → topik; LLM topik AI & ML yang dijadwalkan V1.1 |
| `uses → RAG` | tidak ada topik RAG di [ADR-010](ADR-010-taksonomi-bidang.md) |
| `uses → MCP` | **keanggotaan bidang** — MCP topik di bidang AI Agents (`FieldId`) |
| `uses → Tool Calling` | idem |
| `related → LangGraph` | kerangka kerja → katalog `Tool`; LangGraph dibuang ([ADR-007](ADR-007-agent-platform.md)) |
| `related → AutoGen` | katalog `Tool`; *maintenance mode* ([ADR-012](ADR-012-template-halaman.md)) |
| `used_by → OpenAI` · `used_by → Microsoft` | `Company` — bukan entitas V1 ([ADR-015](ADR-015-skema-data-v1.md)) |
| `enables → AI Engineer` | peran kerja — dicabut [ADR-009](ADR-009-tulang-punggung-navigasi.md)/010 |

**Nol dari sembilan adalah sisi topik→topik sebagaimana tertulis**, dan sumbernya
sendiri — "AI Agent" — adalah **bidang** menurut ADR-010. Model graf KERANGKA
bahkan mengarahkan `REQUIRES` ke `(:Skill)`, entitas yang tidak ada di V1.

### Ketegangan yang harus dinyatakan terbuka

[#54](https://github.com/xtheoputra/techverse-x/issues/54) dan [ADR-021](ADR-021-jalan-menuju-tinjau.md)
sama-sama menolak **membangun untuk isi yang belum ada**. Produksi hari ini nol
topik, jadi produsen relasi yang dibangun sekarang adalah — sekilas — pelanggaran
aturan yang sama. Kenapa bukan, dijawab di bagian tersendiri di bawah.

## Keputusan

### 1. Produsen: satu endpoint tulis di grup redaksi; produksi hanya lewat ADR-021

`POST /api/v1/technologies/{slug}/requires`, dipasang **di bawah cabang
`editorialWrites`** [ADR-020](ADR-020-permukaan-tulis-api.md) — jadi ia hilang di
produksi tanpa aturan tambahan, dan ketiga uji ADR-020 langsung menjaganya
begitu alamatnya masuk `SemuaEndpointTulis`.

- **Pemanggil pengembangan:** `database/seeds/seed.mjs` dan uji integrasi.
- **Pemanggil produksi:** hanya workflow bergerbang ADR-021 begitu dibangun
  (pemicunya tetap [#40](https://github.com/xtheoputra/techverse-x/issues/40)), yang memanggil endpoint yang sama
  lewat `localhost` di dalam runner.
- **Tidak ada satu sisi pun yang mencapai produksi di Bulan 3.** Setiap dokumen
  wajib mengatakannya.

### 2. Jenis relasi: `Requires` saja

`RelationshipKind = { Requires = 0 }`. `Uses`, `RelatedTo`, `UsedBy`, dan
`Enables` dibuang — selagi barisnya nol di mana pun.

- Hanya `Requires` yang punya contoh topik→topik yang masuk akal di ADR-010:
  orkestrasi AI Agents di atas fondasi AI & ML, Edge AI yang *"beban belajarnya
  ML"*, MCP di atas tool use.
- `UsedBy` dan `Enables` sekadar kebalikan `Uses` dan `Requires` — satu fakta
  jadi bisa disimpan dua cara. `RelatedTo` simetris, dan indeks unik per arah
  tidak bisa menjaganya tanpa indeks ekspresi yang tidak bisa dimodelkan EF.
- **KERANGKA mengarahkan `REQUIRES` ke `(:Skill)`; V1 tidak punya `Skill`.**
  Prasyarat berupa keterampilan tetap PROSA di langkah 0 roadmap (ADR-012). Hanya
  prasyarat yang **sudah punya halaman topik** yang jadi sisi `Requires`.
- `Kind` disimpan sebagai teks, jadi angka `0` tidak berarti apa-apa.
- **Biayanya timpang, dan itu yang memutuskan.** Menambah jenis nanti = satu
  anggota enum + migrasi CHECK + label tampilan (+ ADR). Membuang jenis setelah
  ada baris = migrasi data, dan sampai migrasi itu ada setiap halaman yang
  memuat sisi tersebut membalas 500.
- **Pemicu evaluasi ulang:** begitu ketujuh topik AI Agents [#42](https://github.com/xtheoputra/techverse-x/issues/42)
  ditulis, daftar setiap relasi yang ingin dinyatakan penulisnya tapi tidak bisa.

### 3. Arah, kebalikan, dan siklus

Baris `(From=A, To=B, Requires)` berarti **"A membutuhkan B: pelajari B lebih
dulu"**.

- **Tiap fakta disimpan SEKALI.** Kebalikannya tidak pernah disimpan: halaman A
  menampilkan B di *Pelajari lebih dulu*, halaman B menampilkan A di
  *Dibutuhkan oleh* — diturunkan saat dibaca dari `ToTechnologyId`.
- **Sisi identik diulang = no-op idempoten** (200, tanpa baris kedua).
- **Kebalikan yang bertentangan ditolak 400** oleh agregat (*"Dua topik tidak
  boleh saling mensyaratkan"*); handler menyerahkan tujuan `Requires` langsung
  milik topik tujuan untuk pemeriksaan ini. ⚠️ **Kedua bagian kalimat ini basi
  sejak 2026-09-24** — pesannya berganti dan yang diserahkan kini penutupan
  transitif; lihat pembaruan di bawah. Dibiarkan sebagai rekaman keadaan 17
  September, tapi jangan mencari kalimat galat itu di kode: ia sudah tidak ada.
- **Siklus yang lebih panjang (A→B→C→A) TIDAK diperiksa di V1.** Pembacaan selalu
  satu lompatan, jadi siklus yang lolos tidak pernah bisa membuat halaman
  berputar atau meledak. Peningkatannya hanya menyentuh handler: serahkan
  penutupan transitif alih-alih tujuan langsung; tanda tangan metode agregat
  tidak berubah. **Pemicu:** sebelum fitur apa pun mengurutkan topik berdasarkan
  `Requires` (Learn), atau begitu sisi produksi punya lebih dari satu penulis.

> ✅ **Dibangun 2026-09-24 — dan satu kata di paragraf di atas kurang.**
> `RequireTopicHandler` kini menyerahkan **penutupan transitif** sisi `Requires`
> milik tujuan, disapu berlapis (LINQ EF biasa, bukan CTE rekursif, jadi lapisan
> fitur tetap tanpa SQL mentah; harganya **kedalaman + 1** perjalanan ke basis
> data, dan penyaring "sudah dikunjungi" membuatnya berhenti walau barisnya sudah
> berputar sebelum aturan ini ada).
>
> Ramalan *"hanya menyentuh handler"* benar untuk **tanda tangannya** —
> `RequireTopic(Guid, IReadOnlyCollection<Guid>)` tidak berubah sama sekali — tapi
> **kalimat galatnya harus ikut berubah.** *"Dua topik tidak boleh saling
> mensyaratkan"* adalah pernyataan yang **salah** ketika yang ditutup lingkaran
> bertiga: ia menyuruh penulisnya mencari sisi kebalikan yang tidak ada. Sekarang:
> *"Topik yang diminta sudah mensyaratkan 'X', langsung atau lewat rantai
> prasyarat. Prasyarat tidak boleh berputar (ADR-023)."* Agregatnya menerima
> himpunan rata dan **memang tidak bisa** membedakan tujuan langsung dari ujung
> rantai — itu yang membentuk kata-katanya, bukan kehati-hatian.
>
> Bukti merahnya diukur, bukan diasumsikan: dengan kueri satu lompatan yang lama,
> `Siklus_bertiga_ditolak_400` dan `Rantai_empat_topik_ditolak_di_lompatan_TERJAUH`
> keduanya gagal berbunyi **`Expected: BadRequest / Actual: OK`** — siklusnya
> diterima 200 dan barisnya tersimpan. Dengan pesan yang lama, 2 uji unit + 3 uji
> integrasi merah. Kendalinya juga dibuktikan: jalan pintas di dalam rantai yang
> sama (A→C saat A→B→C→D sudah ada) tetap **200**, jadi pemeriksaannya bukan
> penolak segala sisi baru. Ditagih di
> [#59](https://github.com/xtheoputra/techverse-x/issues/59) sebagai syarat yang
> wajib mendarat sebelum Learn.

### 4. Penjaga di basis data — migrasi `RelasiAntarTopik`

1. `Id` **`ValueGeneratedNever`**.
2. FK `ToTechnologyId → technologies.Id` **`ON DELETE RESTRICT`**, plus indeks
   `ix_technology_relationships_to_technology_id` (PostgreSQL tidak mengindeks
   kolom FK sendiri; indeks unik sudah diawali `From`).
3. CHECK `ck_technology_relationships_bukan_diri_sendiri`: `From <> To`.
4. CHECK `ck_technology_relationships_kind`, **SQL-nya dibangkitkan dari
   `Enum.GetNames<RelationshipKind>()`**. Menambah anggota enum mengubah model EF,
   jadi `HasPendingModelChanges` benar dan `dotnet ef database update` menolak
   jalan (`PendingModelChangesWarning`, exit 1 — diukur) sampai migrasinya dibuat.

FK `FromTechnologyId` tetap `CASCADE` dan indeks unik `(From, To, Kind)` tetap.

**Kenapa `RESTRICT` di ujung tujuan:** mengikuti aturan repo ini untuk tautan ke
agregat lain (`Field`→`Technology`, `Tool`→`TechnologyTool` sama-sama Restrict).
Membuang B tidak boleh diam-diam menghapus *"Pelajari lebih dulu: B"* dari
halaman A, yang mungkin sudah diperiksa manusia. V1 tidak punya endpoint
penghapus topik, jadi biayanya hari ini cuma urutan pembersihan di uji.

⚠️ PostgreSQL menulis ulang `"Kind" IN ('Requires')` yang beranggota satu menjadi
`("Kind")::text = 'Requires'::text`. Definisinya tetap hanya memuat `Requires`.

### 5. Batas agregat: `Technology.RequireTopic(topicId, topicRequires)`

Sisi tetap anak agregat topik **ASAL**. Urutan pemeriksaannya:

1. `topicRequires` tidak boleh null.
2. Ke diri sendiri → `ArgumentException` bermedan `topicSlug`.
3. Sisi `Requires` identik sudah ada → kembali tanpa `Touch()`.
4. `topicRequires` memuat `Id` topik ini → `InvalidOperationException` (siklus).
   Sejak 2026-09-24 isinya penutupan transitif, jadi butir ini menangkap lingkaran
   sepanjang apa pun — bukan cuma yang berdua.
5. Selain itu tambah sisi, lalu `Touch()`.

- **Parameter kedua WAJIB, tanpa nilai bawaan**, supaya tidak ada pemanggil yang
  bisa melewati aturan siklus dengan melupakannya — idiom yang sama dengan
  parameter `editorialWrites` ADR-020.
- **Tidak menggugurkan `HumanReviewed`** (alasan yang sama dengan `Touch`), dan
  **tidak pernah masuk `MissingSections`**.
- `TechnologyRelationship.Create` jadi `internal`, seperti `RoadmapStep` dan
  kawannya. `DbSet<TechnologyRelationship> Relationships` (nol pemanggil)
  **dibuang**: anak agregat tidak punya pintu tulis kedua.
- Tidak ada `Unrelate()`, tidak ada event domain — keduanya akan nol pemanggil,
  pola #55 yang keempat.

⚠️ `ArgumentException(..., "topicSlug")` ditolak CA2208 (peringatan = galat di
repo ini) karena `topicSlug` bukan nama parameter metode. Nama medan itu kontrak
`ValidationProblem`, jadi CA2208 dimatikan untuk **satu baris itu** dengan
alasannya tertulis.

### 6. Bentuk endpoint

`POST /api/v1/technologies/{slug}/requires`, muatan
`RequireTopicRequest(string TopicSlug)`.

| Jawaban | Kapan |
|---|---|
| **200** + `TechnologyResponse` lengkap | berhasil, termasuk pengulangan idempoten |
| **404** | topik di **ALAMAT** tidak ada — satu-satunya arti 404 di sini |
| **400** `topicSlug` | slug tidak sah · slug milik **bidang** (pesannya menyebut "bidang") · topik tujuan tidak ada · ke diri sendiri |
| **400** `body` | kebalikan yang bertentangan |

⚠️ Tabel di atas berlaku untuk muatan yang BENTUKNYA sah. JSON yang terpotong,
atau `topicSlug` berupa angka, hari ini menjawab **500** — bukan cacat irisan ini,
melainkan kelas yang berlaku di **semua** endpoint tulis: `BadHttpRequestException`
ditangkap `UseExceptionHandler()` sebelum statusnya terbaca. Terukur 2026-09-17,
ditagih di [#61](https://github.com/xtheoputra/techverse-x/issues/61).

> ✅ **Diperbaiki hari yang sama:** kini **400** yang menyebut sebabnya
> (`Path: $.topicSlug`), di `Development` maupun `Production` — lihat Pembaruan
> kedua [ADR-020](ADR-020-permukaan-tulis-api.md). Paragraf di atas dibiarkan
> sebagai rekaman.

🔑 **Tujuannya di MUATAN, bukan di alamat** — meniru `AttachTool`. Dengan begitu
gerbang ADR-020 tetap terbaca dari luar: sakelar hidup → handler membaca muatan
→ **400**; sakelar mati → tidak ada rute → **404**. `PUT /requires/{topicSlug}`
akan membuat "tujuan tidak ada" jadi 404 yang tidak bisa dibedakan dari rute yang
tidak dipasang.

🔑 **Satu rute per jenis**, bukan `/relationships` bermedan jenis: tidak ada teks
jenis yang diurai, jadi celah `Enum.TryParse` yang meluluskan angka tidak punya
jalan masuk. Celah itu **nyata di tempat lain** — terukur, `POST …/resources`
dengan `"type":"0"` tersimpan sebagai `OfficialDocs` ([#60](https://github.com/xtheoputra/techverse-x/issues/60);
diperbaiki 2026-09-17 — jenis sumber kini diurai berdasarkan nama saja).

⚠️ **Balapan yang diterima:** pencarian tujuan, pembacaan penutupan milik tujuan,
dan `SaveChanges` bukan satu transaksi. Dua penulis serentak bisa menyelipkan
siklus, dan tujuan yang dibuang di antaranya jadi 500 dari kunci asingnya. Itu
celah yang sama dengan pemeriksaan slug↔bidang di `CreateTechnologyHandler`,
diterima dengan alasan yang sama: penulisnya tunggal (seed, atau alur ADR-021).

### 7. Tempat operasinya: irisan sendiri + jalur mutasi bersama

`Features/RequireTopic/{Handler,Endpoint}.cs` — **bukan** metode di
`EditContentSectionsHandler`, yang satu irisan justru karena seluruh operasinya
berubah karena ADR-012. Relasi berubah karena ADR ini.

Jalur *muat → ubah → simpan → kembalikan bentuk lengkap* pindah utuh ke
`Features/TopicMutation.cs` (`TopicMutationOutcome`, `RunAsync`, `ToHttpResult`)
dan dipakai kedua irisan — menyalinnya ke irisan kedua menghidupkan lagi
salinan yang menyimpang. Tambahannya dua: `.Include(t => t.Relationships)` (tanpa
itu idempotensi hilang dan pengulangan jadi 500 dari indeks unik — terukur) dan
pemuat relasi sebelum pabrik respons.

⚠️ **Terukur saat membangunnya:** `RunAsync` menerjemahkan
`InvalidOperationException` dari lambda mutasi jadi 400 — dan tidak bisa
membedakan galat agregat dari galat pemrograman. `topicId!.Value` yang dipanggil
DI DALAM lambda menjawab **400 `{"body":["Nullable object must have a
value."]}`**, bukan 500. Aturannya karena itu: **setiap pencarian selesai SEBELUM
`RunAsync`, dan lambdanya cukup satu panggilan metode agregat.**

### 8. Kontrak baca

`TechnologyResponse` + `Requires` dan `RequiredBy`, keduanya
`IReadOnlyList<TechnologySummaryResponse>`, urut `Field.DisplayOrder` lalu nama.

- **Item bertipe `TechnologySummaryResponse`**, jadi judul yang ditautkan tidak
  bisa bepergian tanpa label kematangannya sendiri — dijamin tipe (ADR-012).
- **Server menurunkan kebalikannya**, seperti `MissingSections`; klien tidak
  menghitung ulang.
- Satu pemuat, `Features/TopikTerhubung.cs`, dipakai **GET maupun setiap respons
  tulis**. Kalau keduanya memuat dengan caranya sendiri, respons `PUT` bagian isi
  bisa melaporkan relasi berbeda dari GET sesudahnya.
- `TechnologyResponseFactory.From(technology, field, related, toolCatalog = null)`:
  **`related` tanpa nilai bawaan dan SEBELUM parameter opsional.** Diukur
  dua-duanya: membuang argumennya → `CS7036`; menaruhnya sesudah `toolCatalog` →
  `CS1737`.
- ⚠️ Proyeksi ringkasnya **salinan** dari `SearchTechnologyHandler`, dicatat di
  kedua tempat; yang menjaganya hanya komentar itu dan asersi kematangan + nama
  bidang di `PrasyaratTopikTests`.

### 9. Tampilan: blok "Topik terhubung" SESUDAH template

Satu komponen server, `apps/web/src/components/TopikTerhubung.tsx`, dirender
`TopicView` sesudah `TopicSections`:

- judul **"Topik terhubung"**, dengan *"Pelajari lebih dulu"* (requires) dan
  *"Dibutuhkan oleh"* (requiredBy); tiap sub-daftar hanya muncul kalau berisi;
- tiap item: tautan `/teknologi/<slug>`, nama bidang **hanya kalau berbeda**, dan
  **`MaturityBadge` wajib** milik item itu sendiri;
- **seluruh blok disembunyikan kalau kosong** — pengecualian sadar dari aturan
  *"bagian kosong tetap ditampilkan"* di `TopicSections`. Relasi bukan bagian yang
  wajib diisi; baris "belum ada relasi" akan menandai setiap halaman tidak
  lengkap secara palsu;
- **tidak pernah dihitung di `missingSections`**.

**Hubungannya dengan langkah 0 roadmap (ADR-012):** langkah 0 tetap satu-satunya
prasyarat PROSA — keterampilan yang belum tentu punya halaman di situs ini. Sisi
`Requires` adalah navigasi ke halaman yang ada. **Topik yang sudah jadi tujuan
sisi tidak diulang di langkah 0.** Itu menjawab kekhawatiran ADR-012 bagian 1
("membacanya dua kali, menulisnya dua kali") secara tertulis.

Tidak ada `/graph`, tidak ada visualisasi. Halaman bidang, pencarian, dan daftar
tidak menampilkan apa pun yang baru.

### 10. Selisih versi web ↔ API

Web dibangun dari sumber tiap kali `main` bergerak; API berjalan dari citra
ber-SHA yang dipasang tangan (ADR-018/019), dan selama [#57](https://github.com/xtheoputra/techverse-x/issues/57)
citra baru bahkan tidak bisa dibangun. `getTechnology()` karena itu mengisi `[]`
untuk kedua larik yang tidak ada. Terukur: tanpa pengisian itu, API lama membuat
halaman topik **HTTP 500** (`Cannot read properties of undefined (reading
'length')`); dengan pengisian, 200 tanpa blok relasi — dan itu memang benar untuk
API itu. Membuang `?? []` begitu saja tidak bisa dikompilasi (`TS2322`), jadi
tipenya ikut menjaga.

## Kenapa produsen dibangun sebelum ada isi — dan kenapa itu bukan kegagalan #54/ADR-021

1. **Bentuk penyimpanannya sudah diputuskan lama.** Tabelnya ada sejak
   `InitialTechnologySchema` (ADR-006/015). Yang hilang adalah cara yang **dijaga**
   untuk memakainya — dan tabel dengan nol produsen persis pola yang #55 tagih.
2. **Tidak ada jalan tulis produksi yang ditambahkan.** Endpointnya hilang di
   produksi (ADR-020); produsen produksinya tetap alur ADR-021 yang belum dibangun.
3. **Membaliknya murah selama barisnya nol di mana pun.**
4. **Kosakatanya dipangkas ke satu jenis**, dengan pemicu evaluasi ulang tertulis.

Bedanya dengan #54: pencarian isi halaman menuntut **memilih bentuk** (trigger,
view, tabel dokumen) yang baru bisa dinilai setelah ada isi. Di sini bentuknya
tidak dipilih ulang — ia dijaga dan diberi satu pintu.

## Yang TIDAK diputuskan, dan tidak dibangun

| Tidak dibangun | Pemicu |
|---|---|
| Jenis relasi lain | sesudah #42: daftar relasi yang ingin dinyatakan penulis tapi tidak bisa |
| ~~Pemeriksaan siklus transitif~~ | ✅ **dibangun 2026-09-24**, sebelum pemicunya tiba — lihat pembaruan di bagian 3 |
| Jalan membuang sisi (`Unrelate`, endpoint) | diputuskan bersama ADR-021 dibangun — bersama jalan membuang bagian isi, yang juga belum ada |
| `/graph` dan visualisasi | EPIC 10 (Neo4j, ADR-006) |
| Relasi bidang↔bidang | nanti, sebagai proyeksi saat-baca dari sisi topik |
| Penyaringan `Status`/`Publish` pada relasi | tidak ada satu halaman pun yang menyaring `Status` hari ini |
| Relasi di hasil pencarian atau daftar topik | belum ada kebutuhan yang terukur |

## Alternatif yang ditolak, berikut alasannya

| Alternatif | Kenapa ditolak |
|---|---|
| **Menyemai sisi lewat migrasi** | ADR-021 menolak isi-sebagai-migrasi; topik adalah data. |
| **SQL mentah ke Neon** | Melewati penjaga agregat sepenuhnya. |
| **Menyalakan `Editorial__WritesEnabled` sementara di produksi** | Sudah ditolak tabel alternatif ADR-021: selama menyala, dunia ikut bisa menulis. |
| **Membangun workflow ADR-021 sekarang** | Pemicunya #40. |
| **Daftar sisi di dalam `CreateTechnologyRequest`** | Mengikat pembuatan topik pada tujuan yang mungkin belum ada. |
| **Mempertahankan kelima jenis** | Tidak satu pun pernah diuji sisi sungguhan, dan fakta yang sama jadi bisa disimpan dua cara. |
| **`{Requires, RelatedTo}` dengan indeks `LEAST/GREATEST`** | Indeks yang tidak terlihat EF + pengurai jenis, demi jenis simetris yang contoh kuatnya cuma A2A↔MCP. Ditinjau ulang di pemicu #42. |
| **Menulis kedua arah** | Dua baris yang bisa menyimpang dan saling membantah. |
| **Hanya menampilkan sisi keluar** | Separuh sisi tidak terjangkau dari halaman tujuannya. |
| **BFS atas seluruh graf di tiap tulis** | Belum ada yang menelusuri graf, dan banyak yang harus dibuktikan merah selagi CI mati. |
| **`CASCADE` di ujung tujuan** | Hilangnya isi diam-diam di halaman lain. |
| **Pemeriksaan keberadaan hanya di handler** | Balapan, dan SQL tangan atau penghapus kelak meninggalkan yatim. |
| **Daftar jenis ditulis tangan di SQL CHECK** | Hanyut dari enum. |
| **`PUT /requires/{topicSlug}`** | Tujuan tak ada jadi 404 yang sama dengan rute tak terpasang. |
| **`/relationships` bermedan jenis** | Pengurai untuk satu nilai, plus celah angka `Enum.TryParse`. |
| **Metode di `EditContentSectionsHandler`** | Membuat alasan irisan itu bohong. |
| **Irisan baru dengan salinan `MutateAsync`/`Translate`** | Salinan yang menyimpang — pelajaran #26. |
| **Endpoint `GET …/relationships` terpisah** | Permintaan kedua dan keadaan galat kedua; relasi bisa tampil dari panggilan yang gagal sementara topiknya berhasil. |
| **Parameter `related` bernilai bawaan** | Respons tulis diam-diam melaporkan topik tanpa relasi. |
| **Baris header "Pelajari dulu" sebelum isi** | Menghidupkan lagi blok prasyarat terpisah yang ditolak ADR-012 bagian 1. |
| **Tautan di dalam bagian Learning Roadmap** | Mematahkan kontrak "Belum diisi." `TopicSections`, dan menyembunyikan sisi di halaman kurasi yang roadmap-nya kosong — keadaan normal V1 untuk 13 bidang. |
| **Bagian template keenam bertanda "Belum diisi"** | Menandai setiap halaman tidak lengkap secara palsu. |

## Konsekuensi

- 🔴 **Produksi nol sisi.** Kalimat apa pun yang menyebut Knowledge Graph "hidup"
  atau "selesai" tanpa keterangan itu mengulang klaim basi yang #55 peringatkan.
- **Menghapus topik yang dibutuhkan topik lain ditolak basis data** (RESTRICT).
  Pembersihan — uji, dan penghapus kelak — wajib membuang sisi lebih dulu.
  Terukur di dev DB: `DELETE` topik tujuan membalas `violates foreign key
  constraint "FK_technology_relationships_technologies_ToTechnologyId"`.
- **Balapan siklus diterima**, seperti pemeriksaan slug↔bidang.
- **Web mentoleransi API yang lebih tua** (bagian 10).
- **Uji:** 7 unit (`PrasyaratTopikTests` domain), 1 integrasi penjaga model
  (`ModelMigrasiTests`), 4 integrasi basis data (`PrasyaratTopikPersistenceTests`),
  10 integrasi endpoint (`PrasyaratTopikTests`). Hitungan repo 95+32 → **102+47**.
  Setiap penjaga dibuktikan merah — rincian dan kalimat kegagalannya di
  [SESSION-LOG](../SESSION-LOG.md) Sesi 14.
  **Pemeriksaan siklus transitif 2026-09-24 menambah 2 unit + 2 integrasi endpoint:
  9 unit dan 12 integrasi endpoint, hitungan repo → 104+63.**
- ⚠️ **`ModelMigrasiTests` TIDAK menjaga `ValueGeneratedNever`** — terukur: baris
  itu dikomentari, ujinya tetap hijau, sebab `ValueGenerated` pada kunci uuid
  tanpa identity tidak mengubah kolom. Yang menjaganya P2 (persistensi) dan
  `PrasyaratTopikTests` (endpoint).
- **Cara membalikkan:** buang migrasi `RelasiAntarTopik`, hapus
  `Features/RequireTopic` dan `Features/TopikTerhubung.cs`, buang dua medan
  respons dan `TopikTerhubung.tsx`, kembalikan `RequireTopic` dan enum. Tidak ada
  data yang hilang selama produksi nol baris.
