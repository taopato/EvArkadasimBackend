using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillMissingPaymentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
IF OBJECT_ID(N'[Payments]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Payments', 'HouseId') IS NULL
        ALTER TABLE [Payments] ADD [HouseId] INT NULL;

    IF COL_LENGTH('Payments', 'CreatedDate') IS NULL
        ALTER TABLE [Payments] ADD [CreatedDate] DATETIME2 NOT NULL CONSTRAINT [DF_Payments_CreatedDate] DEFAULT(GETUTCDATE());

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_Payments_HouseId'
          AND object_id = OBJECT_ID(N'[Payments]')
    )
        CREATE INDEX [IX_Payments_HouseId] ON [Payments]([HouseId]);

    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Payments_Houses_HouseId'
    )
        ALTER TABLE [Payments] WITH NOCHECK
        ADD CONSTRAINT [FK_Payments_Houses_HouseId]
        FOREIGN KEY ([HouseId]) REFERENCES [Houses]([Id]);
END;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
IF OBJECT_ID(N'[Payments]', N'U') IS NOT NULL
BEGIN
    DECLARE @CreatedDateDefaultConstraint NVARCHAR(128);

    IF EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Payments_Houses_HouseId'
    )
        ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Houses_HouseId];

    IF EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_Payments_HouseId'
          AND object_id = OBJECT_ID(N'[Payments]')
    )
        DROP INDEX [IX_Payments_HouseId] ON [Payments];

    SELECT @CreatedDateDefaultConstraint = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[Payments]')
      AND c.name = 'CreatedDate';

    IF @CreatedDateDefaultConstraint IS NOT NULL
        EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @CreatedDateDefaultConstraint + ']');

    IF COL_LENGTH('Payments', 'CreatedDate') IS NOT NULL
        ALTER TABLE [Payments] DROP COLUMN [CreatedDate];

    IF COL_LENGTH('Payments', 'HouseId') IS NOT NULL
        ALTER TABLE [Payments] DROP COLUMN [HouseId];
END;
""");
        }
    }
}
