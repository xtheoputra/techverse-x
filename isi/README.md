# isi/ — isi sungguhan halaman teknologi

Satu berkas JSON per topik: `isi/<bidang>/<slug>.json`. Berkas inilah **sumber** isi;
Neon hanya proyeksinya. Keputusannya dan alasannya ada di
[ADR-027](../docs/adr/ADR-027-jalan-isi-sungguhan.md).

## Bentuk berkas

Sama dengan muatan API. Kunci yang tak dikenal ditolak (salah ketik `resorces` tidak
boleh lolos jadi "tak ada sumber").

| Kunci | Isi | Batas |
|---|---|---|
| `slug` | sama dengan nama berkas | 160 |
| `name` | nama topik | 200 |
| `fieldSlug` | sama dengan nama folder (salah satu bidang [ADR-010](../docs/adr/ADR-010-taksonomi-bidang.md)) | |
| `summary` | **Overview** (bagian 1) — teks polos, situs tidak merender Markdown | 2000 |
| `prerequisite` | `{title, description}` — **langkah 0** roadmap ([ADR-012](../docs/adr/ADR-012-template-halaman.md)) | 200 / 2000 |
| `roadmap` | daftar `{title, description}`, minimal 1, berurut | 200 / 2000 |
| `tools` | daftar `{slug, name, summary, homepage, note}`, minimal 1. Satu slug = satu definisi di **seluruh** `isi/` | |
| `projects` | daftar `{title, brief}`, minimal 1 | 200 / 4000 |
| `resources` | daftar `{type, title, url}`, minimal 1. `type` persis salah satu `OfficialDocs`, `Video`, `Paper`, `Repository`. URL http/https absolut dan unik | 300 / 1000 |
| `requires` | daftar slug topik prasyarat; hanya yang punya berkas di `isi/`, tanpa siklus | |

Contoh lengkap: [`ai-agents/model-context-protocol.json`](ai-agents/model-context-protocol.json).

## Aturan isi — bukan sekadar bentuk

- **Setiap klaim harus bisa ditelusuri ke sumber primer yang dibaca saat menulis**, bukan
  diingat ([ADR-012 §5](../docs/adr/ADR-012-template-halaman.md): *klaim yang tidak bisa
  diverifikasi independen tidak masuk kurikulum*). Pilot MCP menunjukkan kenapa: spesifikasi
  yang tayang ternyata revisi `2026-07-28`, berbeda dari yang diingat penyusunnya.
- **Tautan sumber diperiksa hidup** (HTTP 200) sebelum masuk berkas. Utamakan alamat yang
  stabil (`/specification/latest`) daripada yang menyebut tanggal.
- Tulis **kriteria selesai** di proyek mini, bukan hanya perintahnya.

## Alur kerja

```
tulis berkas  →  .\run.ps1 isi              (periksa bentuk, tanpa jaringan)
              →  PR                         (pemilik MEMBACA teksnya di diff — langkah baca pertama)
              →  merge ke main
              →  Actions → "Pasang isi"     (sha citra + slug; hanya dari main)  →  topik berstatus draf
              →  pemilik membaca halaman jadinya di situs
              →  Actions → "Naikkan ke tinjau"                                    →  topik berstatus tinjau
```

Dua workflow terakhir **sengaja dua langkah**: `Pasang isi` paling jauh membawa topik ke
`draf` dan tak punya nama pemeriksa; hanya `Naikkan ke tinjau` yang melahirkan klaim
"sudah diperiksa manusia" ([ADR-021](../docs/adr/ADR-021-jalan-menuju-tinjau.md)), dengan
nama dari `github.actor`.

## Di mesin sendiri

```powershell
.\run.ps1 isi           # periksa semua berkas (sama dengan gerbang CI `cek:isi`)
.\run.ps1 up; .\run.ps1 migrate; .\run.ps1 api     # API pengembangan dengan tulis hidup
.\run.ps1 isi-pasang    # pasang SEMUA berkas ke API itu, prasyarat lebih dulu
```

⚠️ Basis data berisi contoh `seed` punya topik bernama sama dengan isi sungguhan
(`model-context-protocol`). Pemasang tidak menimpa — ia berhenti dengan `BEDA`. Pakai
`.\run.ps1 reset` lalu `migrate` untuk basis data kosong.

## Pemasang tidak menimpa — dan batasnya

Berkas dan server dibandingkan dulu. Kalau berbeda, pemasang berhenti **sebelum menulis apa
pun** dan menyebut bedanya. Topik yang sudah `tinjau` **terkunci**. Yang bisa diperbaiki
sesudah terpasang: **sumber, proyek, dan alat** (jalan buang sudah ada, lalu pasang ulang).
Yang **belum bisa**: mengubah `name`, `summary`, prasyarat, atau langkah roadmap — itu utang
yang tercatat di ADR-027.
