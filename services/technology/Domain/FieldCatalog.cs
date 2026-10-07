using System.Collections.ObjectModel;

namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Bidang-bidang TechVerse X, apa adanya dari ADR-010 (dan Pembaruan 2026-10-07-nya).
/// </summary>
/// <remarks>
/// Ini <b>satu-satunya sumber</b> daftar bidang. Migrasi menyemai tabel dari sini
/// lewat <c>HasData</c>, dan uji membandingkan isinya dengan angka di ADR-010 —
/// jadi daftar ini tidak bisa hanyut dari dokumennya tanpa ada yang merah.
/// <para>
/// Id-nya tetap dan ditulis eksplisit, bukan dibangkitkan. <c>HasData</c> menuntut
/// nilai yang sama persis di setiap kali model dibangun; Id acak akan membuat EF
/// mengira barisnya berubah dan melahirkan migrasi baru tiap kali build.
/// </para>
/// </remarks>
public static class FieldCatalog
{
    /// <summary>Jumlah bidang menurut ADR-010. Dipakai uji sebagai penjaga.</summary>
    public const int ExpectedCount = 17;

    // Sampai 2026-09-28 di sini ada CoreTopicCount = 42, yang mengaku dipakai
    // docs/RENCANA-V1.md untuk mengukur kemajuan. Nol rujukan di kode, uji, maupun
    // dokumen - RENCANA-V1 menulis 42 sebagai prosa (#68). Angka tanpa pembaca yang
    // mengaku punya pembaca hanya bisa hanyut, jadi ia dibuang; 42 tinggal di tabel
    // ADR-010, satu-satunya tempat yang benar-benar menghitungnya.

    // 🔴 Ringkasan di sini TEKS PEMBACA, bukan catatan redaksi. Ia tercetak di
    // kartu-kartu halaman muka, di kepala halaman bidang, dan di hasil
    // /cari - dan selama produksi nol topik, kartu-kartu inilah isi utama situsnya.
    // Sampai 2026-09-17 tiga di antaranya menyuruh pengunjung "lihat ADR-010",
    // dokumen di repositori privat yang tidak bisa mereka buka. Ketahuannya dari
    // memindai teks yang TERLIHAT di build produksi, bukan dari membaca kode web:
    // teksnya datang dari basis data (ADR-024). Alasan redaksinya tetap di ADR-010.
    //
    // ⚠️ Mengubah ringkasan menuntut migrasi - HasData menyemai tabel dari daftar
    // ini, dan ModelMigrasiTests memerah kalau migrasinya lupa dibuat.
    private static readonly ReadOnlyCollection<Field> Items = new(
    [
        // Prioritas 1 — ditulis manusia sampai tuntas. 7+7+6+7+7+8 = 42 topik.
        Make("f1e10000-0000-7000-8000-000000000001", "AI & Machine Learning", "Fondasi: ML klasik, deep learning, LLM, computer vision, NLP, reinforcement learning, multimodal.", FieldPriority.Core, 1),
        Make("f1e10000-0000-7000-8000-000000000002", "AI Agents", "Lapisan orkestrasi: tool use, MCP, A2A, memori agen, evals, keamanan agen, human-in-the-loop.", FieldPriority.Core, 2),
        Make("f1e10000-0000-7000-8000-000000000003", "Cybersecurity", "Ethical hacking, SOC, malware, reverse engineering, keamanan awan, dan keamanan AI.", FieldPriority.Core, 3),
        Make("f1e10000-0000-7000-8000-000000000004", "Cloud & Infrastructure", "Docker, Kubernetes, tiga hyperscaler, DevOps, dan platform engineering.", FieldPriority.Core, 4),
        Make("f1e10000-0000-7000-8000-000000000005", "Data Engineering", "Lapisan yang menentukan proyek AI hidup atau mati: ingestion, orkestrasi, lakehouse, format tabel terbuka, streaming, kontrak data, pemodelan.", FieldPriority.Core, 5),
        Make("f1e10000-0000-7000-8000-000000000006", "IoT", "Konektivitas, firmware & RTOS, Matter/Thread, gateway tepi, keamanan perangkat, IIoT, telemetri deret waktu, dan satu halaman jembatan TinyML.", FieldPriority.Core, 6),

        // Prioritas 2 — lahir sebagai draf.
        Make("f1e10000-0000-7000-8000-000000000007", "Edge AI", "AI yang berjalan di perangkat: kuantisasi, distilasi, NPU, runtime on-device. Bidang sendiri, BUKAN anak IoT - tinyML Foundation sendiri sudah berganti nama jadi Edge AI Foundation.", FieldPriority.Supporting, 7),
        Make("f1e10000-0000-7000-8000-000000000008", "Robotics", "ROS2, humanoid, drone, kendaraan otonom, dan Robotics AI yang pindah ke sini dari AI & ML.", FieldPriority.Supporting, 8),
        Make("f1e10000-0000-7000-8000-000000000009", "Quantum Computing", "Qubit, Qiskit, algoritma kuantum, kriptografi kuantum. Era qubit logis; keunggulan komersial belum ada.", FieldPriority.Supporting, 9),
        Make("f1e10000-0000-7000-8000-00000000000a", "Biotechnology", "CRISPR, AlphaFold, biologi sintetis, kesehatan digital.", FieldPriority.Supporting, 10),
        Make("f1e10000-0000-7000-8000-00000000000b", "Blockchain", "Smart contract, Ethereum, Solana, Layer 2, DeFi.", FieldPriority.Supporting, 11),
        Make("f1e10000-0000-7000-8000-00000000000c", "Renewable Energy", "Solar PV, angin, panas bumi, hidro, bioenergi & SAF, hidrogen hijau, integrasi jaringan & penyimpanan, ekonomi & kebijakan. Fusi nuklir tidak termasuk.", FieldPriority.Supporting, 12),

        // Prioritas 3 — kurasi tautan saja.
        Make("f1e10000-0000-7000-8000-00000000000d", "Space Technology", "Konektivitas LEO, akses ke orbit, smallsat, segmen darat, observasi Bumi, GNSS/PNT, keselamatan orbit. Irisan terkuatnya: 3GPP NTN.", FieldPriority.Peripheral, 13),
        Make("f1e10000-0000-7000-8000-00000000000e", "XR (AR/VR/MR)", "Realitas diperluas. Dipertahankan sebagai pintu pencarian, tapi sengaja tidak diinvestasikan.", FieldPriority.Peripheral, 14, slug: "xr"),

        // Tiga bidang Deep Tech yang ditambahkan pemilik 2026-10-07 (ADR-010 Pembaruan
        // 2026-10-07), diletakkan di bawah XR sesuai permintaannya. Perhatikan bahwa urutan
        // tampil KINI tak sepenuhnya menurut prioritas: Advanced Computing & Hardware
        // berprioritas 2 tetapi duduk sesudah XR yang berprioritas 3. Itu akibat
        // penempatan, bukan kelalaian — memindahkannya ke atas adalah migrasi data kecil.
        Make("f1e10000-0000-7000-8000-00000000000f", "Advanced Computing & Hardware", "Arsitektur mikro dan komputasi masa depan: desain semikonduktor, RISC-V, neuromorphic computing, fotonika, dan DNA data storage.", FieldPriority.Supporting, 15),
        Make("f1e10000-0000-7000-8000-000000000010", "Neurotechnology & BCI", "Antarmuka langsung manusia-mesin: Brain-Computer Interface (BCI) invasif dan non-invasif, neuroprostetika, EEG signal processing, dan implan saraf.", FieldPriority.Peripheral, 16),
        // ⚠️ "Suhu kamar" TIDAK ditulis sebagai bahan yang ada: belum ada superkonduktor suhu
        // kamar bertekanan atmosfer yang terbukti (klaim LK-99 2023 gugur oleh replikasi
        // independen; senyawa hidrida mencatat suhu kritis tinggi hanya di tekanan jutaan
        // atmosfer). Teks ini tercetak di kartu publik, dan sebuah uji menjaganya.
        // Catatannya sengaja di bagian AWAL kalimat: kartu halaman muka memotong ringkasan
        // di tiga baris, dan versi pertama yang meletakkannya di tengah terpotong persis di
        // "yang belum…" — ketahuan saat kartunya DILIHAT, bukan dari membaca kode.
        Make("f1e10000-0000-7000-8000-000000000011", "Advanced Materials & Nanotech", "Landasan fisik deep tech: grafena, metamaterial, superkonduktor (suhu kamar belum terbukti), dan rekayasa material skala nano untuk baterai dan antariksa.", FieldPriority.Peripheral, 17),
    ]);

    /// <summary>Seluruh bidang, dalam urutan tampil.</summary>
    public static IReadOnlyList<Field> All => Items;

    private static Field Make(string id, string name, string summary, FieldPriority priority, int displayOrder, string? slug = null)
        => Field.Create(Guid.Parse(id), name, summary, priority, displayOrder, slug);
}
