# SQL Server TVP change-state contract

Generated Save stored procedures should classify operations by the `__ChangeState` metadata column instead of primary-key sign values.

Recommended values:

- `0` = Unchanged / ignored
- `1` = Added / insert
- `2` = Modified / update
- `3` = Deleted / delete or soft delete

The primary key should identify an existing database row for update/delete. It should not encode the requested operation. For database-generated identity/sequence keys, an added row may have `Id = 0` until the database returns the real key and post-save merge reconciles it.

Generated SQL pattern:

```sql
-- Insert
where __ChangeState = 1

-- Update
where __ChangeState = 2

-- Delete or soft delete
where __ChangeState = 3
```

The SQL Server adapter exposes `SqlServerTableValuedParameterBuilder` and `SqlDataAdapter.CreateTableValuedParameterDataTables(...)` to build ADO.NET `DataTable` instances with this metadata column.
