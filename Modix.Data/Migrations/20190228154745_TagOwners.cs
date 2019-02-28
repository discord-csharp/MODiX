using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class TagOwners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagOwners",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: true),
                    RoleId = table.Column<long>(nullable: true),
                    TagName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagOwners_GuildRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "GuildRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TagOwners_GuildUsers_GuildId_UserId",
                        columns: x => new { x.GuildId, x.UserId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagOwners_GuildId",
                table: "TagOwners",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_TagOwners_RoleId",
                table: "TagOwners",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TagOwners_TagName",
                table: "TagOwners",
                column: "TagName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagOwners_UserId",
                table: "TagOwners",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TagOwners_GuildId_UserId",
                table: "TagOwners",
                columns: new[] { "GuildId", "UserId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagOwners");
        }
    }
}
