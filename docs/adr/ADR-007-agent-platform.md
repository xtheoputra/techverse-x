# ADR-007 — Orkestrasi agen belum dipilih

**Status:** Ditunda. Issue [#13](../../../../issues/13) masih terbuka.
**Tanggal:** 2026-09-03

## Konteks

`AUDIT-KELAYAKAN.md` B2 menemukan **LangGraph tidak punya SDK .NET**, sehingga rancangan "backend .NET + LangGraph" diam-diam berarti dua runtime, dua bahasa, dua pipeline CI, dan dua permukaan pemindaian kerentanan.

Audit menawarkan empat opsi, dan menandai empat jebakan yang baru terasa setelah dibangun — streaming token lewat dua lompatan, kepemilikan state jadi ganda, identitas bocor di lompatan kedua, dan retry berlapis yang melipatgandakan tagihan.

Alternatif asli .NET yang matang: **Microsoft Agent Framework**.

## Keputusan

Tidak memilih apa pun sekarang, dan **tidak memasang kode AI apa pun** di Fase 1. Tidak ada `apps/ai-gateway`, tidak ada `agents/`, tidak ada panggilan ke penyedia model.

Alasannya: keputusan ini bergantung pada dua issue lain yang juga masih terbuka — peran MCP dan penguncian penyedia AI (Issue [#16](../../../../issues/16)). Menulis satu baris kode agen sekarang berarti mengunci ketiganya diam-diam.

## Konsekuensi

- Direktori `agents/` yang ada di 4.7 sengaja **belum dibuat**. Folder kosong yang menunggu keputusan hanya memberi ilusi kemajuan.
- Yang sudah aman diketahui apa pun keputusannya: SDK C# resmi `ModelContextProtocol` sudah stabil, jadi MCP bukan alasan untuk pindah ke Python.
- Kalau nanti jatuh ke opsi Agent Server LangGraph, audit mengingatkan ada **lisensi LangSmith/Plus/Enterprise** yang mudah terlewat saat prototipe — itu masuk hitungan biaya Issue [#22](../../../../issues/22).
