using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledChargeCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountingMode",
                table: "RecurringCharges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CollectionStartDay",
                table: "RecurringCharges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RecurringCharges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "RecurringCharges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ParticipantsJson",
                table: "RecurringCharges",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "RecurringCharges",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PaidByUserId",
                table: "ChargeCycles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ChargeCycles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE RecurringCharges
                SET AccountingMode = 1,
                    CollectionStartDay = CASE
                        WHEN DueDay IS NULL THEN 1
                        WHEN DueDay > 5 THEN DueDay - 5
                        ELSE 1
                    END,
                    CreatedAt = SYSUTCDATETIME(),
                    Title = CASE Type
                        WHEN 0 THEN N'Kira'
                        WHEN 1 THEN N'İnternet'
                        WHEN 2 THEN N'Elektrik'
                        WHEN 3 THEN N'Su'
                        ELSE N'Düzenli ödeme'
                    END;");

            migrationBuilder.CreateTable(
                name: "ChargeCycleShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChargeCycleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeCycleShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargeCycleShares_ChargeCycles_ChargeCycleId",
                        column: x => x.ChargeCycleId,
                        principalTable: "ChargeCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChargeCycleShares_ChargeCycleId_UserId",
                table: "ChargeCycleShares",
                columns: new[] { "ChargeCycleId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChargeCycleShares");

            migrationBuilder.DropColumn(
                name: "AccountingMode",
                table: "RecurringCharges");

            migrationBuilder.DropColumn(
                name: "CollectionStartDay",
                table: "RecurringCharges");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RecurringCharges");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "RecurringCharges");

            migrationBuilder.DropColumn(
                name: "ParticipantsJson",
                table: "RecurringCharges");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "RecurringCharges");

            migrationBuilder.DropColumn(
                name: "PaidByUserId",
                table: "ChargeCycles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ChargeCycles");
        }
    }
}
