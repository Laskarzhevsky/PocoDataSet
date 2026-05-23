# Lifecycle and Memory Safety

## Primary risk categories

| Area | Risk type | Current design |
|---|---|---|
| `DataRow` primary-key cache | Stale copied metadata | Cleared on physical detach. |
| `DataSet` relations | Stale table-name metadata | Relations referencing removed tables are removed by `RemoveTable`. |
| Observable views | Event-retention risk | Views are disposed and unsubscribed when removed. |
| Observable tables | Event-retention risk | Dataset unsubscribes from table events during `RemoveObservableTable`. |
| Serializer bridge dictionaries | Duplicate reference storage | Intentional duplicate storage for JSON bridge; must stay synchronized. |

## Core lifecycle rules

### Physical row removal

Physical removal means the row no longer belongs to the table. It must be detached.

Physical removal happens through:

```text
IDataTable.RemoveRow(row)
IDataTable.RemoveRowAt(index)
IDataTable.RemoveAllRows()
IDataTable.AcceptChanges() for Deleted rows
IDataTable.RejectChanges() for Added rows
```

### Logical row deletion

Logical deletion means the row stays in the table but is marked as `Deleted`.

Logical deletion happens through:

```text
IDataRow.Delete()
IDataTable.DeleteRow(row) for Unchanged/Modified rows
```

### Table removal

Table removal must clean both table dictionaries and relation metadata.

```text
DataSet.RemoveTable(tableName)
```

must leave no relation where the removed table is the parent or child.

## Observable lifecycle rules

Observable objects introduce event subscriptions. Unlike plain collections, event subscriptions can retain objects even when collections are cleaned.

The observable layer must always unsubscribe when removing or disposing:

```text
ObservableDataSet -> ObservableDataTable events
ObservableDataView -> ObservableDataTable events
ObservableDataTable -> ObservableDataRow events, if any are introduced or used
```

Any new observable feature should include a detach/unsubscribe test.
