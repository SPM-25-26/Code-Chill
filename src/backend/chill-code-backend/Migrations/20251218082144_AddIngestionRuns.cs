using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIngestionRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngestionRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Municipality = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    TotalPoiCount = table.Column<int>(type: "INTEGER", nullable: false),
                    EventCount = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizationCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RouteCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ArticleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Errors = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngestionRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngestionRuns_Municipality_Language_StartedAt",
                table: "IngestionRuns",
                columns: new[] { "Municipality", "Language", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngestionRuns");
        }
    }
}
