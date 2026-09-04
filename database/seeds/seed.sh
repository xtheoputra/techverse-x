#!/bin/sh
# Isi contoh untuk pengembangan lokal.
#
# ⚠️ Ini BUKAN kurikulum. Isinya sengaja hanya beberapa entri sekadar supaya
# layar dan endpoint ada isinya. Siapa yang menulis isi sungguhan masih
# Issue #10, dan peta teknologinya sendiri masih menunggu koreksi audit
# (Issue #18) — menaruh isi "sungguhan" sekarang berarti menyemai data usang.

set -e

API="${API_BASE_URL:-http://localhost:5080}"

echo "Mengisi contoh ke $API ..."

seed() {
  code=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$API/api/v1/technologies" \
    -H "Content-Type: application/json" \
    -d "$1")
  case "$code" in
    201) echo "  dibuat  $2" ;;
    409) echo "  ada     $2" ;;
    *)   echo "  GAGAL   $2 (HTTP $code)" ;;
  esac
}

seed '{"name":"AI Agents","summary":"Pergeseran dari AI yang menjawab ke AI yang mengerjakan.","fieldSlug":"ai-agents"}' "AI Agents"
seed '{"name":"Edge AI","summary":"AI berjalan langsung di perangkat, bukan di awan.","fieldSlug":"edge-ai"}' "Edge AI"
seed '{"name":"Quantum Computing","summary":"Era qubit logis; keunggulan komersial belum ada.","fieldSlug":"quantum-computing"}' "Quantum Computing"
seed '{"name":"Digital Twin","summary":"Kembaran digital dari objek atau sistem nyata.","fieldSlug":"iot"}' "Digital Twin"
seed '{"name":"Cybersecurity Berbasis AI","summary":"Ancaman otomatis, maka pertahanannya ikut otomatis.","fieldSlug":"cybersecurity"}' "Cybersecurity Berbasis AI"

echo "Selesai."
