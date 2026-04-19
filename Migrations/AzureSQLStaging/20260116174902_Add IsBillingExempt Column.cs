using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CherAmiAPI.Migrations.AzureSQLStaging
{
    /// <inheritdoc />
    public partial class AddIsBillingExemptColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBillingExempt",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 7L,
                column: "IsBillingExempt",
                value: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 8L,
                column: "IsBillingExempt",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBillingExempt",
                table: "AspNetUsers");
        }
    }
}
