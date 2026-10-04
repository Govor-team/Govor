using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Govor.Domain.Migrations
{
    /// <inheritdoc />
    public partial class PrimitiveGroupsAndRequiredChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMemberships_ChatGroups_ChatGroupId",
                table: "GroupMemberships");

            migrationBuilder.DropIndex(
                name: "IX_GroupMemberships_ChatGroupId",
                table: "GroupMemberships");

            migrationBuilder.DropIndex(
                name: "IX_GroupMemberships_GroupId",
                table: "GroupMemberships");

            migrationBuilder.DropIndex(
                name: "IX_GroupAdmins_GroupId",
                table: "GroupAdmins");

            migrationBuilder.DropColumn(
                name: "ChatGroupId",
                table: "GroupMemberships");

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "GroupInvitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UsedCount",
                table: "GroupInvitations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ChatGroups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "ChatGroups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServerCommunitySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    RequiredChannelId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllowLeave = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerCommunitySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerCommunitySettings_ChatGroups_RequiredChannelId",
                        column: x => x.RequiredChannelId,
                        principalTable: "ChatGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ServerCommunitySettings",
                columns: new[] { "Id", "AllowLeave", "RequiredChannelId" },
                values: new object[] { 1, true, null });

            // Preserve bans when consolidating legacy duplicate memberships; remove orphan rows
            // before adding user foreign keys. Invitation codes that were ambiguous are revoked.
            migrationBuilder.Sql("""
                DELETE FROM "GroupMemberships" WHERE "UserId" NOT IN (SELECT "Id" FROM "Users");
                DELETE FROM "GroupAdmins" WHERE "UserId" NOT IN (SELECT "Id" FROM "Users");
                DELETE FROM "GroupMemberships" WHERE "Id" IN (
                    SELECT "Id" FROM (SELECT "Id", ROW_NUMBER() OVER (
                        PARTITION BY "GroupId", "UserId" ORDER BY "IsBanned" DESC, "MemberSince", "Id") AS rn
                        FROM "GroupMemberships") AS duplicates WHERE rn > 1);
                DELETE FROM "GroupAdmins" WHERE "Id" IN (
                    SELECT "Id" FROM (SELECT "Id", ROW_NUMBER() OVER (
                        PARTITION BY "GroupId", "UserId" ORDER BY "Id") AS rn
                        FROM "GroupAdmins") AS duplicates WHERE rn > 1);
                DELETE FROM "GroupAdmins" AS a WHERE NOT EXISTS (
                    SELECT 1 FROM "GroupMemberships" AS m WHERE m."GroupId" = a."GroupId"
                        AND m."UserId" = a."UserId" AND NOT m."IsBanned");
                UPDATE "GroupInvitations" SET "IsRevoked" = TRUE,
                    "InvitationCode" = REPLACE(CAST("Id" AS TEXT), '-', '')
                    WHERE "InvitationCode" IN (SELECT "InvitationCode" FROM "GroupInvitations"
                        GROUP BY "InvitationCode" HAVING COUNT(*) > 1);
                UPDATE "GroupInvitations" AS i SET "UsedCount" = (
                    SELECT COUNT(*) FROM "GroupMemberships" AS m WHERE m."InvitationId" = i."Id");
                UPDATE "ChatGroups" AS g SET "OwnerUserId" = (
                    SELECT m."UserId" FROM "GroupMemberships" AS m
                    WHERE m."GroupId" = g."Id" AND NOT m."IsBanned"
                    ORDER BY CASE WHEN EXISTS (SELECT 1 FROM "GroupAdmins" AS a
                        WHERE a."GroupId" = m."GroupId" AND a."UserId" = m."UserId") THEN 0 ELSE 1 END,
                        m."MemberSince", m."UserId" LIMIT 1);
                UPDATE "ChatGroups" AS g SET "CreatedAt" = COALESCE((
                    SELECT MIN(m."MemberSince") FROM "GroupMemberships" AS m
                    WHERE m."GroupId" = g."Id"), CURRENT_TIMESTAMP);
                """);
            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_GroupId_UserId",
                table: "GroupMemberships",
                columns: new[] { "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_UserId",
                table: "GroupMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupInvitations_InvitationCode",
                table: "GroupInvitations",
                column: "InvitationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupAdmins_GroupId_UserId",
                table: "GroupAdmins",
                columns: new[] { "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupAdmins_UserId",
                table: "GroupAdmins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroups_OwnerUserId",
                table: "ChatGroups",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerCommunitySettings_RequiredChannelId",
                table: "ServerCommunitySettings",
                column: "RequiredChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatGroups_Users_OwnerUserId",
                table: "ChatGroups",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupAdmins_Users_UserId",
                table: "GroupAdmins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMemberships_Users_UserId",
                table: "GroupMemberships",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatGroups_Users_OwnerUserId",
                table: "ChatGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupAdmins_Users_UserId",
                table: "GroupAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMemberships_Users_UserId",
                table: "GroupMemberships");

            migrationBuilder.DropTable(
                name: "ServerCommunitySettings");

            migrationBuilder.DropIndex(
                name: "IX_GroupMemberships_GroupId_UserId",
                table: "GroupMemberships");

            migrationBuilder.DropIndex(
                name: "IX_GroupMemberships_UserId",
                table: "GroupMemberships");

            migrationBuilder.DropIndex(
                name: "IX_GroupInvitations_InvitationCode",
                table: "GroupInvitations");

            migrationBuilder.DropIndex(
                name: "IX_GroupAdmins_GroupId_UserId",
                table: "GroupAdmins");

            migrationBuilder.DropIndex(
                name: "IX_GroupAdmins_UserId",
                table: "GroupAdmins");

            migrationBuilder.DropIndex(
                name: "IX_ChatGroups_OwnerUserId",
                table: "ChatGroups");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "UsedCount",
                table: "GroupInvitations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ChatGroups");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "ChatGroups");

            migrationBuilder.AddColumn<Guid>(
                name: "ChatGroupId",
                table: "GroupMemberships",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_ChatGroupId",
                table: "GroupMemberships",
                column: "ChatGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMemberships_GroupId",
                table: "GroupMemberships",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupAdmins_GroupId",
                table: "GroupAdmins",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMemberships_ChatGroups_ChatGroupId",
                table: "GroupMemberships",
                column: "ChatGroupId",
                principalTable: "ChatGroups",
                principalColumn: "Id");
        }
    }
}
