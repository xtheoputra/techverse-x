using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVerseX.TechnologyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MediaDanOverview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Overview",
                schema: "technology",
                table: "technologies",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "technology_media",
                schema: "technology",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnologyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    VideoId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Alt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Caption = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    License = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technology_media", x => x.Id);
                    table.CheckConstraint("ck_technology_media_alt", "btrim(\"Alt\") <> ''");
                    table.CheckConstraint("ck_technology_media_bentuk", "(\"Kind\" = 'Image' AND \"Url\" IS NOT NULL AND \"VideoId\" IS NULL) OR (\"Kind\" = 'Video' AND \"VideoId\" IS NOT NULL AND \"Url\" IS NULL)");
                    table.CheckConstraint("ck_technology_media_berkas_sendiri", "\"Url\" IS NULL OR \"Url\" LIKE '/media/%'");
                    table.CheckConstraint("ck_technology_media_kind", "\"Kind\" IN ('Image', 'Video')");
                    table.CheckConstraint("ck_technology_media_kunci", "\"Key\" ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("ck_technology_media_lisensi", "btrim(\"License\") <> '' AND (\"License\" = 'Karya sendiri' OR (\"SourceName\" IS NOT NULL AND \"SourceUrl\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_technology_media_technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalSchema: "technology",
                        principalTable: "technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_technology_media_technology_key",
                schema: "technology",
                table: "technology_media",
                columns: new[] { "TechnologyId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "technology_media",
                schema: "technology");

            migrationBuilder.DropColumn(
                name: "Overview",
                schema: "technology",
                table: "technologies");
        }
    }
}
