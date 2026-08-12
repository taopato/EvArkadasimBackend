using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentOriginatedLedgerCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ExpenseId",
                table: "LedgerLines",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SourcePaymentId",
                table: "LedgerLines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Ledger_SourcePayment",
                table: "LedgerLines",
                column: "SourcePaymentId",
                unique: true,
                filter: "[SourcePaymentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerLines_Payments_SourcePaymentId",
                table: "LedgerLines",
                column: "SourcePaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerLines_Payments_SourcePaymentId",
                table: "LedgerLines");

            migrationBuilder.DropIndex(
                name: "UX_Ledger_SourcePayment",
                table: "LedgerLines");

            migrationBuilder.DropColumn(
                name: "SourcePaymentId",
                table: "LedgerLines");

            migrationBuilder.AlterColumn<int>(
                name: "ExpenseId",
                table: "LedgerLines",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
