using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CherAmiAPI.Migrations.AzureSQLStaging
{
    /// <inheritdoc />
    public partial class AddExternalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProvidedPaymentDetails",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 7L,
                column: "ExternalId",
                value: null);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 8L,
                column: "ExternalId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "ProvidedPaymentDetails",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 7L,
                column: "ProvidedPaymentDetails",
                value: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 8L,
                column: "ProvidedPaymentDetails",
                value: false);
        }
    }
}
