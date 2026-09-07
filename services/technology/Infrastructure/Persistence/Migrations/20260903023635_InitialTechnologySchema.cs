using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVerseX.TechnologyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialTechnologySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "technology");

            migrationBuilder.CreateTable(
                name: "technologies",
                schema: "technology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technologies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "technology_relationships",
                schema: "technology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromTechnologyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToTechnologyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technology_relationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_technology_relationships_technologies_FromTechnologyId",
                        column: x => x.FromTechnologyId,
                        principalSchema: "technology",
                        principalTable: "technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_technologies_category",
                schema: "technology",
                table: "technologies",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "ix_technologies_slug",
                schema: "technology",
                table: "technologies",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_technology_relationships_edge",
                schema: "technology",
                table: "technology_relationships",
                columns: new[] { "FromTechnologyId", "ToTechnologyId", "Kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "technology_relationships",
                schema: "technology");

            migrationBuilder.DropTable(
                name: "technologies",
                schema: "technology");
        }
    }
}
