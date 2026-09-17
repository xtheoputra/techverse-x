using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVerseX.TechnologyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RingkasanBidangTanpaRujukanADR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-000000000004"),
                column: "Summary",
                value: "Docker, Kubernetes, tiga hyperscaler, DevOps, dan platform engineering.");

            migrationBuilder.UpdateData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-00000000000c"),
                column: "Summary",
                value: "Solar PV, angin, panas bumi, hidro, bioenergi & SAF, hidrogen hijau, integrasi jaringan & penyimpanan, ekonomi & kebijakan. Fusi nuklir tidak termasuk.");

            migrationBuilder.UpdateData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-00000000000e"),
                column: "Summary",
                value: "Realitas diperluas. Dipertahankan sebagai pintu pencarian, tapi sengaja tidak diinvestasikan.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-000000000004"),
                column: "Summary",
                value: "Docker, Kubernetes, tiga hyperscaler, DevOps, dan platform engineering. Nama lebar dipertahankan dengan sengaja - lihat ADR-010.");

            migrationBuilder.UpdateData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-00000000000c"),
                column: "Summary",
                value: "Solar PV, angin, panas bumi, hidro, bioenergi & SAF, hidrogen hijau, integrasi jaringan & penyimpanan, ekonomi & kebijakan. Fusi TIDAK di sini - lihat ADR-010.");

            migrationBuilder.UpdateData(
                schema: "technology",
                table: "fields",
                keyColumn: "Id",
                keyValue: new Guid("f1e10000-0000-7000-8000-00000000000e"),
                column: "Summary",
                value: "Realitas diperluas. Dipertahankan sebagai pintu pencarian, tapi sengaja tidak diinvestasikan - lihat ADR-010.");
        }
    }
}
