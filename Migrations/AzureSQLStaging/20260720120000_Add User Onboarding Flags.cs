using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CherAmiAPI.Migrations.AzureSQLStaging
{
    /// <inheritdoc />
    public partial class AddUserOnboardingFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NameProvidedByUser",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OnboardingCompleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Accounts that predate onboarding keep their circle and name: treat
            // them as already onboarded so they are never sent through the flow.
            // Prospective users (invited, never signed in) must still onboard.
            migrationBuilder.Sql("UPDATE AspNetUsers SET NameProvidedByUser = 1, OnboardingCompleted = 1 WHERE AccountStatus <> 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameProvidedByUser",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OnboardingCompleted",
                table: "AspNetUsers");
        }
    }
}
