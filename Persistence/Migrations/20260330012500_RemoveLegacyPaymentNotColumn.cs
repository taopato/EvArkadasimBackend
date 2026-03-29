using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyPaymentNotColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
IF OBJECT_ID(N'[Payments]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Payments', 'Aciklama') IS NULL
        ALTER TABLE [Payments] ADD [Aciklama] NVARCHAR(MAX) NULL;

    IF COL_LENGTH('Payments', 'Not') IS NOT NULL
    BEGIN
        UPDATE [Payments]
        SET [Aciklama] = CASE
            WHEN [Aciklama] IS NULL OR LTRIM(RTRIM([Aciklama])) = '' THEN [Not]
            ELSE [Aciklama]
        END;

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
