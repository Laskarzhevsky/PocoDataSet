# DataSet Contract

## Owns

`IDataSet` owns:

```text
tables by name
relations by name
```

## Does not own

`IDataSet` should not own UI state, EF Core entities, SQL Server connections, Blazor components, or external service clients.

## AddTable contract

```text
TableName must not be null or empty.
Duplicate table names are rejected.
Concrete DataTable instances must be added to TablesJson as well as runtime Tables.
```

## RemoveTable contract

```text
If table exists:
    remove from runtime table dictionary
    remove from TablesJson
    remove all relations referencing the table as parent or child
If table does not exist:
    throw KeyNotFoundException
```

## AddRelation contract

```text
Parent column list cannot be empty.
Child column list cannot be empty.
Parent and child column counts must match.
Relation name must be unique case-insensitively.
```

## RemoveRelation contract

```text
Remove by relation name case-insensitively.
Return true if removed.
Return false if not found.
```

## Serialization contract

Concrete `DataSet` must keep `Tables` and `TablesJson` synchronized for concrete tables.
