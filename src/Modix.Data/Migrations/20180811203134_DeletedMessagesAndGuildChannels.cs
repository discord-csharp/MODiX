using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class DeletedMessagesAndGuildChannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeletedMessageId",
                table: "ModerationActions",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuildChannels",
                columns: table => new
                {
                    ChannelId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildChannels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "DeletedMessages",
                columns: table => new
                {
                    MessageId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false),
                    AuthorId = table.Column<long>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Reason = table.Column<string>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_DeletedMessages_GuildChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "GuildChannels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeletedMessages_ModerationActions_CreateActionId",
                        column: x => x.CreateActionId,
                        principalTable: "ModerationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeletedMessages_GuildUsers_GuildId_AuthorId",
                        columns: x => new { x.GuildId, x.AuthorId },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_DeletedMessageId",
                table: "ModerationActions",
                column: "DeletedMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedMessages_ChannelId",
                table: "DeletedMessages",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedMessages_CreateActionId",
                table: "DeletedMessages",
                column: "CreateActionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedMessages_GuildId_AuthorId",
                table: "DeletedMessages",
                columns: new[] { "GuildId", "AuthorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationActions_DeletedMessages_DeletedMessageId",
                table: "ModerationActions",
                column: "DeletedMessageId",
                principalTable: "DeletedMessages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                @"UPDATE `ClaimMappings`
                  SET `Claim` = 'ModerationDeleteInfraction'
                  WHERE `Claim` = 'ModerationDelete'"
                    .Replace('`', '"'));

            migrationBuilder.Sql(
                @"DELETE
                  FROM `BehaviourConfigurations`
                  WHERE `Category` = 'InvitePurging'"
                    .Replace('`', '"'));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationActions_DeletedMessages_DeletedMessageId",
                table: "ModerationActions");

            migrationBuilder.DropTable(
                name: "DeletedMessages");

            migrationBuilder.DropTable(
                name: "GuildChannels");

            migrationBuilder.DropIndex(
                name: "IX_ModerationActions_DeletedMessageId",
                table: "ModerationActions");

            migrationBuilder.DropColumn(
                name: "DeletedMessageId",
                table: "ModerationActions");

            migrationBuilder.Sql(
                @"UPDATE `ClaimMappings`
                  SET `Claim` = 'ModerationDelete'
                  WHERE `Claim` = 'ModerationDeleteInfraction'"
                    .Replace('`', '"'));

            migrationBuilder.InsertData("BehaviourConfigurations", new[] { "Category", "Key", "Value" }, new[] { "InvitePurging", "IsEnabled", "True" });
            migrationBuilder.InsertData("BehaviourConfigurations", new[] { "Category", "Key", "Value" }, new[] { "InvitePurging", "ExemptRoleIds", "[268470383571632128,155770800392110082,155771334779994112,410138389283602432]" });
            migrationBuilder.InsertData("BehaviourConfigurations", new[] { "Category", "Key", "Value" }, new[] { "InvitePurging", "LoggingChannelId", "380603776412811267" });
        }
    }
}
