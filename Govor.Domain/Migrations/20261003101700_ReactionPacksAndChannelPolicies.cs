using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Govor.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ReactionPacksAndChannelPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ReactionsVersion",
                table: "Messages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "ReactionId",
                table: "MessageReactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReactionMode",
                table: "ChatGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ReactionPolicyVersion",
                table: "ChatGroups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ReactionPacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ShareCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionPacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReactionPacks_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReactionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Emoji = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MediaFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    SizeBytes = table.Column<int>(type: "integer", nullable: false),
                    DurationMilliseconds = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReactionItems_MediaFiles_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReactionItems_ReactionPacks_PackId",
                        column: x => x.PackId,
                        principalTable: "ReactionPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserReactionPacks",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserReactionPacks", x => new { x.UserId, x.PackId });
                    table.ForeignKey(
                        name: "FK_UserReactionPacks_ReactionPacks_PackId",
                        column: x => x.PackId,
                        principalTable: "ReactionPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserReactionPacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelAllowedReactions",
                columns: table => new
                {
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelAllowedReactions", x => new { x.GroupId, x.ReactionId });
                    table.ForeignKey(
                        name: "FK_ChannelAllowedReactions_ChatGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ChatGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelAllowedReactions_ReactionItems_ReactionId",
                        column: x => x.ReactionId,
                        principalTable: "ReactionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ReactionPacks",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Description", "IsDefault", "IsEnabled", "Name", "ShareCode" },
                values: new object[] { new Guid("b3000000-0000-0000-0000-000000000001"), new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), null, "Default public emoji reactions", true, true, "Emoji", "default" });

            migrationBuilder.InsertData(
                table: "ReactionItems",
                columns: new[] { "Id", "Code", "CreatedAt", "DurationMilliseconds", "Emoji", "Height", "IsEnabled", "Kind", "MediaFileId", "Name", "PackId", "SizeBytes", "Width" },
                values: new object[,]
                {
                    { new Guid("b3000000-0000-0000-0001-000000000001"), "👍", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "👍", 0, true, 0, null, "👍", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000002"), "👎", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "👎", 0, true, 0, null, "👎", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000003"), "❤️", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "❤️", 0, true, 0, null, "❤️", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000004"), "🔥", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "🔥", 0, true, 0, null, "🔥", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000005"), "🥰", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "🥰", 0, true, 0, null, "🥰", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000006"), "👏", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "👏", 0, true, 0, null, "👏", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000007"), "😁", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "😁", 0, true, 0, null, "😁", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000008"), "🤔", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "🤔", 0, true, 0, null, "🤔", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000009"), "😢", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "😢", 0, true, 0, null, "😢", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000010"), "🎉", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "🎉", 0, true, 0, null, "🎉", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000011"), "🤯", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "🤯", 0, true, 0, null, "🤯", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 },
                    { new Guid("b3000000-0000-0000-0001-000000000012"), "🙏", new DateTime(2026, 10, 3, 0, 0, 0, 0, DateTimeKind.Utc), 0, "🙏", 0, true, 0, null, "🙏", new Guid("b3000000-0000-0000-0000-000000000001"), 0, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_ReactionId",
                table: "MessageReactions",
                column: "ReactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelAllowedReactions_ReactionId",
                table: "ChannelAllowedReactions",
                column: "ReactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionItems_Code",
                table: "ReactionItems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReactionItems_MediaFileId",
                table: "ReactionItems",
                column: "MediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionItems_PackId",
                table: "ReactionItems",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionPacks_CreatedByUserId",
                table: "ReactionPacks",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionPacks_IsDefault",
                table: "ReactionPacks",
                column: "IsDefault",
                unique: true,
                filter: "\"IsDefault\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionPacks_ShareCode",
                table: "ReactionPacks",
                column: "ShareCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserReactionPacks_PackId",
                table: "UserReactionPacks",
                column: "PackId");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReactions_ReactionItems_ReactionId",
                table: "MessageReactions",
                column: "ReactionId",
                principalTable: "ReactionItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            // Link known legacy emoji receipts to the seeded catalogue; preserve unknown codes.
            migrationBuilder.Sql("""
                UPDATE "MessageReactions" AS mr
                SET "ReactionId" = (SELECT ri."Id" FROM "ReactionItems" AS ri
                    WHERE ri."Code" = mr."ReactionCode" AND ri."PackId" = 'b3000000-0000-0000-0000-000000000001')
                WHERE mr."ReactionId" IS NULL AND EXISTS (SELECT 1 FROM "ReactionItems" AS ri
                    WHERE ri."Code" = mr."ReactionCode" AND ri."PackId" = 'b3000000-0000-0000-0000-000000000001');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReactions_ReactionItems_ReactionId",
                table: "MessageReactions");

            migrationBuilder.DropTable(
                name: "ChannelAllowedReactions");

            migrationBuilder.DropTable(
                name: "UserReactionPacks");

            migrationBuilder.DropTable(
                name: "ReactionItems");

            migrationBuilder.DropTable(
                name: "ReactionPacks");

            migrationBuilder.DropIndex(
                name: "IX_MessageReactions_ReactionId",
                table: "MessageReactions");

            migrationBuilder.DropColumn(
                name: "ReactionsVersion",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReactionId",
                table: "MessageReactions");

            migrationBuilder.DropColumn(
                name: "ReactionMode",
                table: "ChatGroups");

            migrationBuilder.DropColumn(
                name: "ReactionPolicyVersion",
                table: "ChatGroups");
        }
    }
}
