using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageToContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Routes",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Events",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Articles",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Articles");
        }
    }
}
