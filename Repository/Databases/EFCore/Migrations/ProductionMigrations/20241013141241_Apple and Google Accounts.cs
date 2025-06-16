using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Databases.EFCore.Migrations.ProductionMigrations
{
    /// <inheritdoc />
    public partial class AppleandGoogleAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessTries", "AccountStatus", "Age", "Athleticisme", "Chaos", "Competitiveness", "CurrentGathering", "CurrentLocation", "CurrentRadius", "DateOfBirth", "Email", "Extroversion", "Haunt", "HauntRadius", "HauntWheight", "Industriousness", "IsEmailConfirmed", "IsPendingDeletion", "IsPhoneConfirmed", "JoinDate", "LockoutDate", "Name", "NightOwl", "NormalisedEmail", "NotificationId", "Openness", "PhoneNumber", "Pseudonym", "Reputation", "SecurityStamp", "TimeOfUserAgreement" },
                values: new object[,]
                {
                    { -8L, 3, 0, 25, 50, 50, 50, null, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.544 53.483)"), 10.0, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", 50, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.54 53.483)"), 10.0, 0, 50, false, false, true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Google Test Account", 50, "", new Guid("00000000-0000-0000-0000-000000000000"), 50, "11002003008", "", 50, "", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { -7L, 3, 0, 25, 50, 50, 50, null, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.544 53.483)"), 10.0, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", 50, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.54 53.483)"), 10.0, 0, 50, false, false, true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Apple Test Account", 50, "", new Guid("00000000-0000-0000-0000-000000000000"), 50, "11002003007", "", 50, "", new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -8L);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -7L);
        }
    }
}
