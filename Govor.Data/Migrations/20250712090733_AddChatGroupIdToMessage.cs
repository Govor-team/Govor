using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Govor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChatGroupIdToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ChatGroupId",
                table: "Messages",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatGroupId",
                table: "Messages",
                column: "ChatGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_ChatGroups_ChatGroupId",
                table: "Messages",
                column: "ChatGroupId",
                principalTable: "ChatGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_ChatGroups_ChatGroupId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChatGroupId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ChatGroupId",
                table: "Messages");
        }
    }
}
