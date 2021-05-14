using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Modix.Data.Migrations
{
    public partial class Authorization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_RoleClaims_RoleClaimId",
                table: "ConfigurationActions");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "RoleClaimId",
                table: "ConfigurationActions",
                newName: "ClaimMappingId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationActions_RoleClaimId",
                table: "ConfigurationActions",
                newName: "IX_ConfigurationActions_ClaimMappingId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ModerationActions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Infractions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ConfigurationActions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "ClaimMappings",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Type = table.Column<string>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    RoleId = table.Column<long>(nullable: true),
                    UserId = table.Column<long>(nullable: true),
                    Claim = table.Column<string>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false),
                    RescindActionId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimMappings_ConfigurationActions_CreateActionId",
                        column: x => x.CreateActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClaimMappings_ConfigurationActions_RescindActionId",
                        column: x => x.RescindActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClaimMappings_CreateActionId",
                table: "ClaimMappings",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaimMappings_RescindActionId",
                table: "ClaimMappings",
                column: "RescindActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_ClaimMappings_ClaimMappingId",
                table: "ConfigurationActions",
                column: "ClaimMappingId",
                principalTable: "ClaimMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationActions_ClaimMappings_ClaimMappingId",
                table: "ConfigurationActions");

            migrationBuilder.DropTable(
                name: "ClaimMappings");

            migrationBuilder.RenameColumn(
                name: "ClaimMappingId",
                table: "ConfigurationActions",
                newName: "RoleClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationActions_ClaimMappingId",
                table: "ConfigurationActions",
                newName: "IX_ConfigurationActions_RoleClaimId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Created",
                table: "Users",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "ModerationActions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Infractions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "ConfigurationActions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Claim = table.Column<int>(nullable: false),
                    CreateActionId = table.Column<long>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    RescindActionId = table.Column<long>(nullable: true),
                    RoleId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_ConfigurationActions_CreateActionId",
                        column: x => x.CreateActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleClaims_ConfigurationActions_RescindActionId",
                        column: x => x.RescindActionId,
                        principalTable: "ConfigurationActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_CreateActionId",
                table: "RoleClaims",
                column: "CreateActionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RescindActionId",
                table: "RoleClaims",
                column: "RescindActionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationActions_RoleClaims_RoleClaimId",
                table: "ConfigurationActions",
                column: "RoleClaimId",
                principalTable: "RoleClaims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
