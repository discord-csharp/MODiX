using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class ModerationRework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "ModerationActions");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Infractions");

            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "Infractions",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Infractions",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Infractions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Infractions");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "ModerationActions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Created",
                table: "Infractions",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
