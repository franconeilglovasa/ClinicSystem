using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicSystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAISuggestionManualEditTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAt",
                table: "AISuggestions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EditedByUserId",
                table: "AISuggestions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AISuggestions_EditedByUserId",
                table: "AISuggestions",
                column: "EditedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AISuggestions_AspNetUsers_EditedByUserId",
                table: "AISuggestions",
                column: "EditedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AISuggestions_AspNetUsers_EditedByUserId",
                table: "AISuggestions");

            migrationBuilder.DropIndex(
                name: "IX_AISuggestions_EditedByUserId",
                table: "AISuggestions");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                table: "AISuggestions");

            migrationBuilder.DropColumn(
                name: "EditedByUserId",
                table: "AISuggestions");
        }
    }
}
