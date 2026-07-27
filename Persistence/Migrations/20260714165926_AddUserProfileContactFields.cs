using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Users', 'Iban') IS NULL
                    ALTER TABLE [Users] ADD [Iban] NVARCHAR(26) NULL;
                IF COL_LENGTH('Users', 'PhoneNumber') IS NULL
                    ALTER TABLE [Users] ADD [PhoneNumber] NVARCHAR(16) NULL;
                IF COL_LENGTH('Users', 'ProfileImageUrl') IS NULL
                    ALTER TABLE [Users] ADD [ProfileImageUrl] NVARCHAR(1024) NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Users', 'Iban') IS NOT NULL
                    ALTER TABLE [Users] DROP COLUMN [Iban];
                IF COL_LENGTH('Users', 'PhoneNumber') IS NOT NULL
                    ALTER TABLE [Users] DROP COLUMN [PhoneNumber];
                IF COL_LENGTH('Users', 'ProfileImageUrl') IS NOT NULL
                    ALTER TABLE [Users] DROP COLUMN [ProfileImageUrl];
                """);
        }
    }
}
