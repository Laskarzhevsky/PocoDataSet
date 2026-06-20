CREATE TABLE [dbo].[HostedApplicationLayerLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,

	[LoggedEntityId] bigint NULL,

	[ApplicationLayerName] varchar(32) NOT NULL,
	[DomainName] varchar(128) NOT NULL,
	[Url] varchar(1024) NOT NULL,
	[UseCaseName] varchar(128) NOT NULL,
	
	[LoggedEntityBusinessGuid] [uniqueidentifier] NULL,
	[LoggedEntityBusinessStringRepresentation] [nvarchar](1024) NULL,
	[LoggedEntityCreatedByUserGuid] [uniqueidentifier] NULL,
	[LoggedEntityCreatedByUserName] [varchar](50) NULL,
	[LoggedEntityDateOfCreation] datetime2(7) NULL,
	[LoggedEntityDateOfModification] datetime2(7) NULL,
	[LoggedEntityGuid] [uniqueidentifier] NULL,
	[LoggedEntityIsArchived] [bit] NULL,
	[LoggedEntityIsDeleted] [bit] NULL,
	[LoggedEntityModifiedByUserGuid] [uniqueidentifier] NULL,
	[LoggedEntityModifiedByUserName] [varchar](50) NULL,
	[LoggedEntityStringRepresentation] [varchar](290) NULL,
	
	[BusinessGuid] [uniqueidentifier] NULL,
	[BusinessStringRepresentation] [nvarchar](1024) NULL,
	[CreatedByUserGuid] uniqueidentifier NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_CreatedByUserGuid]
		DEFAULT ('00000000-0000-0000-0000-000000000001'),
	[CreatedByUserName] varchar(50) NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_CreatedByUserName]
		DEFAULT ('System'),
	[DateOfCreation] datetime2(7) NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_DateOfCreation]
		DEFAULT (SYSUTCDATETIME()),
	[DateOfModification] datetime2(7) NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_DateOfModification]
		DEFAULT (SYSUTCDATETIME()),
	[Guid] uniqueidentifier NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_Guid]
		DEFAULT (NEWSEQUENTIALID()),
	[IsArchived] [bit] NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_IsArchived]
		DEFAULT (0),
	[IsDeleted] [bit] NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_IsDeleted]
		DEFAULT (0),
	[ModifiedByUserGuid] [uniqueidentifier] NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_ModifiedByUserGuid]
		DEFAULT ('00000000-0000-0000-0000-000000000001'),
	[ModifiedByUserName] varchar(50) NOT NULL
		CONSTRAINT [DF_HostedApplicationLayerLog_ModifiedByUserName]
		DEFAULT ('System'),
	[StringRepresentation] AS
	(
		CONVERT(varchar(290), [DomainName] + '.' + [UseCaseName] + '.' + [ApplicationLayerName])
	) PERSISTED,

    CONSTRAINT [PK_HostedApplicationLayerLog]
	PRIMARY KEY CLUSTERED ([Id] ASC)
)
GO