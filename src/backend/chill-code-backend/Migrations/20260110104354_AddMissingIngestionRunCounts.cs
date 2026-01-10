using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingIngestionRunCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationCount",
                table: "IngestionRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "TotalPoiCount",
                table: "IngestionRuns",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.Sql(
                "UPDATE IngestionRuns SET TotalPoiCount = COALESCE(PoiCount, 0) WHERE TotalPoiCount = 0;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationCount",
                table: "IngestionRuns"
            );

            migrationBuilder.DropColumn(
                name: "TotalPoiCount",
                table: "IngestionRuns"
            );
        }
    }
}
