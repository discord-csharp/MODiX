using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class final : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_DiscordUser_AuthorId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordUser",
                table: "DiscordUser");

            migrationBuilder.RenameTable(
                name: "DiscordUser",
                newName: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Guilds",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_OwnerId",
                table: "Guilds",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_Users_OwnerId",
                table: "Guilds",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_AuthorId",
                table: "Messages",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_Users_OwnerId",
                table: "Guilds");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_AuthorId",
                table: "Messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Guilds_OwnerId",
                table: "Guilds");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "DiscordUser");

            migrationBuilder.AlterColumn<long>(
                name: "OwnerId",
                table: "Guilds",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordUser",
                table: "DiscordUser",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_DiscordUser_AuthorId",
                table: "Messages",
                column: "AuthorId",
                principalTable: "DiscordUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
