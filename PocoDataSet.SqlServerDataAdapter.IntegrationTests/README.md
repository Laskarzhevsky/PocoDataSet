# PocoDataSet.SqlServerDataAdapter.IntegrationTests

These tests exercise `PocoDataSet.SqlServerDataAdapter` against a real SQL Server database.
They are intentionally separated from unit tests because SQL Server TVPs, SQL user-defined table types,
stored procedures, schema metadata, and triggers cannot be reliably tested with an in-memory database.

## Required database

Publish `BPUA.InfrastructureServer.Database` before running these tests. The database must contain:

- `dbo.HostedApplicationLayer`
- `dbo.HostedApplicationLayerLog`
- `dbo.HostedApplicationLayer` table type
- `[HostedApplicationLayer].[SynchronizeHostedApplicationLayers]`
- `[HostedApplicationLayer].[FindHostedApplicationLayersByIdentifiers]`

## Connection string

Set connection string in IntegrationTestConnectionString.cs file
