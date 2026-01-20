using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationsAndServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    TaxCode = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SourceLastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "it"),
                    LegalName = table.Column<string>(type: "TEXT", nullable: false),
                    PrimaryImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    MainFunction = table.Column<string>(type: "TEXT", nullable: false),
                    FoundationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LegalStatus = table.Column<string>(type: "TEXT", nullable: false),
                    Gallery = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", nullable: false),
                    Website = table.Column<string>(type: "TEXT", nullable: false),
                    Instagram = table.Column<string>(type: "TEXT", nullable: false),
                    Facebook = table.Column<string>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Neighbors = table.Column<string>(type: "TEXT", nullable: false),
                    NearestCarPark_Latitude = table.Column<double>(type: "REAL", nullable: true),
                    NearestCarPark_Longitude = table.Column<double>(type: "REAL", nullable: true),
                    NearestCarPark_Address = table.Column<string>(type: "TEXT", nullable: true),
                    NearestCarPark_Distance = table.Column<double>(type: "REAL", nullable: true),
                    OwnedPoi = table.Column<string>(type: "TEXT", nullable: false),
                    Offers = table.Column<string>(type: "TEXT", nullable: false),
                    Events = table.Column<string>(type: "TEXT", nullable: false),
                    MunicipalityData_Name = table.Column<string>(type: "TEXT", nullable: false),
                    MunicipalityData_LogoPath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.TaxCode);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SourceLastUpdated = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "it"),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Typology = table.Column<string>(type: "TEXT", nullable: false),
                    SpacesForDisabled = table.Column<int>(type: "INTEGER", nullable: true),
                    PayingParkingSpaces = table.Column<int>(type: "INTEGER", nullable: true),
                    AvailableParkingSpaces = table.Column<int>(type: "INTEGER", nullable: true),
                    PostiAutoVenduti = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalNumberOfCarParkSpaces = table.Column<int>(type: "INTEGER", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    PrimaryImage = table.Column<string>(type: "TEXT", nullable: false),
                    Gallery = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", nullable: false),
                    Website = table.Column<string>(type: "TEXT", nullable: false),
                    Instagram = table.Column<string>(type: "TEXT", nullable: false),
                    Facebook = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<string>(type: "TEXT", nullable: false),
                    ReservationUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Neighbors = table.Column<string>(type: "TEXT", nullable: false),
                    NearestCarPark_Latitude = table.Column<double>(type: "REAL", nullable: true),
                    NearestCarPark_Longitude = table.Column<double>(type: "REAL", nullable: true),
                    NearestCarPark_Address = table.Column<string>(type: "TEXT", nullable: true),
                    NearestCarPark_Distance = table.Column<double>(type: "REAL", nullable: true),
                    OpeningHours_Opens = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    OpeningHours_Closes = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    OpeningHours_Description = table.Column<string>(type: "TEXT", nullable: true),
                    OpeningHours_AdmissionType_Name = table.Column<string>(type: "TEXT", nullable: true),
                    OpeningHours_AdmissionType_Description = table.Column<string>(type: "TEXT", nullable: true),
                    OpeningHours_TimeInterval_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OpeningHours_TimeInterval_StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OpeningHours_TimeInterval_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OpeningHours_Day = table.Column<string>(type: "TEXT", nullable: true),
                    TemporaryClosure_ReasonForClosure = table.Column<string>(type: "TEXT", nullable: true),
                    TemporaryClosure_Opens = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    TemporaryClosure_Closes = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    TemporaryClosure_Description = table.Column<string>(type: "TEXT", nullable: true),
                    TemporaryClosure_TimeInterval_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TemporaryClosure_TimeInterval_StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TemporaryClosure_TimeInterval_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TemporaryClosure_Day = table.Column<string>(type: "TEXT", nullable: true),
                    Booking_TimeInterval_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Booking_TimeInterval_StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Booking_TimeInterval_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Booking_Name = table.Column<string>(type: "TEXT", nullable: true),
                    Booking_Description = table.Column<string>(type: "TEXT", nullable: true),
                    Locations = table.Column<string>(type: "TEXT", nullable: false),
                    AssociatedServices = table.Column<string>(type: "TEXT", nullable: false),
                    MunicipalityData_Name = table.Column<string>(type: "TEXT", nullable: false),
                    MunicipalityData_LogoPath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Identifier);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Language",
                table: "Organizations",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Language",
                table: "Services",
                column: "Language");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
