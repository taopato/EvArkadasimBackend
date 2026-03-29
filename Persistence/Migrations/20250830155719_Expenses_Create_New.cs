using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class Expenses_Create_New : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Eski index'i varsa sil (bazı şemalarda var)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE name = 'IX_Expenses_HouseId' 
             AND object_id = OBJECT_ID('dbo.Expenses'))
    DROP INDEX IX_Expenses_HouseId ON dbo.Expenses;
");

            // ── Expenses: kolonlar (YOKSA EKLE)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Expenses','Type') IS NULL
    ALTER TABLE dbo.Expenses ADD [Type] TINYINT NOT NULL CONSTRAINT DF_Expenses_Type DEFAULT (1); -- 1=Irregular

IF COL_LENGTH('dbo.Expenses','Category') IS NULL
    ALTER TABLE dbo.Expenses ADD [Category] TINYINT NOT NULL CONSTRAINT DF_Expenses_Category DEFAULT (99); -- 99=Other

IF COL_LENGTH('dbo.Expenses','PostDate') IS NULL
    ALTER TABLE dbo.Expenses ADD [PostDate] DATETIME2 NOT NULL CONSTRAINT DF_Expenses_PostDate DEFAULT (sysutcdatetime());

IF COL_LENGTH('dbo.Expenses','DueDate') IS NULL
    ALTER TABLE dbo.Expenses ADD [DueDate] DATETIME2 NULL;

IF COL_LENGTH('dbo.Expenses','PeriodMonth') IS NULL
    ALTER TABLE dbo.Expenses ADD [PeriodMonth] NVARCHAR(7) NOT NULL CONSTRAINT DF_Expenses_PeriodMonth DEFAULT (N'');

IF COL_LENGTH('dbo.Expenses','SplitPolicy') IS NULL
    ALTER TABLE dbo.Expenses ADD [SplitPolicy] INT NOT NULL CONSTRAINT DF_Expenses_SplitPolicy DEFAULT (0);

IF COL_LENGTH('dbo.Expenses','ParticipantsJson') IS NULL
    ALTER TABLE dbo.Expenses ADD [ParticipantsJson] NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.Expenses','PersonalBreakdownJson') IS NULL
    ALTER TABLE dbo.Expenses ADD [PersonalBreakdownJson] NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.Expenses','VisibilityMode') IS NULL
    ALTER TABLE dbo.Expenses ADD [VisibilityMode] TINYINT NOT NULL CONSTRAINT DF_Expenses_VisibilityMode DEFAULT (0);

IF COL_LENGTH('dbo.Expenses','PreShareDays') IS NULL
    ALTER TABLE dbo.Expenses ADD [PreShareDays] SMALLINT NULL;

IF COL_LENGTH('dbo.Expenses','RecurrenceBatchKey') IS NULL
    ALTER TABLE dbo.Expenses ADD [RecurrenceBatchKey] UNIQUEIDENTIFIER NULL;

IF COL_LENGTH('dbo.Expenses','Currency') IS NULL
    ALTER TABLE dbo.Expenses ADD [Currency] NCHAR(3) NOT NULL CONSTRAINT DF_Expenses_Currency DEFAULT (N'TRY');

IF COL_LENGTH('dbo.Expenses','Note') IS NULL
    ALTER TABLE dbo.Expenses ADD [Note] NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.Expenses','UpdatedAt') IS NULL
    ALTER TABLE dbo.Expenses ADD [UpdatedAt] DATETIME2 NULL;
");

            // ── PeriodMonth backfill: PostDate varsa yyyy-MM doldur
            migrationBuilder.Sql(@"
UPDATE E
   SET PeriodMonth = CONVERT(CHAR(7), E.PostDate, 126)  -- 'yyyy-mm'
FROM dbo.Expenses E
WHERE (E.PeriodMonth IS NULL OR E.PeriodMonth = N'');
");

            // ── Expenses: indexler (YOKSA OLUŞTUR)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_Expenses_House_Post' 
                 AND object_id = OBJECT_ID('dbo.Expenses'))
    CREATE INDEX IX_Expenses_House_Post 
        ON dbo.Expenses(HouseId, PostDate)
        INCLUDE (Tutar, Category, [Type], OdeyenUserId, IsActive);

IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_Expenses_Batch' 
                 AND object_id = OBJECT_ID('dbo.Expenses'))
    CREATE INDEX IX_Expenses_Batch 
        ON dbo.Expenses(RecurrenceBatchKey);

IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_Expenses_Period' 
                 AND object_id = OBJECT_ID('dbo.Expenses'))
    CREATE INDEX IX_Expenses_Period 
        ON dbo.Expenses(HouseId, PeriodMonth);
");

            // ── LedgerLines: tablo ve indexler (YOKSA OLUŞTUR)
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.LedgerLines','U') IS NULL
BEGIN
    CREATE TABLE dbo.LedgerLines
    (
        Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LedgerLines PRIMARY KEY,
        HouseId     INT NOT NULL,
        ExpenseId   INT NOT NULL,
        FromUserId  INT NOT NULL,
        ToUserId    INT NOT NULL,
        Amount      DECIMAL(18,2) NOT NULL,
        PostDate    DATETIME2 NOT NULL CONSTRAINT DF_LedgerLines_PostDate DEFAULT (sysutcdatetime()),
        IsActive    BIT NOT NULL CONSTRAINT DF_LedgerLines_IsActive DEFAULT (1),
        CreatedAt   DATETIME2 NOT NULL CONSTRAINT DF_LedgerLines_CreatedAt DEFAULT (sysutcdatetime()),
        UpdatedAt   DATETIME2 NULL
    );

    ALTER TABLE dbo.LedgerLines
        ADD CONSTRAINT FK_LedgerLines_Expenses_ExpenseId
            FOREIGN KEY (ExpenseId) REFERENCES dbo.Expenses(Id) ON DELETE CASCADE;

    CREATE INDEX IX_Ledger_Expense      ON dbo.LedgerLines(ExpenseId);
    CREATE INDEX IX_Ledger_House_Post   ON dbo.LedgerLines(HouseId, PostDate);
    CREATE INDEX IX_Ledger_House_FromTo ON dbo.LedgerLines(HouseId, FromUserId, ToUserId, PostDate);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── LedgerLines sil
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.LedgerLines','U') IS NOT NULL
    DROP TABLE dbo.LedgerLines;
");

            // ── Expenses indexleri sil (varsa)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE name = 'IX_Expenses_Period' 
             AND object_id = OBJECT_ID('dbo.Expenses'))
    DROP INDEX IX_Expenses_Period ON dbo.Expenses;

IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE name = 'IX_Expenses_Batch' 
             AND object_id = OBJECT_ID('dbo.Expenses'))
    DROP INDEX IX_Expenses_Batch ON dbo.Expenses;

IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE name = 'IX_Expenses_House_Post' 
             AND object_id = OBJECT_ID('dbo.Expenses'))
    DROP INDEX IX_Expenses_House_Post ON dbo.Expenses;
");

            // ── Expenses kolonları geri al (varsa)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Expenses','UpdatedAt') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [UpdatedAt];

IF COL_LENGTH('dbo.Expenses','Note') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [Note];

IF COL_LENGTH('dbo.Expenses','Currency') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [Currency];

IF COL_LENGTH('dbo.Expenses','RecurrenceBatchKey') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [RecurrenceBatchKey];

IF COL_LENGTH('dbo.Expenses','PreShareDays') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [PreShareDays];

IF COL_LENGTH('dbo.Expenses','VisibilityMode') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [VisibilityMode];

IF COL_LENGTH('dbo.Expenses','PersonalBreakdownJson') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [PersonalBreakdownJson];

IF COL_LENGTH('dbo.Expenses','ParticipantsJson') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [ParticipantsJson];

IF COL_LENGTH('dbo.Expenses','SplitPolicy') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [SplitPolicy];

IF COL_LENGTH('dbo.Expenses','PeriodMonth') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [PeriodMonth];

IF COL_LENGTH('dbo.Expenses','DueDate') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [DueDate];

IF COL_LENGTH('dbo.Expenses','PostDate') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [PostDate];

IF COL_LENGTH('dbo.Expenses','Category') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [Category];

IF COL_LENGTH('dbo.Expenses','Type') IS NOT NULL
    ALTER TABLE dbo.Expenses DROP COLUMN [Type];
");

            // (İstersen geri ekle)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_Expenses_HouseId' 
                 AND object_id = OBJECT_ID('dbo.Expenses'))
    CREATE INDEX IX_Expenses_HouseId ON dbo.Expenses(HouseId);
");
        }
    }
}
