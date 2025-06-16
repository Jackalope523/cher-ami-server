using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Repository.Databases.EFCore.Migrations.ProductionMigrations
{
    /// <inheritdoc />
    public partial class AddCANARYAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessTries", "AccountStatus", "Age", "Athleticisme", "Chaos", "CompanionActivity", "CompanionshipCode", "Competitiveness", "CurrentGathering", "CurrentLocation", "CurrentRadius", "DateOfBirth", "Email", "Extroversion", "GatheringActivity", "GatheringDiscovery", "GatheringReminders", "Haunt", "HauntRadius", "HauntWeight", "Industriousness", "IsEmailConfirmed", "IsPhoneConfirmed", "JoinDate", "LockoutDate", "Name", "NightOwl", "NormalisedEmail", "NotificationId", "Openness", "PhoneNumber", "Reputation", "SecurityStamp", "SocialInvitations", "SoftDeleted", "TimeOfUserAgreement" },
                values: new object[] { -2L, 3, 0, 25, 50, 50, true, "", 50, null, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.544 53.483)"), 10.0, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", 50, true, true, true, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.54 53.483)"), 10.0, 0, 50, false, true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "CANARY", 50, "", new Guid("00000000-0000-0000-0000-000000000000"), 50, "15734922666", 50, "", true, false, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -2L);
        }
    }
}
