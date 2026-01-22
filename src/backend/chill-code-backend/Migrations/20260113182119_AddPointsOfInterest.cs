using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsOfInterest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointsOfInterest",
                columns: table => new
                {
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SourceLastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    OfficialName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Typology = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PrimaryImage = table.Column<string>(type: "TEXT", nullable: false),
                    ImageThumbPath = table.Column<string>(type: "TEXT", nullable: false),
                    Gallery = table.Column<string>(type: "TEXT", nullable: false),
                    VirtualTours = table.Column<string>(type: "TEXT", nullable: false),
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
                    OpeningHours = table.Column<string>(type: "TEXT", nullable: false),
                    Accessibility = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Prices = table.Column<string>(type: "TEXT", nullable: false),
                    Services = table.Column<string>(type: "TEXT", nullable: false),
                    Facilities = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationTaxCode = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    OrganizationLegalName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    MunicipalityData_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MunicipalityData_LogoPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointsOfInterest", x => x.Identifier);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointsOfInterest");
        }
    }
}
