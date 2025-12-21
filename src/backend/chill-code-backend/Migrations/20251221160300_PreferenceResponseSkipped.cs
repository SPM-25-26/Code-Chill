using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    /// <inheritdoc />
    public partial class PreferenceResponseSkipped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Skipped",
                table: "PreferenceResponses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skipped",
                table: "PreferenceResponses");
        }
    }
}
