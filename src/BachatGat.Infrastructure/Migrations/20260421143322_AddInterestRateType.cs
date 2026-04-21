using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BachatGat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInterestRateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InterestRateType",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InterestRateType",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestRateType",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "InterestRateType",
                table: "Groups");
        }
    }
}
