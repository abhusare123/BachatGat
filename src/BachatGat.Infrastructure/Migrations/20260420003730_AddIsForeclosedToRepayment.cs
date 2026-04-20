using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BachatGat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsForeclosedToRepayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForeclosed",
                table: "LoanRepayments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForeclosed",
                table: "LoanRepayments");
        }
    }
}
