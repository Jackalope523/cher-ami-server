using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CherAmiAPI.Migrations.AzureSQLProduction
{
    /// <inheritdoc />
    public partial class AddNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IssuePosts",
                table: "AspNetUsers",
                newName: "PushNewPosts");

            migrationBuilder.RenameColumn(
                name: "IssueReminders",
                table: "AspNetUsers",
                newName: "PushIssueReminders");

            // Defaults to opted in, matching the OneSignal tags these replace.
            migrationBuilder.AddColumn<bool>(
                name: "PushNewMembers",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailIssueReminders",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailMarketing",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PushNewMembers",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailIssueReminders",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailMarketing",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PushNewPosts",
                table: "AspNetUsers",
                newName: "IssuePosts");

            migrationBuilder.RenameColumn(
                name: "PushIssueReminders",
                table: "AspNetUsers",
                newName: "IssueReminders");
        }
    }
}
