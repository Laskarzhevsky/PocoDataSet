# ADR-004: Replace Negative Primary Keys With ChangeState

Status: Accepted

## Context

The original save workflow used temporary negative primary key values to determine database operations:

```text
Id < 0     -> INSERT
Id > 0     -> UPDATE
IsDeleted  -> DELETE
```

This approach introduced several problems:

- Primary keys served multiple responsibilities:
  - Identifying rows
  - Encoding operation type
  - Temporary client identity generation
  - Post-save correlation

- Client-side temporary negative key generation raised concerns:
  - Thread safety
  - Global counters
  - Parent/child temporary key propagation
  - Ambiguous semantics

- Database-generated identities (IDENTITY/SEQUENCE) naturally belong to the database.

## Decision

Replace operation detection based on primary key values with explicit change metadata.

The new contract:

```text
Primary key
    -> identifies database row only

__ChangeState
    -> identifies requested operation

__ClientKey
    -> correlates inserted rows during post-save merge
```

ChangeState values:

| Value | Meaning |
|---------|---------|
| 0 | Unchanged |
| 1 | Added |
| 2 | Modified |
| 3 | Deleted |
| 4 | Detached |

Generated Save procedures use:

```sql
__ChangeState = 1 -> INSERT
__ChangeState = 2 -> UPDATE
__ChangeState = 3 -> DELETE
```

POCO DataSet itself remains independent of ADO.NET.

PocoDataSet.SqlServerDataAdapter converts POCO row state into TVP metadata.

## Consequences

Positive:

- Primary keys return to a single responsibility
- No temporary negative identity generation
- No thread-safety concerns around generated IDs
- Clear separation of identity vs operation
- Save procedures become easier to reason about
- Better support for database-generated identities
- Cleaner post-save merge behavior

Tradeoffs:

- TVP contracts changed
- Generated Save procedures changed
- Additional metadata columns required:
  - __ChangeState
  - __ClientKey

## Related components

- PocoDataSet.SqlServerDataAdapter
- Save stored procedure generation
- DoPostSaveMerge()
- TVP types (ListOfLong/ListOfInt/ListOfString)

## Date

2026
