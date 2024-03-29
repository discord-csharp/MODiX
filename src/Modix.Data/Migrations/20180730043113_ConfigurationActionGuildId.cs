using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class ConfigurationActionGuildId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "ConfigurationActions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions` AS ca
                  SET `GuildId` = (
                      SELECT cm.`GuildId`
                      FROM `ClaimMappings` AS cm
                      WHERE cm.`Id` = ca.`ClaimMappingId`)
                  WHERE ca.`GuildId` = 0
                      AND ca.`ClaimMappingId` IS NOT NULL;"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions` AS ca
                  SET `GuildId` = (
                      SELECT mmrm.`GuildId`
                      FROM `ModerationMuteRoleMappings` AS mmrm
                      WHERE mmrm.`Id` = ca.`ModerationMuteRoleMappingId`)
                  WHERE ca.`GuildId` = 0
                      AND ca.`ModerationMuteRoleMappingId` IS NOT NULL;"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions` AS ca
                  SET `GuildId` = (
                      SELECT mlcm.`GuildId`
                      FROM `ModerationLogChannelMappings` AS mlcm
                      WHERE mlcm.`Id` = ca.`ModerationLogChannelMappingId`)
                  WHERE ca.`GuildId` = 0
                      AND ca.`ModerationLogChannelMappingId` IS NOT NULL;"
                    .Replace('`', '"'));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "ConfigurationActions");
        }
    }
}
