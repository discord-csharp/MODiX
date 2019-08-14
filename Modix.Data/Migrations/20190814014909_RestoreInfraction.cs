using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class RestoreInfraction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RestoreActionId",
                table: "Infractions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_RestoreActionId",
                table: "Infractions",
                column: "RestoreActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_ModerationActions_RestoreActionId",
                table: "Infractions",
                column: "RestoreActionId",
                principalTable: "ModerationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_ModerationActions_RestoreActionId",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_RestoreActionId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "RestoreActionId",
                table: "Infractions");
        }
    }
}
