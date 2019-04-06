using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BehaviourConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Category = table.Column<string>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BehaviourConfigurations", x => x.Id);
                    table.UniqueConstraint("IX_Category_Key", arg => new { arg.Category, arg.Key });
                });

            migrationBuilder.InsertData("BehaviourConfigurations", new[] { "Category", "Key", "Value" }, new[] { "InvitePurging", "IsEnabled", "True" });
            migrationBuilder.InsertData("BehaviourConfigurations", new[] { "Category", "Key", "Value" }, new[] { "InvitePurging", "ExemptRoleIds", "[268470383571632128,155770800392110082,155771334779994112,410138389283602432]" });
            migrationBuilder.InsertData("BehaviourConfigurations", new[] { "Category", "Key", "Value" }, new[] { "InvitePurging", "LoggingChannelId", "380603776412811267" });

            migrationBuilder.CreateTable(
                name: "ModerationConfigs",
                columns: table => new
                {
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    GuildId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    MuteRoleId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationConfigs", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    Username = table.Column<string>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    Nickname = table.Column<string>(nullable: true),
                    FirstSeen = table.Column<DateTimeOffset>(nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionCampaigns",
                columns: table => new
                {
                    PromotionCampaignId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    PromotionForId = table.Column<long>(nullable: false),
                    StartDate = table.Column<DateTimeOffset>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCampaigns", x => x.PromotionCampaignId);
                    table.ForeignKey(
                        name: "FK_PromotionCampaigns_Users_PromotionForId",
                        column: x => x.PromotionForId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionComments",
                columns: table => new
                {
                    PromotionCommentId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    PostedDate = table.Column<DateTimeOffset>(nullable: false),
                    Sentiment = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    PromotionCampaignId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionComments", x => x.PromotionCommentId);
                    table.ForeignKey(
                        name: "FK_PromotionComments_PromotionCampaigns_PromotionCampaignId",
                        column: x => x.PromotionCampaignId,
                        principalTable: "PromotionCampaigns",
                        principalColumn: "PromotionCampaignId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    RoleId = table.Column<long>(nullable: false),
                    Claim = table.Column<int>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false),
                    RescindActionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationActions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Type = table.Column<int>(nullable: false),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    CreatedById = table.Column<long>(nullable: false),
                    RoleClaimId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationActions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfigurationActions_RoleClaims_RoleClaimId",
                        column: x => x.RoleClaimId,
                        principalTable: "RoleClaims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModerationActions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: false),
                    CreatedById = table.Column<long>(nullable: false),
                    InfractionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationActions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Infractions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: true),
                    SubjectId = table.Column<long>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false),
                    RescindActionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Infractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Infractions_ModerationActions_CreateActionId",
                        column: x => x.CreateActionId,
                        principalTable: "ModerationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Infractions_ModerationActions_RescindActionId",
                        column: x => x.RescindActionId,
                        principalTable: "ModerationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Infractions_Users_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationActions_CreatedById",
                table: "ConfigurationActions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationActions_RoleClaimId",
                table: "ConfigurationActions",
                column: "RoleClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_CreateActionId",
                table: "Infractions",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_RescindActionId",
                table: "Infractions",
                column: "RescindActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infractions_SubjectId",
                table: "Infractions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_CreatedById",
                table: "ModerationActions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_InfractionId",
                table: "ModerationActions",
                column: "InfractionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_PromotionForId",
                table: "PromotionCampaigns",
                column: "PromotionForId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionComments_PromotionCampaignId",
                table: "PromotionComments",
                column: "PromotionCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_CreateActionId",
                table: "RoleClaims",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RescindActionId",
                table: "RoleClaims",
                column: "RescindActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleClaims_ConfigurationActions_CreateActionId",
                table: "RoleClaims",
                column: "CreateActionId",
                principalTable: "ConfigurationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleClaims_ConfigurationActions_RescindActionId",
                table: "RoleClaims",
                column: "RescindActionId",
                principalTable: "ConfigurationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationActions_Infractions_InfractionId",
                table: "ModerationActions",
                column: "InfractionId",
                principalTable: "Infractions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_Users_CreatedById",
                table: "ConfigurationActions");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_Users_SubjectId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationActions_Users_CreatedById",
                table: "ModerationActions");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_RoleClaims_RoleClaimId",
                table: "ConfigurationActions");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_ModerationActions_CreateActionId",
                table: "Infractions");

            migrationBuilder.DropForeignKey(
                name: "FK_Infractions_ModerationActions_RescindActionId",
                table: "Infractions");

            migrationBuilder.DropTable(
                name: "BehaviourConfigurations");

            migrationBuilder.DropTable(
                name: "ModerationConfigs");

            migrationBuilder.DropTable(
                name: "PromotionComments");

            migrationBuilder.DropTable(
                name: "PromotionCampaigns");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "ConfigurationActions");

            migrationBuilder.DropTable(
                name: "ModerationActions");

            migrationBuilder.DropTable(
                name: "Infractions");
        }
    }
}
