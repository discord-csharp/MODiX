using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class DeletedMessagesCreateActionRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeletedMessages_CreateActionId",
                table: "DeletedMessages");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedMessages_CreateActionId",
                table: "DeletedMessages",
                column: "CreateActionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeletedMessages_CreateActionId",
                table: "DeletedMessages");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedMessages_CreateActionId",
                table: "DeletedMessages",
                column: "CreateActionId");
        }
    }
}
