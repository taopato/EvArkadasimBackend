using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    public partial class Rename_PaymentAllocation_LedgerEntryId_to_LedgerLineId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0) Eski FK (LedgerEntryId’a bağlı) varsa kaldır
            migrationBuilder.Sql(@"
DECLARE @fk sysname;
SELECT TOP(1) @fk = fk.[name]
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.tables t ON t.object_id = fk.parent_object_id
JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = fkc.parent_column_id
WHERE t.[name] = 'PaymentAllocations' AND (c.[name] = 'LedgerEntryId' OR c.[name] = 'LedgerLineId');

IF @fk IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [dbo].[PaymentAllocations] DROP CONSTRAINT [' + @fk + ']');
END
");

            // 1) Eski index adları varsa kaldır (rename sonrası da kalmış olabilir)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE object_id = OBJECT_ID('dbo.PaymentAllocations') 
             AND name = 'IX_PaymentAllocations_LedgerEntryId')
    DROP INDEX [IX_PaymentAllocations_LedgerEntryId] ON [dbo].[PaymentAllocations];

IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE object_id = OBJECT_ID('dbo.PaymentAllocations') 
             AND name = 'IX_PaymentAllocations_LedgerLineId')
    DROP INDEX [IX_PaymentAllocations_LedgerLineId] ON [dbo].[PaymentAllocations];
");

            // 2) Kolon adı LedgerEntryId ise LedgerLineId yap
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PaymentAllocations', 'LedgerLineId') IS NULL
   AND COL_LENGTH('dbo.PaymentAllocations', 'LedgerEntryId') IS NOT NULL
BEGIN
    EXEC sp_rename 'dbo.PaymentAllocations.LedgerEntryId', 'LedgerLineId', 'COLUMN';
END
");

            // 3) Tür/nullable ayarı (bigint NOT NULL)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PaymentAllocations','LedgerLineId') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[PaymentAllocations] ALTER COLUMN [LedgerLineId] BIGINT NOT NULL;
END
");

            // 4) Yeni index
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE object_id = OBJECT_ID('dbo.PaymentAllocations') 
      AND name = 'IX_PaymentAllocations_LedgerLineId'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocations_LedgerLineId]
    ON [dbo].[PaymentAllocations]([LedgerLineId]);
END
");

            // 5) Yeni FK: PaymentAllocations(LedgerLineId) -> LedgerLines(Id)
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.FK_PaymentAllocations_LedgerLines_LedgerLineId','F') IS NULL
BEGIN
    ALTER TABLE [dbo].[PaymentAllocations] WITH CHECK
    ADD CONSTRAINT [FK_PaymentAllocations_LedgerLines_LedgerLineId]
    FOREIGN KEY([LedgerLineId]) REFERENCES [dbo].[LedgerLines]([Id])
    ON DELETE CASCADE;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // FK kaldır
            migrationBuilder.Sql(@"
IF OBJECT_ID('dbo.FK_PaymentAllocations_LedgerLines_LedgerLineId','F') IS NOT NULL
    ALTER TABLE [dbo].[PaymentAllocations] DROP CONSTRAINT [FK_PaymentAllocations_LedgerLines_LedgerLineId];
");

            // Yeni indexi kaldır
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE object_id = OBJECT_ID('dbo.PaymentAllocations') 
             AND name = 'IX_PaymentAllocations_LedgerLineId')
    DROP INDEX [IX_PaymentAllocations_LedgerLineId] ON [dbo].[PaymentAllocations];
");

            // Kolonu geri çevir (varsa)
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PaymentAllocations', 'LedgerEntryId') IS NULL
   AND COL_LENGTH('dbo.PaymentAllocations', 'LedgerLineId') IS NOT NULL
BEGIN
    EXEC sp_rename 'dbo.PaymentAllocations.LedgerLineId', 'LedgerEntryId', 'COLUMN';
END
");

            // Eski index adı (opsiyonel) – geri kurmak istersen:
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PaymentAllocations','LedgerEntryId') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes 
                WHERE object_id = OBJECT_ID('dbo.PaymentAllocations') 
                  AND name = 'IX_PaymentAllocations_LedgerEntryId')
BEGIN
    CREATE INDEX [IX_PaymentAllocations_LedgerEntryId]
    ON [dbo].[PaymentAllocations]([LedgerEntryId]);
END
");
        }
    }
}
