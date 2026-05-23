# Changesets and Correlation

## Purpose

A changeset is a dataset containing only changed rows. It is the bridge between local edits and persistence layers such as EF Core or SQL Server.

## CreateChangeset behavior

`IDataSet.CreateChangeset()` creates a new dataset containing only tables with changed rows.

Changed row states included:

```text
Added
Modified
Deleted
```

Unchanged rows are not included.

## Row payload rules

| Source row state | Changeset row type | Payload |
|---|---|---|
| Added | regular row | full row values plus generated `__ClientKey` |
| Modified | floating row | primary-key columns plus changed columns |
| Deleted | floating row | primary-key columns plus optional `__ClientKey` |

Floating rows are significant because missing fields mean "not provided", which is different from a provided `null` value.

## Correlation

For inserted rows, the client does not yet know server-generated identifiers. `__ClientKey` allows the post-save response to map server-generated values back to the client row.

## Persistence adapter expectation

Persistence adapters should be able to process a changeset without needing the entire original dataset.

Post-save responses should be minimal: return only what the client needs to reconcile local rows, especially generated primary keys, rowversion/concurrency values, and changed server-side defaults.
