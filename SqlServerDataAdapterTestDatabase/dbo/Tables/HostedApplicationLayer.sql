CREATE TABLE [dbo].[HostedApplicationLayer](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,

	[ApplicationLayerName] varchar(32) NOT NULL,
	[DomainName] varchar(128) NOT NULL,
	[Url] varchar(1024) NOT NULL,
	[UseCaseName] varchar(128) NOT NULL,
	
	[BusinessGuid] [uniqueidentifier] NULL,
	[BusinessStringRepresentation] [nvarchar](1024) NULL,
	[CreatedByUserGuid] uniqueidentifier NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_CreatedByUserGuid]
		DEFAULT ('00000000-0000-0000-0000-000000000001'),
	[CreatedByUserName] varchar(50) NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_CreatedByUserName]
		DEFAULT ('System'),
	[DateOfCreation] datetime2(7) NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_DateOfCreation]
		DEFAULT (SYSUTCDATETIME()),
	[DateOfModification] datetime2(7) NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_DateOfModification]
		DEFAULT (SYSUTCDATETIME()),
	[Guid] uniqueidentifier NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_Guid]
		DEFAULT (NEWSEQUENTIALID()),
	[IsArchived] [bit] NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_IsArchived]
		DEFAULT (0),
	[IsDeleted] [bit] NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_IsDeleted]
		DEFAULT (0),
	[ModifiedByUserGuid] [uniqueidentifier] NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_ModifiedByUserGuid]
		DEFAULT ('00000000-0000-0000-0000-000000000001'),
	[ModifiedByUserName] varchar(50) NOT NULL
		CONSTRAINT [DF_HostedApplicationLayer_ModifiedByUserName]
		DEFAULT ('System'),
	[StringRepresentation] AS
	(
		CONVERT(varchar(290), [DomainName] + '.' + [UseCaseName] + '.' + [ApplicationLayerName])
	) PERSISTED,

    CONSTRAINT [PK_HostedApplicationLayer]
	PRIMARY KEY CLUSTERED ([Id] ASC),
		
    CONSTRAINT [UC_HostedApplicationLayer_BpuIdentifier]
        UNIQUE NONCLUSTERED
        (
            [DomainName],
            [UseCaseName],
            [ApplicationLayerName]
        ),

    CONSTRAINT [UC_HostedApplicationLayer_StringRepresentation]
        UNIQUE NONCLUSTERED
        (
            [StringRepresentation]
        )
)
GO

