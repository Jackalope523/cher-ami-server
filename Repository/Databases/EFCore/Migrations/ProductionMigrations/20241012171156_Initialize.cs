using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Repository.Databases.EFCore.Migrations.ProductionMigrations
{
    /// <inheritdoc />
    public partial class Initialize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NormalisedEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pseudonym = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    JoinDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Reputation = table.Column<int>(type: "int", nullable: false),
                    IsPhoneConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LockoutDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AccessTries = table.Column<int>(type: "int", nullable: false),
                    AccountStatus = table.Column<int>(type: "int", nullable: false),
                    CurrentGathering = table.Column<long>(type: "bigint", nullable: true),
                    IsPendingDeletion = table.Column<bool>(type: "bit", nullable: false),
                    TimeOfUserAgreement = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Extroversion = table.Column<int>(type: "int", nullable: false),
                    Athleticisme = table.Column<int>(type: "int", nullable: false),
                    Openness = table.Column<int>(type: "int", nullable: false),
                    Chaos = table.Column<int>(type: "int", nullable: false),
                    Competitiveness = table.Column<int>(type: "int", nullable: false),
                    Industriousness = table.Column<int>(type: "int", nullable: false),
                    NightOwl = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Haunt = table.Column<Point>(type: "geography", nullable: false),
                    HauntRadius = table.Column<double>(type: "float", nullable: false),
                    HauntWheight = table.Column<int>(type: "int", nullable: false),
                    CurrentLocation = table.Column<Point>(type: "geography", nullable: false),
                    CurrentRadius = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BannerLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    BannerId = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannerLinks_Banners_BannerId",
                        column: x => x.BannerId,
                        principalTable: "Banners",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BannerLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedback_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Gatherings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    HostId = table.Column<long>(type: "bigint", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: false),
                    FriendlyLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    GroupMinimum = table.Column<int>(type: "int", nullable: false),
                    GroupMaximum = table.Column<int>(type: "int", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Radius = table.Column<double>(type: "float", nullable: false),
                    IsDynamic = table.Column<bool>(type: "bit", nullable: false),
                    IsPendingDeletion = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    DegreeOfPrivacy = table.Column<int>(type: "int", nullable: false),
                    Extroversion = table.Column<int>(type: "int", nullable: false),
                    Athleticisme = table.Column<int>(type: "int", nullable: false),
                    Openness = table.Column<int>(type: "int", nullable: false),
                    Chaos = table.Column<int>(type: "int", nullable: false),
                    Competitiveness = table.Column<int>(type: "int", nullable: false),
                    Industriousness = table.Column<int>(type: "int", nullable: false),
                    NightOwl = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gatherings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gatherings_Users_HostId",
                        column: x => x.HostId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Penalties",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PenalizedId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Penalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Penalties_Users_PenalizedId",
                        column: x => x.PenalizedId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Telegrams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotifierId = table.Column<long>(type: "bigint", nullable: false),
                    RecipientId = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Message = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Read = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Telegrams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Telegrams_Users_NotifierId",
                        column: x => x.NotifierId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Telegrams_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SelfId = table.Column<long>(type: "bigint", nullable: false),
                    OtherId = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLinks_Users_OtherId",
                        column: x => x.OtherId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserLinks_Users_SelfId",
                        column: x => x.SelfId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GatheringLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GatheringId = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatheringLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatheringLinks_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GatheringLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GatheringReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    GatheringId = table.Column<long>(type: "bigint", nullable: false),
                    FilingDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatheringReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatheringReports_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GatheringReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GuestClearances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GatheringId = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Degree = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestClearances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestClearances_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GuestClearances_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Snapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    GatheringId = table.Column<long>(type: "bigint", nullable: false),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Snapshots_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Snapshots_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SelfId = table.Column<long>(type: "bigint", nullable: true),
                    OtherId = table.Column<long>(type: "bigint", nullable: false),
                    GatheringId = table.Column<long>(type: "bigint", nullable: true),
                    FilingDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserReports_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserReports_Users_OtherId",
                        column: x => x.OtherId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserReports_Users_SelfId",
                        column: x => x.SelfId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SnapshotLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnapshotLinks_Snapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "Snapshots",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SnapshotLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SnapshotReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    FilingDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
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
                        principalColumn: "Id");
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
                name: "IX_UserLinks_OtherId",
                table: "UserLinks",
                column: "OtherId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLinks_SelfId_OtherId",
                table: "UserLinks",
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
                name: "UserLinks");

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
