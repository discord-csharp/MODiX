using Microsoft.EntityFrameworkCore.Migrations;
using Modix.Data.Models;

namespace Modix.Data.Migrations
{
    public partial class MessageLogBehaviourDefaults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("BehaviourConfigurations", new[] { "Category", "Key", "Value" }, new[] { BehaviourCategory.MessageLogging.ToString(), "LoggingChannelId", "380603419410432002" });
            migrationBuilder.InsertData("BehaviourConfigurations", new[] { "Category", "Key", "Value" }, new[] { BehaviourCategory.MessageLogging.ToString(), "OldMessageAgeLimit", "7" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
