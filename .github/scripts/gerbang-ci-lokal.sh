#!/usr/bin/env bash
#
# GERBANG CI DI MESIN PENGEMBANG - bagian ci.yml yang TIDAK ada di `verify`,
# dijalankan sebelum push (ADR-026).
#
# ⚠️ Ini ALAT PENGEMBANG, bukan pengganti CI. CI proyek ini tetap GitHub Actions,
# dan infrastruktur proyek tidak boleh bergantung pada mesin pemilik (arahan
# pemilik 2026-09-28). Gunanya: menangkap merah SEBELUM menit Actions terpakai,
# dan tetap bisa memeriksa selama Actions mati - sejak 10 September 2026 kuota
# 2.000 menit yang dibagi semua repo privat akun pribadi habis, terutama oleh
# repo lain (#57).
#
#   .\run.ps1 ci   /   make ci   =   verify  +  berkas ini
#
# Urutannya sama dengan ci.yml:
#
#   1. Pindai rahasia   - gitleaks atas SELURUH riwayat git, perintah dan digest
#                         yang sama dengan job `secret-scan` di ci.yml.
#   2. Citra peti kemas - ketiga `docker build`, gerbang ADR-020, tumpukan
#                         produksi dari volume KOSONG (= migrasi dari nol lewat
#                         bundel migrasi), pemindai halaman ADR-024, lalu Trivy
#                         atas ketiga citra.
#
# Yang TIDAK di sini: job backend dan frontend - itu `verify`. Uji integrasi
# menyematkan localhost:5432, jadi Postgres sekali pakai di port lain tidak bisa
# dipakai tanpa mengubah ujinya; "migrasi dari nol" karena itu dibuktikan bundel
# migrasi di langkah 2, bukan `dotnet ef database update`.
#
# 🔑 Kedua pemindai DISEMATKAN per versi DAN digest, bukan `:latest`. Alat gratis
# tetap rantai pasok: citra yang ditarik lewat tag bergerak menjalankan apa pun
# yang kebetulan ditunjuk tag itu hari ini, dengan soket Docker terpasang.
# Alasan versi yang dipilih ada di ADR-026.
#
# ⚠️ Port 8080, 8081, dan 18080 wajib kosong. Skrip ini TIDAK mematikan apa pun
# yang memegangnya - proses di mesin ini belum tentu milik repo ini (Sesi 15
# mematikan satu yang bukan miliknya). Ia berhenti dan menyebut pemegangnya.
#
# Pemakaian: bash .github/scripts/gerbang-ci-lokal.sh
set -u

GITLEAKS='zricethezav/gitleaks:v8.30.1@sha256:c00b6bd0aeb3071cbcb79009cb16a60dd9e0a7c60e2be9ab65d25e6bc8abbb7f'
TRIVY='aquasec/trivy:0.74.0@sha256:62b1e65e8869bc4b4c6aa4fa2b21595256c7c2f6018a9d9ad61caf87187c1969'
COMPOSE=(docker compose -f docker-compose.prod.yml)

cd "$(dirname "$0")/../.." || exit 9

# Git Bash (Windows) menerjemahkan argumen berbentuk jalur POSIX. Itu merusak
# `-v <jalur>:/repo` dan `-v /var/run/docker.sock:...`, jadi terjemahan dimatikan
# HANYA untuk perintah docker run di bawah - tidak global, sebab Sesi 14 mengukur
# bahwa mematikannya global merusak `curl -o /dev/null` di periksa-permukaan-tulis.sh.
if pwd -W >/dev/null 2>&1; then REPO="$(pwd -W)"; else REPO="$PWD"; fi

MERAH=$'\033[31m'; HIJAU=$'\033[32m'; ABU=$'\033[90m'; MATI=$'\033[0m'
mulai_semua=$(date +%s)
vonis=0
ringkasan=()

jalankan() {
  local nama="$1"; shift
  local t0; t0=$(date +%s)
  printf '\n%s== %s ==%s\n' "$ABU" "$nama" "$MATI"
  "$@"; local rc=$?
  local dt=$(( $(date +%s) - t0 ))
  if [ "$rc" -eq 0 ]; then
    ringkasan+=("${HIJAU}hijau${MATI}  ${dt}s  $nama")
  else
    ringkasan+=("${MERAH}MERAH${MATI}  ${dt}s  $nama (exit $rc)")
    vonis=1
  fi
  return "$rc"
}

port_kosong() {
  local terpakai=0
  for p in 8080 8081 18080; do
    if (exec 3<>"/dev/tcp/127.0.0.1/$p") 2>/dev/null; then
      echo "port $p TERPAKAI - bebaskan dulu; skrip ini tidak mematikan proses yang bukan miliknya"
      terpakai=1
    fi
  done
  return "$terpakai"
}

pindai_rahasia() {
  MSYS_NO_PATHCONV=1 docker run --rm -v "$REPO:/repo" "$GITLEAKS" \
    git /repo --redact --no-banner --exit-code 1
}

pindai_citra() {
  local rc=0
  for citra in techversex-api:ci techversex-migrate:ci techversex-web:ci; do
    echo "-- $citra"
    # Tembolok basis data kerentanan disimpan di volume bernama: jalan kedua
    # tidak mengunduh ulang ratusan MB. Basis datanya tetap diperbarui sendiri
    # oleh Trivy begitu umurnya lewat sehari.
    MSYS_NO_PATHCONV=1 docker run --rm \
      -v /var/run/docker.sock:/var/run/docker.sock \
      -v techversex-trivy-cache:/root/.cache/ \
      "$TRIVY" image \
        --scanners vuln \
        --severity CRITICAL,HIGH \
        --exit-code 1 \
        --no-progress \
        "$citra" || rc=1
  done
  return "$rc"
}

isi_basis_data() {
  "${COMPOSE[@]}" exec -T postgres psql -U techversex -d techversex -Atc \
    "select 'migrasi', count(*) from technology.__ef_migrations_history
     union all select 'bidang', count(*) from technology.fields
     union all select 'topik', count(*) from technology.technologies
     union all select 'sisi', count(*) from technology.technology_relationships;"
}

bongkar() {
  printf '\n%s== Log tumpukan produksi, lalu bongkar (selalu) ==%s\n' "$ABU" "$MATI"
  "${COMPOSE[@]}" ps --format '{{.Name}} {{.Status}}' 2>/dev/null
  if [ "$vonis" -ne 0 ]; then "${COMPOSE[@]}" logs --no-color --tail 60 2>/dev/null; fi
  "${COMPOSE[@]}" down -v >/dev/null 2>&1
  echo "tumpukan dibongkar, volume dihapus (exit $?)"
}

jalankan "Port 8080/8081/18080 kosong" port_kosong || { echo "berhenti"; exit 1; }

jalankan "Pindai rahasia (gitleaks, seluruh riwayat)" pindai_rahasia

# Langkah citra berantai seperti di ci.yml: yang gagal menghentikan sisanya,
# tapi pembongkaran tetap jalan.
trap bongkar EXIT
jalankan "Citra API" docker build -q -f apps/api/Dockerfile --target final -t techversex-api:ci . &&
jalankan "Bundel migrasi" docker build -q -f apps/api/Dockerfile --target migrator -t techversex-migrate:ci . &&
jalankan "Citra web" docker build -q -f apps/web/Dockerfile -t techversex-web:ci . &&
jalankan "Citra API menolak menulis (ADR-020)" bash .github/scripts/periksa-permukaan-tulis.sh techversex-api:ci &&
jalankan "Tumpukan produksi hidup (volume kosong, --build)" "${COMPOSE[@]}" up -d --build --wait &&
jalankan "Isi basis data produksi" isi_basis_data &&
jalankan "Teks pembaca dan penelusuran halaman (ADR-024)" \
  env WEB_BASE_URL=http://localhost:8080 API_BASE_URL=http://localhost:8081 node .github/scripts/periksa-halaman-web.mjs &&
jalankan "Pindai ketiga citra (Trivy, CRITICAL/HIGH)" pindai_citra

bongkar
trap - EXIT

printf '\n%sRingkasan gerbang CI lokal%s (%ss)\n' "$ABU" "$MATI" "$(( $(date +%s) - mulai_semua ))"
for baris in "${ringkasan[@]}"; do printf '  %s\n' "$baris"; done
if [ "$vonis" -eq 0 ]; then
  printf '\n%sVONIS: HIJAU%s - ditambah `verify`, itu seluruh gerbang ci.yml.\n' "$HIJAU" "$MATI"
else
  printf '\n%sVONIS: MERAH%s\n' "$MERAH" "$MATI"
fi
exit "$vonis"
