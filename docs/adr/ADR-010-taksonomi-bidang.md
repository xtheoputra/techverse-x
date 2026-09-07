# ADR-010 — Taksonomi: empat belas bidang, dan taksonomi 41 submenu disusun ulang

**Status:** Diterima. Menutup Issue [#3](../../../../issues/3), [#4](../../../../issues/4), [#5](../../../../issues/5), [#6](../../../../issues/6), [#7](../../../../issues/7), [#8](../../../../issues/8).
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
