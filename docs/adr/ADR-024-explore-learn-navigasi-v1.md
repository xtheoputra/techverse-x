# ADR-024 — Explore, Learn, dan navigasi V1: rute yang sudah ada melayani Explore, Learn menunggu pemicu, dan teks pembaca tidak menjanjikan apa pun

**Status:** Diterima. **Menunda `/explore`, `/learn`, dan `/graph` dengan pemicu
tertulis** — keputusan untuk *belum* membangun juga keputusan
([adr/README](README.md)). Sasaran Bulan 3 di [RENCANA-V1.md](../RENCANA-V1.md).
**Tanggal:** 2026-09-16 (diputuskan), dibangun 2026-09-17

## Konteks

Sasaran Bulan 3 RENCANA-V1 berbunyi **"Explore + Learn + pencarian"**, dengan bukti
selesai *"orang bisa menemukan halaman tanpa menebak URL"*. Pencarian mendarat di
[ADR-022](ADR-022-pencarian-teks-penuh.md); Knowledge Graph dasar di
[ADR-023](ADR-023-knowledge-graph-dasar.md). Tabel *Keadaan Bulan 3* bahkan tidak
punya baris untuk Explore maupun Learn.

[ADR-009](ADR-009-tulang-punggung-navigasi.md) sudah menetapkan tabel rutenya —
`/explore`, `/learn`, `/labs`, `/ai`, `/intelligence`, `/graph`, `/future`, dan
`/teknologi/<slug>` sebagai satu-satunya URL yang dijanjikan stabil. Bentuk
produksinya hari ini: **14 bidang, 0 topik, permukaan tulis tertutup**
([ADR-020](ADR-020-permukaan-tulis-api.md)).

Dan RENCANA-V1 sudah menulis kegagalan yang paling mungkin: *"Menu yang lengkap
tapi kosong tidak bisa dipakai siapa pun."*

### Garis dasar yang diukur sebelum satu baris pun berubah

Build produksi dari kode lama (`next start` terhadap API dev berisi 6 topik),
dipindai teks yang **terlihat** — `<script>`/`<style>` dan tag dibuang, entitas
didekode — dengan pola
`/Bulan \d|menyusul|beranda sementara|ADR-\d|run\.ps1|localhost|api:8080/`:

| Temuan | Di mana |
|---|---|
| **9 kena di halaman muka** | *"Tahap: Bulan 1 — taksonomi mendarat."*, *"Navigasi tujuh bagian aplikasi menyusul; halaman ini masih beranda sementara."*, ADR-009/010/012 di spanduk — **dan tiga "lihat ADR-010" di ringkasan BIDANG** |
| `id="q"` **dua kali** di `/cari` | kotak di kepala halaman dan di badan; `<label>` kotak kedua menunjuk input pertama |
| Kalimat nol-hasil `/cari` | *"Isi TechVerse X masih dibangun bidang per bidang…"* — mengundang pembaca percaya isi halaman ikut tercari, yang justru [#54](https://github.com/xtheoputra/techverse-x/issues/54) larang dijanjikan |
| URL tebakan (`/learn`) | halaman 404 bawaan Next berbahasa Inggris |

🔴 **Tiga dari sembilan kena datang dari DATA, bukan dari kode web.** Ringkasan
bidang disemai migrasi dari `FieldCatalog`, dan keempat belas kartunya adalah isi
utama halaman muka selama produksi nol topik. Memindai kode web tidak akan pernah
menemukannya; memindai halaman jadinya menemukannya di jalan pertama.

## Keputusan

### 1. Explore dilayani rute yang sudah ada — dan itu DIUKUR

Tidak ada `/explore` di Bulan 3. Explore V1 adalah:

- **`/`** — `FieldGrid` dengan keempat belas bidang, plus daftar topik;
- **`/teknologi/<bidang>`** — topik yang disaring per bidang;
- **`/cari`** ([ADR-022](ADR-022-pencarian-teks-penuh.md));
- **tautan antar-topik** ([ADR-023](ADR-023-knowledge-graph-dasar.md)).

**Ukurannya penelusuran dari `/`**: telusuri melebar setiap `href="/…"` satu asal
(tanpa `/_next`, tanpa query string), kumpulkan setiap `/teknologi/*` yang
tercapai, bandingkan dengan himpunan yang dikenal API (`GET /api/v1/fields` +
`GET /api/v1/technologies?pageSize=100`).

| Bentuk | Tercapai | Dikenal API |
|---|---|---|
| Pengembangan (seed) | **20** | 20 = 14 bidang + 6 topik |
| Produksi (`docker-compose.prod.yml`) | **14** | 14 = 14 bidang + 0 topik |

Kendalinya dibuktikan merah: membuang `<Link>` topik di halaman bidang dan di
daftar halaman muka → **14 tercapai, keenam topik hilang**.

**Pemicu `/explore`:** PR pertama yang harus menaruh sesuatu SELAIN peta bidang di
`/` (kandidat terdekat: arus arXiv Bulan 4). PR itu yang memindahkan peta ke
`/explore`, menambah butir menunya, dan memperbarui langkah verifikasi
[#40](https://github.com/xtheoputra/techverse-x/issues/40) dalam perubahan yang sama.

Alasannya: ADR-009 mendefinisikan `/explore` sebagai *"peta 14 bidang, filter per
bidang"* — dan `/` beserta halaman bidang kanonik sudah memberi persis itu di
produksi. ADR-009 hanya menjanjikan `/teknologi/<slug>` stabil, jadi menunda
`/explore` tidak memutus URL siapa pun. `/explore` duplikat berarti dua halaman
identik yang harus dijaga sama; memindahkan peta sekarang berarti menulis ulang
runbook #40 yang sebentar lagi diikuti pemilik, tanpa kemampuan baru.

### 2. Learn ditunda, dengan pemicu dan penagih

Tidak ada `/learn`. **Learn V1 = bagian Learning Roadmap di tiap halaman topik,
plus tautan *"Pelajari lebih dulu"*.**

**Pemicu `/learn`:** [#42](https://github.com/xtheoputra/techverse-x/issues/42) tutup — tujuh topik AI Agents
berstatus `tinjau`, yang sekaligus mengunci bentuk roadmap (ADR-012 bagian 4) —
**DAN** produksi menampilkan setidaknya satu roadmap yang terisi. Pada titik itu
jalur belajar boleh mengurutkan topik lewat sisi `Requires`, dan **pemeriksaan
siklus transitif ADR-023 wajib mendarat lebih dulu**. Ditagih di
[#59](https://github.com/xtheoputra/techverse-x/issues/59).

Alasannya: produksi nol topik berarti nol roadmap. ADR-009 menaruh Career Mode
(`/learn/karier`) dan AI Roadmap Generator di V1.1. Indeks `/learn` sekarang
adalah kegagalan *"40 halaman setengah jadi"* yang disebut RENCANA-V1 dan
ADR-021. Urutan bulan di RENCANA adalah urutan prioritas, bukan kunci — jadi
penundaan yang **tercatat dengan pemicu** sah; penundaan diam-diam tidak.

### 3. `/graph` menunggu EPIC 10

Knowledge Graph V1 **adalah** daftar tautan di halaman topik (ADR-023). Rute
`/graph` dan visualisasinya menunggu EPIC 10 (Neo4j, [ADR-006](ADR-006-neo4j.md)) —
dan produksi hari ini nol sisi.

`/labs`, `/ai`, `/intelligence`, dan `/future` tetap di bulan RENCANA-nya
masing-masing.

### 4. Aturan navigasi: sebuah bagian masuk menu hanya kalau halamannya berisi

`app/layout.tsx` tetap **merek + kotak cari** saja. **Tautan ke satu bagian ADR-009
masuk layout HANYA di PR yang membuktikan halamannya punya setidaknya satu isi
sungguhan dalam bentuk produksi** (`docker-compose.prod.yml`, tulis tertutup).

`typedRoutes: true` di `next.config.ts` menjaga **separuhnya**: `<Link>` ke rute
yang tidak ada menggagalkan `next build`. Dibuktikan merah — `<Link href="/learn">`
di halaman 404 → `error TS2322: Type '"/learn"' is not assignable to type
'UrlObject | RouteImpl<"/learn">'`. Semua `href` yang ada **lolos tanpa satu cast
pun**.

⚠️ **Separuh yang lain — "halamannya berisi" — tidak dijaga kode apa pun.** Ditulis
di sini supaya tidak dibaca sebagai terjaga.

### 5. Aturan teks pembaca

**Teks yang tercetak di produksi tidak memuat bulan rencana, janji tanpa tanggal
("menyusul"), nomor ADR, perintah pengembang, atau alamat internal.** Aturan ini
berlaku untuk teks dari **mana pun** — kode web maupun data yang disemai migrasi.

Diterapkan:

- Spanduk halaman muka tinggal satu kalimat: *"Setiap topik selalu membawa label
  kematangan isinya: halaman yang belum diperiksa manusia tidak pernah tampil
  tanpa mengatakannya."* *"Tahap: Bulan 1"* basi dalam kurang dari dua minggu;
  *"menyusul"* adalah janji tanpa tanggal yang tampil di keadaan normal produksi.
- Keadaan kosong halaman muka dan halaman bidang berhenti di *"Itu keadaan yang
  jujur, bukan galat."* — tanpa *"Bulan 1 mengejar taksonominya lebih dulu"*.
- **Tiga ringkasan bidang** berhenti menyuruh pembaca *"lihat ADR-010"* — migrasi
  `RingkasanBidangTanpaRujukanADR`.
- Kalimat nol-hasil `/cari` menyatakan batas hari ini: *"Pencarian mencocokkan
  nama dan ringkasan bidang serta topik, belum isi halamannya."* **#54 wajib
  mengubahnya di PR yang sama** begitu isi halaman ikut dicari.
- `KotakCari` menerima `id`; kotak di badan `/cari` jadi `q-halaman`.
- Daftar yang dipotong `pageSize` mengatakannya: *"Menampilkan X dari Y topik."*
  dan *"menampilkan N teratas"* — penelusuran di skala pengembangan tidak akan
  pernah melihat pemotongan diam-diam.

**Diperiksa dengan pemindaian teks terlihat di build produksi**, bukan di CI —
repo ini belum punya harness JS. Sesudah perubahan: **0 kena** di enam halaman
dalam bentuk pengembangan (dari 9), dan 0 kena dalam bentuk produksi.

Yang sengaja ditolak: spanduk *"Tahap: Bulan 3"* (basi lagi bulan depan); baris
status yang dihitung dari data plus legenda kematangan (menggandakan angka
`FieldGrid`, dan legendanya akan mengklaim arti yang tidak ditegakkan domain);
*"isi ditulis bidang per bidang"* (janji yang lebih halus).

### 6. Halaman 404 statis

`app/not-found.tsx` tanpa `fetch` dan tanpa kotak cari sendiri: *"Halaman ini tidak
ada."* dengan tautan ke halaman muka dan pencarian. Ia melayani setiap URL yang
tidak cocok **dan** `notFound()` dari `/teknologi/[slug]`, dan tidak pernah
menyebut bagian yang belum ada.

⚠️ **Dua jalan itu tidak dirender sama — diukur.** URL yang tidak cocok (`/learn`)
dirender server utuh: 404, kalimatnya ada di HTML. `notFound()` dari rute dinamis
juga **404 + `noindex`**, tapi HTML-nya kerangka galat Next berbadan kosong, dan
halaman 404-nya baru tergambar sesudah JavaScript. Bentuk itu **sama persis sebelum
ADR ini** (dulu dengan teks bawaan Inggris), dan itulah harga status 404 sungguhan:
Next hanya bisa mengirim status sebelum badan mulai dialirkan. Tidak diubah.

### 7. Skema URL ADR-009 tidak berubah

Peringatan [KEPUTUSAN.md](../KEPUTUSAN.md) untuk keputusan navigasi — *"Seluruh
bentuk URL berubah. Semakin lama tayang, semakin mahal"* — menyangkut **skema
URL**, dan skema itu tidak disentuh. `/` tetap bekerja saat `/explore` kelak ada.

## Konsekuensi

- **Milestone *Bulan 3 — Explore, Learn & Pencarian* sengaja tetap terbuka** untuk
  #54 dan pemicu Learn. Ia tidak macet; ia menunggu dua pemicu yang tertulis.
- **Verifikasi #40 (*"14 bidang di halaman muka"*) tetap benar.**
- 🔴 **Aturan teks pembaca dan tampilan relasi tidak punya penjaga otomatis.**
  Keduanya hanya bertahan selama pemindaian teks dan pemeriksaan halaman diulang
  tangan di build produksi; PR berikutnya bisa menanam *"Bulan N"* lagi tanpa ada
  yang merah. Harness JS untuk `apps/web` adalah keputusan tersendiri.

  > ✅ **Dijawab sebagian 2026-09-24 — `.github/scripts/periksa-halaman-web.mjs`.**
  > Kedua pemindaian tangan itu jadi satu perintah (`.\run.ps1 halaman` /
  > `make cek-halaman`): ia menelusuri dari `/`, mengambil teks yang **terlihat**
  > (`<script>`/`<style>` dibuang berikut isinya — muatan RSC hidup di sana), memindai
  > pola ADR-024 apa adanya, dan membandingkan jumlah halaman `/teknologi/*` yang
  > tercapai dengan yang **dikenal API**. Ia juga menjaga `id="q"` tunggal di `/cari`.
  >
  > Dijalankan terhadap build produksi hari ini: **22 halaman, 20 dari 20 yang dikenal
  > API, teks terlihat 0 kena** — angka yang sama dengan hitungan tangan 17 September.
  > Ketiga aturannya dibuktikan merah: teks *"Tahap: Bulan 4"* yang ditanam terbaca
  > berikut kalimat sekitarnya; `<Link>` topik yang dibuang memberi **14 dari 20**
  > dengan keenam topiknya disebut satu per satu; `id="q"` ganda memberi dua temuan.
  > Kendalinya juga diukur: sesudah ketiganya dipulihkan, hijau lagi.
  >
  > 🐞 **Versi pertamanya hijau karena tidak melihat.** Ia hanya mengikuti `<a href>`,
  > berhenti di 21 halaman, dan `/cari` tidak pernah dikunjungi — satu-satunya jalan ke
  > sana `<form action="/cari" method="get">` di tata letak. Jadi pemeriksaan `id="q"`
  > tidak pernah berjalan sekali pun. Sekarang formulir GET ikut ditelusuri.
  >
  > ⚠️ **Yang belum dijawab:** ia **tidak** ikut di `verify` maupun di CI, sebab ia
  > menuntut web yang sudah dibangun DAN API yang hidup — job `frontend` di `ci.yml`
  > tidak punya keduanya. Menyalakannya di sana berarti Postgres + API + `next start`
  > di satu job; itu langkah berikutnya yang tertulis, bukan yang sudah ada. Yang juga
  > belum: uji komponen untuk `TopikTerhubung` dan kawannya — harness JS penuh tetap
  > keputusan tersendiri.
  >
  > ✅ **Langkah itu diambil di hari yang sama — pemindainya kini gerbang CI.** Bukan
  > di job `frontend` melainkan di job **`citra`**, dan itu yang membuatnya murah: job
  > itu sudah membangun ketiga citranya beberapa langkah di atas, jadi
  > `docker compose -f docker-compose.prod.yml up -d --wait` di sana menabrak cache
  > build. Ketiga target compose sama persis dengan ketiga `docker build`-nya
  > (`final`, `migrator`, Dockerfile web). Volume-nya dibongkar `down -v` di langkah
  > ber-`always()`, sebab volume yang tertinggal membuat *"migrasi dari nol"* berhenti
  > terukur di jalan berikutnya.
  >
  > **Bentuk produksi yang dijaga, dan itu yang paling murah:** nol topik dan empat
  > belas bidang datang dari migrasi tanpa seed, jadi angkanya deterministik. Job-nya
  > **ditiru utuh di lokal** sebelum ditulis — ketiga citra, gerbang ADR-020,
  > `compose up --wait`, pemindai, `down -v` — dan hijau: 6 migrasi dari nol, 14
  > bidang · 0 topik · 0 sisi, **16 halaman, 14 dari 14, teks terlihat 0 kena**.
  >
  > 🐞 Satu angka di tiruan itu sempat tidak bisa dijelaskan: *"migrasi diterapkan:
  > 12"* padahal berkas migrasinya enam. Sebabnya diukur, bukan ditebak — `efbundle`
  > mencetak `Applying migration '…'` **dan** logger EF Core mencetak barisnya sendiri,
  > jadi setiap migrasi muncul dua kali. Enam nama berbeda; angka 12 dibuang dari
  > laporan, bukan dipakai.
  >
  > **Ditambahkan juga:** blok *"Topik terhubung"* yang tampil kosong kini ditolak
  > (aturan ADR-023, sampai hari ini hanya dijaga sabotase tangan). Kedua bentuknya
  > dibuktikan merah: `return null` awal dibuang → **empat** topik tanpa relasi
  > melaporkan blok ber-NOL butir; penjaga per sub-daftar dibuang → `Dibutuhkan oleh`
  > kosong di `model-context-protocol` dan `Pelajari lebih dulu` kosong di `tool-use`,
  > tepat dua halaman yang memang timpang. Invariannya berlaku di bentuk apa pun, dan
  > di produksi ia vakum.
  >
  > Yang **tetap** belum: uji komponen JS. Harness penuh masih keputusan tersendiri.
  >
  > 🐞 **2026-09-28 — ukuran "N dari N" itu punya batas yang tidak tertulis: 20.**
  > Pembandingnya membaca `GET /api/v1/technologies` sekali, dengan `pageSize` bawaan
  > server, jadi topik ke-21 dan seterusnya tidak pernah masuk hitungan *"dikenal
  > API"*. Di skala hari ini (6 topik di pengembangan, 0 di produksi) angkanya
  > kebetulan benar; target Bulan 4 saja 22. Diukur dengan 55 topik sementara di satu
  > bidang (61 seluruhnya): halaman bidang memotong di 50 dan beranda di 24, jadi lima
  > topik memang tidak punya satu tautan pun — dan versi lama **hijau, exit 0**, sambil
  > mencetak *"70 dari 34"*. Sekarang ia membaca halaman demi halaman sampai
  > `hasNextPage` salah: *"70 dari 75"*, kelima topiknya disebut, exit 1. Topik
  > sementaranya dihapus lagi.
  >
  > Di hari yang sama ia mendapat satu sonde lagi: **ringkasan alat yang dikirim API
  > wajib terlihat di halaman topik.** Sampai hari itu halaman hanya mencetak nama
  > dan catatan alat, padahal `Tool` jadi agregat sendiri justru supaya halaman-halaman
  > tidak berbeda pendapat soal *apa* alat itu. Pola yang sama dengan kata kunci
  > `/cari` ([#67](https://github.com/xtheoputra/techverse-x/pull/67)): kontraknya menyatakan gunanya, kliennya melewatinya. Merah dulu di
  > build lama (`model-context-protocol`, satu temuan), hijau sesudah halaman
  > diperbaiki. Di produksi, yang nol topik, sonde ini vakum dan laporannya menyebut
  > angkanya.
  >
  > 🐞 **Dan tiruan lokal 24 September tidak seutuh yang ditulis di atas.** Langkah
  > compose-nya berjalan tanpa `--build`, jadi compose diam-diam memakai citra
  > `techversex-prod-*` yang sudah ada di mesin — bertanggal **17 September**, dan
  > masih bertanggal itu pada 28 September sebelum dibangun ulang. Jadi *"16 halaman,
  > 14 dari 14"* di atas diukur terhadap citra seminggu lebih tua daripada kode yang
  > di-commit. CI sendiri tidak terkena — runner lahir tanpa citra itu, jadi compose
  > terpaksa membangun — tapi orang yang menyalin barisnya ke mesin sendiri terkena;
  > `ci.yml` kini menulis `--build`. Tiruan 28 September memakai citra yang baru
  > dibangun (tanggalnya diperiksa, bukan dianggap): 6 migrasi dari nol, 14 bidang ·
  > 0 topik · 0 sisi, **16 halaman, 14 dari 14, ringkasan alat 0 diperiksa, teks
  > terlihat 0 kena**.
- **Cara membalikkan:** tambah rute dan butir menunya. Tidak ada yang perlu
  dimigrasikan; migrasi ringkasan bidang berdiri sendiri dan tidak perlu dibalik.
