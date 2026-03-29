using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class FixDuplicateUserForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sadece kolon varsa, DROP COLUMN
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns 
                           WHERE Name = N'UserOdeyenId' AND Object_ID = Object_ID(N'Expenses'))
                BEGIN
                    ALTER TABLE Expenses DROP COLUMN UserOdeyenId
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserOdeyenId",
                table: "Expenses",
                type: "int",
                nullable: true);
        }
    }
}
