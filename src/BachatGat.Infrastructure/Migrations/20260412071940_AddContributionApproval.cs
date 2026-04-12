using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BachatGat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContributionApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Contributions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Contributions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Contributions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Contributions_ApprovedByUserId",
                table: "Contributions",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_Users_ApprovedByUserId",
                table: "Contributions",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_Users_ApprovedByUserId",
                table: "Contributions");

            migrationBuilder.DropIndex(
                name: "IX_Contributions_ApprovedByUserId",
                table: "Contributions");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Contributions");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Contributions");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Contributions");
        }
    }
}
