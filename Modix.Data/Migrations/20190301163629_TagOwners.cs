using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class TagOwners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OwnerRoleId",
                table: "Tags",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OwnerUserId",
                table: "Tags",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_OwnerRoleId",
                table: "Tags",
                column: "OwnerRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_OwnerUserId",
                table: "Tags",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildId_OwnerUserId",
                table: "Tags",
                columns: new[] { "GuildId", "OwnerUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_GuildRoles_OwnerRoleId",
                table: "Tags",
                column: "OwnerRoleId",
                principalTable: "GuildRoles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_GuildUsers_GuildId_OwnerUserId",
                table: "Tags",
                columns: new[] { "GuildId", "OwnerUserId" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"update ""Tags"" set ""OwnerUserId"" = (select ""TagActions"".""CreatedById"" from ""TagActions"" where ""TagActions"".""Id"" = ""Tags"".""CreateActionId"")");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_GuildRoles_OwnerRoleId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_GuildUsers_GuildId_OwnerUserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_OwnerRoleId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_OwnerUserId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_GuildId_OwnerUserId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "OwnerRoleId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Tags");
        }
    }
}
