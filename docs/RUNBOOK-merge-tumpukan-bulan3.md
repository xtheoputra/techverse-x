# Runbook — Merge tumpukan Bulan 3 + migrasi terkoordinasi

**Dibuat:** 2026-10-05. **Status:** siap dieksekusi pemilik.
**Konteks:** situs sudah **TAYANG** (web + API Vercel kontainer + Neon). Operasi ini
membawa `main` naik dari Sesi 12 ke Sesi 20 dan menyalakan **pencarian teks-penuh +
Knowledge Graph** di situs yang hidup.

> ⚠️ Baca seluruhnya dulu. Kode baru menuntut migrasi skema yang belum ada di Neon,
> dan API di Vercel **otomatis bangun ulang dari `main`** tiap kali ada merge.
>
> ✅ **De-risk (diperiksa 2026-10-05 di kode tumpukan):** API **tidak** migrasi/
> validasi skema saat startup, dan `/health/ready` hanya cek konektivitas. Jadi
> kalau tumpukan ter-merge **sebelum** migrasi, **API tetap hidup** — homepage,
> 14 bidang, dan halaman bidang jalan normal; **hanya `/cari`** (pencarian teks-
> penuh baru) yang 500 sampai migrasi dijalankan. Situs **tidak** rusak total.
> Karena itu **Fase 0 di bawah OPSIONAL.**

---

## Keadaan awal (per 2026-10-05)

- `main` = **Sesi 12** (10 Sep). ADR-022–026, pencarian, Knowledge Graph, navigasi —
  semuanya di **14 PR yang belum di-merge**.
- Tumpukan **lurus**, urut dari dasar ke puncak:

  ```
  #56 → #58 → #62 → #63 → #64 → #65 → #66 → #67 → #69 → #70 → #71 → #72 → #73 → #74
  ```

- Live: web `techverse-x-web.vercel.app`, API `techverse-x-api.vercel.app`, Neon
  (9 tabel, 14 bidang, 0 topik).
- Dua commit yang **sudah** di main dan belum ada di tumpukan: tambalan Next.js
  16.3.6 (#76) dan `Dockerfile.vercel` (#75). Karena itu tiap PR perlu **"Update
  branch"** sebelum merge.

---

## Fase 0 — (OPSIONAL) Hentikan auto-deploy API

**Tidak wajib** (lihat de-risk di atas: API tidak crash; hanya `/cari` yang 500
sementara). Lakukan **hanya** kalau Anda ingin `/cari` tetap mulus selama proses
merge. Untuk situs pra-luncur tanpa pengguna nyata, **melewati fase ini aman** —
`/cari` akan pulih sendiri begitu Fase 3 (migrasi) selesai.

Kalau tetap mau menjeda: Vercel → proyek **`techverse-x-api`** → **Settings → Git**
→ setel **"Ignored Build Step"** ke perintah yang membatalkan build, lalu kosongkan
lagi di Fase 4. (Mekanismenya agak fiddly — kalau ragu, **lewati saja fase ini.**)

---

## Fase 1 — Merge 14 PR, dasar ke puncak

Untuk **tiap** PR dalam urutan di atas (mulai #56), lakukan tepat tiga langkah:

1. Buka PR-nya → klik **"Update branch"** (menarik `main` terbaru: Next.js 16.3.6 +
   `Dockerfile.vercel`). Ini yang membuat CI hijau; tanpa ini gerbang "Citra peti
   kemas" merah karena Next.js lama.
2. **Tunggu CI hijau** (±3 menit). Semua cek ✓.
3. **Merge pull request** → **Confirm merge**.

Lalu lanjut ke PR berikutnya (basisnya otomatis berpindah ke `main` setelah yang di
bawahnya ter-merge).

> 💡 Jangan loncat urutan. Kalau satu PR konflik saat "Update branch" (kemungkinan
> kecil, paling-paling di `package-lock.json`), berhenti dan minta bantuan sebelum
> lanjut.
>
> 💡 Percepatan opsional: gerbang "Citra peti kemas" **bukan cek wajib**, jadi secara
> teknis bisa merge tanpa "Update branch". **Tidak disarankan** — "Update branch"
> memastikan yang di-merge benar-benar hijau dengan tambalan Next.js.

Selesai Fase 1 → `main` sudah memuat seluruh Bulan 3 (Sesi 13–20).

---

## Fase 2 — Tunggu citra API baru terbit

Merge terakhir memicu `rilis-citra.yml` di `main`.

1. Tab **Actions** → tunggu run **"Rilis citra"** teratas **hijau**. Ia membangun,
   memindai (kini bebas RCE Next.js), dan mendorong citra `api` baru ke GHCR.
2. Catat **SHA** `main` terbaru (dari ringkasan run, atau `git log`). Sebut **`<SHA>`**.

---

## Fase 3 — Migrasi Neon dengan citra baru

Kode baru butuh kolom `tsvector` (ADR-022) dan tabel relasi Knowledge Graph (ADR-023).

1. Jalankan (dari terminal Anda):
   ```
   gh workflow run migrasi-produksi.yml --ref main -f sha=<SHA>
   ```
2. Pantau sampai **hijau** (`Done.`). Ini menambah migrasi baru ke Neon — **aditif**,
   tidak menghapus data. (Beri tahu saya `<SHA>`-nya, saya pantau untuk Anda.)

---

## Fase 4 — Rapikan deploy API

- **Kalau Fase 0 DILEWATI (jalur biasa):** tidak ada yang perlu dilakukan. API yang
  sudah hidup (kode baru) akan menemukan kolom/tabel baru pada kueri berikutnya
  begitu migrasi (Fase 3) selesai — `/cari` pulih sendiri (paling lama setelah cache
  web 5 menit). Redeploy opsional kalau ingin memastikan.
- **Kalau Fase 0 DIPAKAI:** Vercel `techverse-x-api` → **Settings → Git** →
  **nyalakan kembali** auto-deploy, lalu **Deployments → (⋯) → Redeploy** `main`
  terbaru (tanpa cache).

---

## Fase 5 — Verifikasi (saya bisa jalankan `curl`-nya untuk Anda)

1. **Pencarian teks-penuh hidup:**
   ```
   curl -s "https://techverse-x-api.vercel.app/api/v1/search?q=quantum" | head -c 200
   ```
   Harus memuat **`quantum-computing`** (sebelumnya 404 karena endpoint belum ada).
2. **Situs:** `https://techverse-x-web.vercel.app/cari` terbuka, dan homepage tetap
   menampilkan **14 bidang**.
3. **API baca lama tetap jalan:** `/api/v1/fields` → 200; tulis → 405.

---

## Kalau ada yang salah (rollback)

- **Situs live rusak saat di tengah jalan:** Vercel → `techverse-x-api` →
  **Instant Rollback** ke deployment terakhir yang baik (yang sekarang hidup).
  Migrasi bersifat aditif, jadi skema lama tetap kompatibel dengan kode lama.
- **Migrasi gagal:** baca lognya — `Couldn't set …` = parameter string, `SSL
  connection requested…` = transport. Skema tidak berubah separuh (EF bundel
  transaksional per migrasi).
- **Satu PR tak mau hijau setelah "Update branch":** berhenti, jangan paksa merge
  PR di atasnya — seluruh tumpukan lurus, jadi yang di atas ikut terhambat.

---

## Setelah selesai

- `main` naik ke Sesi 20; ADR-025 (host API = Vercel kontainer) kini ADA di `main`.
- Pencarian + Knowledge Graph hidup di situs.
- **Ukuran resmi (topik `tinjau`) masih 0** — langkah berikutnya: jalan ADR-021
  (menaikkan topik ke `tinjau`) + menulis konten AI Agents. Itu yang akhirnya
  menggerakkan angka resmi dari nol.
