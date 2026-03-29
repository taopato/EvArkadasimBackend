using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class LedgerLine_DecimalPrecision_and_Expense_Note512 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Expenses.Note -> NVARCHAR(512) NULL (varsa ALTER, yoksa ADD + ALTER)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Expenses','Note') IS NULL
BEGIN
    ALTER TABLE dbo.Expenses ADD [Note] NVARCHAR(512) NULL;
END
ELSE
BEGIN
    ALTER TABLE dbo.Expenses ALTER COLUMN [Note] NVARCHAR(512) NULL;
END
");

            // 2) LedgerLines.Amount -> DECIMAL(18,2) NOT NULL
            // (kolon mevcut; tipi güvenceye almak için ALTER)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.LedgerLines','Amount') IS NOT NULL
BEGIN
    ALTER TABLE dbo.LedgerLines ALTER COLUMN [Amount] DECIMAL(18,2) NOT NULL;
END
");

            // 3) LedgerLines.PaidAmount -> yoksa EKLE (DECIMAL(18,2) NOT NULL DEFAULT(0)),
            //    varsa tipe güvence için ALTER
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.LedgerLines','PaidAmount') IS NULL
BEGIN
    ALTER TABLE dbo.LedgerLines ADD [PaidAmount] DECIMAL(18,2) NOT NULL CONSTRAINT DF_LedgerLines_PaidAmount DEFAULT(0);
END
ELSE
BEGIN
    ALTER TABLE dbo.LedgerLines ALTER COLUMN [PaidAmount] DECIMAL(18,2) NOT NULL;
END
");

            // 4) LedgerLines.IsClosed -> yoksa EKLE (BIT NOT NULL DEFAULT(0))
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.LedgerLines','IsClosed') IS NULL
BEGIN
    ALTER TABLE dbo.LedgerLines ADD [IsClosed] BIT NOT NULL CONSTRAINT DF_LedgerLines_IsClosed DEFAULT(0);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri dönüş:
            // - Note'u makul bir uzunluğa indiriyoruz (istersen NVARCHAR(50) yapabilirsin)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Expenses','Note') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Expenses ALTER COLUMN [Note] NVARCHAR(256) NULL;
END
");

            // - PaidAmount & IsClosed kolonlarını, sadece mevcutlarsa düşür (önce default constraint'lerini kaldır)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.LedgerLines','PaidAmount') IS NOT NULL
BEGIN
    DECLARE @dfPaid sysname;
    SELECT @dfPaid = d.name
    FROM sys.default_constraints d
    JOIN sys.columns c ON c.default_object_id = d.object_id
    WHERE d.parent_object_id = OBJECT_ID('dbo.LedgerLines') AND c.name = 'PaidAmount';
    IF @dfPaid IS NOT NULL EXEC('ALTER TABLE dbo.LedgerLines DROP CONSTRAINT ' + QUOTENAME(@dfPaid));
    -- İstersen DROP COLUMN da yapabilirsin; burada sadece tipi geriye çevirmiyoruz.
    -- ALTER TABLE dbo.LedgerLines DROP COLUMN [PaidAmount];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.LedgerLines','IsClosed') IS NOT NULL
BEGIN
    DECLARE @dfClosed sysname;
    SELECT @dfClosed = d.name
    FROM sys.default_constraints d
    JOIN sys.columns c ON c.default_object_id = d.object_id
    WHERE d.parent_object_id = OBJECT_ID('dbo.LedgerLines') AND c.name = 'IsClosed';
    IF @dfClosed IS NOT NULL EXEC('ALTER TABLE dbo.LedgerLines DROP CONSTRAINT ' + QUOTENAME(@dfClosed));
    -- ALTER TABLE dbo.LedgerLines DROP COLUMN [IsClosed];
END
");

            // Amount'ı geri çevirmiyoruz; tip zaten güvenli (18,2).
        }
    }
}
