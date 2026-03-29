using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TarihVeSaatKisimlariDuzeltildi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Yeni kolonlar ---
            migrationBuilder.AddColumn<byte>(
                name: "DueDay",
                table: "Expenses",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentCount",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstallmentIndex",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanStartMonth",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            // --- İndeksler (listeleme/filtre için faydalı) ---
            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PlanStartMonth",
                table: "Expenses",
                column: "PlanStartMonth");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_DueDay",
                table: "Expenses",
                column: "DueDay");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ParentExpenseId_InstallmentIndex",
                table: "Expenses",
                columns: new[] { "ParentExpenseId", "InstallmentIndex" });

            // --- Geriye dönük veri doldurma (SQL Server) ---
            // 1) Tüm satırlar için DueDay boşsa CreatedDate gününü yaz
            migrationBuilder.Sql(@"
UPDATE E
SET E.DueDay = DAY(E.CreatedDate)
FROM Expenses AS E
WHERE E.DueDay IS NULL;
");

            // 2) Taksitli PLAN (parent) satırları: çocuklardan sayıyı ve ilk ayı hesapla
            migrationBuilder.Sql(@"
;WITH Agg AS (
    SELECT  ParentExpenseId AS ParentId,
            COUNT(*) AS Cnt,
            MIN(CreatedDate) AS FirstDate
    FROM Expenses
    WHERE ParentExpenseId IS NOT NULL
    GROUP BY ParentExpenseId
)
UPDATE P
SET    P.InstallmentCount = A.Cnt,
       P.PlanStartMonth   = DATEFROMPARTS(YEAR(A.FirstDate), MONTH(A.FirstDate), 1)
FROM Expenses AS P
JOIN Agg AS A ON P.Id = A.ParentId;
");

            // 3) Taksitli ÇOCUK satırlar: sıra numarası, toplam ve plan başlangıcı
            migrationBuilder.Sql(@"
;WITH Ordered AS (
    SELECT  Id,
            ParentExpenseId,
            ROW_NUMBER() OVER (PARTITION BY ParentExpenseId ORDER BY CreatedDate, Id) AS RN,
            COUNT(*)    OVER (PARTITION BY ParentExpenseId) AS CNT,
            MIN(CreatedDate) OVER (PARTITION BY ParentExpenseId) AS FirstDate
    FROM Expenses
    WHERE ParentExpenseId IS NOT NULL
)
UPDATE C
SET    C.InstallmentIndex = O.RN,
       C.InstallmentCount = O.CNT,
       C.PlanStartMonth   = DATEFROMPARTS(YEAR(O.FirstDate), MONTH(O.FirstDate), 1)
FROM Expenses AS C
JOIN Ordered AS O ON C.Id = O.Id;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // İndeksleri kaldır
            migrationBuilder.DropIndex(
                name: "IX_Expenses_PlanStartMonth",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_DueDay",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ParentExpenseId_InstallmentIndex",
                table: "Expenses");

            // Kolonları geri al
            migrationBuilder.DropColumn(
                name: "DueDay",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "InstallmentCount",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "InstallmentIndex",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "PlanStartMonth",
                table: "Expenses");
        }
    }
}
