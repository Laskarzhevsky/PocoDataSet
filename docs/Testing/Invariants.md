# Invariants Requiring Tests

## DataSet invariants

- `AddTable` rejects duplicate table names.
- `AddTable` rejects empty/null table names.
- `RemoveTable` removes runtime table entry.
- `RemoveTable` removes concrete table from `TablesJson`.
- `RemoveTable` removes parent relations that reference removed table.
- `RemoveTable` removes child relations that reference removed table.
- `RemoveTable` preserves unrelated relations.
- `RemoveTable` throws when table does not exist.
- `AddRelation` rejects duplicate relation names case-insensitively.
- `AddRelation` rejects mismatched parent/child column counts.

## DataTable invariants

- `AddRow` turns Detached row into Added.
- `AddLoadedRow` turns Detached/Added row into Unchanged.
- `AddRow` rejects Deleted row.
- `AddRow` rejects same row instance already in same table.
- `RemoveRow` detaches removed row.
- `RemoveRowAt` detaches removed row.
- `RemoveAllRows` detaches all removed rows.
- Removed rows have cleared copied primary-key metadata.

## DataRow invariants

- Detached row value assignment does not create original-values snapshot.
- Unchanged row changed value creates original-values snapshot and becomes Modified.
- `Delete` on Added row throws.
- `Delete` on Detached row is no-op.
- `AcceptChanges` on Deleted row throws.
- `RejectChanges` on Added row throws.

## Changeset invariants

- Unchanged rows are not included.
- Added rows include full values and `__ClientKey`.
- Modified rows include primary keys and changed values only.
- Deleted rows include primary keys and optional `__ClientKey`.
- Floating rows preserve missing-field semantics.

## Merge invariants

- `DoPostSaveMerge` reconciles server-generated keys.
- `DoReplaceMerge` preserves dataset structural correctness.
- `DoRefreshMergePreservingLocalChanges` does not overwrite local pending edits.
- `MergeWith` routes to explicit methods and remains obsolete.

## Serialization invariants

- Round-trip preserves tables, columns, rows, states, values, original values, relations, and row kind.
- Object values do not deserialize as `JsonElement` where primitive values are expected.
- Old/deprecated primary-key JSON shapes are rejected or normalized according to current policy.

## Observable invariants

- Removing observable table unsubscribes dataset handlers.
- Removing observable table disposes/removes related views.
- Removing observable view unsubscribes from table events.
- Observable merges do not fire duplicate/noisy events for unchanged values.
