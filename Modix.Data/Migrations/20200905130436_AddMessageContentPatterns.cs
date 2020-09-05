using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class AddMessageContentPatterns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageContentPatterns",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Pattern = table.Column<string>(nullable: false),
                    PatternType = table.Column<string>(nullable: false),
                    GuildId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageContentPatterns", x => x.Id);
                });

            migrationBuilder.Sql("UPDATE public.\"ClaimMappings\" SET \"Claim\"='BypassMessageContentPatternCheck' WHERE \"Claim\"='PostInviteLink';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageContentPatterns");
        }
    }
}
