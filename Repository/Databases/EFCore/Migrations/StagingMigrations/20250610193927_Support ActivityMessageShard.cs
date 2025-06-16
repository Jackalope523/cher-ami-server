using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Databases.EFCore.Migrations.StagingMigrations
{
    /// <inheritdoc />
    public partial class SupportActivityMessageShard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ActorId",
                table: "Messages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Info",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TargetId",
                table: "Messages",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ActorId",
                table: "Messages",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TargetId",
                table: "Messages",
                column: "TargetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_ActorId",
                table: "Messages",
                column: "ActorId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_TargetId",
                table: "Messages",
                column: "TargetId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_ActorId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_TargetId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ActorId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_TargetId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ActorId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Info",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Messages");
        }
    }
}
