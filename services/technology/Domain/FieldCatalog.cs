using System.Collections.ObjectModel;

namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Empat belas bidang TechVerse X, apa adanya dari ADR-010.
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
    public const int ExpectedCount = 14;

    /// <summary>
    /// Jumlah topik yang dijanjikan ditulis manusia — enam bidang
    /// <see cref="FieldPriority.Core"/>, 42 topik. Angka ini yang dipakai
    /// <c>docs/RENCANA-V1.md</c> untuk mengukur kemajuan.
    /// </summary>
    public const int CoreTopicCount = 42;

    private static readonly ReadOnlyCollection<Field> Items = new(
    [
        // Prioritas 1 — ditulis manusia sampai tuntas. 7+7+6+7+7+8 = 42 topik.
        Make("f1e10000-0000-7000-8000-000000000001", "AI & Machine Learning", "Fondasi: ML klasik, deep learning, LLM, computer vision, NLP, reinforcement learning, multimodal.", FieldPriority.Core, 1),
        Make("f1e10000-0000-7000-8000-000000000002", "AI Agents", "Lapisan orkestrasi: tool use, MCP, A2A, memori agen, evals, keamanan agen, human-in-the-loop.", FieldPriority.Core, 2),
        Make("f1e10000-0000-7000-8000-000000000003", "Cybersecurity", "Ethical hacking, SOC, malware, reverse engineering, keamanan awan, dan keamanan AI.", FieldPriority.Core, 3),
        Make("f1e10000-0000-7000-8000-000000000004", "Cloud & Infrastructure", "Docker, Kubernetes, tiga hyperscaler, DevOps, dan platform engineering. Nama lebar dipertahankan dengan sengaja - lihat ADR-010.", FieldPriority.Core, 4),
        Make("f1e10000-0000-7000-8000-000000000005", "Data Engineering", "Lapisan yang menentukan proyek AI hidup atau mati: ingestion, orkestrasi, lakehouse, format tabel terbuka, streaming, kontrak data, pemodelan.", FieldPriority.Core, 5),
        Make("f1e10000-0000-7000-8000-000000000006", "IoT", "Konektivitas, firmware & RTOS, Matter/Thread, gateway tepi, keamanan perangkat, IIoT, telemetri deret waktu, dan satu halaman jembatan TinyML.", FieldPriority.Core, 6),

        // Prioritas 2 — lahir sebagai draf.
        Make("f1e10000-0000-7000-8000-000000000007", "Edge AI", "AI yang berjalan di perangkat: kuantisasi, distilasi, NPU, runtime on-device. Bidang sendiri, BUKAN anak IoT - tinyML Foundation sendiri sudah berganti nama jadi Edge AI Foundation.", FieldPriority.Supporting, 7),
        Make("f1e10000-0000-7000-8000-000000000008", "Robotics", "ROS2, humanoid, drone, kendaraan otonom, dan Robotics AI yang pindah ke sini dari AI & ML.", FieldPriority.Supporting, 8),
        Make("f1e10000-0000-7000-8000-000000000009", "Quantum Computing", "Qubit, Qiskit, algoritma kuantum, kriptografi kuantum. Era qubit logis; keunggulan komersial belum ada.", FieldPriority.Supporting, 9),
        Make("f1e10000-0000-7000-8000-00000000000a", "Biotechnology", "CRISPR, AlphaFold, biologi sintetis, kesehatan digital.", FieldPriority.Supporting, 10),
        Make("f1e10000-0000-7000-8000-00000000000b", "Blockchain", "Smart contract, Ethereum, Solana, Layer 2, DeFi.", FieldPriority.Supporting, 11),
        Make("f1e10000-0000-7000-8000-00000000000c", "Renewable Energy", "Solar PV, angin, panas bumi, hidro, bioenergi & SAF, hidrogen hijau, integrasi jaringan & penyimpanan, ekonomi & kebijakan. Fusi TIDAK di sini - lihat ADR-010.", FieldPriority.Supporting, 12),

        // Prioritas 3 — kurasi tautan saja.
        Make("f1e10000-0000-7000-8000-00000000000d", "Space Technology", "Konektivitas LEO, akses ke orbit, smallsat, segmen darat, observasi Bumi, GNSS/PNT, keselamatan orbit. Irisan terkuatnya: 3GPP NTN.", FieldPriority.Peripheral, 13),
        Make("f1e10000-0000-7000-8000-00000000000e", "XR (AR/VR/MR)", "Realitas diperluas. Dipertahankan sebagai pintu pencarian, tapi sengaja tidak diinvestasikan - lihat ADR-010.", FieldPriority.Peripheral, 14, slug: "xr"),
    ]);

    /// <summary>Keempat belas bidang, dalam urutan tampil.</summary>
    public static IReadOnlyList<Field> All => Items;

    private static Field Make(string id, string name, string summary, FieldPriority priority, int displayOrder, string? slug = null)
        => Field.Create(Guid.Parse(id), name, summary, priority, displayOrder, slug);
}
