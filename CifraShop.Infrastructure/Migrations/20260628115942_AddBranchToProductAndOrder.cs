using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CifraShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchToProductAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Orders");
        }
    }
}
