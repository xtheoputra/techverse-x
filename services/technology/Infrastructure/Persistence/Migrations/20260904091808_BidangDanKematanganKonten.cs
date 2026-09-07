using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechVerseX.TechnologyService.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Kategori teks bebas naik jadi entitas <c>fields</c>, dan kematangan konten
    /// (ADR-012) mendapat kolomnya sendiri.
    /// </summary>
    /// <remarks>
    /// ⚠️ <b>Urutan operasi di <c>Up</c> ditulis tangan, bukan hasil scaffold apa
    /// adanya.</b> Yang di-scaffold membuang kolom <c>Category</c> LEBIH DULU lalu
    /// menambahkan <c>FieldId</c> berisi Guid kosong — yang berarti setiap baris
    /// lama kehilangan kategorinya dan langsung melanggar kunci asing.
    /// <para>
    /// Urutan yang benar: semai bidang → tambah kolom yang boleh kosong → isi dari
    /// kategori lama → <b>berhenti dengan pesan yang jelas kalau ada yang tidak
    /// terpetakan</b> → baru wajibkan dan buang kolom lama.
    /// </para>
    /// </remarks>
    public partial class BidangDanKematanganKonten : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            // ── 1. Bidang dibuat dan disemai lebih dulu ──────────────────────────
            migrationBuilder.CreateTable(
                name: "fields",
                schema: "technology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fields", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "technology",
                table: "fields",
                columns: new[] { "Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary" },
                values: new object[,]
                {
                    { new Guid("f1e10000-0000-7000-8000-000000000001"), 1, "AI & Machine Learning", "Core", "ai-machine-learning", "Fondasi: ML klasik, deep learning, LLM, computer vision, NLP, reinforcement learning, multimodal." },
                    { new Guid("f1e10000-0000-7000-8000-000000000002"), 2, "AI Agents", "Core", "ai-agents", "Lapisan orkestrasi: tool use, MCP, A2A, memori agen, evals, keamanan agen, human-in-the-loop." },
                    { new Guid("f1e10000-0000-7000-8000-000000000003"), 3, "Cybersecurity", "Core", "cybersecurity", "Ethical hacking, SOC, malware, reverse engineering, keamanan awan, dan keamanan AI." },
                    { new Guid("f1e10000-0000-7000-8000-000000000004"), 4, "Cloud & Infrastructure", "Core", "cloud-infrastructure", "Docker, Kubernetes, tiga hyperscaler, DevOps, dan platform engineering. Nama lebar dipertahankan dengan sengaja - lihat ADR-010." },
                    { new Guid("f1e10000-0000-7000-8000-000000000005"), 5, "Data Engineering", "Core", "data-engineering", "Lapisan yang menentukan proyek AI hidup atau mati: ingestion, orkestrasi, lakehouse, format tabel terbuka, streaming, kontrak data, pemodelan." },
                    { new Guid("f1e10000-0000-7000-8000-000000000006"), 6, "IoT", "Core", "iot", "Konektivitas, firmware & RTOS, Matter/Thread, gateway tepi, keamanan perangkat, IIoT, telemetri deret waktu, dan satu halaman jembatan TinyML." },
                    { new Guid("f1e10000-0000-7000-8000-000000000007"), 7, "Edge AI", "Supporting", "edge-ai", "AI yang berjalan di perangkat: kuantisasi, distilasi, NPU, runtime on-device. Bidang sendiri, BUKAN anak IoT - tinyML Foundation sendiri sudah berganti nama jadi Edge AI Foundation." },
                    { new Guid("f1e10000-0000-7000-8000-000000000008"), 8, "Robotics", "Supporting", "robotics", "ROS2, humanoid, drone, kendaraan otonom, dan Robotics AI yang pindah ke sini dari AI & ML." },
                    { new Guid("f1e10000-0000-7000-8000-000000000009"), 9, "Quantum Computing", "Supporting", "quantum-computing", "Qubit, Qiskit, algoritma kuantum, kriptografi kuantum. Era qubit logis; keunggulan komersial belum ada." },
                    { new Guid("f1e10000-0000-7000-8000-00000000000a"), 10, "Biotechnology", "Supporting", "biotechnology", "CRISPR, AlphaFold, biologi sintetis, kesehatan digital." },
                    { new Guid("f1e10000-0000-7000-8000-00000000000b"), 11, "Blockchain", "Supporting", "blockchain", "Smart contract, Ethereum, Solana, Layer 2, DeFi." },
                    { new Guid("f1e10000-0000-7000-8000-00000000000c"), 12, "Renewable Energy", "Supporting", "renewable-energy", "Solar PV, angin, panas bumi, hidro, bioenergi & SAF, hidrogen hijau, integrasi jaringan & penyimpanan, ekonomi & kebijakan. Fusi TIDAK di sini - lihat ADR-010." },
                    { new Guid("f1e10000-0000-7000-8000-00000000000d"), 13, "Space Technology", "Peripheral", "space-technology", "Konektivitas LEO, akses ke orbit, smallsat, segmen darat, observasi Bumi, GNSS/PNT, keselamatan orbit. Irisan terkuatnya: 3GPP NTN." },
                    { new Guid("f1e10000-0000-7000-8000-00000000000e"), 14, "XR (AR/VR/MR)", "Peripheral", "xr", "Realitas diperluas. Dipertahankan sebagai pintu pencarian, tapi sengaja tidak diinvestasikan - lihat ADR-010." }
                });

            migrationBuilder.CreateIndex(
                name: "ix_fields_display_order",
                schema: "technology",
                table: "fields",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fields_name",
                schema: "technology",
                table: "fields",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fields_slug",
                schema: "technology",
                table: "fields",
                column: "Slug",
                unique: true);

            // ── 2. Kolom baru ditambahkan dalam keadaan boleh kosong ─────────────
            migrationBuilder.AddColumn<Guid>(
                name: "FieldId",
                schema: "technology",
                table: "technologies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Maturity",
                schema: "technology",
                table: "technologies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                schema: "technology",
                table: "technologies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedBy",
                schema: "technology",
                table: "technologies",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            // ── 3. Baris lama dipetakan dari kategori teksnya ────────────────────
            migrationBuilder.Sql(@"
                UPDATE technology.technologies AS t
                SET ""FieldId"" = f.""Id""
                FROM technology.fields AS f
                WHERE t.""Category"" = f.""Name"";");

            // Semua baris lama lahir sebagai kurasi. Itu memang tingkat terendah
            // di ADR-012, dan menaikkannya menuntut tindakan manusia.
            migrationBuilder.Sql(@"
                UPDATE technology.technologies
                SET ""Maturity"" = 'Curated'
                WHERE ""Maturity"" IS NULL;");

            // ── 4. Berhenti dengan pesan yang bisa ditindaklanjuti ───────────────
            //
            // Tanpa blok ini, yang muncul cuma pesan PostgreSQL "column contains
            // null values" dari langkah berikutnya - benar, tapi tidak memberi
            // tahu apa pun tentang penyebabnya maupun jalan keluarnya.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE tersisa text;
                BEGIN
                    SELECT string_agg(DISTINCT ""Category"", ', ')
                    INTO tersisa
                    FROM technology.technologies
                    WHERE ""FieldId"" IS NULL;

                    IF tersisa IS NOT NULL THEN
                        RAISE EXCEPTION
                            'Migrasi berhenti: kategori lama berikut bukan salah satu dari 14 bidang ADR-010: %. Perbaiki kategorinya dulu, atau kalau ini basis data pengembangan berisi contoh saja, jalankan `run.ps1 reset` lalu migrate ulang.',
                            tersisa;
                    END IF;
                END $$;");

            // ── 5. Baru sekarang kolomnya diwajibkan ────────────────────────────
            migrationBuilder.AlterColumn<Guid>(
                name: "FieldId",
                schema: "technology",
                table: "technologies",
                type: "uuid",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Maturity",
                schema: "technology",
                table: "technologies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false);

            // ── 6. Kolom lama dibuang paling akhir ──────────────────────────────
            migrationBuilder.DropIndex(
                name: "ix_technologies_category",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "technology",
                table: "technologies");

            // ── 7. Indeks dan kunci asing ───────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "ix_technologies_field_id",
                schema: "technology",
                table: "technologies",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "ix_technologies_field_maturity",
                schema: "technology",
                table: "technologies",
                columns: new[] { "FieldId", "Maturity" });

            migrationBuilder.AddForeignKey(
                name: "FK_technologies_fields_FieldId",
                schema: "technology",
                table: "technologies",
                column: "FieldId",
                principalSchema: "technology",
                principalTable: "fields",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            ArgumentNullException.ThrowIfNull(migrationBuilder);

            // Kebalikannya juga ditulis tangan: kategori dikembalikan dari nama
            // bidang SEBELUM tabel bidang dibuang, kalau tidak namanya hilang
            // bersama tabelnya dan turun-versi berarti kehilangan data.
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "technology",
                table: "technologies",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE technology.technologies AS t
                SET ""Category"" = f.""Name""
                FROM technology.fields AS f
                WHERE t.""FieldId"" = f.""Id"";");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                schema: "technology",
                table: "technologies",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "ix_technologies_category",
                schema: "technology",
                table: "technologies",
                column: "Category");

            migrationBuilder.DropForeignKey(
                name: "FK_technologies_fields_FieldId",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropIndex(
                name: "ix_technologies_field_id",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropIndex(
                name: "ix_technologies_field_maturity",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropTable(
                name: "fields",
                schema: "technology");

            migrationBuilder.DropColumn(
                name: "FieldId",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropColumn(
                name: "Maturity",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                schema: "technology",
                table: "technologies");
        }
    }
}
