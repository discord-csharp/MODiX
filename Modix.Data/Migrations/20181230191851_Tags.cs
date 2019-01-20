using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class Tags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false),
                    DeleteActionId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    Uses = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagActions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    GuildId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTimeOffset>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    CreatedById = table.Column<long>(nullable: false),
                    NewTagId = table.Column<long>(nullable: true),
                    OldTagId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagActions_Tags_NewTagId",
                        column: x => x.NewTagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TagActions_Tags_OldTagId",
                        column: x => x.OldTagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TagActions_GuildUsers_GuildId_CreatedById",
                        columns: x => new { x.GuildId, x.CreatedById },
                        principalTable: "GuildUsers",
                        principalColumns: new[] { "GuildId", "UserId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagActions_NewTagId",
                table: "TagActions",
                column: "NewTagId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagActions_OldTagId",
                table: "TagActions",
                column: "OldTagId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagActions_GuildId_CreatedById",
                table: "TagActions",
                columns: new[] { "GuildId", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_CreateActionId",
                table: "Tags",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_DeleteActionId",
                table: "Tags",
                column: "DeleteActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildId",
                table: "Tags",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_TagActions_CreateActionId",
                table: "Tags",
                column: "CreateActionId",
                principalTable: "TagActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_TagActions_DeleteActionId",
                table: "Tags",
                column: "DeleteActionId",
                principalTable: "TagActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TagActions_Tags_NewTagId",
                table: "TagActions");

            migrationBuilder.DropForeignKey(
                name: "FK_TagActions_Tags_OldTagId",
                table: "TagActions");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TagActions");
        }
    }
}
