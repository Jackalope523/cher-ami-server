using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repository.Databases.EFCore.Migrations.TestMigrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Sqlite:InitSpatialMetaData", true);

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    NormalisedEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Pseudonym = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    JoinDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Reputation = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPhoneConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    SecurityStamp = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LockoutDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    AccessTries = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentGathering = table.Column<long>(type: "INTEGER", nullable: true),
                    TimeOfUserAgreement = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    NotificationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Extroversion = table.Column<int>(type: "INTEGER", nullable: false),
                    Athleticisme = table.Column<int>(type: "INTEGER", nullable: false),
                    Openness = table.Column<int>(type: "INTEGER", nullable: false),
                    Chaos = table.Column<int>(type: "INTEGER", nullable: false),
                    Competitiveness = table.Column<int>(type: "INTEGER", nullable: false),
                    Industriousness = table.Column<int>(type: "INTEGER", nullable: false),
                    NightOwl = table.Column<int>(type: "INTEGER", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Haunt = table.Column<Point>(type: "POINT", nullable: false)
                        .Annotation("Sqlite:Srid", 4326),
                    HauntRadius = table.Column<double>(type: "REAL", nullable: false),
                    HauntWheight = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentLocation = table.Column<Point>(type: "POINT", nullable: false)
                        .Annotation("Sqlite:Srid", 4326),
                    CurrentRadius = table.Column<double>(type: "REAL", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BannerLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    BannerId = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannerLinks_Banners_BannerId",
                        column: x => x.BannerId,
                        principalTable: "Banners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BannerLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedback_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Gatherings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    HostId = table.Column<long>(type: "INTEGER", nullable: true),
                    Location = table.Column<Point>(type: "POINT", nullable: false)
                        .Annotation("Sqlite:Srid", 4326),
                    FriendlyLocation = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupMinimum = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupMaximum = table.Column<int>(type: "INTEGER", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Radius = table.Column<double>(type: "REAL", nullable: false),
                    IsDynamic = table.Column<bool>(type: "INTEGER", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "INTEGER", nullable: false),
                    DegreeOfPrivacy = table.Column<int>(type: "INTEGER", nullable: false),
                    Extroversion = table.Column<int>(type: "INTEGER", nullable: false),
                    Athleticisme = table.Column<int>(type: "INTEGER", nullable: false),
                    Openness = table.Column<int>(type: "INTEGER", nullable: false),
                    Chaos = table.Column<int>(type: "INTEGER", nullable: false),
                    Competitiveness = table.Column<int>(type: "INTEGER", nullable: false),
                    Industriousness = table.Column<int>(type: "INTEGER", nullable: false),
                    NightOwl = table.Column<int>(type: "INTEGER", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gatherings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gatherings_Users_HostId",
                        column: x => x.HostId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Penalties",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PenalizedId = table.Column<long>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Penalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Penalties_Users_PenalizedId",
                        column: x => x.PenalizedId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    DeviceToken = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Telegrams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NotifierId = table.Column<long>(type: "INTEGER", nullable: false),
                    RecipientId = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Message = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Read = table.Column<bool>(type: "INTEGER", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Telegrams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Telegrams_Users_NotifierId",
                        column: x => x.NotifierId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Telegrams_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRelationships",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SelfId = table.Column<long>(type: "INTEGER", nullable: false),
                    OtherId = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRelationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRelationships_Users_OtherId",
                        column: x => x.OtherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRelationships_Users_SelfId",
                        column: x => x.SelfId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GatheringLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GatheringId = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatheringLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatheringLinks_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GatheringLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GatheringReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: true),
                    GatheringId = table.Column<long>(type: "INTEGER", nullable: false),
                    FilingDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatheringReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatheringReports_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GatheringReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuestClearances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    GatheringId = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Degree = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestClearances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestClearances_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuestClearances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Snapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<long>(type: "INTEGER", nullable: false),
                    GatheringId = table.Column<long>(type: "INTEGER", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Snapshots_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Snapshots_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    SelfId = table.Column<long>(type: "INTEGER", nullable: true),
                    OtherId = table.Column<long>(type: "INTEGER", nullable: false),
                    GatheringId = table.Column<long>(type: "INTEGER", nullable: true),
                    FilingDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReports_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserReports_Users_OtherId",
                        column: x => x.OtherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserReports_Users_SelfId",
                        column: x => x.SelfId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SnapshotLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    SnapshotId = table.Column<long>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnapshotLinks_Snapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "Snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SnapshotLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SnapshotReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: true),
                    SnapshotId = table.Column<long>(type: "INTEGER", nullable: false),
                    FilingDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    SoftDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnapshotReports_Snapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "Snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SnapshotReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessTries", "AccountStatus", "Age", "Athleticisme", "Chaos", "Competitiveness", "CurrentGathering", "CurrentLocation", "CurrentRadius", "DateOfBirth", "Email", "Extroversion", "Haunt", "HauntRadius", "HauntWheight", "Industriousness", "IsEmailConfirmed", "IsPhoneConfirmed", "JoinDate", "LockoutDate", "Name", "NightOwl", "NormalisedEmail", "NotificationId", "Openness", "PhoneNumber", "Pseudonym", "Reputation", "SecurityStamp", "SoftDeleted", "TimeOfUserAgreement" },
                values: new object[,]
                {
                    { -8L, 3, 0, 25, 50, 50, 50, null, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.544 53.483)"), 10.0, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", 50, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.54 53.483)"), 10.0, 0, 50, false, true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Google Test Account", 50, "", new Guid("00000000-0000-0000-0000-000000000000"), 50, "11002003008", "", 50, "", false, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { -7L, 3, 0, 25, 50, 50, 50, null, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.544 53.483)"), 10.0, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", 50, (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (7.54 53.483)"), 10.0, 0, 50, false, true, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "Apple Test Account", 50, "", new Guid("00000000-0000-0000-0000-000000000000"), 50, "11002003007", "", 50, "", false, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannerLinks_BannerId",
                table: "BannerLinks",
                column: "BannerId");

            migrationBuilder.CreateIndex(
                name: "IX_BannerLinks_UserId",
                table: "BannerLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_UserId",
                table: "Feedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GatheringLinks_GatheringId",
                table: "GatheringLinks",
                column: "GatheringId");

            migrationBuilder.CreateIndex(
                name: "IX_GatheringLinks_UserId",
                table: "GatheringLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GatheringReports_GatheringId",
                table: "GatheringReports",
                column: "GatheringId");

            migrationBuilder.CreateIndex(
                name: "IX_GatheringReports_UserId",
                table: "GatheringReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Gatherings_HostId",
                table: "Gatherings",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestClearances_GatheringId",
                table: "GuestClearances",
                column: "GatheringId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestClearances_UserId_GatheringId",
                table: "GuestClearances",
                columns: new[] { "UserId", "GatheringId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_PenalizedId",
                table: "Penalties",
                column: "PenalizedId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotLinks_SnapshotId",
                table: "SnapshotLinks",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotLinks_UserId_SnapshotId",
                table: "SnapshotLinks",
                columns: new[] { "UserId", "SnapshotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotReports_SnapshotId",
                table: "SnapshotReports",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotReports_UserId",
                table: "SnapshotReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Snapshots_GatheringId",
                table: "Snapshots",
                column: "GatheringId");

            migrationBuilder.CreateIndex(
                name: "IX_Snapshots_OwnerId",
                table: "Snapshots",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Telegrams_NotifierId",
                table: "Telegrams",
                column: "NotifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Telegrams_RecipientId",
                table: "Telegrams",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_OtherId",
                table: "UserRelationships",
                column: "OtherId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_SelfId_OtherId",
                table: "UserRelationships",
                columns: new[] { "SelfId", "OtherId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_GatheringId",
                table: "UserReports",
                column: "GatheringId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_OtherId",
                table: "UserReports",
                column: "OtherId");

            migrationBuilder.CreateIndex(
                name: "IX_UserReports_SelfId",
                table: "UserReports",
                column: "SelfId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannerLinks");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "GatheringLinks");

            migrationBuilder.DropTable(
                name: "GatheringReports");

            migrationBuilder.DropTable(
                name: "GuestClearances");

            migrationBuilder.DropTable(
                name: "Penalties");

            migrationBuilder.DropTable(
                name: "SnapshotLinks");

            migrationBuilder.DropTable(
                name: "SnapshotReports");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Telegrams");

            migrationBuilder.DropTable(
                name: "UserRelationships");

            migrationBuilder.DropTable(
                name: "UserReports");

            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.DropTable(
                name: "Snapshots");

            migrationBuilder.DropTable(
                name: "Gatherings");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
