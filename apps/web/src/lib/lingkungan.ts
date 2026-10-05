/**
 * Satu pertanyaan, satu jawaban: apakah yang membaca halaman ini orang yang
 * memegang reponya?
 *
 * 🔴 Sampai 2026-09-10 dua komponen mencetak bahan pengembang kepada SIAPA PUN
 * yang membuka situsnya — perintah Windows (`run.ps1 up`, `run.ps1 api`,
 * `run.ps1 seed`) dan alamat API internal (`Tidak bisa menghubungi API di
 * http://api:8080`). Yang membuatnya serius bukan keberadaannya, melainkan
 * KAPAN ia muncul: kedua keadaan yang mencetaknya justru keadaan biasa dari
 * situs yang sudah tayang.
 *
 *  - **kosong** — hari ini nol topik, dan satu-satunya jalan menaikkannya
 *    ([#42](https://github.com/xtheoputra/techverse-x/issues/42)) masih tertutup.
 *    Jadi begitu situsnya tayang
 *    ([#40](https://github.com/xtheoputra/techverse-x/issues/40)), inilah tampilan
 *    pembukanya.
 *  - **galat** — instance gratis Koyeb tidur setelah satu jam menganggur
 *    (ADR-019). Keadaan ini bukan kecelakaan langka; ia bagian dari cara kerja
 *    tier yang sengaja dipilih.
 *
 * Sumbunya sengaja **bentuk bangunan**, bukan "apakah API-nya hidup". Build
 * produksi yang disajikan di mana pun — Vercel, `docker-compose.prod.yml`,
 * maupun `next start` di mesin sendiri — tidak boleh mencetak sesuatu yang
 * hanya bisa dipakai orang yang memegang reponya. `next dev` boleh, dan di sana
 * bahan itu memang berguna.
 *
 * ⚠️ Ini BUKAN penjaga keamanan dan bukan pengganti ADR-020. Alamat API memang
 * publik (KERANGKA.md 4.9) dan sakelar tulis API tetap satu-satunya yang
 * menentukan boleh tidaknya menulis. Baris ini cuma memilih kalimat mana yang
 * dicetak untuk siapa.
 */
export const mesinPengembang = process.env.NODE_ENV !== 'production';
