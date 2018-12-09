using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class DeletedMessageBatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeletedMessages_ModerationActions_CreateActionId",
                table: "DeletedMessages");

            migrationBuilder.AddColumn<long>(
                name: "DeletedMessageBatchId",
                table: "ModerationActions",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CreateActionId",
                table: "DeletedMessages",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<long>(
                name: "BatchId",
                table: "DeletedMessages",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeletedMessageBatches",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CreateActionId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeletedMessageBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeletedMessageBatches_ModerationActions_CreateActionId",
                        column: x => x.CreateActionId,
                        principalTable: "ModerationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_DeletedMessageBatchId",
                table: "ModerationActions",
                column: "DeletedMessageBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedMessages_BatchId",
                table: "DeletedMessages",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DeletedMessageBatches_CreateActionId",
                table: "DeletedMessageBatches",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeletedMessages_DeletedMessageBatches_BatchId",
                table: "DeletedMessages",
                column: "BatchId",
                principalTable: "DeletedMessageBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeletedMessages_ModerationActions_CreateActionId",
                table: "DeletedMessages",
                column: "CreateActionId",
                principalTable: "ModerationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationActions_DeletedMessageBatches_DeletedMessageBatch~",
                table: "ModerationActions",
                column: "DeletedMessageBatchId",
                principalTable: "DeletedMessageBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeletedMessages_DeletedMessageBatches_BatchId",
                table: "DeletedMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DeletedMessages_ModerationActions_CreateActionId",
                table: "DeletedMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationActions_DeletedMessageBatches_DeletedMessageBatch~",
                table: "ModerationActions");

            migrationBuilder.DropTable(
                name: "DeletedMessageBatches");

            migrationBuilder.DropIndex(
                name: "IX_ModerationActions_DeletedMessageBatchId",
                table: "ModerationActions");

            migrationBuilder.DropIndex(
                name: "IX_DeletedMessages_BatchId",
                table: "DeletedMessages");

            migrationBuilder.DropColumn(
                name: "DeletedMessageBatchId",
                table: "ModerationActions");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "DeletedMessages");

            migrationBuilder.AlterColumn<long>(
                name: "CreateActionId",
                table: "DeletedMessages",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeletedMessages_ModerationActions_CreateActionId",
                table: "DeletedMessages",
                column: "CreateActionId",
                principalTable: "ModerationActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
