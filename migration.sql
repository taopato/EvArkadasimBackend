IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Houses] (
    [HouseId] int NOT NULL IDENTITY,
    [HouseName] nvarchar(max) NOT NULL,
    [CreatedBy] int NOT NULL,
    CONSTRAINT [PK_Houses] PRIMARY KEY ([HouseId])
);
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [RegistrationDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [VerificationCodes] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(max) NOT NULL,
    [Code] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_VerificationCodes] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Invitations] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(max) NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [HouseId] int NOT NULL,
    [SentAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Invitations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invitations_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses] ([HouseId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Expenses] (
    [Id] int NOT NULL IDENTITY,
    [Tur] nvarchar(max) NOT NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [HouseId] int NOT NULL,
    [OdeyenUserId] int NOT NULL,
    [KaydedenUserId] int NOT NULL,
    [Tarih] datetime2 NOT NULL,
    [OrtakHarcamaTutari] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Expenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Expenses_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses] ([HouseId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Expenses_Users_KaydedenUserId] FOREIGN KEY ([KaydedenUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Expenses_Users_OdeyenUserId] FOREIGN KEY ([OdeyenUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [HouseMembers] (
    [Id] int NOT NULL IDENTITY,
    [HouseId] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_HouseMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HouseMembers_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses] ([HouseId]) ON DELETE CASCADE,
    CONSTRAINT [FK_HouseMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [BorcluUserId] int NOT NULL,
    [AlacakliUserId] int NOT NULL,
    [Not] nvarchar(max) NOT NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [AlacakliOnayi] bit NOT NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Users_AlacakliUserId] FOREIGN KEY ([AlacakliUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Payments_Users_BorcluUserId] FOREIGN KEY ([BorcluUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PersonalExpenses] (
    [Id] int NOT NULL IDENTITY,
    [ExpenseId] int NOT NULL,
    [UserId] int NOT NULL,
    [Tutar] decimal(18,2) NOT NULL,
    [Tarih] datetime2 NOT NULL,
    CONSTRAINT [PK_PersonalExpenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PersonalExpenses_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PersonalExpenses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Shares] (
    [Id] int NOT NULL IDENTITY,
    [HarcamaId] int NOT NULL,
    [PaylasimUserId] int NOT NULL,
    [PaylasimTuru] nvarchar(max) NOT NULL,
    [PaylasimTutar] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Shares] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Shares_Expenses_HarcamaId] FOREIGN KEY ([HarcamaId]) REFERENCES [Expenses] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Shares_Users_PaylasimUserId] FOREIGN KEY ([PaylasimUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Expenses_HouseId] ON [Expenses] ([HouseId]);
GO

CREATE INDEX [IX_Expenses_KaydedenUserId] ON [Expenses] ([KaydedenUserId]);
GO

CREATE INDEX [IX_Expenses_OdeyenUserId] ON [Expenses] ([OdeyenUserId]);
GO

CREATE INDEX [IX_HouseMembers_HouseId] ON [HouseMembers] ([HouseId]);
GO

CREATE INDEX [IX_HouseMembers_UserId] ON [HouseMembers] ([UserId]);
GO

CREATE INDEX [IX_Invitations_HouseId] ON [Invitations] ([HouseId]);
GO

CREATE INDEX [IX_Payments_AlacakliUserId] ON [Payments] ([AlacakliUserId]);
GO

CREATE INDEX [IX_Payments_BorcluUserId] ON [Payments] ([BorcluUserId]);
GO

CREATE INDEX [IX_PersonalExpenses_ExpenseId] ON [PersonalExpenses] ([ExpenseId]);
GO

CREATE INDEX [IX_PersonalExpenses_UserId] ON [PersonalExpenses] ([UserId]);
GO

CREATE INDEX [IX_Shares_HarcamaId] ON [Shares] ([HarcamaId]);
GO

CREATE INDEX [IX_Shares_PaylasimUserId] ON [Shares] ([PaylasimUserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250803214711_InitialCreate', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Expenses] DROP CONSTRAINT [FK_Expenses_Houses_HouseId];
GO

ALTER TABLE [HouseMembers] DROP CONSTRAINT [FK_HouseMembers_Houses_HouseId];
GO

ALTER TABLE [Invitations] DROP CONSTRAINT [FK_Invitations_Houses_HouseId];
GO

ALTER TABLE [HouseMembers] DROP CONSTRAINT [PK_HouseMembers];
GO

ALTER TABLE [Houses] DROP CONSTRAINT [PK_Houses];
GO

EXEC sp_rename N'[Houses].[HouseId]', N'Id', N'COLUMN';
GO

EXEC sp_rename N'[Houses].[HouseName]', N'Name', N'COLUMN';
GO

EXEC sp_rename N'[Expenses].[Tur]', N'Description', N'COLUMN';
GO

EXEC sp_rename N'[Expenses].[Tarih]', N'CreatedDate', N'COLUMN';
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Houses]') AND [c].[name] = N'CreatedBy');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Houses] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Houses] DROP COLUMN [CreatedBy];
GO

ALTER TABLE [Houses] ADD [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
GO

ALTER TABLE [HouseMembers] ADD [JoinedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE());
GO

ALTER TABLE [Houses] ADD CONSTRAINT [PK_Houses] PRIMARY KEY ([Id]);
GO

ALTER TABLE [HouseMembers] ADD CONSTRAINT [PK_HouseMembers] PRIMARY KEY ([HouseId], [UserId]);
GO

ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [HouseMembers] ADD CONSTRAINT [FK_HouseMembers_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [Invitations] ADD CONSTRAINT [FK_Invitations_Houses_HouseId] FOREIGN KEY ([HouseId]) REFERENCES [Houses] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250803230026_ExpenseAndHouseCreate', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [VerificationCodes] ADD [ExpiresAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250804001043_AddExpiresAtToVerificationCodes', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250805080647_AddInvitationColumns_Corrected', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Invitations] ADD [AcceptedByUserId] int NULL;
GO

ALTER TABLE [Invitations] ADD [ExpiresAt] datetime2 NOT NULL DEFAULT '2000-01-01T00:00:00.0000000';
GO

ALTER TABLE [Invitations] ADD [Status] nvarchar(max) NOT NULL DEFAULT N'Pending';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250805080959_AddInvitationFieldsOnly', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Invitations] ADD [AcceptedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250805083504_AddAcceptedFieldsToInvitation', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Houses] ADD [CreatorUserId] int NOT NULL DEFAULT 0;
GO

CREATE INDEX [IX_Houses_CreatorUserId] ON [Houses] ([CreatorUserId]);
GO

ALTER TABLE [Houses] ADD CONSTRAINT [FK_Houses_Users_CreatorUserId] FOREIGN KEY ([CreatorUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250805120730_AddCreatorUserIdToHouse_Correct', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO


                IF EXISTS (SELECT * FROM sys.columns 
                           WHERE Name = N'UserOdeyenId' AND Object_ID = Object_ID(N'Expenses'))
                BEGIN
                    ALTER TABLE Expenses DROP COLUMN UserOdeyenId
                END
            
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250805133338_FixDuplicateUserForeignKey', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250805134553_RemoveUserOdeyenNavigation', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Shares] ADD [ExpenseId] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Shares] ADD [UserId] int NOT NULL DEFAULT 0;
GO

CREATE INDEX [IX_Shares_ExpenseId] ON [Shares] ([ExpenseId]);
GO

CREATE INDEX [IX_Shares_UserId] ON [Shares] ([UserId]);
GO

ALTER TABLE [Shares] ADD CONSTRAINT [FK_Shares_Expenses_ExpenseId] FOREIGN KEY ([ExpenseId]) REFERENCES [Expenses] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Shares] ADD CONSTRAINT [FK_Shares_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250805135140_AddMissingColumnsToShares', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250806133737_RenameTarihToDate', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Shares] ADD [Date] datetime2 NOT NULL DEFAULT (GETUTCDATE());
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250806134203_AddDateToShares', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PersonalExpenses] ADD [Date] datetime2 NOT NULL DEFAULT (GETUTCDATE());
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250807091239_AddDateToPersonalExpenses', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PersonalExpenses]') AND [c].[name] = N'Tarih');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PersonalExpenses] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [PersonalExpenses] DROP COLUMN [Tarih];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250807093135_RemoveTarihFromPersonalExpenses', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250807112654_FixExpenseNavigationClean', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO


                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.columns 
                    WHERE Name = N'PaylasimTuru' 
                      AND Object_ID = Object_ID(N'Shares')
                )
                BEGIN
                    ALTER TABLE Shares ADD PaylasimTuru int NOT NULL DEFAULT 0;
                END
            
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250807114623_AddPaylasimTuruToShare', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Expenses] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250808084345_AddIsActiveToExpenses', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Payments] ADD [Aciklama] nvarchar(max) NULL;
GO

ALTER TABLE [Payments] ADD [DekontUrl] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Payments] ADD [OdemeTarihi] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250808093538_AddPaymentsTable', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250811131315_PaymentTablosuTekrari', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'DekontUrl');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Payments] ALTER COLUMN [DekontUrl] nvarchar(400) NOT NULL;
GO

ALTER TABLE [Payments] ADD [BankReference] nvarchar(100) NULL;
GO

ALTER TABLE [Payments] ADD [PaymentMethod] int NOT NULL DEFAULT 2;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250811135745_AddPaymentMethodAndBankReference', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'CreatedDate');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var3 + '];');
GO

ALTER TABLE [Payments] ADD [ApprovedByUserId] int NULL;
GO

ALTER TABLE [Payments] ADD [ApprovedDate] datetime2 NULL;
GO

ALTER TABLE [Payments] ADD [RejectedByUserId] int NULL;
GO

ALTER TABLE [Payments] ADD [RejectedDate] datetime2 NULL;
GO

ALTER TABLE [Payments] ADD [Status] int NOT NULL DEFAULT 1;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250811142555_AddPaymentStatusFields', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'BankReference');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Payments] DROP COLUMN [BankReference];
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250811224502_RemoveBankReferenceFromPayments', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Bills] (
    [Id] int NOT NULL IDENTITY,
    [HouseId] int NOT NULL,
    [UtilityType] int NOT NULL,
    [Month] nvarchar(7) NOT NULL,
    [ResponsibleUserId] int NOT NULL,
    [Status] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [DueDate] datetime2 NULL,
    [Note] nvarchar(max) NULL,
    [CreatedByUserId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [FinalizedAt] datetime2 NULL,
    CONSTRAINT [PK_Bills] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [LedgerEntries] (
    [Id] int NOT NULL IDENTITY,
    [HouseId] int NOT NULL,
    [FromUserId] int NOT NULL,
    [ToUserId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [IsClosed] bit NOT NULL,
    [BillId] int NULL,
    [UtilityType] int NULL,
    [Month] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Note] nvarchar(max) NULL,
    CONSTRAINT [PK_LedgerEntries] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [PaymentAllocations] (
    [Id] int NOT NULL IDENTITY,
    [PaymentId] int NOT NULL,
    [LedgerEntryId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PaymentAllocations] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [BillDocuments] (
    [Id] int NOT NULL IDENTITY,
    [BillId] int NOT NULL,
    [FileName] nvarchar(260) NOT NULL,
    [FilePathOrUrl] nvarchar(1024) NOT NULL,
    [UploadedByUserId] int NOT NULL,
    [UploadedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_BillDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BillDocuments_Bills_BillId] FOREIGN KEY ([BillId]) REFERENCES [Bills] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [BillShares] (
    [Id] int NOT NULL IDENTITY,
    [BillId] int NOT NULL,
    [UserId] int NOT NULL,
    [ShareAmount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_BillShares] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BillShares_Bills_BillId] FOREIGN KEY ([BillId]) REFERENCES [Bills] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_BillDocuments_BillId] ON [BillDocuments] ([BillId]);
GO

CREATE UNIQUE INDEX [IX_Bills_HouseId_UtilityType_Month] ON [Bills] ([HouseId], [UtilityType], [Month]);
GO

CREATE INDEX [IX_BillShares_BillId] ON [BillShares] ([BillId]);
GO

CREATE INDEX [IX_LedgerEntries_HouseId_FromUserId_ToUserId_CreatedAt] ON [LedgerEntries] ([HouseId], [FromUserId], [ToUserId], [CreatedAt]);
GO

CREATE INDEX [IX_PaymentAllocations_LedgerEntryId] ON [PaymentAllocations] ([LedgerEntryId]);
GO

CREATE INDEX [IX_PaymentAllocations_PaymentId] ON [PaymentAllocations] ([PaymentId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250812130325_Add_Bills_Ledger_PaymentAllocations', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Payments] ADD [ChargeId] int NULL;
GO

CREATE TABLE [RecurringCharges] (
    [Id] int NOT NULL IDENTITY,
    [HouseId] int NOT NULL,
    [Type] int NOT NULL,
    [PayerUserId] int NOT NULL,
    [AmountMode] int NOT NULL,
    [FixedAmount] decimal(18,2) NULL,
    [SplitPolicy] int NOT NULL,
    [WeightsJson] nvarchar(max) NULL,
    [DueDay] int NULL,
    [PaymentWindowDays] int NOT NULL,
    [StartMonth] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_RecurringCharges] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ChargeCycles] (
    [Id] int NOT NULL IDENTITY,
    [ContractId] int NOT NULL,
    [Period] nvarchar(450) NOT NULL,
    [Status] int NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [FundedAmount] decimal(18,2) NOT NULL,
    [BillDate] datetime2 NULL,
    [BillNumber] nvarchar(max) NULL,
    [BillDocumentUrl] nvarchar(max) NULL,
    [DueDate] datetime2 NULL,
    [PaidDate] datetime2 NULL,
    [ExternalReceiptUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_ChargeCycles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChargeCycles_RecurringCharges_ContractId] FOREIGN KEY ([ContractId]) REFERENCES [RecurringCharges] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Payments_ChargeId] ON [Payments] ([ChargeId]);
GO

CREATE UNIQUE INDEX [IX_ChargeCycles_ContractId_Period] ON [ChargeCycles] ([ContractId], [Period]);
GO

ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_ChargeCycles_ChargeId] FOREIGN KEY ([ChargeId]) REFERENCES [ChargeCycles] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250814114714_AddRecurringCharges_ChargeCycles_AndPaymentChargeId', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250814132351_Add_RecurringCharges_And_ChargeCycles', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250814132900_Tune_Decimal_Precision', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250830153847_Create_Expenses', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO


IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE name = 'IX_Expenses_HouseId' 
             AND object_id = OBJECT_ID('dbo.Expenses'))
    DROP INDEX IX_Expenses_HouseId ON dbo.Expenses;

GO


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

GO


UPDATE E
   SET PeriodMonth = CONVERT(CHAR(7), E.PostDate, 126)  -- 'yyyy-mm'
FROM dbo.Expenses E
WHERE (E.PeriodMonth IS NULL OR E.PeriodMonth = N'');

GO


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

GO


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

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250830155719_Expenses_Create_New', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Expenses]') AND [c].[name] = N'PostDate');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Expenses] DROP CONSTRAINT [' + @var5 + '];');
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250830165020_Expenses.Extra', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Expenses]') AND [c].[name] = N'Currency');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Expenses] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Expenses] DROP COLUMN [Currency];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Expenses]') AND [c].[name] = N'Note');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Expenses] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Expenses] ALTER COLUMN [Note] nvarchar(3) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250830172317_RemoveExpenseCurrency', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO


IF COL_LENGTH('dbo.Expenses','Note') IS NULL
BEGIN
    ALTER TABLE dbo.Expenses ADD [Note] NVARCHAR(512) NULL;
END
ELSE
BEGIN
    ALTER TABLE dbo.Expenses ALTER COLUMN [Note] NVARCHAR(512) NULL;
END

GO


IF COL_LENGTH('dbo.LedgerLines','Amount') IS NOT NULL
BEGIN
    ALTER TABLE dbo.LedgerLines ALTER COLUMN [Amount] DECIMAL(18,2) NOT NULL;
END

GO


IF COL_LENGTH('dbo.LedgerLines','PaidAmount') IS NULL
BEGIN
    ALTER TABLE dbo.LedgerLines ADD [PaidAmount] DECIMAL(18,2) NOT NULL CONSTRAINT DF_LedgerLines_PaidAmount DEFAULT(0);
END
ELSE
BEGIN
    ALTER TABLE dbo.LedgerLines ALTER COLUMN [PaidAmount] DECIMAL(18,2) NOT NULL;
END

GO


IF COL_LENGTH('dbo.LedgerLines','IsClosed') IS NULL
BEGIN
    ALTER TABLE dbo.LedgerLines ADD [IsClosed] BIT NOT NULL CONSTRAINT DF_LedgerLines_IsClosed DEFAULT(0);
END

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250906230235_LedgerLine_DecimalPrecision_and_Expense_Note512', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO


IF COL_LENGTH('dbo.Expenses','CurrencyCode') IS NULL
BEGIN
    ALTER TABLE dbo.Expenses ADD [CurrencyCode] NVARCHAR(3) NULL;
END
ELSE
BEGIN
    ALTER TABLE dbo.Expenses ALTER COLUMN [CurrencyCode] NVARCHAR(3) NULL;
END

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250906231446_Add_Expense_CurrencyCode_3', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO


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

GO


IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE object_id = OBJECT_ID('dbo.PaymentAllocations') 
             AND name = 'IX_PaymentAllocations_LedgerEntryId')
    DROP INDEX [IX_PaymentAllocations_LedgerEntryId] ON [dbo].[PaymentAllocations];

IF EXISTS (SELECT 1 FROM sys.indexes 
           WHERE object_id = OBJECT_ID('dbo.PaymentAllocations') 
             AND name = 'IX_PaymentAllocations_LedgerLineId')
    DROP INDEX [IX_PaymentAllocations_LedgerLineId] ON [dbo].[PaymentAllocations];

GO


IF COL_LENGTH('dbo.PaymentAllocations', 'LedgerLineId') IS NULL
   AND COL_LENGTH('dbo.PaymentAllocations', 'LedgerEntryId') IS NOT NULL
BEGIN
    EXEC sp_rename 'dbo.PaymentAllocations.LedgerEntryId', 'LedgerLineId', 'COLUMN';
END

GO


IF COL_LENGTH('dbo.PaymentAllocations','LedgerLineId') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[PaymentAllocations] ALTER COLUMN [LedgerLineId] BIGINT NOT NULL;
END

GO


IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE object_id = OBJECT_ID('dbo.PaymentAllocations') 
      AND name = 'IX_PaymentAllocations_LedgerLineId'
)
BEGIN
    CREATE INDEX [IX_PaymentAllocations_LedgerLineId]
    ON [dbo].[PaymentAllocations]([LedgerLineId]);
END

GO


IF OBJECT_ID('dbo.FK_PaymentAllocations_LedgerLines_LedgerLineId','F') IS NULL
BEGIN
    ALTER TABLE [dbo].[PaymentAllocations] WITH CHECK
    ADD CONSTRAINT [FK_PaymentAllocations_LedgerLines_LedgerLineId]
    FOREIGN KEY([LedgerLineId]) REFERENCES [dbo].[LedgerLines]([Id])
    ON DELETE CASCADE;
END

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250907002022_Rename_PaymentAllocation_LedgerEntryId_to_LedgerLineId', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO


IF COL_LENGTH('dbo.PaymentAllocations','CreatedAt') IS NULL
BEGIN
    ALTER TABLE dbo.PaymentAllocations
    ADD CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_PaymentAllocations_CreatedAt DEFAULT (SYSUTCDATETIME());
END

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250907003738_Add_PaymentAllocation_CreatedAt', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Expenses] ADD [ParentExpenseId] int NULL;
GO

CREATE INDEX [IX_Expenses_ParentExpenseId] ON [Expenses] ([ParentExpenseId]);
GO

ALTER TABLE [Expenses] ADD CONSTRAINT [FK_Expenses_Expenses_ParentExpenseId] FOREIGN KEY ([ParentExpenseId]) REFERENCES [Expenses] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250912082453_AddExpenseParentSelfRef', N'7.0.16');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Expenses] ADD [DueDay] tinyint NULL;
GO

ALTER TABLE [Expenses] ADD [InstallmentCount] int NULL;
GO

ALTER TABLE [Expenses] ADD [InstallmentIndex] int NULL;
GO

ALTER TABLE [Expenses] ADD [PlanStartMonth] datetime2 NULL;
GO

CREATE INDEX [IX_Expenses_PlanStartMonth] ON [Expenses] ([PlanStartMonth]);
GO

CREATE INDEX [IX_Expenses_DueDay] ON [Expenses] ([DueDay]);
GO

CREATE INDEX [IX_Expenses_ParentExpenseId_InstallmentIndex] ON [Expenses] ([ParentExpenseId], [InstallmentIndex]);
GO


UPDATE E
SET E.DueDay = DAY(E.CreatedDate)
FROM Expenses AS E
WHERE E.DueDay IS NULL;

GO


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

GO


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

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250912225727_TarihVeSaatKisimlariDuzeltildi', N'7.0.16');
GO

COMMIT;
GO

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

