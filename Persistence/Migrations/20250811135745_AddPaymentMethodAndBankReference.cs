using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodAndBankReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // DekontUrl: nvarchar(400)
            migrationBuilder.AlterColumn<string>(
                name: "DekontUrl",
                table: "Payments",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // NOT: CreatedDate'e default constraint eklemiyoruz (önceki hatadan dolayı kaldırıldı).

            // AlacakliOnayi ZATEN tablo'da mevcut; EKLEME.
            // migrationBuilder.AddColumn<bool>( ... ) satırlarını kaldırdık.

            // Yeni sütunlar
            migrationBuilder.AddColumn<string>(
                name: "BankReference",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 2); // BankTransfer
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Yeni sütunları geri al
            migrationBuilder.DropColumn(
                name: "BankReference",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Payments");

            // DekontUrl tipini eski haline çevir
            migrationBuilder.AlterColumn<string>(
                name: "DekontUrl",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            // AlacakliOnayi için geri alma yapmıyoruz çünkü Up'ta dokunmadık.
        }
    }
}
