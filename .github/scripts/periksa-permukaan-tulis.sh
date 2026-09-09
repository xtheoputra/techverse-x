#!/usr/bin/env bash
#
# Gerbang ADR-020 di tingkat CITRA - bukan di tingkat kode.
#
# Uji integrasi sudah membuktikan bahwa bawaan `Editorial:WritesEnabled`
# tertutup, tapi ia membuktikannya lewat WebApplicationFactory - yaitu dengan
# MERAKIT host sendiri dari kode. Yang tidak bisa dibuktikan di sana: apakah
# ARTEFAK yang benar-benar disebarkan membawa sesuatu yang membukanya lagi.
# Setidaknya tiga hal bisa melakukannya dan lolos seluruh uji .NET:
#
#   - sebuah `ENV Editorial__WritesEnabled=true` yang masuk ke Dockerfile,
#   - default yang datang dari citra dasar,
#   - `launchSettings.json` yang suatu hari ikut terbawa `dotnet publish`.
#
# Ketiganya baru akan terlihat di produksi. Karena itu pemeriksaannya dijalankan
# terhadap citra yang sudah jadi, di dua tempat yang sama dengan Container Scan:
# `ci.yml` (PR) dan `rilis-citra.yml` (SEBELUM push). Satu berkas dipakai
# berdua supaya keduanya tidak bisa berbeda diam-diam.
#
# 🔑 Tidak butuh Postgres sama sekali, dan itu bukan kebetulan yang dimanfaatkan
# melainkan sifat yang diperiksa: 405 terjadi di ROUTING, sebelum satu baris pun
# dibaca.
#
# `/health/live` dipakai sebagai KENDALI - ia sengaja tidak menyentuh dependensi
# apa pun (`Predicate = _ => false`), jadi 200 di sana membuktikan aplikasinya
# memang hidup dan melayani rute.
#
# ⚠️ Yang dibeli kendali itu perlu ditulis JUJUR, sebab versi pertama komentar
# ini melebih-lebihkannya: peti kemas yang mati TIDAK menjawab 405, ia menjawab
# 000, jadi gerbangnya memang tetap merah tanpa kendali apa pun. Yang benar-benar
# dibeli ada dua, dan keduanya nyata:
#
#   1. DIAGNOSIS YANG BENAR. Tanpa kendali, peti kemas yang gagal menyala
#      dilaporkan sebagai "rutenya masih dipasang" - tuduhan yang salah alamat,
#      dan yang akan menyita waktu orang di tempat yang keliru.
#   2. Perlindungan dari sesuatu yang LAIN menjawab di port itu. 405 dari
#      proses asing akan meluluskan gerbang ini; menuntut /health/live 200
#      DARI PORT YANG SAMA membuat kebetulan itu jauh lebih sempit.
#
# Pemakaian: periksa-permukaan-tulis.sh <ref-citra>

set -uo pipefail

CITRA="${1:?pemakaian: periksa-permukaan-tulis.sh <ref-citra>}"
PORT="${PORT_UJI:-18080}"
NAMA="periksa-adr020-$$"

bersihkan() {
  docker rm -f "$NAMA" >/dev/null 2>&1 || true
}
trap bersihkan EXIT

# Alamat Postgres yang sengaja TIDAK terjangkau: aplikasinya hanya menuntut
# string koneksinya ADA, bukan hidup, dan pemeriksaan di bawah memang tidak
# boleh bergantung pada basis data.
if ! docker run -d --name "$NAMA" -p "$PORT:8080" \
  -e "ConnectionStrings__Postgres=Host=tidak-terjangkau;Port=5432;Database=x;Username=x;Password=x" \
  "$CITRA" >/dev/null; then
  echo "::error::Gagal menjalankan peti kemas dari $CITRA."
  exit 1
fi

# curl sudah mencetak `000` sendiri lewat %{http_code} saat ia gagal - `|| echo
# 000` yang naif menambahkan yang KEDUA dan menghasilkan "000000", angka yang
# terlihat seperti bug pada alat ukurnya sendiri. Sudah kejadian sekali.
kode() {
  local hasil
  if ! hasil="$(curl -s -o /dev/null -w '%{http_code}' --max-time 8 "$@" 2>/dev/null)"; then
    hasil="000"
  fi
  [ -n "$hasil" ] || hasil="000"
  echo "$hasil"
}

for _ in $(seq 1 40); do
  if [ "$(kode "http://localhost:$PORT/health/live")" = "200" ]; then
    break
  fi
  sleep 1
done

hidup="$(kode "http://localhost:$PORT/health/live")"

# 🔑 Muatannya SENGAJA TIDAK SAH, dan itu bukan kecerobohan - jangan
# "diperbaiki" jadi muatan yang sah.
#
# Yang ditanyakan di sini cuma satu: RUTENYA ADA ATAU TIDAK. Muatan tak sah
# dijawab validator sebelum satu baris pun dibaca, jadi jawabannya tegas dan
# tidak bergantung pada basis data:
#
#   405 = rutenya TIDAK dipasang  (yang dituntut ADR-020)
#   400 = rutenya ADA, validator sempat berjalan  (regresi)
#
# Muatan yang SAH akan menyeret basis data ke dalam pemeriksaan ini. Diukur di
# citra yang sengaja dijebolkan: muatan tak sah dijawab 400 dalam 0,0 detik,
# muatan sah dijawab 500 dalam 2,3 detik - dan pada batas waktu yang lebih
# pendek ia malah menggantung sampai curl menyerah. Alat ukur yang jawabannya
# bergantung pada dependensi yang sengaja dimatikan bukan alat ukur.
tulis="$(kode -X POST "http://localhost:$PORT/api/v1/technologies" \
  -H 'Content-Type: application/json' \
  -d '{"name":"","summary":"","fieldSlug":""}')"

echo "citra=$CITRA  health/live=$hidup  POST /api/v1/technologies=$tulis"

# KENDALI lebih dulu. Kalau ini gagal, angka di bawahnya tidak berarti apa-apa.
if [ "$hidup" != "200" ]; then
  echo "::error::$CITRA tidak melayani /health/live (dapat $hidup). Peti kemasnya tidak hidup, jadi hasil pemeriksaan tulis tidak bisa dipercaya."
  docker logs "$NAMA" 2>&1 | tail -40 || true
  exit 1
fi

if [ "$tulis" != "405" ]; then
  if [ "$tulis" = "400" ]; then
    sebab="rutenya ADA - validator sempat berjalan"
  else
    sebab="rutenya menjawab $tulis, yang berarti ia dipasang"
  fi
  echo "::error::$CITRA masih memasang POST /api/v1/technologies ($sebab; harusnya 405). ADR-020 menuntut permukaan tulis TIDAK dipasang di artefak yang disebarkan - V1 nol autentikasi (ADR-013), jadi endpoint tulis yang tayang adalah endpoint milik siapa saja. Periksa ENV di Dockerfile, default citra dasar, dan apakah launchSettings.json ikut terbawa publish."
  docker logs "$NAMA" 2>&1 | tail -40 || true
  exit 1
fi

echo "Permukaan tulis tertutup di citra, dan peti kemasnya terbukti hidup."
