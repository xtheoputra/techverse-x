using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace TechVerseX.TechnologyService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PencarianTeksPenuh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "technology",
                table: "technologies",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "setweight(to_tsvector('english', coalesce(\"Name\", '')), 'A') || setweight(to_tsvector('english', coalesce(\"Summary\", '')), 'B')",
                stored: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "search_vector",
                schema: "technology",
                table: "fields",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "setweight(to_tsvector('english', coalesce(\"Name\", '')), 'A') || setweight(to_tsvector('english', coalesce(\"Summary\", '')), 'B')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_technologies_search_vector",
                schema: "technology",
                table: "technologies",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_fields_search_vector",
                schema: "technology",
                table: "fields",
                column: "search_vector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_technologies_search_vector",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropIndex(
                name: "ix_fields_search_vector",
                schema: "technology",
                table: "fields");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "technology",
                table: "technologies");

            migrationBuilder.DropColumn(
                name: "search_vector",
                schema: "technology",
                table: "fields");
        }
    }
}
