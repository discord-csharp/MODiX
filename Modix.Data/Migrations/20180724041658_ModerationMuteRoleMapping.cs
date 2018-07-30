using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class ModerationMuteRoleMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModerationConfigs");

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
                    CreateActionID = table.Column<long>(nullable: false),
                    DeleteActionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationMuteRoleMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationMuteRoleMappings_ConfigurationActions_CreateActio~",
                        column: x => x.CreateActionID,
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
                name: "IX_ModerationMuteRoleMappings_CreateActionID",
                table: "ModerationMuteRoleMappings",
                column: "CreateActionID",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "ModerationConfigs",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    MuteRoleId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationConfigs", x => x.GuildId);
                });
        }
    }
}
