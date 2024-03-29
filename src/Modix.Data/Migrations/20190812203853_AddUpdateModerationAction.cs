using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class AddUpdateModerationAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalInfractionReason",
                table: "ModerationActions",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdateActionId",
                table: "Infractions",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_UpdateActionId",
                table: "Infractions",
                column: "UpdateActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Infractions_ModerationActions_UpdateActionId",
                table: "Infractions",
                column: "UpdateActionId",
                principalTable: "ModerationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_ModerationActions_UpdateActionId",
                table: "Infractions");

            migrationBuilder.DropIndex(
                name: "IX_Infractions_UpdateActionId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "OriginalInfractionReason",
                table: "ModerationActions");

            migrationBuilder.DropColumn(
                name: "UpdateActionId",
                table: "Infractions");
        }
    }
}
