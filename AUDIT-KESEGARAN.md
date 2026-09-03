# Audit Kesegaran — Peta Teknologi

**Tanggal audit:** 2026-09-02
**Yang diaudit:** `KERANGKA.md` Bagian 1 (dua belas teknologi) dan Bagian 3.2 (empat bidang baru/berubah).

Berkas ini memuat **dua audit** yang dijalankan pada hari yang sama. Nol agen gagal pada keduanya.

| Bagian | Sasaran | Agen | Keyakinan |
|---|---|---|---|
| [A](#bagian-a--peta-dua-belas-teknologi) | `KERANGKA.md` Bagian 1 — dua belas teknologi | 15 | campur, dinyatakan per temuan |
| [B](#bagian-b--empat-bidang-baruberubah) | `KERANGKA.md` Bagian 3.2 — Renewable Energy, IoT, Space Technology, AR/VR | 4 | sedang |

> ⚠️ **Baca [Peringatan Sebelum Memakai Berkas Ini](#peringatan-sebelum-memakai-berkas-ini) di bagian paling bawah lebih dulu.** Sebagian temuan bersandar pada liputan sekunder, dan ada beberapa klaim luar biasa yang wajib Anda konfirmasi sendiri.

---

## Bagian A — Peta Dua Belas Teknologi

> **Sasaran:** `KERANGKA.md` Bagian 1.
> **Cara:** 15 agen paralel dengan pencarian web langsung — 12 memverifikasi tiap entri, 3 mencari yang absen dari peta lewat lensa berbeda.

### Vonis Ringkas

**Kedua belas entri berstatus `sebagian-usang`.** Tidak ada satu pun yang lolos bersih, dan tidak ada satu pun yang salah total. Polanya seragam dan konsisten:

- **Arah besarnya benar** — dua belas bidang itu memang bidang yang tepat untuk 2026.
- **Nama produk yang jadi buktinya banyak yang sudah mati atau ketinggalan versi.**
- **Klaim superlatif ("paling panas", "nomor satu") tidak punya sumber**, dan beberapa justru dibantah data.
- **Daftar pemainnya menyebut merek paling terkenal, bukan pemain terkuat 2026.**

Untuk dokumen berjudul *"tercanggih saat ini (2026)"*, ini bukan cacat kecil. Kalau 41 halaman kurikulum dibangun dari peta ini apa adanya, seluruhnya lahir dalam keadaan usang.

---

### Enam Kesalahan Paling Berbahaya

Diurutkan berdasarkan seberapa besar ia menyesatkan orang yang belajar.

#### 1. AutoGen — mengajari orang belajar barang mati
Entri 1 menyuruh belajar AutoGen. Microsoft memindahkannya ke *maintenance mode* Oktober 2025; README-nya sendiri kini mengarahkan pendatang baru ke tempat lain.
**Ganti dengan:** Microsoft Agent Framework (1.0 GA 3 April 2026, .NET + Python) — penerus resmi peleburan AutoGen + Semantic Kernel. Jalur komunitas yang masih hidup: fork AG2.
**Catatan penting untuk Anda:** framework ini punya dukungan **.NET kelas satu**. Ini langsung menyentuh pilihan arsitektur di Bagian 2.6.

#### 2. Sora — produknya sudah tutup
Entri 2 memakai OpenAI Sora sebagai contoh unggulan video AI. Aplikasi dan web-nya ditutup 26 April 2026; APInya dijadwalkan tutup 24 September 2026 — tiga minggu dari sekarang.
**Ganti dengan:** Kling (Kuaishou), Google Veo 3.1, ByteDance Seedance 2.0.

#### 3. GPT-5 — sudah dipensiunkan
Entri 2 menyebut GPT-5 sebagai model terkini. GPT-5 dipensiunkan dari ChatGPT 13 Februari 2026; GPT-5.1 menyusul 11 Maret 2026.
**Ganti dengan:** keluarga GPT-5.6 (Sol / Terra / Luna, rilis publik 9 Juli 2026).

#### 4. "Robot humanoid sudah dipakai di rumah" — tidak ada buktinya
Entri 3 mencantumkan "rumah" sebagai tempat pemakaian nyata. Per pertengahan Juli 2026 belum ada satu pun pengiriman ke rumah konsumen yang terverifikasi independen. Yang ada baru pra-pesan (1X NEO, $20.000, masih dibantu teleoperasi manusia).

Data pengiriman H1 2026 (Counterpoint, 19 Agustus 2026) membongkar seluruh pembingkaian entri ini: dari >22.000 unit, **lebih dari 60% untuk hiburan dan riset**, manufaktur hanya ~13%, gudang/logistik hanya ~5%.

#### 5. Tesla Optimus dipakai sebagai bukti, padahal contoh terlemah
Optimus Gen 3 belum diluncurkan dan produksi Fremont belum dimulai per Agustus 2026. Sementara pemimpin volume sesungguhnya — AgiBot (~43% pengiriman H1 2026) dan UBTech — tidak disebut sama sekali. Yang paling terbukti bekerja secara komersial adalah **Agility Digit** (>65.000 jam operasi berbayar), juga tidak disebut.

#### 6. Windsurf dan Gemini CLI — jebakan kalau daftar diperluas
Keduanya tidak ada di tempelan Anda, tapi lazim muncul di daftar sejenis. **Windsurf sudah pensiun sebagai merek** (jadi Devin Desktop, 2 Juni 2026) dan **Gemini CLI digantikan Antigravity CLI**. Jangan sampai masuk saat daftar dilengkapi nanti.

---

### Koreksi Per Entri

#### 1. AI Agents
- **AutoGen → Microsoft Agent Framework** (lihat di atas).
- **"Teknologi nomor satu" tidak berdasar.** Gartner menempatkan agentic AI di *Peak of Inflated Expectations* menuju *Trough of Disillusionment* (Hype Cycle perdana, 2 April 2026). Baru 17% organisasi benar-benar men-deploy agent; Gartner memperkirakan ≥40% proyek agentic dibatalkan sebelum 2027.
- **"AI Engineer tumbuh tercepat secara global" terlalu digeneralisasi.** Peringkat #1 itu data LinkedIn untuk **Amerika Serikat** (+143% YoY). Yang global: WEF mencatat AI menambah 1,3 juta peran baru.
- **RAG sudah bukan bingkai 2026.** Payungnya sekarang *context engineering* — mencakup RAG, agent memory, dan manajemen context window.
- **Lubang terbesar: protokol A2A (Agent2Agent) tidak disebut sama sekali** — padahal itu pasangan MCP untuk komunikasi antar-agen, persis kemampuan "berkolaborasi dengan AI lain" yang entri ini klaim sendiri. v1.0.1 Mei 2026, 150+ organisasi, kini di bawah Agentic AI Foundation (Linux Foundation).
- **Perlu ditambah:** SDK agen vendor, evals & observability (LangSmith, Langfuse), keamanan agen (prompt injection, izin tool), kontrol biaya token.
- **Otonomi penuh belum siap produksi.** Yang berhasil adalah agen ber-scope sempit dengan pengawasan manusia.

#### 2. Generative AI Multimodal
- Sora mati, GPT-5 pensiun (lihat di atas).
- **"Claude Opus" polos** → tulis versinya. Opus 5 rilis 24 Juli 2026, tapi Opus bukan lagi puncak lini Anthropic.
- **Gemini disebut tanpa versi** — merek masih hidup, tapi lini ini berputar cepat sepanjang 2026, jadi penyebutan tanpa angka bikin dokumen sulit dinilai kesegarannya.
- **"Ke depan aplikasi akan memakai AI sebagai fitur bawaan" — bingkai waktunya salah.** Itu sudah keadaan sekarang, bukan ramalan. Menaruhnya di kolom masa depan membuat dokumen tampak tertinggal satu-dua tahun.
- **Poin paling tajam yang hilang:** multimodal sudah berhenti jadi kategori khusus. Visi dan penggunaan alat kini bagian dari model umum terbaik. Justru itu yang membuat entri ini terasa seperti berita 2024.

#### 3. Humanoid Robot
- Klaim "rumah" tidak berbukti; Optimus belum produksi; Unitree G1 sudah disusul R1 dan H2, dan ia platform riset/edukasi, bukan robot kerja (lihat di atas).
- **Figure AI itu nama perusahaan, bukan produk.** Produknya Figure 03, ditempatkan di BMW Spartanburg 25 Juni 2026, dikendalikan model VLA bernama Helix.
- **Pemain yang hilang:** AgiBot, UBTech, Agility Robotics, Apptronik, Boston Dynamics, 1X, XPeng. Produsen Tiongkok menguasai >97% pengiriman global.
- **Sisi teknisnya tertinggal satu generasi.** Pusat gravitasi 2026 adalah **model VLA (Vision-Language-Action)** — Helix (Figure), GR00T N1 (NVIDIA) — yang menyatukan persepsi, bahasa, dan kendali motorik dalam satu jaringan. Computer Vision dan RL tetap masuk, tapi sebagai komponen, bukan sebagai judul.

#### 4. Quantum Computing
- **"Masih tahap awal" lag sekitar dua tahun.** 2025–2026 adalah era **qubit logis** dan keunggulan kuantum terverifikasi (Google Quantum Echoes/Willow; IBM + UChicago 70 qubit logis, 30 Juli 2026). Yang belum ada adalah keunggulan *komersial*.
- **Quantinuum hilang** — padahal ia pemimpin qubit logis saat ini (sistem Helios: 98 qubit fisik → 48 qubit logis) dan sudah IPO di Nasdaq Juni 2026.
- **Microsoft Quantum tidak setara tiga lainnya.** Klaim qubit topologisnya (Majorana 2, Juni 2026) masih disengketakan peer-review.
- **IonQ sudah berubah besar** — akuisisi Oxford Ionics (~$1,07 miliar) memindahkan pendekatannya ke kendali qubit elektronik.
- **Kriptografi disebut sebagai "potensi" — padahal justru satu-satunya area yang SUDAH mengubah praktik industri hari ini.** Bukan karena mesin kuantum bisa memecahkan RSA (belum, dan tidak dekat), melainkan karena **post-quantum cryptography sudah jadi kewajiban kepatuhan nyata** (FIPS 203/204/205, NIST IR 8547). Ini dampak paling konkret bidang ini bagi praktisi TI biasa.
- **Skill perlu diperluas:** Python, koreksi galat kuantum, plus satu kerangka kedua (PennyLane / Cirq / CUDA-Q / Braket). Q# dilaporkan menurun adopsinya di luar ekosistem Azure.

#### 5. AI Coding Assistant
- **Kategorinya sudah bergeser** dari "assistant" (autocomplete) ke **agentic coding**. Permukaan kerja 2026 adalah mengelola banyak sesi agen paralel, bukan menerima-menolak saran baris kode.
- **Keempat alat yang disebut masih hidup**, tapi daftarnya melewatkan Google Antigravity, Devin Desktop, Amazon Kiro, Cline, dan lainnya — dan keempatnya tidak berada di kategori yang sama (IDE / ekstensi / CLI / platform cloud).
- **OpenAI Codex berubah bentuk** — aplikasi desktop terpisah dihentikan Juli 2026, dilebur ke aplikasi desktop ChatGPT; yang mandiri tinggal CLI dan ekstensi IDE.
- **Klaim "programmer terbaik = yang mengarahkan AI" belum terbukti terkontrol.** Uji acak METR (Juli 2025) justru menemukan pengembang berpengalaman **19% lebih lambat** dengan alat AI, padahal mereka merasa lebih cepat. Turunkan jadi klaim arah industri, bukan fakta.
- **Adopsi tinggi ≠ kepercayaan tinggi.** Stack Overflow 2025: 84% memakai/berencana memakai alat AI, tapi hanya 3% sangat percaya keluarannya dan 46% aktif tidak percaya.

#### 6. Cybersecurity Berbasis AI
- **"AI mendeteksi malware" bukan teknologi baru** — itu kemampuan dasar yang sudah tertanam di hampir semua EDR/NGAV. Yang benar-benar baru 2026 adalah lapisan agentik di atasnya.
- **Ancaman otomatis sudah terbukti nyata, bukan teori.** Kampanye spionase GTG-1002 yang diungkap Anthropic (November 2025) memakai agen AI untuk pengintaian, penemuan kerentanan, eksploitasi, gerak lateral, panen kredensial, dan eksfiltrasi terhadap ~30 target global.
- **Sisi pertahanan juga sudah punya angka**: Project Glasswing (Anthropic, 7 April 2026) melaporkan >10.000 kerentanan high/critical ditemukan; dari 1.752 yang dinilai, 90,6% valid.
- **Manusia masih di dalam lingkaran.** Kurang dari 1 dari 4 organisasi sudah menaruh agen ke produksi.
- **Skill perlu dirombak:** SIEM/EDR → "platform SecOps terpadu (SIEM+XDR+SOAR)". Tambahkan pertahanan prompt injection, LLM & agentic red teaming, MITRE ATLAS, OWASP Top 10 for LLM Applications.
- **Lanskap vendor berubah besar:** Chronicle → Google SecOps; QRadar SaaS diserap Cortex XSIAM; Wiz diakuisisi Alphabet ($32 miliar, tutup Maret 2026); CyberArk diakuisisi Palo Alto (~$25 miliar).

#### 7. Digital Twin
- **Omniverse bukan "platform" lagi** — kini kumpulan pustaka/SDK dan microservices berbasis OpenUSD yang dipakai **di bawah** platform digital twin vendor lain.
- **Pemain sesungguhnya tidak disebut satu pun:** Siemens, Dassault Systemes, PTC, Bentley iTwin, Azure Digital Twins, Hexagon, Ansys.
- **OpenUSD tidak disebut** — padahal itu fondasi datanya, dan sejak 17 Desember 2025 sudah jadi standar terbuka resmi di bawah Linux Foundation.
- **Contoh penerapan perlu ditambah:** pusat data / pabrik AI, robot, kendaraan otonom.

#### 8. Edge AI
- **Nama hardware-nya semua ketinggalan versi:** "NVIDIA Jetson" → Jetson **Thor**; "Raspberry Pi AI" → **AI HAT+ 2 (Hailo-10H, 40 TOPS)**; "chip AI Qualcomm" → **Dragonwing** (IoT/robotika) dan **Snapdragon X2 Elite/Plus** (AI PC).
- **⚠️ Google Coral Edge TPU praktis sudah mati** — pustakanya berhenti diperbarui bertahun-tahun dan drivernya bermasalah di kernel Linux modern. Jangan direkomendasikan.
- **Wajah utama bidang ini sudah bergeser** ke robotika/physical AI, AI PC ber-NPU, dan agen lokal berbasis model bahasa kecil. CCTV/mobil/drone/IoT tetap valid tapi bukan lagi yang utama.

#### 9. Drone Swarm
- **"Ratusan drone bekerja bersama" mencampur dua hal yang sangat berbeda.** Swarm otonom sungguhan per 2026 masih **3–25 drone**. Angka puluhan ribu itu light show terkoreografi terpusat — tidak ada pengambilan keputusan mandiri, hanya waypoint.
- **Hambatan utamanya bukan hardware, melainkan perangkat lunak orkestrasi** — dibuktikan DIU membuka tantangan senilai hingga $100 juta khusus untuk itu (15 Januari 2026).
- **Sisi counter-swarm sama sekali tidak disebut.** Tanpa itu, peta memberi kesan swarm adalah keunggulan tanpa penawar.

#### 10. Brain-Computer Interface
- **Status "masih berkembang" perlu dinyatakan bertingkat**: di Tiongkok BCI invasif sudah berstatus produk medis komersial berizin sejak Maret 2026; di AS semuanya masih investigasional (belum ada PMA FDA).
- **Pemain jauh lebih banyak** dari dua yang disebut: Precision Neuroscience, Paradromics, Blackrock Neurotech, plus pemain Tiongkok (Neuracle, NeuroXess, StairMed).
- **⚠️ Jangan salah kategori:** Meta Neural Band membaca sinyal listrik **otot** pergelangan tangan (sEMG), bukan aktivitas otak. Itu antarmuka neural perifer, bukan BCI.

#### 11. Bioteknologi + AI
- **AlphaFold perlu versi:** AlphaFold **3** memprediksi **interaksi** biomolekul (protein-ligan, DNA/RNA, antibodi-antigen), bukan sekadar lipatan protein tunggal.
- **Jangan samakan AlphaFold dengan "menemukan obat".** AlphaFold = alat prediksi. Kandidat obat AI paling maju justru bukan dari AlphaFold, melainkan **rentosertib** (Insilico Medicine), yang mulai Fase III 7 Juli 2026.
- **"AI mempercepat penemuan obat" perlu dipertajam:** AI terbukti memangkas tahap **penemuan** dari tahunan jadi bulanan, tetapi **belum terbukti memperbaiki peluang lolos uji klinis**. Hambatan tetap di Fase II/III.
- **"Revolusi terbesar setelah internet" harus diturunkan jadi potensi.** Per 2026: nol persetujuan regulator, hanya ~1% pipeline klinis global.
- **⚠️ Catatan lisensi:** AlphaFold 3 non-komersial dan bobotnya atas permintaan; IsoDDE tertutup total. Yang benar-benar terbuka: Boltz-2 (MIT), Chai-1, Protenix, RFdiffusion3, ESM3.

#### 12. Energi Masa Depan
- **Tiga dari tiga kaitan yang diklaim salah sasaran.** Fusi tidak menyentuh EV (kaitannya ke data center, lewat PPA: Helion–Microsoft ~50 MW, Google–Commonwealth Fusion 200 MW). Solid-state adalah ranah EV, bukan data center. Perovskit tidak punya kaitan khusus dengan EV.
- **Yang benar-benar menopang data center AI:** LFP skala utilitas (Tesla Megapack 3), dan mulai 2026 natrium-ion.
- **Bingkai ulang seluruhnya sebagai pra-komersial** dengan horizon 2027–2030-an. Per 2026 kontribusi nyata ketiganya masih nol.
- **Horizon energi data center yang jujur:** 2026–2030 = gas + nuklir eksisting + surya/BESS LFP; setelah 2030 = SMR; fusi paling spekulatif.

---

### Yang Hilang dari Peta

Tiga agen mencari dari sudut berbeda. Hasilnya saling menguatkan pada satu titik: **peta ini penuh teknologi ujung, dan kosong di lapisan yang menopangnya.**

#### Lensa riset — 13 temuan
Paling menonjol: **world model / kecerdasan spasial** (berbeda dari multimodal — ini memelihara geometri 3D dan konsekuensi spasial, jadi tulang punggung pelatihan robot), **AI untuk sains / laboratorium mandiri** (AI yang menjalankan eksperimen, bukan sekadar membantu), **interpretabilitas mekanistik & ilmu keselamatan model** (dokumen punya banyak entri soal APA yang bisa dilakukan AI, nol entri soal memahami dan mengauditnya), **kriptografi pasca-kuantum**, dan **litografi High-NA EUV** (2026 adalah tahun ia menyeberang ke produksi volume).

Berikutnya: komputasi fotonik, jaringan kuantum, neuromorfik, komputasi termodinamika (p-bit), pusat data orbital, biokomputasi organoid, penyimpanan DNA, riset 6G.

#### Lensa industri — 14 temuan
Yang paling menohok: **semikonduktor AI (GPU/ASIC, memori HBM, advanced packaging) tidak muncul sama sekali**, padahal seluruh entri lain secara fisik berjalan di atasnya. Menyusul **infrastruktur awan hyperscale** (kanal komersial tempat semua ini benar-benar dijual), **tata kelola & kepatuhan AI** (EU AI Act, ISO/IEC 42001 — sudah jadi pasar jasa nyata), dan **platform data & lakehouse** (tanpa ini, agen AI tidak punya bahan bakar).

Dua kelalaian yang mencolok di bawah lensa komersial:
- **Robotaxi** — sudah menagih penumpang tiap hari di belasan kota, sementara peta justru memuat humanoid yang komersialisasinya masih awal.
- **Robotika gudang non-humanoid (AMR, sortir, picking)** — sudah terpasang jutaan unit dan terbukti balik modal. Menyebut humanoid tanpa menyebut ini memberi gambaran terbalik soal robotika mana yang nyata di 2026.

Sisanya: jaringan optik 800G/1,6T, daya & pendinginan cair pusat data, PQC, internet LEO, stablecoin/RWA, kacamata pintar AI, jaringan privat 5G.

#### Lensa relevansi pemilik — 10 temuan
Ini yang paling langsung menyentuh Anda. Vonisnya keras:

> Dua belas entri itu didominasi teknologi ujung yang menarik dibaca tapi **tidak bisa dimasuki tanpa laboratorium, pabrik, atau modal besar**. Lapisan yang justru menghasilkan uang untuk seorang engineer perangkat lunak di Indonesia — integrasi, kepatuhan, data, dan infrastruktur — **tidak muncul sama sekali**.

Yang absen dan justru paling cocok dengan .NET + networking + lokasi Indonesia:

| Jalur | Kenapa cocok |
|---|---|
| **Infrastruktur pembayaran & fintech Indonesia** (QRIS, BI-FAST, Open API SNAP) | Titik temu paling tepat antara .NET, networking, dan pasar lokal. Pasar meledak, integrator masih kurang. |
| **AI on-premise / inferensi privat & kedaulatan data** | Jembatan paling langsung dari networking ke AI, dan pembeda yang tidak bisa disalin penyedia cloud asing — banyak calon pelanggan besar dilarang mengirim data ke luar negeri. |
| **Rekayasa data & platform data** | Dua belas entri itu semuanya menganggap datanya sudah siap. Lapisan inilah yang menentukan proyek AI hidup atau mati. |
| **LLMOps** (evaluasi, observability, guardrail) | Kategori tempat engineer berpengalaman menang atas hobiis prompt. |
| **Modernisasi sistem lama & integrasi enterprise** | Paling membosankan, paling cocok dengan lima tahun .NET Anda, dan bisa jadi mesin uang yang mendanai eksperimen AI. |
| **RegTech & kepatuhan UU PDP** · **e-KYC** · **perangkat lunak pajak (Coretax, e-Faktur)** | Membosankan, wajib, berulang — ciri pendapatan stabil. |
| **Tata kelola AI Indonesia** (Perpres Peta Jalan & Etika AI) | Aturan yang belum turun adalah jendela terbaik membangun produk kepatuhan lebih dulu. |
| **Distribusi, penetapan harga, dan penjualan B2B** | Peta ini 100% teknologi dan 0% cara teknologi berubah jadi kontrak. |

**Yang dinilai salah tempat untuk profil Anda:** Quantum Computing (IBM sendiri baru menargetkan toleran-galat pada 2029), Humanoid Robot, Brain-Computer Interface, Bioteknologi + AI, Energi Masa Depan, dan Drone Swarm. Semuanya butuh lab, pabrik, izin, atau tender negara.

---

### Dua Hal yang Justru Membenarkan Naluri Anda di Bagian 2

1. **Menambahkan bidang "Cloud & Infrastructure"** — yang tampak muncul entah dari mana karena tidak ada di daftar 12 — ternyata **menambal persis lubang terbesar** yang ditemukan lensa industri.
2. **Menaruh Data Engineering di prioritas ⭐⭐⭐⭐** — dua lensa berbeda sampai pada kesimpulan yang sama secara mandiri. Sayangnya bidang ini justru **belum punya menu** di Bagian 2.3.

---

## Bagian B — Empat Bidang Baru/Berubah

> **Sasaran:** `KERANGKA.md` Bagian 3.2 — empat bidang yang baru atau berubah cakupan.
> **Cara:** 4 agen paralel. **Keyakinan keempatnya: sedang** — lihat butir 5 pada peringatan di bawah.

### Ringkasan Empat Vonis

| Bidang | Vonis | Relevansi untuk pemilik |
|---|---|---|
| **Renewable Energy** | ⚠️ **Dua dari tiga isi lamanya tidak sah** — harus diisi ulang | **Sedang**, lewat satu pintu |
| **IoT** | Sah, tapi **jangan taruh Edge AI di sini** | **Sedang** |
| **Space Technology** | Sah, cakupannya jelas | **Rendah** untuk dimasuki |
| **AR/VR** | ⚠️ **Namanya sudah ketinggalan** — industri memakai XR | **Rendah** |

### Renewable Energy — hanya satu dari tiga isi lama yang selamat

Ini temuan paling tegas dari keempatnya, dan keyakinan taksonominya **tinggi** karena bersandar pada definisi eksplisit EIA.

| Isi lama | Vonis |
|---|---|
| **Fusion Energy** | ❌ **Tidak sah.** EIA menempatkan nuklir di bawah *Nonrenewable sources*, terpisah dari lima sumber terbarukan (biomassa, hidro, panas bumi, angin, surya). Fusi memakai bahan bakar yang ditambang. |
| **Solid-State Battery** | ❌ **Tidak sah.** Baterai adalah **penyimpanan** energi, bukan pembangkitan — ia tidak menghasilkan energi, hanya memindahkannya dalam waktu. Belum komersial pula (Toyota mundur ke ≥2027). |
| **Perovskite Solar Cell** | ✅ **Sah dan tetap** — sub-topik yang jelas di bawah Solar PV. |

**Akar masalahnya ada di penggantian nama itu sendiri.** *"Energi Masa Depan"* adalah label **horizon waktu** — apa pun yang terdengar futuristik boleh masuk. *"Renewable Energy"* adalah **kategori statistik ketat** yang dipakai IEA/EIA/IRENA. Penggantian nama ini mempersempit cakupan secara diam-diam, bukan sekadar menerjemahkan.

**Cakupan yang disarankan:** Solar PV (TOPCon, HJT, tandem perovskite-silikon, atap, terapung) · Angin (darat, lepas pantai, terapung) · **Panas bumi** · Hidro · Bioenergi & SAF · Hidrogen hijau & Power-to-X · Integrasi jaringan untuk VRE · Ekonomi & kebijakan (LCOE, PPA, REC, RUPTL).

**Sudut Indonesia di bidang ini kuat dan jarang dipakai orang:** panas bumi terpasang **2.744 MW per 2025 — peringkat ke-2 dunia** setelah AS. PLTS Terapung Cirata 145 MW, terbesar di Asia Tenggara. Potensi EBT nasional 417,8 GW dengan pemanfaatan baru ~2,5%.

**Jebakan lain yang perlu dijaga:** *clean energy* ≠ *renewable* ≠ *low-carbon* (nuklir fisi dan CCS itu low-carbon tapi bukan terbarukan) · hidrogen adalah **pembawa** energi, bukan sumber · kapasitas (GW) rutin tertukar dengan energi yang dihasilkan (GWh).

**Pintu masuk yang terbuka untuk Anda:** lapisan perangkat lunak di atas aset fisik — platform monitoring & O&M aset EBT (inverter, data logger, SCADA) yang secara harfiah pekerjaan networking + backend + time-series; peramalan produksi surya/angin berbasis ML; dan **keamanan OT/ICS untuk aset EBT terdistribusi** (Modbus, IEC 61850, DNP3) — celah nyata yang sedikit dikuasai orang Indonesia.

### IoT — sah, tapi Edge AI jangan ditaruh di sini

**Ini membantah dugaan yang saya tulis sendiri di `KERANGKA.md` E11.** Saya menduga IoT adalah rumah yang masuk akal bagi Edge AI. Audit menolaknya, dengan alasan yang kuat:

- Beban belajar Edge AI adalah **ML** (kuantisasi, distilasi, NPU, runtime LiteRT/ExecuTorch/ONNX), bukan jaringan dan sensor.
- Permukaan terbesarnya justru **bukan** perangkat IoT: NPU ponsel, AI PC, kamera, kendaraan, robot.
- Bukti kelembagaan: **tinyML Foundation kini bernama Edge AI Foundation** — bidangnya memisahkan diri, bukan melebur.

Yang wajar tinggal di IoT hanyalah **satu halaman jembatan TinyML**; sisanya milik bidang Edge AI sendiri.

**Status istilahnya:** "IoT" masih hidup di 2026 tapi **turun pangkat** — dari bidang payung jadi lapisan infrastruktur. Jangan diganti namanya.

**Cakupan yang disarankan:** Protokol & konektivitas (MQTT 5, CoAP, LwM2M di atas BLE/Thread/LoRaWAN/NB-IoT/5G RedCap) · Firmware & RTOS (Zephyr, FreeRTOS, ESP-IDF) · Matter & Thread · Platform IoT & gateway tepi · Keamanan perangkat & identitas · IIoT & otomasi industri (OPC UA, Sparkplug B) · Telemetri deret waktu · TinyML (satu halaman jembatan).

**Jebakan taksonomi:** *embedded systems* bukan sinonim IoT · *smart home* hanya satu vertikal (IIoT jauh lebih besar nilainya) · digital twin lebih dekat ke simulasi/CAD daripada ke sensor · keamanan OT/ICS milik Cybersecurity, bukan IoT · **"IoT Platform" menutupi tiga hal berbeda** (manajemen konektivitas, device management, application enablement) — jangan satu halaman.

**Nama basi yang wajib dihindari:** *TensorFlow Lite* kini **LiteRT**; *tinyML Foundation* kini **Edge AI Foundation**.

**Relevansi:** networking Anda jatuh persis di bagian tersulit dan paling langka dari IoT — konektivitas, gateway, NAT/VPN, anggaran radio, manajemen armada. Itu keunggulan nyata. Tapi **.NET nyaris tidak punya tempat di sisi perangkat** (MCU = C/C++/Rust), dan **IoT adalah bisnis perangkat keras**: BOM, sertifikasi SDPPI/Postel, TKDN, rantai pasok, kunjungan lapangan, dan margin yang jauh lebih tipis daripada SaaS.

### Space Technology — cakupannya jelas, tapi pintunya sempit

**Cakupan yang disarankan:** Konektivitas satelit LEO (broadband, direct-to-cell/NTN 3GPP) · Pusat data orbital & AI di antariksa · Akses ke orbit · Rekayasa smallsat/CubeSat · **Segmen darat, operasi misi & keamanan siber antariksa** · Observasi Bumi berbasis AI · GNSS/PNT dan ketahanannya terhadap jamming · Keselamatan orbit & regulasi.

**Jebakan taksonomi:**
- **Pusat data orbital sering ditulis seolah sudah operasional.** Yang benar-benar terbang baru **Starcloud-1 dengan SATU H100** (Nov 2025). TeraWave, Google Suncatcher, dan pengajuan SpaceX masih proposal.
- **Direct-to-cell bukan "5G dari langit"** — ini layanan pita sempit (SMS/IoT) yang menumpang spektrum operator.
- **"New Space" bukan berarti roket.** Mayoritas pekerjaan dan peluang bisnis ada di segmen darat, perangkat lunak misi, dan hilir data.
- Nama basi: *Project Kuiper* kini **Amazon Leo** (Nov 2025); *Lumen Orbit* kini **Starcloud** (Mar 2025).
- Konstelasi non-Barat sering dihapus dari peta — mengabaikan **Qianfan** membuat materi kurang berguna untuk pembaca Asia Tenggara.

**Relevansi: rendah untuk dimasuki.** Perangkat lunak antariksa hidup di C/C++, Python, dan Rust — NASA cFS berbasis C, Yamcs berbasis Java, COSMOS berbasis Ruby. **.NET praktis tidak dipakai** di jalur penerbangan maupun kontrol darat. Indonesia juga tidak punya industri peluncuran; SATRIA-1 pun dibuat Thales Alenia dan diluncurkan SpaceX. Irisan yang tersisa untuk Anda: **3GPP NTN** dan jaringan di atas LEO.

### AR/VR — namanya sudah ketinggalan, dan uangnya sedang keluar

**Nama bidangnya perlu diganti.** Industri per 2026 memakai **XR** sebagai payung; *spatial computing* adalah istilah pemasaran Apple dan Samsung. Saran audit: judul utama **XR (AR/VR/MR)**, dengan "AR/VR" dipertahankan sebagai alias pencarian, bukan dihapus.

**Dua salah-kategori yang penting:**
- **Kacamata pintar AI sebagian besar BUKAN AR.** Ray-Ban Meta generasi awal secara eksplisit tidak punya heads-up display maupun fitur AR. Memasukkannya ke AR/VR tanpa catatan membuat pelajar mengira harus belajar rendering 3D, padahal skill nyatanya AI multimodal, audio, dan efisiensi daya.
- **World model bukan sub-topik bidang ini.** Genie 3, Cosmos 3, dan World Labs adalah riset AI generatif/robotika — rumahnya di bidang AI.

**Relevansi: rendah**, dan ini vonis paling keras dari keempat bidang. Alasannya:
1. **Uang sedang keluar dari sisi konsumen.** Reality Labs rugi **USD 19,1 miliar pada 2025** (akumulasi ~USD 80 miliar sejak akhir 2020), memangkas 10% pegawai Januari 2026, dan menutup tiga studio pada 13 Januari 2026 — dananya dialihkan ke AI dan wearable.
2. **Harga perangkat di luar jangkauan pasar Indonesia** (Galaxy XR USD 1.799; Vision Pro USD 3.699 sejak 25 Juni 2026), sehingga basis pemasang lokal terlalu tipis.
3. **Platform kacamata pintar tertutup** — tidak ada SDK pihak ketiga terbuka untuk Ray-Ban Meta, jadi ide bisnis di atasnya bergantung pada izin Meta.
4. **.NET tidak menular** — yang menular hanya sintaks C# ke Unity, bukan ekosistemnya.

---

## Peringatan Sebelum Memakai Berkas Ini

**1. Bobot bukti tidak merata.** Temuan berkeyakinan "tinggi" bersandar pada sumber primer (siaran pers, jurnal, blog resmi vendor, NIST). Temuan berkeyakinan "sedang" sebagian bersandar pada liputan sekunder yang belum dilacak ke sumber aslinya.

**2. Angka riset pasar jangan dikutip sebagai fakta tunggal.** Agen sendiri memperingatkan bahwa sebagian angka Gartner/IDC/McKinsey ditemukan lewat situs agregator statistik, bukan laporan aslinya. Proyeksi pasar antar-firma bisa berbeda jauh.

**3. Beberapa klaim luar biasa wajib Anda konfirmasi sendiri sebelum masuk ke halaman publik.** Saya tidak bisa memverifikasinya secara independen, dan klaim sebesar ini pantas dicek langsung ke sumbernya:
- Akuisisi Anysphere (induk Cursor) oleh SpaceX senilai $60 miliar.
- Penutupan API Sora pada 24 September 2026.
- Nama-nama model tertentu dan tanggal rilisnya.
- Angka pengiriman humanoid dan pangsa pasarnya.

**4. Tanggal GA Microsoft Agent Framework berbeda antar-berkas.** Bagian A menulis **3 April 2026**; `AUDIT-KELAYAKAN.md` B2 menulis **2 April 2026** untuk paket `Microsoft.Agents.AI` 1.0.0. Keduanya keluaran agen yang berbeda dan belum dicocokkan ke satu sumber primer. Sebelum tanggal ini masuk halaman publik, ambil satu dari halaman rilis NuGet.

**5. Bagian B diverifikasi tanpa pencarian web.** Kuota WebSearch keempat agennya habis (200/200) sebelum sempat dipakai, jadi seluruh verifikasi terpaksa lewat WebFetch ke URL yang ditebak. Akibatnya:
- **Bagian taksonominya tetap kuat** — khususnya vonis Renewable Energy, yang bersandar pada definisi eksplisit EIA yang berhasil diambil.
- **Bagian "pemain terkini" jauh lebih lemah.** Untuk Renewable Energy, empat sumber paling otoritatif (IRENA, IEA, NREL, Ember) semuanya menolak akses, sehingga sebagian besar bersandar pada Wikipedia tanpa cek silang.
- **Jangan kutip angka pengapalan kacamata pintar dari berkas ini** — agen AR/VR tidak berhasil memverifikasi satu pun (IDC membalas 403, Counterpoint tidak memuat data yang bisa diambil).
- Halaman Wikipedia "Renewable energy in Indonesia" **masih memuat data basi** (klaim panas bumi 1,3 GW dan peringkat ke-3 dunia) — jangan dipakai.

**6. Berkas ini temuan audit, bukan keputusan.** Isi Bagian 1 `KERANGKA.md` — dua belas entri teknologi itu — **belum disentuh satu koreksi pun**; kata-kata Anda dibiarkan utuh sampai Anda memutuskan mana yang dipakai (Issue [#18](../../issues/18)). Dua hal berikut memang sudah berubah di `KERANGKA.md`, dan keduanya **bukan** penerapan audit ini:
- **Tabel arsitektur 2.6** ditulis ulang mengikuti Bagian 4 (Engineering Blueprint v1) milik pemilik sendiri. Hasilnya kebetulan sejalan dengan `AUDIT-KELAYAKAN.md`; rekonsiliasinya ada di 4.20.
- **Butir E5 dan E11** di daftar keputusan memuat penunjuk ✅ ke vonis berkas ini, sebagai rujukan — bukan sebagai koreksi yang sudah diterapkan.
