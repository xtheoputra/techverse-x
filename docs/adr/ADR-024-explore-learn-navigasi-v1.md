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
| Kalimat nol-hasil `/cari` | *"Isi TechVerse X masih dibangun bidang per bidang…"* — mengundang pembaca percaya isi halaman ikut tercari, yang justru [#54](../../../../issues/54) larang dijanjikan |
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
[#40](../../../../issues/40) dalam perubahan yang sama.

Alasannya: ADR-009 mendefinisikan `/explore` sebagai *"peta 14 bidang, filter per
bidang"* — dan `/` beserta halaman bidang kanonik sudah memberi persis itu di
produksi. ADR-009 hanya menjanjikan `/teknologi/<slug>` stabil, jadi menunda
`/explore` tidak memutus URL siapa pun. `/explore` duplikat berarti dua halaman
identik yang harus dijaga sama; memindahkan peta sekarang berarti menulis ulang
runbook #40 yang sebentar lagi diikuti pemilik, tanpa kemampuan baru.

### 2. Learn ditunda, dengan pemicu dan penagih

Tidak ada `/learn`. **Learn V1 = bagian Learning Roadmap di tiap halaman topik,
plus tautan *"Pelajari lebih dulu"*.**

**Pemicu `/learn`:** [#42](../../../../issues/42) tutup — tujuh topik AI Agents
berstatus `tinjau`, yang sekaligus mengunci bentuk roadmap (ADR-012 bagian 4) —
**DAN** produksi menampilkan setidaknya satu roadmap yang terisi. Pada titik itu
jalur belajar boleh mengurutkan topik lewat sisi `Requires`, dan **pemeriksaan
siklus transitif ADR-023 wajib mendarat lebih dulu**.

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
- **Cara membalikkan:** tambah rute dan butir menunya. Tidak ada yang perlu
  dimigrasikan; migrasi ringkasan bidang berdiri sendiri dan tidak perlu dibalik.
