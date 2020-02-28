using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class AddPromotionDialogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromotionDialogs",
                columns: table => new
                {
                    MessageId = table.Column<long>(nullable: false),
                    CampaignId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionDialogs", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_PromotionDialogs_PromotionCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "PromotionCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionDialogs_CampaignId",
                table: "PromotionDialogs",
                column: "CampaignId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromotionDialogs");
        }
    }
}
