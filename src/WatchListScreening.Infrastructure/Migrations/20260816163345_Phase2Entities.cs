using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchListScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScraperClassName",
                table: "ListSources",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "HarvestedEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityType",
                table: "HarvestedEntries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScraperClassName",
                table: "ListSources");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "HarvestedEntries");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "HarvestedEntries");
        }
    }
}
