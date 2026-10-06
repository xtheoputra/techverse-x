# Runbook — Merge tumpukan Bulan 3 + migrasi terkoordinasi

**Dibuat:** 2026-10-05. **Diperbarui:** 2026-10-06.
**Status:** Fase 1–3 **sudah dijalankan** (2026-10-05, bukti di tiap fase). Fase 4 di versi
pertama runbook ini **salah premis** dan sudah diganti — lihat bagian berikut.

**Konteks:** situs sudah **TAYANG** (web + API Vercel kontainer + Neon). Operasi ini
membawa `main` naik dari Sesi 12 ke Sesi 20 dan menyalakan **pencarian teks-penuh +
Knowledge Graph** di situs yang hidup.

---

## ⚠️ Koreksi 2026-10-06 — Vercel TIDAK membangun dari repo ini

Versi pertama runbook menulis: *"API di Vercel otomatis bangun ulang dari `main` tiap
kali ada merge."* **Itu tidak benar**, dan baru ketahuan setelah 14 merge selesai dan
`/api/v1/search` tetap 404.

Kedua proyek Vercel ternyata membangun dari repo privat **salinan** repo ini, bukan dari
repo ini. Salinan itu dibuat otomatis saat proyeknya dibuat (mekanisme tepatnya
disimpulkan dari data di bawah, bukan dari dokumentasi Vercel):

| Proyek Vercel | Repo yang dibangun | Salinan dibuat | Isi |
|---|---|---|---|
| `techverse-x-api` | `xtheoputra/techverse-x-api` | 2026-10-05 06:24 UTC | 1 commit "Initial commit", `2f88402` |
| `techverse-x-web` | `xtheoputra/techverse-x-app` | 2026-10-05 07:49 UTC | 1 commit "Initial commit", `f608825` |

Diukur: kedua repo dibuat 7–8 detik **sebelum** proyek Vercel-nya; isinya `main`@`57d3e94`
dikurangi tiga berkas `.github/workflows/*`; bukan fork. Jadi mereka **cuplikan beku** —
merge ke `xtheoputra/techverse-x` tidak pernah sampai ke sana. Nama `techverse-x-app`
tampaknya berasal dari nama awal proyek web: deployment lamanya masih tercatat atas nama
itu di daftar deployment Vercel.

**Akibatnya** setelah Fase 1–3 selesai: Neon sudah dimigrasi ke skema baru, tetapi kode
yang tayang masih Sesi 12 + tambalan Next.js 16.3.6. `/api/v1/fields` dan `/health/ready`
tetap 200 (migrasinya aditif), hanya `/cari` dan `/api/v1/search` yang 404.

---

## Keadaan awal (per 2026-10-05, sebelum eksekusi)

- `main` = **Sesi 12** (10 Sep). ADR-022–026, pencarian, Knowledge Graph, navigasi —
  semuanya di **14 PR yang belum di-merge**.
- Tumpukan **lurus**, urut dari dasar ke puncak:

  ```
  #56 → #58 → #62 → #63 → #64 → #65 → #66 → #67 → #69 → #70 → #71 → #72 → #73 → #74
  ```

- Live: web `techverse-x-web.vercel.app`, API `techverse-x-api.vercel.app`, Neon
  (9 tabel, 14 bidang, 0 topik).
- Dua commit yang sudah di `main` dan belum ada di tumpukan: tambalan Next.js 16.3.6
  (#76) dan `Dockerfile.vercel` (#75).

> ✅ **De-risk (diperiksa 2026-10-05 di kode tumpukan, dan terbukti benar):** API
> **tidak** migrasi/validasi skema saat startup, dan `/health/ready` hanya cek
> konektivitas. Jadi kode baru yang tayang **sebelum** migrasi tidak menjatuhkan API —
> hanya `/cari` yang 500. Karena itu menjeda deploy API (dulu "Fase 0") **tidak
> diperlukan** dan tidak dilakukan.

---

## Fase 1 — Merge 14 PR, dasar ke puncak ✅ SELESAI 2026-10-05

Untuk **tiap** PR, dari #56 ke atas: ubah basisnya ke `main`, tunggu CI hijau, merge.
Jangan loncat urutan.

**Terukur:** keempat belas PR berstatus MERGED antara 09:37 dan 10:21 UTC; ujung `main`
= `90a4115` (merge #74). Push-push itu memicu 14 run CI dan 13 run "Rilis citra", semuanya hijau.

---

## Fase 2 — Citra API baru terbit ✅ SELESAI 2026-10-05

Merge terakhir memicu `rilis-citra.yml`: bangun, pindai Trivy, dorong ke GHCR.

**Terukur:** run `37296086996` pada `90a4115`, hijau. SHA yang dipakai fase berikutnya:
`90a4115`.

---

## Fase 3 — Migrasi Neon dengan citra baru ✅ SELESAI 2026-10-05

Kode baru butuh kolom `tsvector` (ADR-022) dan tabel relasi Knowledge Graph (ADR-023).

```
gh workflow run migrasi-produksi.yml --ref main -f sha=<SHA>
```

**Terukur:** run `37296775982`, 10:28 UTC, hijau, `Done.`; dua migrasi baru mendarat
(`PencarianTeksPenuh`, `RelasiAntarTopik`). Aditif — tidak menghapus data. Pemicunya
harus pemilik (penjaga izin menolak sesi agen menyentuh produksi).

---

## Fase 4 — Sambungkan Vercel ke repo yang benar, lalu picu build baru

*(Menggantikan Fase 4 versi pertama, yang berasumsi auto-deploy dari `main`.)*

**Satu kali, untuk tiap proyek Vercel** (`techverse-x-api`, lalu `techverse-x-web`):

1. Settings → Git → **Disconnect** repo salinan.
2. **Connect** ke `xtheoputra/techverse-x` (repo asli, tanpa akhiran `-api`/`-app`).
3. Cek **Root Directory** tidak ikut berubah: API = `./` (akar; `Dockerfile.vercel` ada
   di akar), web = `apps/web`.
4. Cek variabel lingkungan masih ada: API → `PORT=8080` dan `ConnectionStrings__Postgres`;
   web → `API_BASE_URL`.

**Lalu picu build dengan commit BARU di `main`** (mis. merge PR berikutnya).

> 🔴 **Jangan memakai Redeploy pada deployment lama.** Redeploy meng-*clone* ulang
> commit yang sama. Ketiga deployment API yang ada semuanya meng-clone `2f88402`, commit
> yang hanya ada di repo salinan. Sesudah repo diganti, Redeploy akan membangun ulang
> kode lama atau gagal karena commitnya tak ada. Menyambung ulang saja juga **tidak**
> memicu build — Vercel hanya membangun saat ada push.

**Bukti yang benar** ada di log build Vercel (`vercel inspect <url> --logs`), baris
pertama harus berbunyi:

```
Cloning github.com/xtheoputra/techverse-x (Branch: main, Commit: <SHA main>)
```

Kalau yang muncul `techverse-x-api` atau `techverse-x-app`, proyek itu masih membangun
dari salinan beku.

---

## Fase 5 — Verifikasi

1. **Pencarian teks-penuh hidup:**
   ```
   curl -s "https://techverse-x-api.vercel.app/api/v1/search?q=quantum" | head -c 200
   ```
   Harus 200 dan memuat **`quantum-computing`** — itu **bidang**, bukan topik: pencarian
   menjawab bidang dan topik sekaligus, dan Neon masih 0 topik, jadi daftar topiknya
   kosong. Sebelumnya 404 karena rutenya belum ada di kode yang tayang.
2. **Situs:** `https://techverse-x-web.vercel.app/cari` terbuka (bukan 404), dan homepage
   tetap menampilkan **14 bidang**.
3. **API baca lama tetap jalan:** `/api/v1/fields` → 200; tulis → 405.
4. **Asal build** (Fase 4): log kedua proyek meng-clone `xtheoputra/techverse-x`.

---

## Kalau ada yang salah (rollback)

- **Situs live rusak:** Vercel → proyek terkait → **Instant Rollback** ke deployment
  terakhir yang baik. Migrasi bersifat aditif, jadi skema lama tetap kompatibel dengan
  kode lama.
- **Migrasi gagal:** baca lognya — `Couldn't set …` = parameter string, `SSL
  connection requested…` = transport. Skema tidak berubah separuh (EF bundel
  transaksional per migrasi).
- **Build Vercel gagal setelah repo diganti:** periksa Root Directory dan variabel
  lingkungan dulu (Fase 4, butir 3–4), lalu baca log build-nya.

---

## Setelah selesai

- Repo salinan `techverse-x-api` dan `techverse-x-app` **tidak punya isi unik** (identik
  dengan `main`@`57d3e94` dikurangi `.github/workflows`) dan boleh dihapus pemilik —
  `techverse-x-app` hanya sesudah proyek web terbukti membangun dari `techverse-x`.
- **Ukuran resmi (topik `tinjau`) masih 0** — langkah berikutnya: jalan ADR-021
  (menaikkan topik ke `tinjau`) + menulis konten AI Agents. Itu yang akhirnya
  menggerakkan angka resmi dari nol.
