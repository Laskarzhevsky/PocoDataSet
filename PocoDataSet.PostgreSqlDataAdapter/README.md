# PocoDataSet.PostgreSqlDataAdapter

`PocoDataSet.PostgreSqlDataAdapter` is the PostgreSQL provider implementation of `IRelationalDataAdapter`.

The adapter uses Npgsql and intentionally keeps PostgreSQL-specific features outside the common abstraction. SQL Server table-valued parameters are not emulated here. For PostgreSQL list and document-style parameters, use provider-specific helpers such as `CreateArrayParameter` and `CreateJsonbParameter`.

## Current scope

Implemented:

- `FillAsync`
- `FillIntoExistingDataSetAsync`
- `ExecuteNonQueryAsync`
- `ExecuteScalarAsync`
- `InsertAsync`
- `UpdateAsync`
- `DeleteAsync`
- PostgreSQL-specific parameter helpers

Not implemented yet:

- Generated `SaveChangesAsync` persistence. PostgreSQL save command generation should be designed separately because PostgreSQL uses different persistence patterns than SQL Server TVPs.
