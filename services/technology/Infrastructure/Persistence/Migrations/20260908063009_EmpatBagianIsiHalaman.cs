using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVerseX.TechnologyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EmpatBagianIsiHalaman : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                schema: "technology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnologyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Brief = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projects_technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalSchema: "technology",
                        principalTable: "technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                schema: "technology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnologyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_resources_technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalSchema: "technology",
                        principalTable: "technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roadmap_steps",
                schema: "technology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnologyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roadmap_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_roadmap_steps_technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalSchema: "technology",
                        principalTable: "technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tools",
                schema: "technology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Homepage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "technology_tools",
                schema: "technology",
                columns: table => new
                {
                    TechnologyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technology_tools", x => new { x.TechnologyId, x.ToolId });
                    table.ForeignKey(
                        name: "FK_technology_tools_technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalSchema: "technology",
                        principalTable: "technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_technology_tools_tools_ToolId",
                        column: x => x.ToolId,
                        principalSchema: "technology",
                        principalTable: "tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_projects_TechnologyId",
                schema: "technology",
                table: "projects",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "ix_resources_technology_type",
                schema: "technology",
                table: "resources",
                columns: new[] { "TechnologyId", "Type" });

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_steps_technology_order",
                schema: "technology",
                table: "roadmap_steps",
                columns: new[] { "TechnologyId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_technology_tools_tool_id",
                schema: "technology",
                table: "technology_tools",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "ix_tools_slug",
                schema: "technology",
                table: "tools",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "projects",
                schema: "technology");

            migrationBuilder.DropTable(
                name: "resources",
                schema: "technology");

            migrationBuilder.DropTable(
                name: "roadmap_steps",
                schema: "technology");

            migrationBuilder.DropTable(
                name: "technology_tools",
                schema: "technology");

            migrationBuilder.DropTable(
                name: "tools",
                schema: "technology");
        }
    }
}
