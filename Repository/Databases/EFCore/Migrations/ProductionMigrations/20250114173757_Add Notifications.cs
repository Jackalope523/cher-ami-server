using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Databases.EFCore.Migrations.ProductionMigrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HauntWheight",
                table: "Users",
                newName: "HauntWeight");

            migrationBuilder.AddColumn<bool>(
                name: "CompanionActivity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "GatheringActivity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "GatheringDiscovery",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "GatheringReminders",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "SocialInvitations",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientId = table.Column<long>(type: "bigint", nullable: false),
                    GatheringId = table.Column<long>(type: "bigint", nullable: false),
                    NotificationId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -8L,
                columns: new[] { "CompanionActivity", "GatheringActivity", "GatheringDiscovery", "GatheringReminders", "SocialInvitations" },
                values: new object[] { true, true, true, true, true });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -7L,
                columns: new[] { "CompanionActivity", "GatheringActivity", "GatheringDiscovery", "GatheringReminders", "SocialInvitations" },
                values: new object[] { true, true, true, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GatheringId",
                table: "Notifications",
                column: "GatheringId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropColumn(
                name: "CompanionActivity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GatheringActivity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GatheringDiscovery",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GatheringReminders",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SocialInvitations",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "HauntWeight",
                table: "Users",
                newName: "HauntWheight");
        }
    }
}
