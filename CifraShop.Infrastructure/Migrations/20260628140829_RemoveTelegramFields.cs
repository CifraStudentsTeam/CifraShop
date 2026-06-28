using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CifraShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTelegramFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramBotToken",
                table: "NotificationSettings");

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "NotificationSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramBotToken",
                table: "NotificationSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramChatId",
                table: "NotificationSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
