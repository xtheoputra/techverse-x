# syntax=docker/dockerfile:1
#
# Citra produksi API. Konteks build = AKAR repo, bukan folder ini:
#
#     docker build -f apps/api/Dockerfile .
#
# Sebabnya manajemen paket terpusat - Directory.Packages.props memegang SELURUH
# versi paket, dan tanpa ia ikut masuk konteks, restore di dalam citra memakai
# resolusi yang berbeda dari mesin lokal maupun CI.

# ---------------------------------------------------------------------------
# Tahap 1: restore. Sengaja berdiri sendiri supaya lapisannya bisa dipakai ulang
# oleh tahap publish MAUPUN tahap migrasi.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src

# global.json memutuskan SDK mana yang dipakai, Directory.*.props memegang
# TargetFramework dan versi paket, .editorconfig memegang aturan gaya yang
# ditegakkan saat build (EnforceCodeStyleInBuild). Keempatnya harus ada SEBELUM
# restore, kalau tidak build di dalam citra tidak setara dengan build di CI.
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY .config/dotnet-tools.json .config/

# Hanya berkas proyek dulu. Lapisan restore adalah yang paling mahal dan paling
# jarang berubah; menyalin kode sumber sebelum ini membuatnya batal setiap kali
# satu baris berubah.
COPY apps/api/TechVerseX.Api.csproj apps/api/
COPY packages/contracts/TechVerseX.Contracts.csproj packages/contracts/
COPY services/technology/TechVerseX.TechnologyService.csproj services/technology/
RUN dotnet restore apps/api/TechVerseX.Api.csproj

COPY apps/api/ apps/api/
COPY packages/contracts/ packages/contracts/
COPY services/technology/ services/technology/

# ---------------------------------------------------------------------------
# Tahap 2: publish.
# ---------------------------------------------------------------------------
FROM restore AS publish
RUN dotnet publish apps/api/TechVerseX.Api.csproj \
        --configuration Release \
        --no-restore \
        --output /app

# ---------------------------------------------------------------------------
# Tahap 3: bundel migrasi.
#
# RENCANA-V1.md menyebut "migrasi di lingkungan asing" sebagai salah satu kejutan
# yang menunggu di penyebaran. Bundel ini jawabannya: satu berkas yang bisa
# dijalankan, TANPA SDK dan tanpa kode sumber di sisi produksi.
# ---------------------------------------------------------------------------
FROM restore AS migrations-build
# Restore kedua, kali ini BER-RID. Restore di tahap `restore` sengaja
# RID-agnostik supaya lapisannya bisa dipakai ulang oleh tahap publish; bundel
# self-contained menuntut aset khusus linux-x64, dan tanpa baris ini ia berhenti
# dengan NETSDK1047 ("Assets file doesn't have a target for net10.0/linux-x64").
RUN dotnet restore apps/api/TechVerseX.Api.csproj --runtime linux-x64 \
 && dotnet tool restore \
 && dotnet ef migrations bundle \
        --project services/technology/TechVerseX.TechnologyService.csproj \
        --startup-project apps/api/TechVerseX.Api.csproj \
        --self-contained --runtime linux-x64 \
        --force \
        --output /migrate/migrate

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS migrator
WORKDIR /migrate

# `apt upgrade` menambal openssl citra dasar (CVE-2026-84782: libssl3t64 +
# openssl) sampai runtime-deps:10.0 sendiri diperbarui di hulu — sejajar dengan
# `apk upgrade libssl3 libcrypto3` di citra web. Dijalankan sebagai root, sebelum
# pindah ke $APP_UID; `--only-upgrade` tidak pernah menambah paket baru.
RUN apt-get update \
 && apt-get install -y --only-upgrade openssl libssl3t64 \
 && rm -rf /var/lib/apt/lists/*

COPY --from=migrations-build /migrate/ ./
USER $APP_UID
# Bundel membaca ConnectionStrings__Postgres dari lingkungan, sama seperti API.
ENTRYPOINT ["./migrate"]

# ---------------------------------------------------------------------------
# Tahap 3b: bundel migrasi KEDUA, kali ini FRAMEWORK-DEPENDENT.
#
# Kenapa ada dua, dan kenapa ini bukan pemborosan (ADR-017):
#
#   - Citra `migrator` di atas berbasis runtime-deps, yang TIDAK punya runtime
#     .NET. Karena itu bundelnya harus --self-contained: ia memikul runtimenya
#     sendiri. Itu bentuk yang dipakai docker-compose, VPS, dan initContainers
#     Azure Container Apps - semuanya menjalankan citra TERPISAH.
#
#   - Tapi *pre-deploy command* milik Render, Railway, dan release_command
#     Fly.io semuanya menjalankan perintah DI DALAM CITRA LAYANAN ITU SENDIRI,
#     bukan citra lain. Supaya urutan `migrate -> api` tetap bisa ditegakkan di
#     sana, bundelnya harus ikut ke dalam citra `api`.
#
#   - Di citra `api` (basis aspnet:10.0) runtimenya SUDAH ADA, jadi bundel di
#     sini framework-dependent - jauh lebih kecil daripada varian self-contained,
#     dan tidak butuh restore ber-RID sendiri.
#
# ⚠️ API TIDAK PERNAH memanggil bundel ini sendiri. Larangan "migrasi di startup
# API" (PENYEBARAN.md) tetap berlaku: yang memanggilnya adalah hook pre-deploy
# platform, SEKALI PER PENYEBARAN - bukan sekali per instans.
# ---------------------------------------------------------------------------
FROM restore AS migrations-fdd
RUN dotnet tool restore \
 && dotnet ef migrations bundle \
        --project services/technology/TechVerseX.TechnologyService.csproj \
        --startup-project apps/api/TechVerseX.Api.csproj \
        --force \
        --output /migrate-fdd/efbundle

# ---------------------------------------------------------------------------
# Tahap 4: citra akhir API.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# `apt upgrade` menambal openssl citra dasar (CVE-2026-84782: libssl3t64 +
# openssl) sampai aspnet:10.0 sendiri diperbarui di hulu — sejajar dengan
# `apk upgrade libssl3 libcrypto3` di citra web. Dijalankan sebagai root, sebelum
# pindah ke $APP_UID; `--only-upgrade` tidak pernah menambah paket baru.
RUN apt-get update \
 && apt-get install -y --only-upgrade openssl libssl3t64 \
 && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app ./

# Bundel migrasi ikut, TIDAK dijalankan sendiri. Lihat catatan tahap 3b.
COPY --from=migrations-fdd /migrate-fdd/ ./

# Berjalan sebagai pengguna tak-berhak yang sudah disiapkan citra dasar .NET.
USER $APP_UID

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# ConnectionStrings__Postgres WAJIB diisi saat menjalankan; kalau tidak, API
# berhenti dengan pesan yang menyebutkan namanya.
#
# ConnectionStrings__Redis sengaja TIDAK disetel - ADR-016 memutuskan Redis
# tidak di-provision di V1, dan kesiapan sudah tahu cara hidup tanpanya.
ENTRYPOINT ["dotnet", "TechVerseX.Api.dll"]
