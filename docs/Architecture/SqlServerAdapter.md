# SQL Server Adapter

## Purpose

`PocoDataSet.SqlServerDataAdapter` connects POCO DataSet to SQL Server while keeping SQL Server-specific metadata and command generation outside the core model.

## Main responsibilities

```text
Load data into IDataSet/IDataTable
Read SQL Server schema and keys
Populate relations from database schema
Build save commands from changesets
Order saves by foreign-key dependencies
Handle transactions
Handle concurrency metadata such as rowversion/original values
```

## Important internal concepts

| Component | Responsibility |
|---|---|
| `MetadataLoader` | Loads table write metadata and foreign-key edges. |
| `RelationsManager` | Loads primary keys, foreign keys, and dataset relations from SQL Server schema. |
| `TableSorter` | Orders changed tables by foreign-key dependencies. |
| `ChangesetProcessor` | Identifies tables and rows with changes. |
| `PrimaryKeyProcessor` | Builds and validates primary-key information. |
| `ServiceParametersProvider` | Builds command parameters, including concurrency clauses. |
| `DataTableValidator` | Validates save preconditions. |

## Boundary rule

SQL Server metadata may enrich `IDataSet` schema and relations, but SQL Server concerns should not enter the core data classes.
