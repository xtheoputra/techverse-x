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
};

export default nextConfig;
