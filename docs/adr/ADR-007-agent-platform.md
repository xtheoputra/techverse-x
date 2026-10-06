# ADR-007 — Orkestrasi agen belum dipilih

**Status:** Diterima. Issue [#13](https://github.com/xtheoputra/techverse-x/issues/13) ditutup 2026-09-04: **Microsoft Agent Framework**.
**Tanggal:** 2026-09-03

## Konteks

`AUDIT-KELAYAKAN.md` B2 menemukan **LangGraph tidak punya SDK .NET**, sehingga rancangan "backend .NET + LangGraph" diam-diam berarti dua runtime, dua bahasa, dua pipeline CI, dan dua permukaan pemindaian kerentanan.

Audit menawarkan empat opsi, dan menandai empat jebakan yang baru terasa setelah dibangun — streaming token lewat dua lompatan, kepemilikan state jadi ganda, identitas bocor di lompatan kedua, dan retry berlapis yang melipatgandakan tagihan.

Alternatif asli .NET yang matang: **Microsoft Agent Framework**.

## Keputusan

Tidak memilih apa pun sekarang, dan **tidak memasang kode AI apa pun** di Fase 1. Tidak ada `apps/ai-gateway`, tidak ada `agents/`, tidak ada panggilan ke penyedia model.

Alasannya: keputusan ini bergantung pada dua issue lain yang juga masih terbuka — peran MCP dan penguncian penyedia AI (Issue [#16](https://github.com/xtheoputra/techverse-x/issues/16)). Menulis satu baris kode agen sekarang berarti mengunci ketiganya diam-diam.

## Konsekuensi

- Direktori `agents/` yang ada di 4.7 sengaja **belum dibuat**. Folder kosong yang menunggu keputusan hanya memberi ilusi kemajuan.
- Yang sudah aman diketahui apa pun keputusannya: SDK C# resmi `ModelContextProtocol` sudah stabil, jadi MCP bukan alasan untuk pindah ke Python.
- Kalau nanti jatuh ke opsi Agent Server LangGraph, audit mengingatkan ada **lisensi LangSmith/Plus/Enterprise** yang mudah terlewat saat prototipe — itu masuk hitungan biaya Issue [#22](https://github.com/xtheoputra/techverse-x/issues/22).

---

## Pembaruan 2026-09-04 - keputusan diambil

**Opsi 1: buang LangGraph, pakai Microsoft Agent Framework asli .NET.**

Alasan utamanya bukan perbandingan fitur, melainkan siapa yang mengerjakan proyek
ini. Untuk pengembang tunggal, **dua runtime adalah kesalahan termahal yang bisa
diambil tanpa terasa** - ia menggandakan CI, permukaan pemindaian, jalur
penyebaran, dan tagihan, demi orkestrator yang punya padanan asli di .NET.

Empat jebakan yang audit tandai semuanya lahir dari lompatan kedua itu, dan
semuanya hilang bersama keputusan ini: streaming SSE dua lompatan, kepemilikan
state ganda (LangGraph menyimpan thread sendiri sementara EF Core punya tabel
percakapan), identitas bocor saat .NET memanggil Python dengan satu API key
layanan, dan retry berlapis yang mengubah satu kegagalan sementara jadi delapan
panggilan berbayar.

Dua hal yang membuat opsi ini bukan sekadar "yang paling mudah":

- `Microsoft.Agents.AI` adalah **penerus resmi Semantic Kernel dan AutoGen** -
  jadi ia menyambung langsung dengan koreksi kurikulum di
  [ADR-012](ADR-012-template-halaman.md), yang harus mencoret AutoGen dari peta.
- **MCP bukan alasan memilih Python**: SDK C# resmi `ModelContextProtocol` stabil
  sejak 1.0.0 (25 Februari 2026).

Opsi 3 (LangGraph Agent Server) ditolak dengan alasan tambahan yang mudah
terlewat saat prototipe: ia terikat lisensi LangSmith/Plus/Enterprise - biaya
yang tidak muncul sampai produk dipakai orang.

Keputusan ini juga membebaskan [ADR-014](ADR-014-mcp-dan-penyedia-ai.md): Agent
Framework mendukung OpenAI, Anthropic, dan Ollama sekaligus, jadi "jangan dikunci
ke satu penyedia" tidak menuntut lapisan abstraksi buatan sendiri.

**Kode AI tetap belum ditulis di Fase 1** - yang berubah cuma bahwa pilihannya
sudah tidak menggantung.
