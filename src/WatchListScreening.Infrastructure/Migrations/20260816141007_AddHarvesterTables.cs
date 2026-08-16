using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WatchListScreening.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHarvesterTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScreeningRequests_RequestedAt",
                table: "ScreeningRequests");

            migrationBuilder.DropIndex(
                name: "IX_SanctionEntries_ListSource",
                table: "SanctionEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_PerformedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "RequestedAt",
                table: "ScreeningRequests");

            migrationBuilder.DropColumn(
                name: "ListSource",
                table: "SanctionEntries");

            migrationBuilder.DropColumn(
                name: "ListSourceUrl",
                table: "SanctionEntries");

            migrationBuilder.DropColumn(
                name: "PerformedAt",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ScreeningResults",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReviewedAt",
                table: "ScreeningResults",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ScreeningResults",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ScreeningRequests",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ScreeningRequests",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "ScreeningRequests",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SanctionEntries",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeactivatedAt",
                table: "SanctionEntries",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SanctionEntries",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddedAt",
                table: "SanctionEntries",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "ListSourceId",
                table: "SanctionEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "AuditLogs",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuditLogs",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "ListSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    ScraperType = table.Column<int>(type: "integer", nullable: false),
                    ScraperConfig = table.Column<string>(type: "text", nullable: true),
                    CronExpression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HangfireJobId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 120),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    HasScraperImpl = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastHarvestAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ListSourceRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListSourceId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TriggeredBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    TotalScraped = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalNew = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalUpdated = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalSkipped = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListSourceRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListSourceRuns_ListSources_ListSourceId",
                        column: x => x.ListSourceId,
                        principalTable: "ListSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HarvestedEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListSourceId = table.Column<int>(type: "integer", nullable: false),
                    ListSourceRunId = table.Column<int>(type: "integer", nullable: false),
                    SanctionEntryId = table.Column<int>(type: "integer", nullable: true),
                    RawFullName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RawFirstName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RawLastName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RawCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CleanedFullName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CleanedFirstName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CleanedLastName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DateOfBirth = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NationalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Aliases = table.Column<string>(type: "text", nullable: true),
                    Passports = table.Column<string>(type: "text", nullable: true),
                    Addresses = table.Column<string>(type: "text", nullable: true),
                    Positions = table.Column<string>(type: "text", nullable: true),
                    AdditionalData = table.Column<string>(type: "text", nullable: true),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HarvestedEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HarvestedEntries_ListSourceRuns_ListSourceRunId",
                        column: x => x.ListSourceRunId,
                        principalTable: "ListSourceRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HarvestedEntries_ListSources_ListSourceId",
                        column: x => x.ListSourceId,
                        principalTable: "ListSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HarvestedEntries_SanctionEntries_SanctionEntryId",
                        column: x => x.SanctionEntryId,
                        principalTable: "SanctionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 1,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 2,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 3,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 4,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 5,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 6,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 7,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 8,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 9,
                column: "ListSourceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 10,
                column: "ListSourceId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_SanctionEntries_ListSourceId",
                table: "SanctionEntries",
                column: "ListSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestedEntries_CleanedFullName",
                table: "HarvestedEntries",
                column: "CleanedFullName");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestedEntries_ContentHash_UNIQUE",
                table: "HarvestedEntries",
                column: "ContentHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HarvestedEntries_IsProcessed",
                table: "HarvestedEntries",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestedEntries_ListSourceId",
                table: "HarvestedEntries",
                column: "ListSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestedEntries_ListSourceRunId",
                table: "HarvestedEntries",
                column: "ListSourceRunId");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestedEntries_SanctionEntryId",
                table: "HarvestedEntries",
                column: "SanctionEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ListSourceRuns_ListSourceId",
                table: "ListSourceRuns",
                column: "ListSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ListSourceRuns_Status",
                table: "ListSourceRuns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ListSources_Category",
                table: "ListSources",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ListSources_IsActive",
                table: "ListSources",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ListSources_Name",
                table: "ListSources",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_SanctionEntries_ListSources_ListSourceId",
                table: "SanctionEntries",
                column: "ListSourceId",
                principalTable: "ListSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanctionEntries_ListSources_ListSourceId",
                table: "SanctionEntries");

            migrationBuilder.DropTable(
                name: "HarvestedEntries");

            migrationBuilder.DropTable(
                name: "ListSourceRuns");

            migrationBuilder.DropTable(
                name: "ListSources");

            migrationBuilder.DropIndex(
                name: "IX_SanctionEntries_ListSourceId",
                table: "SanctionEntries");

            migrationBuilder.DropColumn(
                name: "ListSourceId",
                table: "SanctionEntries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ScreeningResults",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReviewedAt",
                table: "ScreeningResults",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ScreeningResults",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ScreeningRequests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ScreeningRequests",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "ScreeningRequests",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAt",
                table: "ScreeningRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SanctionEntries",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeactivatedAt",
                table: "SanctionEntries",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SanctionEntries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddedAt",
                table: "SanctionEntries",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "ListSource",
                table: "SanctionEntries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ListSourceUrl",
                table: "SanctionEntries",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "PerformedAt",
                table: "AuditLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "OFAC", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "UN", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "EU", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "MASAK", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "OFAC", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "UN", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "EU", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "OFAC", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "UN", null });

            migrationBuilder.UpdateData(
                table: "SanctionEntries",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ListSource", "ListSourceUrl" },
                values: new object[] { "MASAK", null });

            migrationBuilder.CreateIndex(
                name: "IX_ScreeningRequests_RequestedAt",
                table: "ScreeningRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SanctionEntries_ListSource",
                table: "SanctionEntries",
                column: "ListSource");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PerformedAt",
                table: "AuditLogs",
                column: "PerformedAt");
        }
    }
}
