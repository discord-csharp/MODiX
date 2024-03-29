using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class MessageEntityColumnTypeCompatibility : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "StarboardEntryId",
                table: "Messages",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "Messages",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20)");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "Messages",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20)");

            migrationBuilder.AlterColumn<long>(
                name: "AuthorId",
                table: "Messages",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20)");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Messages",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "StarboardEntryId",
                table: "Messages",
                type: "numeric(20)",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "Messages",
                type: "numeric(20)",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "ChannelId",
                table: "Messages",
                type: "numeric(20)",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "AuthorId",
                table: "Messages",
                type: "numeric(20)",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "Messages",
                type: "numeric(20)",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
