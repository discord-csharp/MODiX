using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class AllowDeletingPromotionComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeleteActionId",
                table: "PromotionComments",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PromotionCommentMessages",
                columns: table => new
                {
                    MessageId = table.Column<long>(nullable: false),
                    CommentId = table.Column<long>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCommentMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_PromotionCommentMessages_GuildChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "GuildChannels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromotionCommentMessages_PromotionComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "PromotionComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionComments_DeleteActionId",
                table: "PromotionComments",
                column: "DeleteActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCommentMessages_ChannelId",
                table: "PromotionCommentMessages",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCommentMessages_CommentId",
                table: "PromotionCommentMessages",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionComments_PromotionActions_DeleteActionId",
                table: "PromotionComments",
                column: "DeleteActionId",
                principalTable: "PromotionActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromotionComments_PromotionActions_DeleteActionId",
                table: "PromotionComments");

            migrationBuilder.DropTable(
                name: "PromotionCommentMessages");

            migrationBuilder.DropIndex(
                name: "IX_PromotionComments_DeleteActionId",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "DeleteActionId",
                table: "PromotionComments");
        }
    }
}
