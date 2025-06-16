using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Databases.EFCore.Migrations.StagingMigrations
{
    /// <inheritdoc />
    public partial class DropImageUrlfromImageMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageURL",
                table: "Messages");

            migrationBuilder.AddColumn<Guid>(
                name: "StorageId",
                table: "Messages",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageId",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "ImageURL",
                table: "Messages",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
