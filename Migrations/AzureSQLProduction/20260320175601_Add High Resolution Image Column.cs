using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CherAmiAPI.Migrations.AzureSQLProduction
{
    /// <inheritdoc />
    public partial class AddHighResolutionImageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HighResolutionImagePath",
                table: "Posts",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighResolutionImagePath",
                table: "Posts");
        }
    }
}
