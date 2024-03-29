using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class ModerationChannelLogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "ModerationActions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql(
                @"UPDATE `ModerationActions` AS ma
                  SET `GuildId` = (
                      SELECT i.`GuildId`
                      FROM `Infractions` AS i
                      WHERE i.`Id` = ma.`InfractionId`)
                  WHERE ma.`GuildId` = 0
                      AND ma.`InfractionId` IS NOT NULL;"
                    .Replace('`', '"'));

            migrationBuilder.AddColumn<long>(
                name: "ModerationLogChannelMappingId",
                table: "ConfigurationActions",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModerationLogChannelMappings",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    LogChannelId = table.Column<long>(nullable: false),
                    CreateActionID = table.Column<long>(nullable: false),
                    DeleteActionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationLogChannelMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationLogChannelMappings_ConfigurationActions_CreateAct~",
                        column: x => x.CreateActionID,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModerationLogChannelMappings_ConfigurationActions_DeleteAct~",
                        column: x => x.DeleteActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationActions_ModerationLogChannelMappingId",
                table: "ConfigurationActions",
                column: "ModerationLogChannelMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationLogChannelMappings_CreateActionID",
                table: "ModerationLogChannelMappings",
                column: "CreateActionID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationLogChannelMappings_DeleteActionId",
                table: "ModerationLogChannelMappings",
                column: "DeleteActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_ModerationLogChannelMappings_Moderatio~",
                table: "ConfigurationActions",
                column: "ModerationLogChannelMappingId",
                principalTable: "ModerationLogChannelMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_ModerationLogChannelMappings_Moderatio~",
                table: "ConfigurationActions");

            migrationBuilder.DropTable(
                name: "ModerationLogChannelMappings");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationActions_ModerationLogChannelMappingId",
                table: "ConfigurationActions");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "ModerationActions");

            migrationBuilder.DropColumn(
                name: "ModerationLogChannelMappingId",
                table: "ConfigurationActions");
        }
    }
}
