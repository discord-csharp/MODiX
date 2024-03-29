using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class ModerationRescindDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeleteActionId",
                table: "Infractions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_DeleteActionId",
                table: "Infractions",
                column: "DeleteActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_ModerationActions_DeleteActionId",
                table: "Infractions",
                column: "DeleteActionId",
                principalTable: "ModerationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_ModerationActions_DeleteActionId",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_DeleteActionId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "DeleteActionId",
                table: "Infractions");
        }
    }
}
