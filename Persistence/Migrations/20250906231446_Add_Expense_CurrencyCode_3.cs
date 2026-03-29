using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class Add_Expense_CurrencyCode_3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Expenses.CurrencyCode -> NVARCHAR(3) NULL
            // Yoksa ekler, varsa uzunluğunu/nullable durumunu garanti altına alır.
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Expenses','CurrencyCode') IS NULL
BEGIN
    ALTER TABLE dbo.Expenses ADD [CurrencyCode] NVARCHAR(3) NULL;
END
ELSE
BEGIN
    ALTER TABLE dbo.Expenses ALTER COLUMN [CurrencyCode] NVARCHAR(3) NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri dönüş: kolonu varsa düşür (isteğe bağlı)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Expenses','CurrencyCode') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Expenses DROP COLUMN [CurrencyCode];
END
");
        }
    }
}
