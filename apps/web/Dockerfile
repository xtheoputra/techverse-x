# syntax=docker/dockerfile:1
#
# Citra produksi web. Konteks build = AKAR repo, bukan folder ini:
#
#     docker build -f apps/web/Dockerfile .
#
# Sebabnya npm workspaces - package-lock.json hanya ada di akar, dan `npm ci`
# menolak berjalan tanpanya.

# ---------------------------------------------------------------------------
# Tahap 1: dependensi. Berdiri sendiri supaya `npm ci` - langkah termahal - tidak
# batal setiap kali satu berkas TSX berubah.
# ---------------------------------------------------------------------------
FROM node:22-alpine AS deps
WORKDIR /repo
COPY package.json package-lock.json ./
COPY apps/web/package.json apps/web/
RUN npm ci

# ---------------------------------------------------------------------------
# Tahap 2: build.
# ---------------------------------------------------------------------------
FROM node:22-alpine AS build
WORKDIR /repo
ENV NEXT_TELEMETRY_DISABLED=1
COPY --from=deps /repo/node_modules ./node_modules
COPY package.json package-lock.json ./
COPY apps/web/ apps/web/
RUN npm run build --workspace web

# ---------------------------------------------------------------------------
# Tahap 3: citra akhir.
#
# Tidak ada npm, tidak ada node_modules yang disalin utuh, tidak ada kode sumber.
# Yang masuk hanya keluaran `output: "standalone"` - Next sudah menelusuri sendiri
# modul mana yang benar-benar dipakai saat berjalan.
# ---------------------------------------------------------------------------
FROM node:22-alpine AS final
WORKDIR /app
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# `standalone` mempertahankan bentuk monorepo-nya, jadi server.js duduk di
# apps/web/ - bukan di akar.
COPY --from=build /repo/apps/web/.next/standalone ./
COPY --from=build /repo/apps/web/.next/static ./apps/web/.next/static
COPY --from=build /repo/apps/web/public ./apps/web/public

# `node` adalah pengguna tak-berhak yang sudah disiapkan citra dasar.
USER node

EXPOSE 3000
ENV PORT=3000
# Tanpa ini server hanya mendengar di localhost DI DALAM peti kemas, dan
# pemetaan port dari luar akan tampak "menolak koneksi" tanpa satu pun galat.
ENV HOSTNAME=0.0.0.0

# API_BASE_URL menunjuk ke nama service API di jaringan compose, bukan localhost.
CMD ["node", "apps/web/server.js"]
