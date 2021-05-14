
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class Issue100PromotionsRework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DELETE
                  FROM `PromotionComments`"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"DELETE
                  FROM `PromotionCampaigns`"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ClaimMappings`
                  SET `Claim` = 'PromotionsCreateCampaign'
                  WHERE `Claim` = 'PromotionCreate'"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ClaimMappings`
                  SET `Claim` = 'PromotionsComment'
                  WHERE `Claim` = 'PromotionComment'"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ClaimMappings`
                  SET `Claim` = 'PromotionsCloseCampaign'
                  WHERE `Claim` = 'PromotionExecute'"
                .Replace('`', '"'));

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionCampaigns_Users_PromotionForId",
                table: "PromotionCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionComments_PromotionCampaigns_PromotionCampaignId",
                table: "PromotionComments");

            migrationBuilder.DropIndex(
                name: "IX_PromotionComments_PromotionCampaignId",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "PostedDate",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "PromotionCampaignId",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PromotionCampaigns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PromotionCampaigns");

            migrationBuilder.RenameColumn(
                name: "PromotionCommentId",
                table: "PromotionComments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PromotionForId",
                table: "PromotionCampaigns",
                newName: "TargetRoleId");

            migrationBuilder.RenameColumn(
                name: "PromotionCampaignId",
                table: "PromotionCampaigns",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_PromotionCampaigns_PromotionForId",
                table: "PromotionCampaigns",
                newName: "IX_PromotionCampaigns_TargetRoleId");

            migrationBuilder.AlterColumn<string>(
                name: "Sentiment",
                table: "PromotionComments",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<long>(
                name: "CampaignId",
                table: "PromotionComments",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "PromotionComments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "CreateActionId",
                table: "PromotionComments",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CloseActionId",
                table: "PromotionCampaigns",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreateActionId",
                table: "PromotionCampaigns",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "PromotionCampaigns",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "PromotionCampaigns",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SubjectId",
                table: "PromotionCampaigns",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "PromotionActions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    CreatedById = table.Column<long>(nullable: false),
                    CampaignId = table.Column<long>(nullable: true),
                    CommentId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionActions_PromotionCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "PromotionCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionActions_PromotionComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "PromotionComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionActions_GuildUsers_GuildId_CreatedById",
                        columns: x => new { x.GuildId, x.CreatedById },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionComments_CampaignId",
                table: "PromotionComments",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionComments_CreateActionId",
                table: "PromotionComments",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_CloseActionId",
                table: "PromotionCampaigns",
                column: "CloseActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_CreateActionId",
                table: "PromotionCampaigns",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_GuildId_SubjectId",
                table: "PromotionCampaigns",
                columns: new[] { "GuildId", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionActions_CampaignId",
                table: "PromotionActions",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionActions_CommentId",
                table: "PromotionActions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionActions_GuildId_CreatedById",
                table: "PromotionActions",
                columns: new[] { "GuildId", "CreatedById" });

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionCampaigns_PromotionActions_CloseActionId",
                table: "PromotionCampaigns",
                column: "CloseActionId",
                principalTable: "PromotionActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionCampaigns_PromotionActions_CreateActionId",
                table: "PromotionCampaigns",
                column: "CreateActionId",
                principalTable: "PromotionActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionCampaigns_GuildRoles_TargetRoleId",
                table: "PromotionCampaigns",
                column: "TargetRoleId",
                principalTable: "GuildRoles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionCampaigns_GuildUsers_GuildId_SubjectId",
                table: "PromotionCampaigns",
                columns: new[] { "GuildId", "SubjectId" },
                principalTable: "GuildUsers",
                principalColumns: new[] { "GuildId", "UserId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionComments_PromotionCampaigns_CampaignId",
                table: "PromotionComments",
                column: "CampaignId",
                principalTable: "PromotionCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionComments_PromotionActions_CreateActionId",
                table: "PromotionComments",
                column: "CreateActionId",
                principalTable: "PromotionActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DELETE
                  FROM `PromotionActions`"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"DELETE
                  FROM `PromotionComments`"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"DELETE
                  FROM `PromotionCampaigns`"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ClaimMappings`
                  SET `Claim` = 'PromotionCreate'
                  WHERE `Claim` = 'PromotionsCreateCampaign'"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ClaimMappings`
                  SET `Claim` = 'PromotionComment'
                  WHERE `Claim` = 'PromotionsComment'"
                .Replace('`', '"'));

            migrationBuilder.Sql(
                @"UPDATE `ClaimMappings`
                  SET `Claim` = 'PromotionExecute'
                  WHERE `Claim` = 'PromotionsCloseCampaign'"
                .Replace('`', '"'));

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionCampaigns_PromotionActions_CloseActionId",
                table: "PromotionCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionCampaigns_PromotionActions_CreateActionId",
                table: "PromotionCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionCampaigns_GuildRoles_TargetRoleId",
                table: "PromotionCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionCampaigns_GuildUsers_GuildId_SubjectId",
                table: "PromotionCampaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionComments_PromotionCampaigns_CampaignId",
                table: "PromotionComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionComments_PromotionActions_CreateActionId",
                table: "PromotionComments");

            migrationBuilder.DropTable(
                name: "PromotionActions");

            migrationBuilder.DropIndex(
                name: "IX_PromotionComments_CampaignId",
                table: "PromotionComments");

            migrationBuilder.DropIndex(
                name: "IX_PromotionComments_CreateActionId",
                table: "PromotionComments");

            migrationBuilder.DropIndex(
                name: "IX_PromotionCampaigns_CloseActionId",
                table: "PromotionCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_PromotionCampaigns_CreateActionId",
                table: "PromotionCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_PromotionCampaigns_GuildId_SubjectId",
                table: "PromotionCampaigns");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "CreateActionId",
                table: "PromotionComments");

            migrationBuilder.DropColumn(
                name: "CloseActionId",
                table: "PromotionCampaigns");

            migrationBuilder.DropColumn(
                name: "CreateActionId",
                table: "PromotionCampaigns");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "PromotionCampaigns");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "PromotionCampaigns");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "PromotionCampaigns");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PromotionComments",
                newName: "PromotionCommentId");

            migrationBuilder.RenameColumn(
                name: "TargetRoleId",
                table: "PromotionCampaigns",
                newName: "PromotionForId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PromotionCampaigns",
                newName: "PromotionCampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_PromotionCampaigns_TargetRoleId",
                table: "PromotionCampaigns",
                newName: "IX_PromotionCampaigns_PromotionForId");

            migrationBuilder.AlterColumn<int>(
                name: "Sentiment",
                table: "PromotionComments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "PromotionComments",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PostedDate",
                table: "PromotionComments",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<long>(
                name: "PromotionCampaignId",
                table: "PromotionComments",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartDate",
                table: "PromotionCampaigns",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PromotionCampaigns",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionComments_PromotionCampaignId",
                table: "PromotionComments",
                column: "PromotionCampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionCampaigns_Users_PromotionForId",
                table: "PromotionCampaigns",
                column: "PromotionForId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionComments_PromotionCampaigns_PromotionCampaignId",
                table: "PromotionComments",
                column: "PromotionCampaignId",
                principalTable: "PromotionCampaigns",
                principalColumn: "PromotionCampaignId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
