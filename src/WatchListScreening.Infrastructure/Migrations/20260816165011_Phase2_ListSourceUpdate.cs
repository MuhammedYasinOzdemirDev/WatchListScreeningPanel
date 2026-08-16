using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WatchListScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_ListSourceUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastHarvestStatus",
                table: "ListSources",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalRecordsHarvested",
                table: "ListSources",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastHarvestStatus",
                table: "ListSources");

            migrationBuilder.DropColumn(
                name: "TotalRecordsHarvested",
                table: "ListSources");
        }
    }
}
