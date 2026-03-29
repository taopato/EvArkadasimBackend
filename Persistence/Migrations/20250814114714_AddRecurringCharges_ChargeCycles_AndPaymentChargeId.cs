using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringCharges_ChargeCycles_AndPaymentChargeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChargeId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecurringCharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PayerUserId = table.Column<int>(type: "int", nullable: false),
                    AmountMode = table.Column<int>(type: "int", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SplitPolicy = table.Column<int>(type: "int", nullable: false),
                    WeightsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDay = table.Column<int>(type: "int", nullable: true),
                    PaymentWindowDays = table.Column<int>(type: "int", nullable: false),
                    StartMonth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringCharges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChargeCycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FundedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillDocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalReceiptUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargeCycles_RecurringCharges_ContractId",
                        column: x => x.ContractId,
                        principalTable: "RecurringCharges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ChargeId",
                table: "Payments",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeCycles_ContractId_Period",
                table: "ChargeCycles",
                columns: new[] { "ContractId", "Period" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ChargeCycles_ChargeId",
                table: "Payments",
                column: "ChargeId",
                principalTable: "ChargeCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ChargeCycles_ChargeId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "ChargeCycles");

            migrationBuilder.DropTable(
                name: "RecurringCharges");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ChargeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ChargeId",
                table: "Payments");
        }
    }
}
