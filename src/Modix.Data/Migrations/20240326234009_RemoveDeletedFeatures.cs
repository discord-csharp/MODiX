using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modix.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeletedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE ""DesignatedRoleMappings"" SET ""DeleteActionId"" = NULL WHERE ""Type"" IN ('RestrictedMentionability', 'ResctrictedMentionability')");
            migrationBuilder.Sql(@"DELETE FROM ""ConfigurationActions"" WHERE ""Id"" IN (392, 393)");

            migrationBuilder.Sql(@"delete from ""ConfigurationActions"" where ""DesignatedChannelMappingId"" in (select ""Id"" from ""DesignatedChannelMappings"" where ""Type"" = 'GiveawayLog')");
            migrationBuilder.Sql(@"delete from ""DesignatedChannelMappings"" where ""Type"" = 'GiveawayLog'");

            migrationBuilder.Sql(@"delete from ""ConfigurationActions"" where ""DesignatedRoleMappingId"" in (select ""Id"" from ""DesignatedRoleMappings"" where ""Type"" = 'RestrictedMentionability')");
            migrationBuilder.Sql(@"delete from ""DesignatedRoleMappings"" where ""Type"" = 'RestrictedMentionability'");

            migrationBuilder.Sql(@"delete from ""ConfigurationActions"" where ""DesignatedRoleMappingId"" in (select ""Id"" from ""DesignatedRoleMappings"" where ""Type"" = 'ResctrictedMentionability')");
            migrationBuilder.Sql(@"delete from ""DesignatedRoleMappings"" where ""Type"" = 'ResctrictedMentionability'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
