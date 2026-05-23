# Relations

## Relation model

Relations are DataSet-level metadata that describe parent/child table relationships.

A relation stores names, not object references:

```text
ParentTableName
ParentColumnNames
ChildTableName
ChildColumnNames
```

This keeps the relation model serializable and prevents relation objects from retaining removed tables or rows.

## AddRelation invariants

`DataSet.AddRelation` enforces:

```text
parent column list is not empty
child column list is not empty
parent and child column counts match
duplicate relation names are rejected case-insensitively
```

## RemoveRelation invariants

`RemoveRelation(name)` removes by relation name using case-insensitive comparison.

## RemoveTable relation cleanup

When a table is removed, every relation where that table is parent or child must also be removed.

This avoids stale metadata after table removal.

## Validation layer

Relation validation is implemented in extensions (`ValidateRelations`, `EnsureRelationsValid`, and relation helper classes). This keeps core `DataSet` lightweight while still allowing callers to enforce integrity when needed.
