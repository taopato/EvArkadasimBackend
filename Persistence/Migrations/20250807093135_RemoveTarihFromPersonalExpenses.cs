using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class RemoveTarihFromPersonalExpenses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PersonalExpenses tablosundaki eski "Tarih" sütununu kaldır
            migrationBuilder.DropColumn(
                name: "Tarih",
                table: "PersonalExpenses");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alırken "Tarih" sütununu tekrar ekle
            migrationBuilder.AddColumn<DateTime>(
                name: "Tarih",
                table: "PersonalExpenses",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");
        }
    }
}
