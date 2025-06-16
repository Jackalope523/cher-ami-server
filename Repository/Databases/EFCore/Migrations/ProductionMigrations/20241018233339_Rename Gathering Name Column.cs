using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Databases.EFCore.Migrations.ProductionMigrations
{
    /// <inheritdoc />
    public partial class RenameGatheringNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Gatherings",
                newName: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Gatherings",
                newName: "Name");
        }
    }
}
