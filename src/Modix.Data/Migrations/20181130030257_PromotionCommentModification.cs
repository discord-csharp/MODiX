using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class PromotionCommentModification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromotionActions_PromotionComments_CommentId",
                table: "PromotionActions");

            migrationBuilder.DropIndex(
                name: "IX_PromotionActions_CommentId",
                table: "PromotionActions");

            migrationBuilder.RenameColumn(
                name: "CommentId",
                table: "PromotionActions",
                newName: "NewCommentId");

            migrationBuilder.AddColumn<long>(
                name: "ModifyActionId",
                table: "PromotionComments",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OldCommentId",
                table: "PromotionActions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionComments_ModifyActionId",
                table: "PromotionComments",
                column: "ModifyActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionActions_NewCommentId",
                table: "PromotionActions",
                column: "NewCommentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionActions_OldCommentId",
                table: "PromotionActions",
                column: "OldCommentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionActions_PromotionComments_NewCommentId",
                table: "PromotionActions",
                column: "NewCommentId",
                principalTable: "PromotionComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionActions_PromotionComments_OldCommentId",
                table: "PromotionActions",
                column: "OldCommentId",
                principalTable: "PromotionComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionComments_PromotionActions_ModifyActionId",
                table: "PromotionComments",
                column: "ModifyActionId",
                principalTable: "PromotionActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromotionActions_PromotionComments_NewCommentId",
                table: "PromotionActions");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionActions_PromotionComments_OldCommentId",
                table: "PromotionActions");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionComments_PromotionActions_ModifyActionId",
                table: "PromotionComments");

            migrationBuilder.DropIndex(
                name: "IX_PromotionComments_ModifyActionId",
                table: "PromotionComments");

            migrationBuilder.DropIndex(
                name: "IX_PromotionActions_NewCommentId",
                table: "PromotionActions");

            migrationBuilder.DropIndex(
                name: "IX_PromotionActions_OldCommentId",
                table: "PromotionActions");

            migrationBuilder.DropColumn(
                name: "ModifyActionId",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "OldCommentId",
                table: "PromotionActions");

            migrationBuilder.RenameColumn(
                name: "NewCommentId",
                table: "PromotionActions",
                newName: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionActions_CommentId",
                table: "PromotionActions",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionActions_PromotionComments_CommentId",
                table: "PromotionActions",
                column: "CommentId",
                principalTable: "PromotionComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
