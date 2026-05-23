# DataTable Contract

## Owns

`IDataTable` owns:

```text
schema metadata
row collection
primary-key metadata derived from columns
```

## AddRow contract

```text
null row -> no-op
Deleted row -> throw
row already in same table -> throw
missing columns are created with null for non-floating rows
Detached row becomes Added
concrete DataRow receives copied primary-key column names
```

## AddLoadedRow contract

```text
Deleted row -> throw
missing columns are created with null for non-floating rows
concrete DataRow receives copied primary-key column names
Detached or Added row becomes Unchanged
```

## RemoveRow contract

```text
null row -> false
row not in table -> throw
row in table -> remove, detach, return true
```

## RemoveRowAt contract

```text
invalid index -> throw ArgumentOutOfRangeException
valid index -> remove row and detach it
```

## RemoveAllRows contract

```text
detach every row
clear row list
```

## Primary-key contract

`PrimaryKeys` is derived from `ColumnMetadata.IsPrimaryKey` flags. Do not introduce a second independent primary-key source.
