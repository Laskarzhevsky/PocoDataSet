create type [dbo].[HostedApplicationLayer] AS TABLE 
(
    [Id] [bigint] NULL,
    [ApplicationLayerName] [varchar] (32) NULL,
    [DomainName] [varchar] (128) NULL,
    [Guid] [uniqueidentifier] NULL,
    [Url] [varchar] (1024) NULL,
    [UseCaseName] [varchar] (128) NULL,
    [BusinessGuid] [uniqueidentifier] NULL,
    [BusinessStringRepresentation] [nvarchar] (1024) NULL,
    [CreatedByUserGuid] [uniqueidentifier] NULL,
    [CreatedByUserName] [varchar] (50) NULL,
    [DateOfCreation] [datetime2] NULL,
    [DateOfModification] [datetime2] NULL,
    [IsArchived] [bit] NULL,
    [IsDeleted] [bit] NULL,
    [ModifiedByUserGuid] [uniqueidentifier] NULL,
    [ModifiedByUserName] [varchar] (50) NULL,
    [__ClientKey] [uniqueidentifier] NULL,
    [__ChangeState] [int] NULL
)


GO

