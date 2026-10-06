using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVerseX.TechnologyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RelasiAntarTopik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_technology_relationships_to_technology_id",
                schema: "technology",
                table: "technology_relationships",
                column: "ToTechnologyId");

            migrationBuilder.AddCheckConstraint(
                name: "ck_technology_relationships_bukan_diri_sendiri",
                schema: "technology",
                table: "technology_relationships",
                sql: "\"FromTechnologyId\" <> \"ToTechnologyId\"");

            migrationBuilder.AddCheckConstraint(
                name: "ck_technology_relationships_kind",
                schema: "technology",
                table: "technology_relationships",
                sql: "\"Kind\" IN ('Requires')");

            migrationBuilder.AddForeignKey(
                name: "FK_technology_relationships_technologies_ToTechnologyId",
                schema: "technology",
                table: "technology_relationships",
                column: "ToTechnologyId",
                principalSchema: "technology",
                principalTable: "technologies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_technology_relationships_technologies_ToTechnologyId",
                schema: "technology",
                table: "technology_relationships");

            migrationBuilder.DropIndex(
                name: "ix_technology_relationships_to_technology_id",
                schema: "technology",
                table: "technology_relationships");

            migrationBuilder.DropCheckConstraint(
                name: "ck_technology_relationships_bukan_diri_sendiri",
                schema: "technology",
                table: "technology_relationships");

            migrationBuilder.DropCheckConstraint(
                name: "ck_technology_relationships_kind",
                schema: "technology",
                table: "technology_relationships");
        }
    }
}
