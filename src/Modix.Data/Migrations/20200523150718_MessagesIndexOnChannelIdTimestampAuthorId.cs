using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class MessagesIndexOnChannelIdTimestampAuthorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId_Timestamp_AuthorId",
                table: "Messages",
                columns: new[] { "ChannelId", "Timestamp", "AuthorId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_ChannelId_Timestamp_AuthorId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages",
                column: "ChannelId");
        }
    }
}
