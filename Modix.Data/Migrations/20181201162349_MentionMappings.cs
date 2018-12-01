using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class MentionMappings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MentionMappings",
                columns: table => new
                {
                    RoleId = table.Column<long>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    Mentionability = table.Column<string>(nullable: false),
                    MinimumRankId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentionMappings", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_MentionMappings_GuildRoles_MinimumRankId",
                        column: x => x.MinimumRankId,
                        principalTable: "GuildRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentionMappings_GuildRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "GuildRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MentionMappings_MinimumRankId",
                table: "MentionMappings",
                column: "MinimumRankId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MentionMappings");
        }
    }
}
