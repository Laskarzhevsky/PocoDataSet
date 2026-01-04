# PocoDataSet.EfCoreBridge.Tests

xUnit tests for the `PocoDataSet.EfCoreBridge` package.

## What is covered

Essentials:
- Applying changesets using **tracked** entities (`CopyToPoco` applier):
  - Added/Modified/Deleted for single key
  - Added/Modified/Deleted for composite key using `IDataTable.PrimaryKeys`
  - Throws when `PrimaryKeys` is missing
- Applying changesets using **detached** entities (`ToPoco` applier):
  - Added/Modified/Deleted

## How it runs

Uses EF Core `InMemory` provider with a unique database name per test.
