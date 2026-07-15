using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CherAmiAPI.Migrations.AzureSQLProduction
{
    /// <inheritdoc />
    public partial class RenameColumns : Migration
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

            migrationBuilder.AlterColumn<string>(
                name: "AddressLine1",
                table: "Recipients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            // Increase character limit for AddressLine2
            migrationBuilder.AlterColumn<string>(
                name: "AddressLine2",
                table: "Recipients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddressLine2",
                table: "Recipients",
                newName: "UnitNumber");

            migrationBuilder.RenameColumn(
                name: "AddressLine1",
                table: "Recipients",
                newName: "Street");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Recipients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Recipients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
