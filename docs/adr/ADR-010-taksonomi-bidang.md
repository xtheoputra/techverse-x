# ADR-010 — Taksonomi: empat belas bidang, dan taksonomi 41 submenu disusun ulang

**Status:** Diterima. Menutup Issue [#3](https://github.com/xtheoputra/techverse-x/issues/3), [#4](https://github.com/xtheoputra/techverse-x/issues/4), [#5](https://github.com/xtheoputra/techverse-x/issues/5), [#6](https://github.com/xtheoputra/techverse-x/issues/6), [#7](https://github.com/xtheoputra/techverse-x/issues/7), [#8](https://github.com/xtheoputra/techverse-x/issues/8). **Diperbarui 2026-10-07: tujuh belas bidang** — lihat Pembaruan di bawah.
**Tanggal:** 2026-09-04

## Konteks

`KERANGKA.md` 2.3 memerinci 41 submenu untuk delapan bidang. `KERANGKA.md` 3.2
menyebut dua belas bidang tanpa submenu sama sekali. Keduanya tidak nyambung:
empat bidang di daftar dua belas tidak pernah punya submenu, dan delapan bidang
di daftar 41 memakai nama yang sebagian sudah diganti.

## Keputusan

**Taksonomi disusun ulang** (opsi B di Issue #3), bukan ditambal. Angka "41" dan
"±287 blok" dari 2.3 **tidak berlaku lagi** — keduanya artefak dari daftar
delapan bidang yang sudah tidak ada.

### Empat belas bidang

| # | Bidang | Topik | Perubahan | Prioritas |
|---|---|---|---|---|
| 1 | AI & Machine Learning | 7 | dipersempit jadi fondasi | **1** |
| 2 | AI Agents | 7 | naik dari submenu jadi bidang | **1** |
| 3 | Cybersecurity | 6 | tetap | **1** |
| 4 | Cloud & Infrastructure | 7 | **nama lebar dipertahankan** | **1** |
| 5 | Data Engineering | 7 | **bidang baru** | **1** |
| 6 | IoT | 8 | cakupan dari audit | **1** |
| 7 | Edge AI | 6 | **bidang baru** | 2 |
| 8 | Robotics | 5 | menerima "Robotics AI" | 2 |
| 9 | Quantum Computing | 4 | tetap | 2 |
| 10 | Biotechnology | 4 | tetap | 2 |
| 11 | Blockchain | 5 | tetap | 2 |
| 12 | Renewable Energy | 8 | **diisi ulang** | 2 |
| 13 | Space Technology | 8 | cakupan dari audit | **3** |
| 14 | XR (AR/VR/MR) | — | **diganti nama, tidak diinvestasikan** | **3** |

Prioritas **1** ditulis manusia sampai tuntas. **2** lahir sebagai draf. **3**
lahir sebagai kurasi tautan dan berhenti di situ sampai ada bukti permintaan.
Tingkat kematangan ini didefinisikan di [ADR-012](ADR-012-template-halaman.md).

### Batas AI & Machine Learning vs AI Agents (Issue #4)

Usul audit diterima apa adanya:

- **AI & Machine Learning — fondasi.** ML klasik · Deep learning · LLM ·
  Computer Vision · NLP · Reinforcement Learning · Multimodal AI.
- **AI Agents — orkestrasi.** Tool use · MCP · **A2A** · Memori agen ·
  Evals & observability · Keamanan agen · Human-in-the-loop.

**A2A wajib ada.** Audit menemukan protokol Agent2Agent tidak disebut sama
sekali di peta, padahal itu pasangan MCP dan persis kemampuan "berkolaborasi
dengan AI lain" yang peta itu klaim sendiri.

Dua pemindahan yang mengikuti batas ini: **"Robotics AI" pindah ke Robotics**
(ia aplikasi, bukan fondasi), dan **"AI Engineer" dicabut** — peran ditangani
Career Mode, lihat [ADR-009](ADR-009-tulang-punggung-navigasi.md).

### Edge AI dan Data Engineering jadi bidang sendiri (Issue #5)

Keduanya jadi bidang ke-13 dan ke-14 dalam urutan pembuatan, bukan dicoret.

- **Edge AI** — dugaan "taruh di bawah IoT" ditolak audit dengan tiga alasan:
  beban belajarnya ML bukan jaringan; permukaan terbesarnya NPU ponsel/AI PC/
  kamera/kendaraan, bukan perangkat IoT; dan tinyML Foundation sudah berganti
  nama jadi **Edge AI Foundation** — bidangnya memisahkan diri. Yang tinggal di
  IoT hanya **satu halaman jembatan TinyML**.
  Cakupan: Kuantisasi & pruning · Distilasi · NPU & akselerator · Runtime
  on-device (LiteRT, ExecuTorch, ONNX Runtime) · Model kecil di perangkat ·
  Pengukuran latensi & daya.
- **Data Engineering** — 4 bintang di tabel prioritas pemilik sendiri, dan **dua
  lensa audit berbeda sampai kesimpulan sama secara mandiri**. Cakupan usulan
  (ini usulan ADR ini, bukan audit): Ingestion & ELT · Orkestrasi · Warehouse &
  lakehouse · Format tabel terbuka · Streaming · Kualitas & kontrak data ·
  Pemodelan.

Cakupan Data Engineering dan Edge AI **belum diverifikasi audit** — ditandai
begitu supaya tidak dikutip sebagai temuan.

### Renewable Energy diisi ulang (Issue #6)

Dua dari tiga isi lama tidak sah: **Fusion** (EIA menaruh nuklir di
*nonrenewable*) dan **Solid-State Battery** (itu penyimpanan, bukan
pembangkitan). Akarnya ada di penggantian nama itu sendiri — "Energi Masa Depan"
adalah label horizon waktu, "Renewable Energy" adalah kategori statistik ketat
IEA/EIA/IRENA.

Cakupan audit diterima: Solar PV · Angin · Panas bumi · Hidro · Bioenergi & SAF ·
Hidrogen hijau · **Integrasi jaringan & penyimpanan (VRE)** · Ekonomi & kebijakan.

Dua penempatan yang menyertainya:

- **Solid-State Battery tinggal**, tapi di bawah "Integrasi jaringan &
  penyimpanan", dengan label tegas bahwa ia penyimpanan — bukan sumber. Membuang
  penyimpanan dari cerita energi terbarukan justru menghilangkan bagian yang
  paling menentukan.
- **Fusion pindah ke `/future`.** Di sanalah tempat yang jujur untuk teknologi
  yang belum bisa diajarkan sebagai keterampilan.

**Sudut Indonesia dipakai sebagai pembeda bidang ini:** panas bumi 2.744 MW
(2025), peringkat ke-2 dunia setelah AS; Cirata 145 MW terbesar di Asia
Tenggara; potensi EBT nasional 417,8 GW dengan pemanfaatan baru sekitar 2,5%.

### Cloud & Infrastructure — nama lebar dipertahankan (Issue #8)

Penyempitan jadi "Cloud" di 3.2 dinilai **penyingkatan penulisan, bukan
keputusan**. Kalau dipersempit sungguhan, Docker, DevOps, dan Platform
Engineering kehilangan rumah — sementara audit justru menyebut infrastruktur
awan hyperscale sebagai salah satu lapisan terbesar yang **hilang** dari peta,
"kanal komersial tempat hampir semua teknologi di daftar itu benar-benar dijual
dan ditagih". Menyempitkan bidang ini berlawanan arah dengan temuan itu.

### XR (AR/VR/MR) — diganti nama, lalu tidak diinvestasikan (Issue #7)

Nama jadi **XR (AR/VR/MR)**, dengan "AR/VR" dipertahankan sebagai alias
pencarian. *Spatial computing* tidak dipakai — itu istilah pemasaran Apple dan
Samsung.

Bidangnya **tidak dihapus, tapi tidak diinvestasikan** (prioritas 3). Vonis
relevansi audit adalah yang paling keras dari empat bidang yang ditinjau: Reality
Labs rugi USD 19,1 miliar pada 2025, memangkas 10% pegawai Januari 2026, menutup
tiga studio; harga perangkat di luar jangkauan pasar Indonesia; dan platform
kacamata pintar tertutup — tidak ada SDK pihak ketiga untuk Ray-Ban Meta.

Dihapus akan menghilangkan pintu pencarian yang sah; ditulis dalam akan membuang
minggu untuk bidang yang uangnya sedang keluar. Kurasi tautan adalah jawaban
yang jujur untuk keduanya.

Dua salah-kategori yang wajib dijaga di halamannya nanti: **kacamata pintar AI
sebagian besar BUKAN AR** (Ray-Ban Meta generasi awal tidak punya heads-up
display), dan **world model bukan sub-topik bidang ini** — Genie 3, Cosmos 3,
World Labs rumahnya di bidang AI.

### Space Technology — cakupan jelas, prioritas rendah

Cakupan audit diterima (8 topik, dari konektivitas LEO sampai keselamatan orbit
dan GNSS/PNT), tapi prioritas 3. Alasan audit: perangkat lunak antariksa hidup di
C/C++, Python, dan Rust, dan Indonesia tidak punya industri peluncuran. Irisan
yang tersisa — **3GPP NTN dan jaringan di atas LEO** — ditulis lebih dulu kalau
bidang ini nanti dinaikkan, karena itu irisannya dengan kekuatan pemilik.

## Konsekuensi

- Angka penggantinya, dihitung dari kolom "Topik" di tabel atas:

  | Prioritas | Bidang | Topik | Diperlakukan |
  |---|---|---|---|
  | 1 | 6 | **42** | ditulis manusia sampai tuntas |
  | 2 | 6 | **32** | lahir sebagai draf |
  | 3 | 2 | **8** + XR belum dirinci | kurasi tautan saja |
  | | **14** | **82** + XR | |

  Yang **dijadwalkan ditulis manusia dalam enam bulan pertama cuma 42 topik**,
  dan bahkan itu pun tidak sekaligus — lihat [RENCANA-V1](../RENCANA-V1.md).
  Jadi meski jumlah bidang naik dari 12 ke 14, beban penulisan yang dijanjikan
  justru **turun** dari 41 halaman penuh menjadi 42 topik bertingkat yang
  sebagian besarnya boleh berhenti di kurasi.
- Empat nama basi yang wajib dihindari di seluruh konten: *TensorFlow Lite* →
  **LiteRT**; *tinyML Foundation* → **Edge AI Foundation**; *Project Kuiper* →
  **Amazon Leo**; *Lumen Orbit* → **Starcloud**.
- Prioritas 1 sengaja memuat **IoT**, yang menurut audit justru bertemu dengan
  kekuatan jaringan pemilik di "bagian tersulit dan paling langka" — konektivitas,
  gateway, NAT/VPN, anggaran radio, manajemen armada. Itu pembeda yang tidak bisa
  ditiru penulis lain dengan cepat.

## Pembaruan 2026-10-07 — tiga bidang Deep Tech ditambahkan (tujuh belas bidang)

Pemilik menambahkan tiga bidang ke menu utama, di bawah XR, dari draf *"hub Deep Tech"* yang ia tempelkan (pilihan: bidang baru di menu utama, bukan sub-topik di bidang yang ada). Judul dan tabel di atas **dibiarkan sebagai rekaman keputusan 2026-09-04**; yang berlaku sekarang adalah **tujuh belas bidang**.

### Tiga bidang baru

| # | Bidang | Slug | Prioritas | Target | Cakupan awal (dari pemilik) |
|---|---|---|---|---|---|
| 15 | **Advanced Computing & Hardware** | `advanced-computing-hardware` | **2** | draf | desain semikonduktor · RISC-V · neuromorphic computing · fotonika · DNA data storage |
| 16 | **Neurotechnology & BCI** | `neurotechnology-bci` | 3 | kurasi tautan | BCI invasif dan non-invasif · neuroprostetika · EEG signal processing · implan saraf |
| 17 | **Advanced Materials & Nanotech** | `advanced-materials-nanotech` | 3 | kurasi tautan | grafena · metamaterial · superkonduktor · rekayasa material skala nano untuk baterai dan antariksa |

Kolom "Topik" tabel di atas **tidak diisi** untuk ketiganya: pemilik menyebut cakupan, bukan jumlah topik, dan angka yang dikarang akan masuk hitungan beban penulisan tanpa dasar. Cakupan itu juga **belum diverifikasi audit** — ditandai begitu, seperti cakupan Edge AI dan Data Engineering di atas, supaya tak dikutip sebagai temuan.

**Dampak pada angka resmi: tidak ada.** Hanya bidang berprioritas 1 yang memuat topik yang dijanjikan ditulis manusia, dan ketiganya bukan. Enam bidang dan 42 topik prioritas 1 serta target `tinjau` di [RENCANA-V1](../RENCANA-V1.md) tidak berubah. Hitungan sesudahnya: prioritas 1 — **6** bidang; prioritas 2 — **7**; prioritas 3 — **4**.

### Tiga batas dengan bidang yang sudah ada (usulan ADR ini, belum diverifikasi audit)

Ketiganya bersinggungan dengan bidang lain, dan tanpa batas tertulis topik yang sama akan punya dua rumah:

- **Advanced Computing & Hardware ↔ Edge AI.** NPU dan akselerator **sebagai target inferensi** tetap di Edge AI; di sini arsitektur mikro, semikonduktor, dan teknologi komputasi alternatifnya. **Quantum Computing tetap bidangnya sendiri** — tidak diduplikasi di sini.
- **Advanced Materials & Nanotech ↔ Renewable Energy.** **Baterai sebagai penyimpanan energi** tetap di "Integrasi jaringan & penyimpanan" milik Renewable Energy (ADR ini, bagian *Renewable Energy diisi ulang*); di sini materialnya (kimia dan fisika bahan), bukan sistem penyimpanannya.
- **Neurotechnology & BCI ↔ Biotechnology.** Antarmuka saraf, sinyal, dan implan di sini; rekayasa genetika, protein, dan biologi sintetis tetap di Biotechnology.

### Satu koreksi atas teks pemilik: "superkonduktor suhu kamar"

Draf pemilik menulis *"superkonduktor suhu kamar"* sebagai salah satu landasan fisik. Ringkasan bidang tercetak di kartu **publik**, dan frasa itu terbaca sebagai bahan yang sudah ada — padahal **belum ada superkonduktor suhu kamar bertekanan atmosfer yang terbukti**. Klaim paling ramai, LK-99 (2023), gugur oleh replikasi independen: anomalinya berasal dari pengotor Cu₂S, bukan superkonduktivitas ([The Conversation](https://theconversation.com/hopes-fade-for-room-temperature-superconductor-lk-99-but-quantum-zero-resistance-research-continues-211733); [replikasi dan studi anomali, *Superconductor Science and Technology*](https://iopscience.iop.org/article/10.1088/1361-6668/ad2b78/ampdf)). Gambaran 2026: sistem hidrida mencatat suhu kritis tertinggi hanya pada tekanan di atas 100 GPa, dan kandidat bertekanan atmosfer **belum tervalidasi** ([tinjauan lanskap 2026](https://www.patsnap.com/resources/blog/articles/room-temperature-superconductor-research-2026-landscape/) — sumber sekunder; yang menentukan adalah konsensus replikasinya).

Maka ringkasannya ditulis **"superkonduktor (suhu kamar belum terbukti)"**, dan catatannya sengaja di **bagian awal** kalimat: kartu halaman muka memotong ringkasan di tiga baris, dan versi pertama yang meletakkannya di tengah ("…termasuk pencarian bahan suhu kamar, yang belum terbukti…") terpotong persis di *"yang belum…"* — ketahuan saat kartunya **dilihat** di browser, bukan dari membaca kode. Itu satu-satunya perubahan pada teks pemilik selain penulisan ulang "&" dan "/" menjadi "dan" di dua tempat. Sebuah uji (`Katalog_TidakMengklaimSuperkonduktorSuhuKamarSudahAda`) menjaganya supaya kata *"belum terbukti"* tak bisa hilang diam-diam. Kalau pemilik ingin kata-kata aslinya, itu perubahan satu baris plus satu migrasi — tapi klaimnya tetap harus punya sumber primer sebelum tampil sebagai fakta.

### Penempatan, dan satu akibatnya

Sesuai permintaan, ketiganya di **bawah XR** (urutan tampil 15–17). Akibatnya **urutan tampil tak lagi sepenuhnya menurut prioritas**: Advanced Computing & Hardware (prioritas 2) kini tampil sesudah XR (prioritas 3). Komentar `Field.DisplayOrder` yang menulis *"prioritas dulu"* berlaku untuk empat belas bidang awal. Memindahkan Advanced Computing ke posisi 13 (dan menggeser Space dan XR) adalah migrasi data kecil; tak diambil di sini karena penempatannya diminta eksplisit. Uji `Katalog_MemuatKetigaBidangDeepTechDiBawahXR_DenganNamaDariPemilik` mengunci penempatan sekarang, supaya pergeserannya disengaja.

### Dibangun

`FieldCatalog` (17 entri, `ExpectedCount = 17`), migrasi **`TigaBidangDeepTech`** (hanya `InsertData`; `Down` menghapus tiga baris itu), tiga ikon, dan tes katalog. **Migrasi ini data saja, tanpa perubahan skema**, dan API membaca bidang dari basis data saat berjalan (bukan dari `FieldCatalog`, yang hanya dipakai saat membangun model dan di tes) — jadi API baru yang tayang **sebelum** migrasi dijalankan tetap sehat; ketiga kartu baru muncul begitu *Migrasi produksi* dijalankan. Tak ada jendela `migrate → api` yang rusak, tidak seperti yang akan dihadapi Tahap 3b ([ADR-028](ADR-028-antarmuka-futuristik-dan-isi-kaya.md)).
