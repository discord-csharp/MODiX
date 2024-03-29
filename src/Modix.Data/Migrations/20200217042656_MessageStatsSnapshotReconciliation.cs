using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class MessageStatsSnapshotReconciliation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageCountByDate",
                columns: table => new
                {
                    Date = table.Column<DateTime>(nullable: false),
                    MessageCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "MessageCountPerChannel",
                columns: table => new
                {
                    ChannelId = table.Column<decimal>(nullable: false),
                    ChannelName = table.Column<string>(nullable: false),
                    MessageCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageCountByDate");

            migrationBuilder.DropTable(
                name: "MessageCountPerChannel");
        }
    }
}
