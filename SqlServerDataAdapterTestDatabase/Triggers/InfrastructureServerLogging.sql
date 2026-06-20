CREATE TRIGGER [dbo].[HostedApplicationLayerLogging]
ON [dbo].[HostedApplicationLayer]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[HostedApplicationLayerLog]
    (
        [LoggedEntityId],

        [ApplicationLayerName],
        [DomainName],
        [Url],
        [UseCaseName],

        [LoggedEntityBusinessGuid],
        [LoggedEntityBusinessStringRepresentation],
        [LoggedEntityCreatedByUserGuid],
        [LoggedEntityCreatedByUserName],
        [LoggedEntityDateOfCreation],
        [LoggedEntityDateOfModification],
        [LoggedEntityGuid],
        [LoggedEntityIsArchived],
        [LoggedEntityIsDeleted],
        [LoggedEntityModifiedByUserGuid],
        [LoggedEntityModifiedByUserName],
        [LoggedEntityStringRepresentation],

        [BusinessGuid],
        [BusinessStringRepresentation],
        [CreatedByUserGuid],
        [CreatedByUserName],
        [DateOfCreation],
        [DateOfModification],
        [Guid],
        [IsArchived],
        [IsDeleted],
        [ModifiedByUserGuid],
        [ModifiedByUserName]
    )
    SELECT
        [Id],

        [ApplicationLayerName],
        [DomainName],
        [Url],
        [UseCaseName],

        [BusinessGuid],
        [BusinessStringRepresentation],
        [CreatedByUserGuid],
        [CreatedByUserName],
        [DateOfCreation],
        [DateOfModification],
        [Guid],
        [IsArchived],
        [IsDeleted],
        [ModifiedByUserGuid],
        [ModifiedByUserName],
        [StringRepresentation],

        [BusinessGuid],
        [BusinessStringRepresentation],
        '00000000-0000-0000-0000-000000000001',
        'System',
        SYSUTCDATETIME(),
        SYSUTCDATETIME(),
        NEWID(),
        0,
        0,
        '00000000-0000-0000-0000-000000000001',
        'System'
    FROM
        Inserted;
END
GO