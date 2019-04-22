using Microsoft.EntityFrameworkCore.Migrations;

namespace Modix.Data.Migrations
{
    public partial class UserInfoIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_GuildId",
                table: "PromotionCampaigns",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_Outcome",
                table: "PromotionCampaigns",
                column: "Outcome");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionCampaigns_SubjectId",
                table: "PromotionCampaigns",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Timestamp",
                table: "Messages",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PromotionCampaigns_GuildId",
                table: "PromotionCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_PromotionCampaigns_Outcome",
                table: "PromotionCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_PromotionCampaigns_SubjectId",
                table: "PromotionCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_Messages_Timestamp",
                table: "Messages");
        }
    }
}
