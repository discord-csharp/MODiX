using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class EmojiStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Emoji",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    ChannelId = table.Column<long>(nullable: false),
                    MessageId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    EmojiId = table.Column<long>(nullable: true),
                    EmojiName = table.Column<string>(nullable: false),
                    IsAnimated = table.Column<bool>(nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(nullable: false),
                    UsageType = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emoji", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Emoji_EmojiId",
                table: "Emoji",
                column: "EmojiId");

            migrationBuilder.CreateIndex(
                name: "IX_Emoji_EmojiName",
                table: "Emoji",
                column: "EmojiName");

            migrationBuilder.CreateIndex(
                name: "IX_Emoji_GuildId",
                table: "Emoji",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Emoji_MessageId",
                table: "Emoji",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Emoji_Timestamp",
                table: "Emoji",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Emoji_UsageType",
                table: "Emoji",
                column: "UsageType");

            migrationBuilder.CreateIndex(
                name: "IX_Emoji_UserId",
                table: "Emoji",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emoji");
        }
    }
}
