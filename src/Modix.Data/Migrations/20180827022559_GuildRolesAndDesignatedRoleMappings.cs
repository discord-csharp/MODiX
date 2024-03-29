using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class GuildRolesAndDesignatedRoleMappings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DesignatedRoleMappingId",
                table: "ConfigurationActions",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuildRoles",
                columns: table => new
                {
                    RoleId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Position = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildRoles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "DesignatedRoleMappings",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    RoleId = table.Column<long>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false),
                    DeleteActionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignatedRoleMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignatedRoleMappings_ConfigurationActions_CreateActionId",
                        column: x => x.CreateActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignatedRoleMappings_ConfigurationActions_DeleteActionId",
                        column: x => x.DeleteActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DesignatedRoleMappings_GuildRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "GuildRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationActions_DesignatedRoleMappingId",
                table: "ConfigurationActions",
                column: "DesignatedRoleMappingId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignatedRoleMappings_CreateActionId",
                table: "DesignatedRoleMappings",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignatedRoleMappings_DeleteActionId",
                table: "DesignatedRoleMappings",
                column: "DeleteActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignatedRoleMappings_RoleId",
                table: "DesignatedRoleMappings",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_DesignatedRoleMappings_DesignatedRoleM~",
                table: "ConfigurationActions",
                column: "DesignatedRoleMappingId",
                principalTable: "DesignatedRoleMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_DesignatedRoleMappings_DesignatedRoleM~",
                table: "ConfigurationActions");

            migrationBuilder.DropTable(
                name: "DesignatedRoleMappings");

            migrationBuilder.DropTable(
                name: "GuildRoles");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationActions_DesignatedRoleMappingId",
                table: "ConfigurationActions");

            migrationBuilder.DropColumn(
                name: "DesignatedRoleMappingId",
                table: "ConfigurationActions");
        }
    }
}
