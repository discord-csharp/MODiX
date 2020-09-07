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

            migrationBuilder.CreateIndex(
                name: "IX_MessageContentPatterns_GuildId_Pattern",
                table: "MessageContentPatterns",
                columns: new[] { "GuildId", "Pattern" },
                unique: true);

            migrationBuilder.Sql("UPDATE public.\"ClaimMappings\" SET \"Claim\"='BypassMessageContentPatternCheck' WHERE \"Claim\"='PostInviteLink';");

            migrationBuilder.Sql(
                "INSERT INTO \"MessageContentPatterns\" (\"Pattern\", \"PatternType\", \"GuildId\") SELECT DISTINCT '(https?://)?(www\\.)?(discord\\.(gg|io|me|li)|discord(app)?\\.com/invite)/(?<Code>\\w+)' as \"Pattern\", 'Blocked' as \"PatternType\", \"GuildId\" as \"GuildId\" FROM \"GuildRoles\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageContentPatterns");

            migrationBuilder.Sql("UPDATE public.\"ClaimMappings\" SET \"Claim\"='PostInviteLink' WHERE \"Claim\"='BypassMessageContentPatternCheck';");
        }
    }
}
