# Ownership Rules

## DataSet owns tables

`DataSet` owns table references through its internal table dictionary. When the table is concrete `PocoDataSet.Data.DataTable`, it is also stored in `TablesJson` for serialization.

### Required invariant

When a table is removed, all table storage inside `DataSet` must be cleaned:

```text
_tables.Remove(tableName)
TablesJson.Remove(tableName)
Remove relations that reference the removed table
```

The current code follows this rule in `PocoDataSet.Data/DataSet.cs`.

## DataSet owns relation metadata

Relations live at DataSet level. A relation stores:

```text
RelationName
ParentTableName
ParentColumnNames
ChildTableName
ChildColumnNames
DisplayField
```

A relation must not hold references to `IDataTable`, `IDataRow`, or `IColumnMetadata` objects.

## DataTable owns rows and schema

`DataTable` owns:

```text
_dataTableSchema
_rows
```

The schema helper owns the column metadata list, column lookup dictionary, client-key column cache, and cached primary-key-name list derived from column flags.

## DataRow owns values only

`DataRow` owns:

```text
_values
_originalValues
_stateBeforeDelete
DataRowState
Selected flag
copied primary-key column names
```

`DataRow` may copy primary-key column names from its owning table when attached. That is copied metadata, not ownership of table schema.

## Detach rule

When a row is physically removed from a table, table-owned attachment metadata must be cleared:

```text
IsLoadedRow = false
PrimaryKeyColumns.Clear()
DataRowState = Detached
```

This is currently centralized through `DataRow.DetachFromTable()` and called by `DataTable.RemoveRow`, `RemoveRowAt`, and `RemoveAllRows`.

## Memory-leak review result

The core dataset/table/row graph does not show a classic memory leak caused by table-to-row-to-table cycles. The bigger risk area is stale metadata, especially relations and observable event subscriptions.

The recent cleanup rules address the two important lifecycle gaps:

1. Removed rows clear table-owned metadata.
2. Removed tables remove relations referencing the removed table.
