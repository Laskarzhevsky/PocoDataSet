# DataRow Contract

## Owns

`IDataRow` owns:

```text
current values
optional original-values snapshot
row state
selection flag
```

Concrete `DataRow` also keeps copied primary-key column names for infrastructure use.

## Does not own

`IDataRow` must not own:

```text
DataTable
DataSet
ColumnMetadata objects from table schema
relation objects
observable views
```

## Indexer contract

Setting a value on a detached row should not create a misleading original-values snapshot.

Setting a value on an unchanged row should create an original-values snapshot and move the row to Modified when the value changes.

## Delete contract

`Delete()` marks a row as deleted. It must not physically remove the row because the row does not know its owning table.

## AcceptChanges contract

`AcceptChanges()` accepts Added and Modified rows. It throws for Deleted rows because accepting deletion is table-level.

## RejectChanges contract

`RejectChanges()` reverts Modified/Deleted rows. It throws for Added rows because rejecting an add is table-level.

## Detach contract

When a row is physically removed from a table, concrete `DataRow.DetachFromTable()` must clear table-owned attachment metadata and set state to Detached.
