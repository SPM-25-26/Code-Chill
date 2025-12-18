using System;
using ChillCodeBackend.Data.Db;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chill_code_backend.Migrations
{
    [DbContext(typeof(ChillCodeDbContext))]
    [Migration("20251218090000_IngestionUpsertMetadata")]
    public partial class IngestionUpsertMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Articles",
                newName: "SourceLastUpdated"
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Articles",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP"
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Articles",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP"
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Events",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP"
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Events",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP"
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "SourceLastUpdated",
                table: "Events",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Routes",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.Sql(
                "UPDATE Routes SET ExternalId = Name WHERE ExternalId IS NULL OR ExternalId = ''"
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Routes",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP"
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Routes",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP"
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "SourceLastUpdated",
                table: "Routes",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Routes_ExternalId",
                table: "Routes",
                column: "ExternalId",
                unique: true
            );

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Municipalities",
                type: "TEXT",
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.Sql(
                "UPDATE Municipalities SET ExternalId = Name WHERE ExternalId IS NULL OR ExternalId = ''"
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Municipalities",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP"
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Municipalities",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP"
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "SourceLastUpdated",
                table: "Municipalities",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_ExternalId",
                table: "Municipalities",
                column: "ExternalId",
                unique: true
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Routes_ExternalId", table: "Routes");
            migrationBuilder.DropIndex(name: "IX_Municipalities_ExternalId", table: "Municipalities");

            migrationBuilder.DropColumn(name: "CreatedAt", table: "Articles");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Articles");
            migrationBuilder.RenameColumn(
                name: "SourceLastUpdated",
                table: "Articles",
                newName: "UpdatedAt"
            );

            migrationBuilder.DropColumn(name: "CreatedAt", table: "Events");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Events");
            migrationBuilder.DropColumn(name: "SourceLastUpdated", table: "Events");

            migrationBuilder.DropColumn(name: "ExternalId", table: "Routes");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Routes");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Routes");
            migrationBuilder.DropColumn(name: "SourceLastUpdated", table: "Routes");

            migrationBuilder.DropColumn(name: "ExternalId", table: "Municipalities");
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Municipalities");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "Municipalities");
            migrationBuilder.DropColumn(name: "SourceLastUpdated", table: "Municipalities");
        }
    }
}

