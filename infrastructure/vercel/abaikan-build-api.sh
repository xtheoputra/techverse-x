#!/usr/bin/env bash
#
# Ignored Build Step proyek Vercel `techverse-x-api` — dipanggil `ignoreCommand` di vercel.json akar.
#
#   kode 0  = LEWATI build (deployment berakhir CANCELED, citra lama tetap melayani)
#   kode 1  = BANGUN       (Vercel membangun untuk kode 1 ATAU LEBIH — galat apa pun berarti bangun)
#
# 🔴 Kenapa ini ada: paket Hobby membatasi registri kontainer Vercel (VCR) 50 citra per repositori,
# dan TANPA berkas ini setiap push ke cabang mana pun membangun dan menyimpan satu citra API —
# termasuk PR yang hanya menyentuh isi/ atau docs/. Dalam tiga hari (2026-10-05 → 2026-10-08)
# repositorinya penuh, dan build produksi merge #94 ditolak dengan "repository has reached the
# maximum allowed number of images". Rinciannya di docs/PENYEBARAN.md.
#
# Jalur di bawah = semua yang dibaca Dockerfile.vercel (baris COPY-nya), ditambah konfigurasi ini
# sendiri. ⚠️ Begitu Dockerfile.vercel menyalin jalur baru, tambahkan jalurnya di sini — kalau tidak,
# perubahan di jalur itu tak pernah sampai ke produksi.
set -u

# 🔴 vercel.json di akar ternyata dibaca JUGA oleh proyek web (Root Directory `apps/web`) — diukur
# 2026-10-08: pratinjau web Error karena perintah relatif tak menemukan skrip ini dari apps/web,
# dan Vercel memperlakukan kode 127 sebagai GAGAL, bukan "bangun". Karena itu ignoreCommand memanggil
# skrip lewat `git rev-parse --show-toplevel`, dan di sini proyek dikenali dari direktori kerjanya:
# hanya proyek API yang Root Directory-nya akar repo. Proyek lain selalu dibangun, seperti sebelumnya.
akar="$(git rev-parse --show-toplevel 2> /dev/null)" || { echo 'abaikan-build-api: bukan repo git — BANGUN'; exit 1; }
if [ "$(cd "$akar" && pwd -P)" != "$(pwd -P)" ]; then
  echo "abaikan-build-api: Root Directory bukan akar repo ($(pwd -P)) — bukan proyek API, BANGUN seperti biasa"
  exit 1
fi

JALUR=(
  Dockerfile.vercel
  apps/api
  services/technology
  packages/contracts
  global.json
  Directory.Build.props
  Directory.Packages.props
  .editorconfig
  .config/dotnet-tools.json
  vercel.json
  infrastructure/vercel/abaikan-build-api.sh
)

# Pembanding: deployment SUKSES terakhir untuk proyek dan cabang ini (variabel ini hanya ada di
# Ignored Build Step). Ia menangkap juga perubahan dari deployment yang GAGAL di antaranya — HEAD^
# saja akan melewatkannya. Vercel meng-clone --depth=10, jadi SHA yang lebih tua bisa tak ada:
# jatuh ke HEAD^ (induk pertama; untuk commit merge = seluruh PR), lalu ke "bangun".
dasar="${VERCEL_GIT_PREVIOUS_SHA:-}"
if [ -n "$dasar" ] && git cat-file -e "${dasar}^{commit}" 2>/dev/null; then
  asal="deployment sukses terakhir ${dasar:0:7}"
elif git rev-parse --verify -q 'HEAD^' > /dev/null; then
  dasar='HEAD^'
  asal='commit induk (HEAD^)'
else
  echo 'abaikan-build-api: tak ada pembanding di clone ini — BANGUN'
  exit 1
fi

if git diff --quiet "$dasar" HEAD -- "${JALUR[@]}"; then
  echo "abaikan-build-api: jalur API tak berubah sejak ${asal} — LEWATI"
  exit 0
fi

echo "abaikan-build-api: jalur API berubah (atau git diff gagal) sejak ${asal} — BANGUN"
git diff --name-only "$dasar" HEAD -- "${JALUR[@]}" 2> /dev/null | head -20
exit 1
