using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Modix.Data.Migrations
{
    public partial class BetterStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildConfigs",
                table: "GuildConfigs");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Game",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsBot",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Mention",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Messages");

            migrationBuilder.RenameTable(
                name: "GuildConfigs",
                newName: "GuildConfig");

            migrationBuilder.RenameColumn(
                name: "MessageId",
                table: "Messages",
                newName: "DiscordId");

            migrationBuilder.RenameColumn(
                name: "DiscriminatorValue",
                table: "Messages",
                newName: "DiscordGuildId");

            migrationBuilder.AlterColumn<int>(
                name: "GuildId",
                table: "Bans",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildConfig",
                table: "GuildConfig",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ConfigId = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DiscordId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    OwnerId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guilds_GuildConfig_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "GuildConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscordUser",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AvatarUrl = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DiscordId = table.Column<long>(nullable: false),
                    IsBot = table.Column<bool>(nullable: false),
                    Nickname = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUser", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bans_GuildId",
                table: "Bans",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AuthorId",
                table: "Messages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DiscordGuildId",
                table: "Messages",
                column: "DiscordGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_ConfigId",
                table: "Guilds",
                column: "ConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bans_Guilds_GuildId",
                table: "Bans",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_DiscordUser_AuthorId",
                table: "Messages",
                column: "AuthorId",
                principalTable: "DiscordUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Guilds_DiscordGuildId",
                table: "Messages",
                column: "DiscordGuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bans_Guilds_GuildId",
                table: "Bans");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_DiscordUser_AuthorId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Guilds_DiscordGuildId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "DiscordUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildConfig",
                table: "GuildConfig");

            migrationBuilder.DropIndex(
                name: "IX_Bans_GuildId",
                table: "Bans");

            migrationBuilder.DropIndex(
                name: "IX_Messages_AuthorId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_DiscordGuildId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Messages");

            migrationBuilder.RenameTable(
                name: "GuildConfig",
                newName: "GuildConfigs");

            migrationBuilder.RenameColumn(
                name: "DiscordId",
                table: "Messages",
                newName: "MessageId");

            migrationBuilder.RenameColumn(
                name: "DiscordGuildId",
                table: "Messages",
                newName: "DiscriminatorValue");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Bans",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarId",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Messages",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Game",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "Messages",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsBot",
                table: "Messages",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Mention",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Messages",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildConfigs",
                table: "GuildConfigs",
                column: "Id");
        }
    }
}
