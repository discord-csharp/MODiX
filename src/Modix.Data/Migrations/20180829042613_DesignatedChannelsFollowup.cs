using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class DesignatedChannelsFollowup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'DesignatedChannelMappingCreated'
                  WHERE `Type` = 'ModerationLogChannelMappingCreated'"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'DesignatedChannelMappingDeleted'
                  WHERE `Type` = 'ModerationLogChannelMappingDeleted'"
                .Replace('`', '"'));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
