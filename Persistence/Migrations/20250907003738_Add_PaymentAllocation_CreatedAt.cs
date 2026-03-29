using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_PaymentAllocation_CreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.PaymentAllocations','CreatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.PaymentAllocations
    ADD CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_PaymentAllocations_CreatedAt DEFAULT (SYSUTCDATETIME());
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PaymentAllocations");
        }
    }
}
