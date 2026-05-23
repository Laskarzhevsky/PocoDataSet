# Regression Test Ideas

## Lifecycle and memory-oriented tests

1. Create table with primary key, add row, remove row, assert row is Detached and copied PK list is cleared if accessible through internal tests.
2. Create dataset with three tables and two relations, remove middle table, assert only dependent relations are removed.
3. Create observable dataset, create view, remove table, verify view cache no longer returns the removed view.
4. Use weak-reference tests around observable view/table removal if practical. These tests can be fragile, so prefer event-unsubscribe behavioral tests unless a real leak is suspected.

## Serialization tests

1. Serialize and deserialize regular and floating rows.
2. Serialize and deserialize deleted/modified rows with original values.
3. Verify no `JsonElement` remains in ordinary primitive value fields after deserialization.
4. Verify `__ClientKey` round-trips as expected.

## Merge tests

1. Post-save insert with server-generated Id.
2. Post-save update with server-generated rowversion.
3. Refresh preserving local Modified row.
4. Replace merge with relations present.
5. Replace merge with tables missing from refreshed dataset.

## Adapter tests

1. EF bridge composite key update.
2. EF bridge delete with relation-aware ordering.
3. SQL adapter transaction rollback on second-table failure.
4. SQL adapter concurrency conflict when rowversion/original values do not match.
