using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Databases.EFCore.Migrations.ProductionMigrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLinks_Users_OtherId",
                table: "UserLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLinks_Users_SelfId",
                table: "UserLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLinks",
                table: "UserLinks");

            migrationBuilder.RenameTable(
                name: "UserLinks",
                newName: "UserRelationships");

            migrationBuilder.RenameColumn(
                name: "IsPendingDeletion",
                table: "Users",
                newName: "SoftDeleted");

            migrationBuilder.RenameColumn(
                name: "IsPendingDeletion",
                table: "Gatherings",
                newName: "SoftDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_UserLinks_SelfId_OtherId",
                table: "UserRelationships",
                newName: "IX_UserRelationships_SelfId_OtherId");

            migrationBuilder.RenameIndex(
                name: "IX_UserLinks_OtherId",
                table: "UserRelationships",
                newName: "IX_UserRelationships_OtherId");

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "UserReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Telegrams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Snapshots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "SnapshotReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "SnapshotLinks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Penalties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "GuestClearances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "GatheringReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "GatheringLinks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Feedback",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Banners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "BannerLinks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "UserRelationships",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRelationships",
                table: "UserRelationships",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelationships_Users_OtherId",
                table: "UserRelationships",
                column: "OtherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRelationships_Users_SelfId",
                table: "UserRelationships",
                column: "SelfId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRelationships_Users_OtherId",
                table: "UserRelationships");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRelationships_Users_SelfId",
                table: "UserRelationships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRelationships",
                table: "UserRelationships");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "UserReports");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Telegrams");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Snapshots");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "SnapshotReports");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "SnapshotLinks");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Penalties");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "GuestClearances");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "GatheringReports");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "GatheringLinks");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "BannerLinks");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "UserRelationships");

            migrationBuilder.RenameTable(
                name: "UserRelationships",
                newName: "UserLinks");

            migrationBuilder.RenameColumn(
                name: "SoftDeleted",
                table: "Users",
                newName: "IsPendingDeletion");

            migrationBuilder.RenameColumn(
                name: "SoftDeleted",
                table: "Gatherings",
                newName: "IsPendingDeletion");

            migrationBuilder.RenameIndex(
                name: "IX_UserRelationships_SelfId_OtherId",
                table: "UserLinks",
                newName: "IX_UserLinks_SelfId_OtherId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRelationships_OtherId",
                table: "UserLinks",
                newName: "IX_UserLinks_OtherId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLinks",
                table: "UserLinks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLinks_Users_OtherId",
                table: "UserLinks",
                column: "OtherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLinks_Users_SelfId",
                table: "UserLinks",
                column: "SelfId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
