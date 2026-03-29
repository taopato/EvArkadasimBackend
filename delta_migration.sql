BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [DeactivatedAt] datetime2 NULL;
GO

ALTER TABLE [Users] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Payments] ADD [DeletedAt] datetime2 NULL;
GO

ALTER TABLE [Payments] ADD [DeletedByUserId] int NULL;
GO

ALTER TABLE [Payments] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [HouseMembers] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [HouseMembers] ADD [LeftAt] datetime2 NULL;
GO

ALTER TABLE [HouseMembers] ADD [RemovedByUserId] int NULL;
GO

CREATE TABLE [Receipts] (
    [Id] int NOT NULL IDENTITY,
    [HouseId] int NOT NULL,
    [UploadedByUserId] int NOT NULL,
    [ImageUrl] nvarchar(1024) NOT NULL,
    [RawOcrText] nvarchar(max) NULL,
    [StoreName] nvarchar(256) NULL,
    [ReceiptDate] datetime2 NULL,
    [DetectedTotalAmount] decimal(18,2) NULL,
    [Status] int NOT NULL,
    [ConvertedExpenseId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Receipts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Receipts_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Receipts_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ReceiptItems] (
    [Id] int NOT NULL IDENTITY,
    [ReceiptId] int NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Quantity] decimal(18,2) NOT NULL,
    [LineTotal] decimal(18,2) NOT NULL,
    [BoxLeft] int NULL,
    [BoxTop] int NULL,
    [BoxWidth] int NULL,
    [BoxHeight] int NULL,
    [IsAssigned] bit NOT NULL DEFAULT CAST(0 AS bit),
    [IsShared] bit NOT NULL,
    [PersonalUserId] int NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_ReceiptItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ReceiptItems_Receipts_ReceiptId] FOREIGN KEY ([ReceiptId]) REFERENCES [Receipts] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ReceiptItems_ReceiptId] ON [ReceiptItems] ([ReceiptId]);
GO

CREATE INDEX [IX_Receipts_CreatedAt] ON [Receipts] ([CreatedAt]);
GO

CREATE INDEX [IX_Receipts_HouseId] ON [Receipts] ([HouseId]);
GO

CREATE INDEX [IX_Receipts_HouseId_UploadedByUserId_CreatedAt] ON [Receipts] ([HouseId], [UploadedByUserId], [CreatedAt]);
GO

CREATE INDEX [IX_Receipts_UploadedByUserId] ON [Receipts] ([UploadedByUserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260329174406_AddSoftDeleteAndProfileFields', N'7.0.16');
GO

COMMIT;
GO

