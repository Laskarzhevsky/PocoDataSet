CREATE PROCEDURE [HostedApplicationLayer].[SynchronizeHostedApplicationLayers]
    @HostedApplicationLayer dbo.HostedApplicationLayer READONLY,
    @UserGuid uniqueidentifier = NULL,
    @UserName varchar(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now datetime2(7);
    DECLARE @EffectiveUserGuid uniqueidentifier;
    DECLARE @EffectiveUserName varchar(50);

    SET @Now = SYSUTCDATETIME();
    SET @EffectiveUserGuid = ISNULL(@UserGuid, '00000000-0000-0000-0000-000000000001');
    SET @EffectiveUserName = ISNULL(@UserName, 'System');

    DECLARE @RequestedHostedApplicationLayer TABLE
    (
        [ApplicationLayerName] varchar(32) NOT NULL,
        [DomainName] varchar(128) NOT NULL,
        [Url] varchar(1024) NOT NULL,
        [UseCaseName] varchar(128) NOT NULL,
        [BusinessGuid] uniqueidentifier NULL,
        [BusinessStringRepresentation] nvarchar(1024) NULL,
        PRIMARY KEY
        (
            [DomainName],
            [UseCaseName],
            [ApplicationLayerName]
        )
    );

    INSERT INTO @RequestedHostedApplicationLayer
    (
        [ApplicationLayerName],
        [DomainName],
        [Url],
        [UseCaseName],
        [BusinessGuid],
        [BusinessStringRepresentation]
    )
    SELECT
        source.[ApplicationLayerName],
        source.[DomainName],
        MAX(source.[Url]),
        source.[UseCaseName],
        MAX(source.[BusinessGuid]),
        MAX(source.[BusinessStringRepresentation])
    FROM
        @HostedApplicationLayer source
    WHERE
        source.[ApplicationLayerName] IS NOT NULL
        AND source.[DomainName] IS NOT NULL
        AND source.[Url] IS NOT NULL
        AND source.[UseCaseName] IS NOT NULL
    GROUP BY
        source.[ApplicationLayerName],
        source.[DomainName],
        source.[UseCaseName];

    -- Layers that were previously registered for this host URL but are not present
    -- in the current host snapshot are marked as deleted.  The logging trigger keeps
    -- the historical record of this change.
    UPDATE destination
    SET
        destination.[DateOfModification] = @Now,
        destination.[IsDeleted] = 1,
        destination.[ModifiedByUserGuid] = @EffectiveUserGuid,
        destination.[ModifiedByUserName] = @EffectiveUserName
    FROM
        [dbo].[HostedApplicationLayer] destination
    WHERE
        destination.[IsDeleted] = 0
        AND EXISTS
        (
            SELECT 1
            FROM @RequestedHostedApplicationLayer requestedUrl
            WHERE requestedUrl.[Url] = destination.[Url]
        )
        AND NOT EXISTS
        (
            SELECT 1
            FROM @RequestedHostedApplicationLayer requested
            WHERE requested.[DomainName] = destination.[DomainName]
              AND requested.[UseCaseName] = destination.[UseCaseName]
              AND requested.[ApplicationLayerName] = destination.[ApplicationLayerName]
        );

    -- Existing layers from the current host snapshot are reactivated and touched.
    -- DateOfModification therefore acts as the host last-seen-online timestamp.
    UPDATE destination
    SET
        destination.[Url] = source.[Url],
        destination.[BusinessGuid] = source.[BusinessGuid],
        destination.[BusinessStringRepresentation] = source.[BusinessStringRepresentation],
        destination.[DateOfModification] = @Now,
        destination.[IsArchived] = 0,
        destination.[IsDeleted] = 0,
        destination.[ModifiedByUserGuid] = @EffectiveUserGuid,
        destination.[ModifiedByUserName] = @EffectiveUserName
    FROM
        [dbo].[HostedApplicationLayer] destination
        INNER JOIN @RequestedHostedApplicationLayer source
            ON source.[DomainName] = destination.[DomainName]
            AND source.[UseCaseName] = destination.[UseCaseName]
            AND source.[ApplicationLayerName] = destination.[ApplicationLayerName];

    -- New layers are inserted as active registrations.
    INSERT INTO [dbo].[HostedApplicationLayer]
    (
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
        [IsArchived],
        [IsDeleted],
        [ModifiedByUserGuid],
        [ModifiedByUserName]
    )
    SELECT
        source.[ApplicationLayerName],
        source.[DomainName],
        source.[Url],
        source.[UseCaseName],
        source.[BusinessGuid],
        source.[BusinessStringRepresentation],
        @EffectiveUserGuid,
        @EffectiveUserName,
        @Now,
        @Now,
        0,
        0,
        @EffectiveUserGuid,
        @EffectiveUserName
    FROM
        @RequestedHostedApplicationLayer source
    WHERE
        NOT EXISTS
        (
            SELECT 1
            FROM [dbo].[HostedApplicationLayer] destination
            WHERE destination.[DomainName] = source.[DomainName]
              AND destination.[UseCaseName] = source.[UseCaseName]
              AND destination.[ApplicationLayerName] = source.[ApplicationLayerName]
        );
END
GO
