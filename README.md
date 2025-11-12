# PocoDataSet

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

- **PocoDataSet.Serializer**  
  JSON serialization helpers using `System.Text.Json`:
  - `DataSetSerializer.FromJsonString(string? json) : IDataSet?`
  - `DataSetSerializer.ToJsonString(IDataSet dataSet) : string`

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

