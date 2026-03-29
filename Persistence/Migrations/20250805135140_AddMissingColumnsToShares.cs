using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsToShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpenseId",
                table: "Shares",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Shares",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Shares_ExpenseId",
                table: "Shares",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_Shares_UserId",
                table: "Shares",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shares_Expenses_ExpenseId",
                table: "Shares",
                column: "ExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict); // ← Burayı değiştirdik

            migrationBuilder.AddForeignKey(
                name: "FK_Shares_Users_UserId",
                table: "Shares",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict); // ← Burayı da değiştirdik
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shares_Expenses_ExpenseId",
                table: "Shares");

            migrationBuilder.DropForeignKey(
                name: "FK_Shares_Users_UserId",
                table: "Shares");

            migrationBuilder.DropIndex(
                name: "IX_Shares_ExpenseId",
                table: "Shares");

            migrationBuilder.DropIndex(
                name: "IX_Shares_UserId",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "ExpenseId",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Shares");
        }
    }
}
