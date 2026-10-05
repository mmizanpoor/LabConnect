BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929081130_AddCenterProfileApiKeyEnabled'
)
BEGIN
    ALTER TABLE [dbo].[CenterProfiles] ADD [IsApiKeyEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929081130_AddCenterProfileApiKeyEnabled'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929081130_AddCenterProfileApiKeyEnabled', N'9.0.16');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929094018_AddApiKeys'
)
BEGIN
    CREATE TABLE [dbo].[ApiKeys] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [KeyName] nvarchar(64) NOT NULL,
        [KeyPrefix] nvarchar(3) NOT NULL,
        [KeySuffix] nvarchar(3) NOT NULL,
        [KeyLength] int NOT NULL,
        [CreateDate] datetime2 NOT NULL,
        [AllowAdd] bit NOT NULL,
        [AllowEdit] bit NOT NULL,
        [CenterProfileId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ApiKeys] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApiKeys_CenterProfiles_CenterProfileId] FOREIGN KEY ([CenterProfileId]) REFERENCES [dbo].[CenterProfiles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929094018_AddApiKeys'
)
BEGIN
    CREATE INDEX [IX_ApiKeys_CenterProfileId_CreateDate] ON [dbo].[ApiKeys] ([CenterProfileId], [CreateDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929094018_AddApiKeys'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ApiKeys_KeyName] ON [dbo].[ApiKeys] ([KeyName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929094018_AddApiKeys'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929094018_AddApiKeys', N'9.0.16');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929125939_AddApiKeyAllowView'
)
BEGIN
    ALTER TABLE [dbo].[ApiKeys] ADD [AllowView] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260929125939_AddApiKeyAllowView'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260929125939_AddApiKeyAllowView', N'9.0.16');
END;

COMMIT;
GO

