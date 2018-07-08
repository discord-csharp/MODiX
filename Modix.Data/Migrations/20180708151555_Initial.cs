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
                name: "DiscordUsers",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Username = table.Column<string>(nullable: false),
                    Nickname = table.Column<string>(nullable: true),
                    Discriminator = table.Column<long>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    FirstSeen = table.Column<DateTimeOffset>(nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(nullable: false),
                    AvatarUrl = table.Column<string>(nullable: true),
                    IsBot = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DiscordGuilds",
                columns: table => new
                {
                    GuildId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    OwnerUserId = table.Column<long>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGuilds", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_DiscordGuilds_DiscordUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Infractions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Type = table.Column<int>(nullable: false),
                    SubjectId = table.Column<long>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: true),
                    Reason = table.Column<string>(nullable: false),
                    IsRescinded = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Infractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Infractions_DiscordUsers_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "DiscordUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionCampaigns",
                columns: table => new
                {
                    PromotionCampaignId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    PromotionForUserId = table.Column<long>(nullable: false),
                    StartDate = table.Column<DateTimeOffset>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionCampaigns", x => x.PromotionCampaignId);
                    table.ForeignKey(
                        name: "FK_PromotionCampaigns_DiscordUsers_PromotionForUserId",
                        column: x => x.PromotionForUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelLimits",
                columns: table => new
                {
                    ChannelLimitID = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ChannelId = table.Column<long>(nullable: false),
                    ModuleName = table.Column<string>(nullable: true),
                    GuildId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelLimits", x => x.ChannelLimitID);
                    table.ForeignKey(
                        name: "FK_ChannelLimits_DiscordGuilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "DiscordGuilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscordMessages",
                columns: table => new
                {
                    DiscordMessageId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DiscordId = table.Column<long>(nullable: false),
                    DiscordGuildGuildId = table.Column<long>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    AuthorUserId = table.Column<long>(nullable: true),
                    Attachments = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordMessages", x => x.DiscordMessageId);
                    table.ForeignKey(
                        name: "FK_DiscordMessages_DiscordUsers_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscordMessages_DiscordGuilds_DiscordGuildGuildId",
                        column: x => x.DiscordGuildGuildId,
                        principalTable: "DiscordGuilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuildConfig",
                columns: table => new
                {
                    GuildConfigId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    AdminRoleId = table.Column<long>(nullable: false),
                    ModeratorRoleId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildConfig", x => x.GuildConfigId);
                    table.ForeignKey(
                        name: "FK_GuildConfig_DiscordGuilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "DiscordGuilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModerationActions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Type = table.Column<int>(nullable: false),
                    InfractionId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    CreatedById = table.Column<long>(nullable: false),
                    Comment = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationActions_DiscordUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "DiscordUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModerationActions_Infractions_InfractionId",
                        column: x => x.InfractionId,
                        principalTable: "Infractions",
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

            migrationBuilder.CreateIndex(
                name: "IX_ChannelLimits_GuildId",
                table: "ChannelLimits",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGuilds_OwnerUserId",
                table: "DiscordGuilds",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessages_AuthorUserId",
                table: "DiscordMessages",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordMessages_DiscordGuildGuildId",
                table: "DiscordMessages",
                column: "DiscordGuildGuildId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildConfig_GuildId",
                table: "GuildConfig",
                column: "GuildId",
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
                name: "IX_PromotionCampaigns_PromotionForUserId",
                table: "PromotionCampaigns",
                column: "PromotionForUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionComments_PromotionCampaignId",
                table: "PromotionComments",
                column: "PromotionCampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelLimits");

            migrationBuilder.DropTable(
                name: "DiscordMessages");

            migrationBuilder.DropTable(
                name: "GuildConfig");

            migrationBuilder.DropTable(
                name: "ModerationActions");

            migrationBuilder.DropTable(
                name: "PromotionComments");

            migrationBuilder.DropTable(
                name: "DiscordGuilds");

            migrationBuilder.DropTable(
                name: "Infractions");

            migrationBuilder.DropTable(
                name: "PromotionCampaigns");

            migrationBuilder.DropTable(
                name: "DiscordUsers");
        }
    }
}
