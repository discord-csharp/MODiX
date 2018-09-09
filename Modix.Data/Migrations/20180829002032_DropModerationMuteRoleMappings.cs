using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class DropModerationMuteRoleMappings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"INSERT INTO `GuildRoles` (
                      `RoleId`,
                      `GuildId`,
                      `Name`,
                      `Position`)
                  SELECT
                      `MuteRoleId`,
                      `GuildId`,
                      '[UNKNOWN ROLE]',
                      0
                  FROM `ModerationMuteRoleMappings` mmrm
                  WHERE NOT EXISTS (
                      SELECT *
                      FROM `GuildRoles` gr
                      WHERE gr.`RoleId` = mmrm.`MuteRoleId`)"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"INSERT INTO `DesignatedRoleMappings` (
                      `Id`,
                      `GuildId`,
                      `RoleId`,
                      `Type`,
                      `CreateActionId`,
                      `DeleteActionId`)
                  SELECT
                      nextval('`DesignatedRoleMappings_Id_seq`'),
                      `GuildId`,
                      `MuteRoleId`,
                      'ModerationMute',
                      `CreateActionId`,
                      `DeleteActionId`
                  FROM `ModerationMuteRoleMappings`"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions` AS ca
                  SET `Type` = 'DesignatedRoleMappingCreated',
                      `DesignatedRoleMappingId` = (
                          SELECT drm.`Id`
                          FROM `DesignatedRoleMappings` AS drm
                          WHERE drm.`CreateActionId` = ca.`Id`),
                      `ModerationMuteRoleMappingId` = NULL
                  WHERE `Type` = 'ModerationMuteRoleMappingCreated'"
              .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions` AS ca
                  SET `Type` = 'DesignatedRoleMappingDeleted',
                      `DesignatedRoleMappingId` = (
                          SELECT drm.`Id`
                          FROM `DesignatedRoleMappings` AS drm
                          WHERE drm.`DeleteActionId` = ca.`Id`),
                      `ModerationMuteRoleMappingId` = NULL
                  WHERE `Type` = 'ModerationMuteRoleMappingDeleted'"
              .Replace('`', '"'));

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_ModerationMuteRoleMappings_ModerationM~",
                table: "ConfigurationActions");

            migrationBuilder.DropTable(
                name: "ModerationMuteRoleMappings");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationActions_ModerationMuteRoleMappingId",
                table: "ConfigurationActions");

            migrationBuilder.DropColumn(
                name: "ModerationMuteRoleMappingId",
                table: "ConfigurationActions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ModerationMuteRoleMappingId",
                table: "ConfigurationActions",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModerationMuteRoleMappings",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    MuteRoleId = table.Column<long>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false),
                    DeleteActionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationMuteRoleMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationMuteRoleMappings_ConfigurationActions_CreateActio~",
                        column: x => x.CreateActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModerationMuteRoleMappings_ConfigurationActions_DeleteActio~",
                        column: x => x.DeleteActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationActions_ModerationMuteRoleMappingId",
                table: "ConfigurationActions",
                column: "ModerationMuteRoleMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationMuteRoleMappings_CreateActionId",
                table: "ModerationMuteRoleMappings",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationMuteRoleMappings_DeleteActionId",
                table: "ModerationMuteRoleMappings",
                column: "DeleteActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_ModerationMuteRoleMappings_ModerationM~",
                table: "ConfigurationActions",
                column: "ModerationMuteRoleMappingId",
                principalTable: "ModerationMuteRoleMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                @"INSERT INTO `ModerationMuteRoleMappings` (
                      `Id`,
                      `GuildId`,
                      `MuteRoleId`,
                      `CreateActionId`,
                      `DeleteActionId`)
                  SELECT
                      nextval('`ModerationMuteRoleMappings_Id_seq`'),
                      `GuildId`,
                      `RoleId`,
                      `CreateActionId`,
                      `DeleteActionId`
                  FROM `DesignatedRoleMappings`
                  WHERE `Type` = 'ModerationMute'"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions` AS ca
                  SET `Type` = 'ModerationMuteRoleMappingCreated',
                      `ModerationMuteRoleMappingId` = (
                          SELECT mmrm.`Id`
                          FROM `ModerationMuteRoleMappings` mmrm
                          WHERE mmrm.`CreateActionId` = ca.`Id`),
                      `DesignatedRoleMappingId` = NULL
                  WHERE EXISTS(
                      SELECT*
                      FROM `ModerationMuteRoleMappings` mmrm
                      WHERE mmrm.`CreateActionId` = ca.`Id`)"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions` AS ca
                  SET `Type` = 'ModerationMuteRoleMappingDeleted',
                      `ModerationMuteRoleMappingId` = (
                          SELECT mmrm.`Id`
                          FROM `ModerationMuteRoleMappings` mmrm
                          WHERE mmrm.`DeleteActionId` = ca.`Id`),
                      `DesignatedRoleMappingId` = NULL
                  WHERE EXISTS(
                      SELECT*
                      FROM `ModerationMuteRoleMappings` mmrm
                      WHERE mmrm.`DeleteActionId` = ca.`Id`)"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"DELETE FROM `DesignatedRoleMappings`
                  WHERE `Type` = 'ModerationMute'"
                    .Replace('`', '"'));
        }
    }
}
