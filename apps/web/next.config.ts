import path from "node:path";
import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Citra produksi menyalin keluaran ini apa adanya, tanpa menjalankan
  // `npm install` sama sekali: Next menelusuri modul mana saja yang benar-benar
  // dipakai saat berjalan lalu menyalinnya sendiri.
  output: "standalone",

  // Wajib di monorepo npm workspaces. Tanpa ini Next mengira akar proyeknya
  // apps/web, lalu melewatkan node_modules yang di-hoist ke akar repo - dan
  // citranya baru gagal saat DIJALANKAN, bukan saat dibangun.
  outputFileTracingRoot: path.join(import.meta.dirname, "../.."),

  // <Link> ke rute yang tidak ada jadi galat `next build`, bukan 404 yang baru
  // ketahuan saat diklik. Ia menjaga KEBERADAAN rute saja: aturan ADR-024 bahwa
  // sebuah bagian masuk menu hanya kalau halamannya berisi di produksi tetap
  // tidak dijaga kode apa pun.
  typedRoutes: true,
};

export default nextConfig;
