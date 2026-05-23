# SQL Server Save Mode Contract

`SqlDataAdapter` now exposes `SqlDataAdapterOptions.SaveMode`.

## RowByRow

`SqlSaveMode.RowByRow` is the default. It preserves the existing relation-aware save pipeline.

## BatchStoredProcedure

`SqlSaveMode.BatchStoredProcedure` is the contract for generated Save stored procedures that accept SQL Server table-valued parameters.

The TVP conversion layer adds the `__ChangeState` column to each ADO.NET `DataTable`.

Values:

- `0` = Unchanged
- `1` = Added
- `2` = Modified
- `3` = Deleted

Generated Save procedures should use `__ChangeState` to classify insert, update, and delete operations instead of relying on negative primary-key values.

At this stage, the adapter exposes the options API and TVP conversion contract. Runtime execution of generated batch Save stored procedures still requires the SQL generation layer to emit procedures and table types that consume this contract.
