using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CherAmiAPI.Migrations.AzureSQLStaging
{
    /// <inheritdoc />
    public partial class AddCircleLastPhotoPushAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastPhotoPushAt",
                table: "Circles",
                type: "datetimeoffset",
                nullable: true);

            // IsVeteran, HighResolutionImagePath and UploadId reached production in Feb/Mar 2026 but
            // were never written into this folder, so they are added here only where still missing.
            migrationBuilder.Sql(@"
IF COL_LENGTH('Recipients', 'IsVeteran') IS NULL
    ALTER TABLE [Recipients] ADD [IsVeteran] bit NOT NULL DEFAULT CAST(0 AS bit);");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Posts', 'HighResolutionImagePath') IS NULL
    ALTER TABLE [Posts] ADD [HighResolutionImagePath] nvarchar(1024) NULL;");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Posts', 'UploadId') IS NULL
    ALTER TABLE [Posts] ADD [UploadId] nvarchar(50) NULL;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Posts_UploadId' AND object_id = OBJECT_ID('Posts'))
    CREATE UNIQUE INDEX [IX_Posts_UploadId] ON [Posts] ([UploadId]) WHERE [UploadId] IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPhotoPushAt",
                table: "Circles");
        }
    }
}
