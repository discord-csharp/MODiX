using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class RemoveBogusKeylessEntityTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EmojiStatsDto table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.

            // GuildEmojiStats table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.

            // GuildUserParticipationStatistics table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.

            migrationBuilder.DropTable(
                name: "MessageCountByDate");

            migrationBuilder.DropTable(
                name: "MessageCountPerChannel");

            // PerUserMessageCount table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.

            // SingleEmojiStatsDto table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // EmojiStatsDto table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.

            // GuildEmojiStats table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.

            // GuildUserParticipationStatistics table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.

            migrationBuilder.CreateTable(
                name: "MessageCountByDate",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MessageCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "MessageCountPerChannel",
                columns: table => new
                {
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelName = table.Column<string>(type: "text", nullable: false),
                    MessageCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                });

            // PerUserMessageCount table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.

            // SingleEmojiStatsDto table was never actually create in any prior migration, even though the ModelSnapshots say it should be there.
        }
    }
}
