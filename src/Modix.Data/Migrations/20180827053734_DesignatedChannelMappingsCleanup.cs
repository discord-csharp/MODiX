using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class DesignatedChannelMappingsCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChannelDesignation",
                table: "DesignatedChannelMappings",
                newName: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DesignatedChannelMappings_ChannelId",
                table: "DesignatedChannelMappings",
                column: "ChannelId");

            migrationBuilder.Sql(
                @"INSERT INTO `GuildChannels` (
                      `ChannelId`,
                      `GuildId`,
                      `Name`)
                  SELECT
                      `ChannelId`,
                      `GuildId`,
                      '[UNKNOWN]'
                  FROM `DesignatedChannelMappings` dcm
                  WHERE NOT EXISTS (
                      SELECT*
                      FROM `GuildChannels` gc
                      WHERE gc.`ChannelId` = dcm.`ChannelId`)"
                    .Replace('`', '"'));

            migrationBuilder.AddForeignKey(
                name: "FK_DesignatedChannelMappings_GuildChannels_ChannelId",
                table: "DesignatedChannelMappings",
                column: "ChannelId",
                principalTable: "GuildChannels",
                principalColumn: "ChannelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'DesignatedChannelMappingCreate'
                  WHERE `Type` = 'ChannelDesignationCreate'"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'DesignatedChannelMappingRead'
                  WHERE `Type` = 'ChannelDesignationRead'"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'DesignatedChannelMappingDelete'
                  WHERE `Type` = 'ChannelDesignationDelete'"
                    .Replace('`', '"'));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DesignatedChannelMappings_GuildChannels_ChannelId",
                table: "DesignatedChannelMappings");

            migrationBuilder.DropIndex(
                name: "IX_DesignatedChannelMappings_ChannelId",
                table: "DesignatedChannelMappings");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "DesignatedChannelMappings",
                newName: "ChannelDesignation");

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'ChannelDesignationCreate'
                  WHERE `Type` = 'DesignatedChannelMappingCreate'"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'ChannelDesignationRead'
                  WHERE `Type` = 'DesignatedChannelMappingRead'"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'ChannelDesignationDelete'
                  WHERE `Type` = 'DesignatedChannelMappingDelete'"
                    .Replace('`', '"'));
        }
    }
}
