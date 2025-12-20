# PocoDataSet

The POCO DataSet documentation is published in the "POCO DataSet" section of **businessprocessprogramming.net** website.

**PocoDataSet** is a lightweight, in-memory `DataSet` / `DataTable` / `DataRow` model built around simple POCOs and interfaces.

It is designed to be:

- **UI-friendly** – works well with Blazor, WPF, WinUI, etc.
- **POCO-based** – data is represented as plain C# objects, not tied to any specific framework.
- **Extensible** – core contracts live in `PocoDataSet.IData`, with implementations and helpers in separate projects.

---

## Project structure

The solution contains four projects:

- **PocoDataSet.IData**  
  Interfaces and contracts only:
  - `IDataSet`, `IDataTable`, `IDataRow`
  - `IColumnMetadata`, `IDataRelation`, `IForeignKeyData`
  - `IDataSetValidator`, `IMergeOptions`, `IDataRowFilter`
  - `IValueConverter` (for pluggable value conversion)

  Interfaces and contracts for the merge infrastructure:
  - `IDataSetMergeEngine` – orchestrates DataSet / DataTable / DataRow merge
  - `IDataSetMergeConfiguration` – provides merge policies and handlers
  - `ITableMergeHandler` – table-level merge behavior
  - `IRowMergeHandler` – row-level merge behavior
  - `IDataTypeDefaultValueProvider` – pluggable provider for metadata-based default values

- **PocoDataSet.Data**  
  Concrete implementations of the interfaces:
  - `DataSet` – holds a dictionary of named tables and relations
  - `DataTable` – holds columns and rows
  - `DataRow` – a dictionary-backed row with helper methods and state (`RowState`, `Selected`)
  - `ColumnMetadata`, `DataRelation`, `ForeignKeyData`

- **PocoDataSet.Extensions**  
  Convenience extension methods and helpers:
  - `DataSetExtensions`
    - `AddTable`, `AddNewTable`, `AddRow`, `RemoveRow`, `RemoveTable`
    - `GetTable`, `GetRequiredTable`, `TryGetTable`, `GetFieldValue<T>`
    - `MergeWith` and related merge helpers
  - `DataTableExtensions`
    - `AddColumn`, `AddColumns`
    - `AddColumnsFromInterface` (build schema from a .NET interface)
  - `DataRowExtensions`
    - `CreateRowFromColumns`, `CreateRowFromColumnsWithDefaultValues`
    - `GetDataFieldValue<T>` (with friendly conversion)
    - `CopyFromPoco`, `ToPoco`, `MergeWith`
  - `DataSetFactory`
    - `CreateDataSet()` – simple factory for `IDataSet`
  - Internal helpers (data type names, default value helpers, type info, etc.)

  Default implementation of the merge infrastructure:
  - `DataSetMergeEngine` – handler-based merge engine
  - `DataSetMergeConfiguration` – default configuration and wiring
  - `DefaultTableMergeHandler` – primary-key based table merge
  - `DefaultRowMergeHandler` – value-level row merge
  - `DataTypeDefaultValueProvider` – default metadata-to-value mapping

- **PocoDataSet.Serializer**  
  JSON serialization helpers using `System.Text.Json`:
  - `DataSetSerializer.FromJsonString(string? json) : IDataSet?`
  - `DataSetSerializer.ToJsonString(IDataSet dataSet) : string`

 - **PocoDataSet.SqlServerDataAdapter**  
The `PocoDataSet.SqlServerDataAdapter` project provides a lightweight SQL Server adapter that fills `PocoDataSet` in-memory `IDataSet`/`IDataTable` structures from T-SQL queries. It is intended as an example/utility that demonstrates reading schema, primary key and foreign key information and populating `PocoDataSet.Data` model objects.

## Purpose
- Execute queries against SQL Server and populate `IDataSet`/`IDataTable`/`IDataRow`.
- Preserve table/column metadata (column names, data types, max length, primary keys, foreign keys, nullability).
- Provide an async-friendly `FillAsync` API that accepts parameter dictionaries to avoid inline concatenation and reduce risk of SQL injection.

## Key types
- `PocoDataSet.SqlServerDataAdapter.SqlDataAdapter` — main adapter class used to execute queries and return an `IDataSet`.
- `PocoDataSet.SqlServerDataAdapter.DataTableCreator` — internal helper used by the adapter to create `DataTable` instances with columns, primary keys and rows.
- `PocoDataSet.SqlServerDataAdapter.ForeignKeyData` — holds foreign key mapping information used when building `ColumnMetadata`.

## Notable behaviors
- `DataTableCreator` reads column schema from `SqlDataReader.GetSchemaTable()` and populates `ColumnMetadata` fields such as `ColumnName`, `DataType`, `MaxLength`, and (when implemented) `IsNullable`. The project also supports loading primary key information and populating `DataTable.PrimaryKey`.
- The adapter supports a parameter dictionary passed into `FillAsync`, e.g. `IDictionary<string, object?>` to map command parameters.

## Integration notes
- The adapter uses `Microsoft.Data.SqlClient`. Ensure your application has the package referenced.
- `FillAsync` supports parameter dictionaries; keys should match the parameter names used in the SQL (including the `@` if you use it in your code).
- `DataTableCreator` exposes an event (`LoadDataTableKeysInformationRequest`) so callers can supply primary key and foreign-key lookup data before columns/rows are finalized. This allows key population logic to run before rows are added.

## Recommended improvements (already implemented or easy to add)
- Populate `ColumnMetadata.IsNullable` from `GetSchemaTable()` (`AllowDBNull` / `IsNullable`) when available.
- Populate `DataTable.PrimaryKey` from key discovery (the adapter already provides hooks to fetch primary key information — call these before finalizing the `DataTable`).
- Ensure the adapter infers nullability conservatively when the provider schema does not include it (for example, treat CLR non-nullable value types as non-nullable).

## Where to find it in the solution
- Project folder: `PocoDataSet.SqlServerDataAdapter`
- Key files:
  - `SqlDataAdapter.cs` — public adapter API
  - `DataTableCreator/*.cs` — schema / row building helpers
  - `ForeignKeyData.cs` — FK metadata model

---

## Getting started

### 1. Add the projects to your solution

If you cloned this repo standalone:

- Open `PocoDataSetSolution.sln` in Visual Studio or `dotnet` CLI.
- Or reference the projects from your own solution:

<ItemGroup>
  <ProjectReference Include="..\PocoDataSet.IData\PocoDataSet.IData.csproj" />
  <ProjectReference Include="..\PocoDataSet.Data\PocoDataSet.Data.csproj" />
  <ProjectReference Include="..\PocoDataSet.Extensions\PocoDataSet.Extensions.csproj" />
  <ProjectReference Include="..\PocoDataSet.Serializer\PocoDataSet.Serializer.csproj" />
  <ProjectReference Include="..\PocoDataSet.SqlServerDataAdapter\PocoDataSet.SqlServerDataAdapter.csproj" />
</ItemGroup>

## License

This project is licensed under the MIT License.  
See the [LICENSE](LICENSE) file for details.

## Installation

The easiest way to get started is to install the meta-package:

dotnet add package PocoDataSet

This will bring in:
PocoDataSet.IData
PocoDataSet.Data
PocoDataSet.Extensions
PocoDataSet.Serializer
PocoDataSet.SqlServerDataAdapter





