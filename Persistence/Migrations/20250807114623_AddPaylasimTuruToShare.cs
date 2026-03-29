using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class AddPaylasimTuruToShare : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eğer tabloya daha önce eklenmişse hata almamak için ön kontrol önerilir:
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.columns 
                    WHERE Name = N'PaylasimTuru' 
                      AND Object_ID = Object_ID(N'Shares')
                )
                BEGIN
                    ALTER TABLE Shares ADD PaylasimTuru int NOT NULL DEFAULT 0;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns 
                    WHERE Name = N'PaylasimTuru' 
                      AND Object_ID = Object_ID(N'Shares')
                )
                BEGIN
                    ALTER TABLE Shares DROP COLUMN PaylasimTuru;
                END
            ");
        }
    }
}
