using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CherAmiAPI.Migrations.AzureSQLStaging
{
    /// <inheritdoc />
    public partial class IdkABunchofEdits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Recipients");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Recipients",
                newName: "AddressLine1");

            migrationBuilder.RenameColumn(
                name: "UnitNumber",
                table: "Recipients",
                newName: "AddressLine2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "AvatarTimestamp",
                table: "Recipients",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Recipients",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Recipients");

            migrationBuilder.RenameColumn(
                name: "AddressLine2",
                table: "Recipients",
                newName: "UnitNumber");

            migrationBuilder.RenameColumn(
                name: "AddressLine1",
                table: "Recipients",
                newName: "Street");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "AvatarTimestamp",
                table: "Recipients",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Recipients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitNumber",
                table: "Recipients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
