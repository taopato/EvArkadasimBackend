using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Contexts;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260330012500_RemoveLegacyPaymentNotColumn")]
    public partial class RemoveLegacyPaymentNotColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
IF OBJECT_ID(N'[Payments]', N'U') IS NOT NULL
BEGIN
    DECLARE @NotDefaultConstraint NVARCHAR(128);

    IF COL_LENGTH('Payments', 'Aciklama') IS NULL
        ALTER TABLE [Payments] ADD [Aciklama] NVARCHAR(MAX) NULL;

    IF COL_LENGTH('Payments', 'Not') IS NOT NULL
    BEGIN
        UPDATE [Payments]
        SET [Aciklama] = CASE
            WHEN [Aciklama] IS NULL OR LTRIM(RTRIM([Aciklama])) = '' THEN [Not]
            ELSE [Aciklama]
        END;

        SELECT @NotDefaultConstraint = dc.name
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c
            ON c.default_object_id = dc.object_id
        WHERE dc.parent_object_id = OBJECT_ID(N'[Payments]')
          AND c.name = 'Not';

        IF @NotDefaultConstraint IS NOT NULL
            EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @NotDefaultConstraint + ']');

        ALTER TABLE [Payments] DROP COLUMN [Not];
    END
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
    IF COL_LENGTH('Payments', 'Not') IS NULL
    BEGIN
        ALTER TABLE [Payments] ADD [Not] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_Payments_Not] DEFAULT(N'');

        UPDATE [Payments]
        SET [Not] = ISNULL([Aciklama], N'');
    END
END;
""");
        }
    }
}
