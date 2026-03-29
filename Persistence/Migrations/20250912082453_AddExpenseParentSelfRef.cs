using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseParentSelfRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentExpenseId",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ParentExpenseId",
                table: "Expenses",
                column: "ParentExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Expenses_ParentExpenseId",
                table: "Expenses",
                column: "ParentExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction // veya Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Expenses_ParentExpenseId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ParentExpenseId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ParentExpenseId",
                table: "Expenses");
        }
    }
}
