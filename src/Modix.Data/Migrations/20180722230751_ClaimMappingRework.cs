using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class ClaimMappingRework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimMappings_ConfigurationActions_RescindActionId",
                table: "ClaimMappings");

            migrationBuilder.RenameColumn(
                name: "RescindActionId",
                table: "ClaimMappings",
                newName: "DeleteActionId");

            migrationBuilder.RenameIndex(
                name: "IX_ClaimMappings_RescindActionId",
                table: "ClaimMappings",
                newName: "IX_ClaimMappings_DeleteActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimMappings_ConfigurationActions_DeleteActionId",
                table: "ClaimMappings",
                column: "DeleteActionId",
                principalTable: "ConfigurationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClaimMappings_ConfigurationActions_DeleteActionId",
                table: "ClaimMappings");

            migrationBuilder.RenameColumn(
                name: "DeleteActionId",
                table: "ClaimMappings",
                newName: "RescindActionId");

            migrationBuilder.RenameIndex(
                name: "IX_ClaimMappings_DeleteActionId",
                table: "ClaimMappings",
                newName: "IX_ClaimMappings_RescindActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClaimMappings_ConfigurationActions_RescindActionId",
                table: "ClaimMappings",
                column: "RescindActionId",
                principalTable: "ConfigurationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
