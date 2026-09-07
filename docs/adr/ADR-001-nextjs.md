# ADR-001 — Next.js 16 App Router untuk `apps/web`

**Status:** Diterima
**Tanggal:** 2026-09-03

## Konteks

`KERANGKA.md` 2.6 menetapkan Next.js + React + TypeScript + Tailwind. Yang terpasang: **Next.js 16.3.4, React 19.2.8, Tailwind 4**.

## Keputusan

App Router, Server Components, TypeScript, Tailwind 4. Web bicara ke backend lewat **HTTP** (`src/lib/api.ts`), tidak pernah menyentuh basis data langsung — sesuai 4.9 yang menempatkan web sebagai salah satu klien API, bukan pemilik data.

Dua hal khas Next.js 16 yang wajib diketahui sebelum menyunting halaman:

1. **`fetch` TIDAK lagi di-cache secara bawaan** dan menahan render sampai selesai. Pola lama `fetch(url, { next: { revalidate } })` bukan lagi defaultnya. Untuk meng-cache, pakai direktif `use cache`.
2. **Halaman yang mengambil data tetap dipanggang saat build** kalau tidak dinyatakan dinamis. Ini sempat terjadi di sini: build pertama menghasilkan rute statis, dan yang ikut terpanggang justru jawaban API pada saat build — yaitu galat "tidak bisa dihubungi", karena saat build API memang mati. Perbaikannya `await connection()` dari `next/server`; rutenya sekarang dirender per-permintaan.

`node_modules/next/dist/docs/` memuat dokumentasi versi yang benar-benar terpasang. Baca dari situ, bukan dari ingatan — Next.js 16 sendiri menaruh peringatan itu di `apps/web/AGENTS.md`.

## Konsekuensi

- `apps/web/AGENTS.md` ditulis ulang oleh `next dev` setiap kali dijalankan. Ia sengaja ikut di-commit; menghapusnya hanya membuat pohon kerja kotor lagi.
- Layar sengaja **minim**: satu halaman yang membuktikan jalur Web → API → PostgreSQL hidup. Navigasi sungguhan menunggu Issue [#1](../../../../issues/1).
