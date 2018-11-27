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

            migrationBuilder.AddColumn<long>(
                name: "LogMessageId",
                table: "PromotionComments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionComments_DeleteActionId",
                table: "PromotionComments",
                column: "DeleteActionId",
                unique: true);

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

            migrationBuilder.DropIndex(
                name: "IX_PromotionComments_DeleteActionId",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "DeleteActionId",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "LogMessageId",
                table: "PromotionComments");
        }
    }
}
