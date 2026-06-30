using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CifraShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPasswordColumnLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Initial migration created Password as nvarchar(16), but SHA256 hashes are 44 chars.
            // The model/snapshot was updated to nvarchar(128) without a migration being generated.
            // This raw SQL fixes the actual DB schema to match the model.
            migrationBuilder.Sql("ALTER TABLE [Users] ALTER COLUMN [Password] nvarchar(128) NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [Users] ALTER COLUMN [Password] nvarchar(16) NOT NULL");
        }
    }
}
