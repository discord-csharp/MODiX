using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class DesignatedChannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_ModerationLogChannelMappings_Moderatio~",
                table: "ConfigurationActions");

            migrationBuilder.RenameColumn(
                name: "CreateActionID",
                table: "ModerationMuteRoleMappings",
                newName: "CreateActionId");

            migrationBuilder.RenameIndex(
                name: "IX_ModerationMuteRoleMappings_CreateActionID",
                table: "ModerationMuteRoleMappings",
                newName: "IX_ModerationMuteRoleMappings_CreateActionId");

            migrationBuilder.RenameColumn(
                name: "ModerationLogChannelMappingId",
                table: "ConfigurationActions",
                newName: "DesignatedChannelMappingId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationActions_ModerationLogChannelMappingId",
                table: "ConfigurationActions",
                newName: "IX_ConfigurationActions_DesignatedChannelMappingId");

            migrationBuilder.CreateTable(
                name: "DesignatedChannelMappings",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false),
                    ChannelDesignation = table.Column<string>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false),
                    DeleteActionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignatedChannelMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignatedChannelMappings_ConfigurationActions_CreateAction~",
                        column: x => x.CreateActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignatedChannelMappings_ConfigurationActions_DeleteAction~",
                        column: x => x.DeleteActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DesignatedChannelMappings_CreateActionId",
                table: "DesignatedChannelMappings",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignatedChannelMappings_DeleteActionId",
                table: "DesignatedChannelMappings",
                column: "DeleteActionId",
                unique: true);

            migrationBuilder.Sql(
                @"INSERT INTO `DesignatedChannelMappings` (`Id`, `GuildId`, `ChannelId`, `ChannelDesignation`, `CreateActionId`)
                  SELECT `Id`, `GuildId`, `LogChannelId`, 'ModerationLog', `CreateActionID`
                  FROM `ModerationLogChannelMappings`;"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"select setval('`DesignatedChannelMappings_Id_seq`', (SELECT MAX(`Id`) FROM `DesignatedChannelMappings`))"
                .Replace('`', '"'));

            migrationBuilder.DropTable(
                name: "ModerationLogChannelMappings");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_DesignatedChannelMappings_DesignatedCh~",
                table: "ConfigurationActions",
                column: "DesignatedChannelMappingId",
                principalTable: "DesignatedChannelMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_DesignatedChannelMappings_DesignatedCh~",
                table: "ConfigurationActions");

            migrationBuilder.RenameColumn(
                name: "CreateActionId",
                table: "ModerationMuteRoleMappings",
                newName: "CreateActionID");

            migrationBuilder.RenameIndex(
                name: "IX_ModerationMuteRoleMappings_CreateActionId",
                table: "ModerationMuteRoleMappings",
                newName: "IX_ModerationMuteRoleMappings_CreateActionID");

            migrationBuilder.RenameColumn(
                name: "DesignatedChannelMappingId",
                table: "ConfigurationActions",
                newName: "ModerationLogChannelMappingId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationActions_DesignatedChannelMappingId",
                table: "ConfigurationActions",
                newName: "IX_ConfigurationActions_ModerationLogChannelMappingId");

            migrationBuilder.CreateTable(
                name: "ModerationLogChannelMappings",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CreateActionID = table.Column<long>(nullable: false),
                    DeleteActionId = table.Column<long>(nullable: true),
                    GuildId = table.Column<long>(nullable: false),
                    LogChannelId = table.Column<long>(nullable: false)
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
                name: "IX_ModerationLogChannelMappings_CreateActionID",
                table: "ModerationLogChannelMappings",
                column: "CreateActionID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationLogChannelMappings_DeleteActionId",
                table: "ModerationLogChannelMappings",
                column: "DeleteActionId",
                unique: true);

            migrationBuilder.Sql(
                @"INSERT INTO `ModerationLogChannelMappings` (`Id`, `GuildId`, `LogChannelId`, `CreateActionID`)
                  SELECT `Id`, `GuildId`, `ChannelId`, `CreateActionId`
                  FROM `DesignatedChannelMappings` 
                  WHERE `ChannelDesignation` = 'ModerationLog';"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"DELETE FROM `ConfigurationActions`
                  WHERE `Id` IN
                    (
                        SELECT `CreateActionId` FROM `DesignatedChannelMappings`
                        WHERE `ChannelDesignation` != 'ModerationLog'
                    );"
                .Replace('`', '"'));

            migrationBuilder.DropTable(
                name: "DesignatedChannelMappings");

            migrationBuilder.Sql(
                @"select setval('`ModerationLogChannelMappings_Id_seq`', (SELECT MAX(`Id`) FROM `ModerationLogChannelMappings`))"
                .Replace('`', '"'));

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_ModerationLogChannelMappings_Moderatio~",
                table: "ConfigurationActions",
                column: "ModerationLogChannelMappingId",
                principalTable: "ModerationLogChannelMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'ModerationLogChannelMappingCreated'
                  WHERE `Type` = 'DesignatedChannelMappingCreated'"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ConfigurationActions`
                  SET `Type` = 'ModerationLogChannelMappingDeleted'
                  WHERE `Type` = 'DesignatedChannelMappingDeleted'"
                .Replace('`', '"'));
        }
    }
}
