using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Databases.EFCore.Migrations.ProductionMigrations
{
    /// <inheritdoc />
    public partial class Messagingv10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRelationships_SelfId_OtherId",
                table: "UserRelationships");

            migrationBuilder.DropIndex(
                name: "IX_SnapshotLinks_UserId_SnapshotId",
                table: "SnapshotLinks");

            migrationBuilder.DropIndex(
                name: "IX_GuestClearances_UserId_GatheringId",
                table: "GuestClearances");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Telegrams");

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatheringId = table.Column<long>(type: "bigint", nullable: true),
                    SoftDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chats_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SoftDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatLinks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ConversationId = table.Column<long>(type: "bigint", nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    HiddenFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Muted = table.Column<bool>(type: "bit", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: true),
                    SoftDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatLinks_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    ConversationId = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ChatId = table.Column<long>(type: "bigint", nullable: true),
                    ActivityType = table.Column<int>(type: "int", nullable: true),
                    ActorId = table.Column<long>(type: "bigint", nullable: true),
                    TargetId = table.Column<long>(type: "bigint", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GatheringId = table.Column<long>(type: "bigint", nullable: true),
                    StorageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProfileId = table.Column<long>(type: "bigint", nullable: true),
                    SoftDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Users_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ChatLinks",
                columns: new[] { "Id", "ChatId", "ConversationId", "HiddenFrom", "LastSeen", "Muted", "SoftDeleted", "Type", "UserId" },
                values: new object[] { -2L, null, -2L, null, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, false, 1, -2L });

            migrationBuilder.InsertData(
                table: "Chats",
                columns: new[] { "Id", "CreatedAt", "SoftDeleted", "Title", "Type" },
                values: new object[] { -2L, new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "CANARY Team", 3 });

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_SelfId",
                table: "UserRelationships",
                column: "SelfId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotLinks_UserId",
                table: "SnapshotLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestClearances_UserId",
                table: "GuestClearances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatLinks_ChatId",
                table: "ChatLinks",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatLinks_UserId",
                table: "ChatLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_GatheringId",
                table: "Chats",
                column: "GatheringId",
                unique: true,
                filter: "[GatheringId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_UserId",
                table: "Connections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ActorId",
                table: "Messages",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId",
                table: "Messages",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GatheringId",
                table: "Messages",
                column: "GatheringId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ProfileId",
                table: "Messages",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_TargetId",
                table: "Messages",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatLinks");

            migrationBuilder.DropTable(
                name: "Connections");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_UserRelationships_SelfId",
                table: "UserRelationships");

            migrationBuilder.DropIndex(
                name: "IX_SnapshotLinks_UserId",
                table: "SnapshotLinks");

            migrationBuilder.DropIndex(
                name: "IX_GuestClearances_UserId",
                table: "GuestClearances");

            migrationBuilder.AddColumn<int>(
                name: "Message",
                table: "Telegrams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserRelationships_SelfId_OtherId",
                table: "UserRelationships",
                columns: new[] { "SelfId", "OtherId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotLinks_UserId_SnapshotId",
                table: "SnapshotLinks",
                columns: new[] { "UserId", "SnapshotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestClearances_UserId_GatheringId",
                table: "GuestClearances",
                columns: new[] { "UserId", "GatheringId" },
                unique: true);
        }
    }
}
