using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class FixExpenseNavigationClean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bu migration yalnızca model düzenlemesi içeriyor.
            // EF Core Fluent API ile navigation tanımlandı,
            // Veritabanında fiziksel bir kolon eklenmiyor.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alınacak fiziksel bir değişiklik olmadığı için boş.
        }
    }
}
