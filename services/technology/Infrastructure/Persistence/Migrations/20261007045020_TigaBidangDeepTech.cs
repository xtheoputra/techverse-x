using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechVerseX.TechnologyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TigaBidangDeepTech : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "technology",
                table: "fields",
                columns: new[] { "Id", "DisplayOrder", "Name", "Priority", "Slug", "Summary" },
                values: new object[,]
                {
                    { new Guid("f1e10000-0000-7000-8000-00000000000f"), 15, "Advanced Computing & Hardware", "Supporting", "advanced-computing-hardware", "Arsitektur mikro dan komputasi masa depan: desain semikonduktor, RISC-V, neuromorphic computing, fotonika, dan DNA data storage." },
                    { new Guid("f1e10000-0000-7000-8000-000000000010"), 16, "Neurotechnology & BCI", "Peripheral", "neurotechnology-bci", "Antarmuka langsung manusia-mesin: Brain-Computer Interface (BCI) invasif dan non-invasif, neuroprostetika, EEG signal processing, dan implan saraf." },
                    { new Guid("f1e10000-0000-7000-8000-000000000011"), 17, "Advanced Materials & Nanotech", "Peripheral", "advanced-materials-nanotech", "Landasan fisik deep tech: grafena, metamaterial, superkonduktor (suhu kamar belum terbukti), dan rekayasa material skala nano untuk baterai dan antariksa." }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-00000000000f"));

            migrationBuilder.DeleteData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-000000000010"));

            migrationBuilder.DeleteData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-000000000011"));
        }
    }
}
