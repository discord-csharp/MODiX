using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class CreateMessageEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20)", nullable: false),
                    AuthorId = table.Column<decimal>(type: "numeric(20)", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GuildId_AuthorId",
                table: "Messages",
                columns: new[] { "GuildId", "AuthorId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
