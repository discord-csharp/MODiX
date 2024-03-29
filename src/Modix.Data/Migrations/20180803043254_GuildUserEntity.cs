using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class GuildUserEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildUsers",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    Nickname = table.Column<string>(nullable: true),
                    FirstSeen = table.Column<DateTimeOffset>(nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildUsers", x => new { x.GuildId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GuildUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                @"INSERT INTO `GuildUsers` (
                      `GuildId`,
                      `UserId`,
                      `Nickname`,
                      `FirstSeen`,
                      `LastSeen`)
                  SELECT
                      cm.`GuildId`,
                      u.`Id`,
                      u.`Nickname`,
                      u.`FirstSeen`,
                      u.`LastSeen`
                  FROM `Users` AS u
                  CROSS JOIN(
                      SELECT DISTINCT `GuildId`
                      FROM `ClaimMappings`) AS cm"
                    .Replace('`', '"'));

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_Users_CreatedById",
                table: "ConfigurationActions");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Users_SubjectId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationActions_Users_CreatedById",
                table: "ModerationActions");

            migrationBuilder.DropIndex(
                name: "IX_ModerationActions_CreatedById",
                table: "ModerationActions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_SubjectId",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationActions_CreatedById",
                table: "ConfigurationActions");

            migrationBuilder.DropColumn(
                name: "FirstSeen",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastSeen",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_GuildId_CreatedById",
                table: "ModerationActions",
                columns: new[] { "GuildId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_GuildId_SubjectId",
                table: "Infractions",
                columns: new[] { "GuildId", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationActions_GuildId_CreatedById",
                table: "ConfigurationActions",
                columns: new[] { "GuildId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_GuildUsers_UserId",
                table: "GuildUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_GuildUsers_GuildId_CreatedById",
                table: "ConfigurationActions",
                columns: new[] { "GuildId", "CreatedById" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_GuildUsers_GuildId_SubjectId",
                table: "Infractions",
                columns: new[] { "GuildId", "SubjectId" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationActions_GuildUsers_GuildId_CreatedById",
                table: "ModerationActions",
                columns: new[] { "GuildId", "CreatedById" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstSeen",
                table: "Users",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSeen",
                table: "Users",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "Users",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE `Users2` AS u
                  SET `Nickname` = gu.`Nickname`,
                      `FirstSeen` = gu.`FirstSeen`,
                      `LastSeen` = gu.`LastSeen`
                  FROM `GuildUsers` AS gu
                  WHERE gu.`UserId` = u.`Id`
                      AND gu.`GuildId` = (
                          SELECT DISTINCT `GuildId`
                          FROM `GuildUsers`
                          LIMIT 1)"
                    .Replace('`', '"'));

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_GuildUsers_GuildId_CreatedById",
                table: "ConfigurationActions");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_GuildUsers_GuildId_SubjectId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationActions_GuildUsers_GuildId_CreatedById",
                table: "ModerationActions");

            migrationBuilder.DropTable(
                name: "GuildUsers");

            migrationBuilder.DropIndex(
                name: "IX_ModerationActions_GuildId_CreatedById",
                table: "ModerationActions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_GuildId_SubjectId",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationActions_GuildId_CreatedById",
                table: "ConfigurationActions");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_CreatedById",
                table: "ModerationActions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_SubjectId",
                table: "Infractions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationActions_CreatedById",
                table: "ConfigurationActions",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_Users_CreatedById",
                table: "ConfigurationActions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_Users_SubjectId",
                table: "Infractions",
                column: "SubjectId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationActions_Users_CreatedById",
                table: "ModerationActions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
