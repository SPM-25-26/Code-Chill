using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageCompositeKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Services",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_Language",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Routes",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_ExternalId",
                table: "Routes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsOfInterest",
                table: "PointsOfInterest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_Language",
                table: "Organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Municipalities",
                table: "Municipalities");

            migrationBuilder.DropIndex(
                name: "IX_Municipalities_ExternalId",
                table: "Municipalities");

            migrationBuilder.DropIndex(
                name: "IX_Municipalities_Name",
                table: "Municipalities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Events",
                table: "Events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Articles",
                table: "Articles");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Municipalities",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "it");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Services",
                table: "Services",
                columns: new[] { "Identifier", "Language" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Routes",
                table: "Routes",
                columns: new[] { "ExternalId", "Language" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsOfInterest",
                table: "PointsOfInterest",
                columns: new[] { "Identifier", "Language" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations",
                columns: new[] { "TaxCode", "Language" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Municipalities",
                table: "Municipalities",
                columns: new[] { "ExternalId", "Language" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                table: "Events",
                columns: new[] { "Identifier", "Language" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Articles",
                table: "Articles",
                columns: new[] { "Identifier", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_Services_Identifier_Language",
                table: "Services",
                columns: new[] { "Identifier", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ExternalId_Language",
                table: "Routes",
                columns: new[] { "ExternalId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_TaxCode_Language",
                table: "Organizations",
                columns: new[] { "TaxCode", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_ExternalId_Language",
                table: "Municipalities",
                columns: new[] { "ExternalId", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_Name_Language",
                table: "Municipalities",
                columns: new[] { "Name", "Language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Services",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_Identifier_Language",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Routes",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_ExternalId_Language",
                table: "Routes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PointsOfInterest",
                table: "PointsOfInterest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_TaxCode_Language",
                table: "Organizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Municipalities",
                table: "Municipalities");

            migrationBuilder.DropIndex(
                name: "IX_Municipalities_ExternalId_Language",
                table: "Municipalities");

            migrationBuilder.DropIndex(
                name: "IX_Municipalities_Name_Language",
                table: "Municipalities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Events",
                table: "Events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Articles",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Municipalities");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Services",
                table: "Services",
                column: "Identifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Routes",
                table: "Routes",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PointsOfInterest",
                table: "PointsOfInterest",
                column: "Identifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations",
                column: "TaxCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Municipalities",
                table: "Municipalities",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                table: "Events",
                column: "Identifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Articles",
                table: "Articles",
                column: "Identifier");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Language",
                table: "Services",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ExternalId",
                table: "Routes",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Language",
                table: "Organizations",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_ExternalId",
                table: "Municipalities",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_Name",
                table: "Municipalities",
                column: "Name",
                unique: true);
        }
    }
}
