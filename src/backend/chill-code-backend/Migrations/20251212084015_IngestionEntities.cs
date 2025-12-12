using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    /// <inheritdoc />
    public partial class IngestionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Script = table.Column<string>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Subtitle = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TimeToRead = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    Paragraphs = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Themes = table.Column<string>(type: "TEXT", nullable: false),
                    MunicipalityData_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MunicipalityData_LogoPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Typology = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryImage = table.Column<string>(type: "TEXT", nullable: false),
                    Gallery = table.Column<string>(type: "TEXT", nullable: false),
                    VirtualTours = table.Column<string>(type: "TEXT", nullable: false),
                    Audience = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Website = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Facebook = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Instagram = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Neighbors = table.Column<string>(type: "TEXT", nullable: false),
                    NearestCarPark_Latitude = table.Column<double>(type: "REAL", nullable: true),
                    NearestCarPark_Longitude = table.Column<double>(type: "REAL", nullable: true),
                    NearestCarPark_Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    NearestCarPark_Distance = table.Column<double>(type: "REAL", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Organizer_TaxCode = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Organizer_LegalName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Organizer_Website = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TicketsAndCosts = table.Column<string>(type: "TEXT", nullable: false),
                    MunicipalityData_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MunicipalityData_LogoPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LegalName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Contacts_Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Contacts_Telephone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Contacts_Website = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Contacts_Facebook = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Contacts_Instagram = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    LogoPath = table.Column<string>(type: "TEXT", nullable: false),
                    HomeImages = table.Column<string>(type: "TEXT", nullable: true),
                    Events = table.Column<string>(type: "TEXT", nullable: true),
                    ArticlesAndPaths = table.Column<string>(type: "TEXT", nullable: true),
                    PanoramaPath = table.Column<string>(type: "TEXT", nullable: false),
                    PanoramaWidth = table.Column<int>(type: "INTEGER", nullable: true),
                    VirtualTourUrls = table.Column<string>(type: "TEXT", nullable: true),
                    NameAndProvince = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    PathTheme = table.Column<string>(type: "TEXT", nullable: false),
                    TravellingMethod = table.Column<string>(type: "TEXT", nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationWebsite = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OrganizationEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    OrganizationFacebook = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    OrganizationInstagram = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    OrganizationTelephone = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SecurityLevel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NumberOfStages = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantifiedPathwayPaving = table.Column<int>(type: "INTEGER", nullable: false),
                    Duration = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RouteLength = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Gallery = table.Column<string>(type: "TEXT", nullable: false),
                    VirtualTours = table.Column<string>(type: "TEXT", nullable: false),
                    Stages = table.Column<string>(type: "TEXT", nullable: false),
                    StagesPoi = table.Column<string>(type: "TEXT", nullable: false),
                    StartingPoint_Latitude = table.Column<double>(type: "REAL", nullable: false),
                    StartingPoint_Longitude = table.Column<double>(type: "REAL", nullable: false),
                    StartingPoint_Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BestWhen = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MunicipalityData_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MunicipalityData_LogoPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Name);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_Name",
                table: "Municipalities",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Municipalities");

            migrationBuilder.DropTable(
                name: "Routes");
        }
    }
}
