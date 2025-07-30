using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Govor.Data.Migrations
{
    /// <inheritdoc />
    public partial class CryptKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DeviceInfo",
                table: "UserSessions",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvitationId",
                table: "GroupMemberships",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "MemberSince",
                table: "GroupMemberships",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "UserCryptoSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserSessionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PublicIdentityKey = table.Column<byte[]>(type: "longblob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCryptoSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCryptoSessions_UserSessions_UserSessionId",
                        column: x => x.UserSessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OneTimePreKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserCryptoSessionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PublicKey = table.Column<byte[]>(type: "longblob", nullable: false),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimePreKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneTimePreKeys_UserCryptoSessions_UserCryptoSessionId",
                        column: x => x.UserCryptoSessionId,
                        principalTable: "UserCryptoSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SignedPreKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserCryptoSessionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PublicSignedPreKey = table.Column<byte[]>(type: "longblob", nullable: false),
                    SignedPreKeySignature = table.Column<byte[]>(type: "longblob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignedPreKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignedPreKeys_UserCryptoSessions_UserCryptoSessionId",
                        column: x => x.UserCryptoSessionId,
                        principalTable: "UserCryptoSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePreKeys_UploadedAt",
                table: "OneTimePreKeys",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePreKeys_UserCryptoSessionId_IsUsed",
                table: "OneTimePreKeys",
                columns: new[] { "UserCryptoSessionId", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_SignedPreKeys_UserCryptoSessionId",
                table: "SignedPreKeys",
                column: "UserCryptoSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCryptoSessions_UserSessionId",
                table: "UserCryptoSessions",
                column: "UserSessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OneTimePreKeys");

            migrationBuilder.DropTable(
                name: "SignedPreKeys");

            migrationBuilder.DropTable(
                name: "UserCryptoSessions");

            migrationBuilder.DropColumn(
                name: "MemberSince",
                table: "GroupMemberships");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceInfo",
                table: "UserSessions",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvitationId",
                table: "GroupMemberships",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");
        }
    }
}
