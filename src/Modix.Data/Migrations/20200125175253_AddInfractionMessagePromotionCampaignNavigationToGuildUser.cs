using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class AddInfractionMessagePromotionCampaignNavigationToGuildUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages",
                column: "ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_GuildChannels_ChannelId",
                table: "Messages",
                column: "ChannelId",
                principalTable: "GuildChannels",
                principalColumn: "ChannelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_GuildUsers_GuildId_AuthorId",
                table: "Messages",
                columns: new[] { "GuildId", "AuthorId" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_GuildChannels_ChannelId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_GuildUsers_GuildId_AuthorId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages");
        }
    }
}
