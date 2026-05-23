# Row State Lifecycle

## States

`IDataRow.DataRowState` follows this conceptual model:

| State | Meaning |
|---|---|
| `Detached` | Row is not physically owned by a table. |
| `Added` | Row was added locally and has not been accepted/saved. |
| `Unchanged` | Row is baseline/current and has no pending changes. |
| `Modified` | Row has changed from its original baseline. |
| `Deleted` | Row is logically deleted but still physically present until deletion is accepted. |

## Add workflows

### `DataTable.AddRow`

Used for client/local addition.

Rules:

```text
null row -> return
Deleted row -> throw
row already in table -> throw
missing columns are added with null for non-floating rows
Detached row becomes Added
row receives copied primary-key column names
row is appended to Rows
```

### `DataTable.AddLoadedRow`

Used for data loaded from storage or infrastructure workflows.

Rules:

```text
Deleted row -> throw
missing columns are added with null for non-floating rows
row receives copied primary-key column names
row is appended to Rows
Detached or Added row becomes Unchanged
```

## Delete workflows

### `IDataRow.Delete()`

Marks an existing row as logically deleted. It does not remove the row from the table.

Rules:

```text
Deleted or Detached -> no-op
Added -> throw; undoing an add is a table-level remove operation
Unchanged/Modified -> create original-values snapshot if needed, remember prior state, set Deleted
```

### `IDataTable.DeleteRow(row)` extension

Table-level delete semantics:

```text
Added -> physically remove row
Unchanged/Modified -> logical delete
Deleted/Detached -> no-op
```

## Accept workflows

### `IDataRow.AcceptChanges()`

Accepts changes for Added/Modified rows only. It intentionally throws for Deleted rows because deletion is table-level.

```text
Added/Modified -> clear original-values snapshot and become Unchanged
Deleted -> throw
Detached/Unchanged -> no-op
```

### `IDataTable.AcceptChanges()` extension

```text
Deleted -> physically remove row
Added/Modified -> row.AcceptChanges()
Unchanged/Detached -> no-op
```

## Reject workflows

### `IDataRow.RejectChanges()`

```text
Added -> throw; rejecting an add is table-level
Modified/Deleted -> restore original values, clear original snapshot, become Unchanged
Detached/Unchanged -> no-op
```

### `IDataTable.RejectChanges()` extension

```text
Added/Detached -> physically remove row
Modified/Deleted -> row.RejectChanges()
Unchanged -> no-op
```

## Maintainer warning

Do not make `DataRow.AcceptChanges()` remove a row. A row does not know its table, by design. Physical row removal belongs to `IDataTable`.
