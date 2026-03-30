using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "HouseMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeftAt",
                table: "HouseMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemovedByUserId",
                table: "HouseMembers",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
IF OBJECT_ID(N'[Receipts]', N'U') IS NULL
BEGIN
    CREATE TABLE [Receipts] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [HouseId] INT NOT NULL,
        [UploadedByUserId] INT NOT NULL,
        [ImageUrl] NVARCHAR(1024) NOT NULL,
        [RawOcrText] NVARCHAR(MAX) NULL,
        [StoreName] NVARCHAR(256) NULL,
        [ReceiptDate] DATETIME2 NULL,
        [DetectedTotalAmount] DECIMAL(18,2) NULL,
        [Status] INT NOT NULL DEFAULT(0),
        [ConvertedExpenseId] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [FK_Receipts_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Receipts_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users]([Id])
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receipts_HouseId' AND object_id = OBJECT_ID(N'[Receipts]'))
    CREATE INDEX [IX_Receipts_HouseId] ON [Receipts]([HouseId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receipts_UploadedByUserId' AND object_id = OBJECT_ID(N'[Receipts]'))
    CREATE INDEX [IX_Receipts_UploadedByUserId] ON [Receipts]([UploadedByUserId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receipts_CreatedAt' AND object_id = OBJECT_ID(N'[Receipts]'))
    CREATE INDEX [IX_Receipts_CreatedAt] ON [Receipts]([CreatedAt]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Receipts_HouseId_UploadedByUserId_CreatedAt' AND object_id = OBJECT_ID(N'[Receipts]'))
    CREATE INDEX [IX_Receipts_HouseId_UploadedByUserId_CreatedAt] ON [Receipts]([HouseId], [UploadedByUserId], [CreatedAt]);

IF OBJECT_ID(N'[ReceiptItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [ReceiptItems] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ReceiptId] INT NOT NULL,
        [Name] NVARCHAR(256) NOT NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [Quantity] DECIMAL(18,2) NOT NULL,
        [LineTotal] DECIMAL(18,2) NOT NULL,
        [BoxLeft] INT NULL,
        [BoxTop] INT NULL,
        [BoxWidth] INT NULL,
        [BoxHeight] INT NULL,
        [IsAssigned] BIT NOT NULL DEFAULT(0),
        [IsShared] BIT NOT NULL DEFAULT(1),
        [PersonalUserId] INT NULL,
        [SortOrder] INT NOT NULL DEFAULT(0),
        CONSTRAINT [FK_ReceiptItems_Receipts_ReceiptId] FOREIGN KEY ([ReceiptId]) REFERENCES [Receipts]([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReceiptItems_ReceiptId' AND object_id = OBJECT_ID(N'[ReceiptItems]'))
    CREATE INDEX [IX_ReceiptItems_ReceiptId] ON [ReceiptItems]([ReceiptId]);
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptItems");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "HouseMembers");

            migrationBuilder.DropColumn(
                name: "LeftAt",
                table: "HouseMembers");

            migrationBuilder.DropColumn(
                name: "RemovedByUserId",
                table: "HouseMembers");
        }
    }
}
