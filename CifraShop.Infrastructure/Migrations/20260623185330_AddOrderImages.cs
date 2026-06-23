using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CifraShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<int>(
                name: "Balance",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Products",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Sum",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "OrderItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "OrderItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.CreateTable(
                name: "OrderImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderImages_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderImages_OrderId",
                table: "OrderImages",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderImages");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<short>(
                name: "Balance",
                table: "Users",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "Quantity",
                table: "Products",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "Price",
                table: "Products",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "Sum",
                table: "Orders",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "Quantity",
                table: "OrderItems",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "Price",
                table: "OrderItems",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "ImageUrl", "Name", "Price", "Quantity", "StatusProduct" },
                values: new object[,]
                {
                    { 1, "Линейный блонкот на 60 листов", "notebook_a5.jpg", "Блокнот A5", (short)250, (short)50, "InStock" },
                    { 2, "Шариковая ручка синего цвета", "pen_blue.jpg", "Ручка шариковая", (short)35, (short)200, "InStock" },
                    { 3, "Вместительный порфель для документов", "portfolio.jpg", "Портфель студента", (short)1200, (short)15, "InStock" },
                    { 4, "USB-флешка для хранения данных", "usb_32gb.jpg", "USB-флешка 32GB", (short)450, (short)0, "OutOfStock" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Balance", "Branch", "Email", "Password", "UserRole" },
                values: new object[,]
                {
                    { 1, null, null, "admin@cifrashop.ru", "admin123", "Admin" },
                    { 2, (short)5000, null, "student@cifrashop.ru", "student123", "Student" }
                });
        }
    }
}
