# TVP Search Modifications

This package keeps the generic generated entity TVP direction:

- Added `FillAsync` overloads that accept `SqlParameter[]` / `params SqlParameter[]`.
- Added `FillIntoExistingDataSet` / `FillIntoExistingDataSetAsync` overloads that accept `SqlParameter[]`.
- Kept `CreateTableValuedParameter(string parameterName, string typeName, IDataTable dataTable, bool changedRowsOnly = false)` as the generic full-entity TVP entry point.
- Kept `CreateTableValuedParameterDataTables(IDataSet changeset, bool changedRowsOnly = true)` for Save/batch scenarios.
- Added `__ClientKey` support in generated ADO.NET DataTables, before `__ChangeState`, matching generated entity TVP types.
- Did not add procedure-specific or selected-column TVP builders.

Example search usage:

```csharp
SqlParameter parameter = adapter.CreateTableValuedParameter(
    "@HostedApplicationLayer",
    "dbo.HostedApplicationLayer",
    hostedApplicationLayerTable);

await adapter.FillIntoExistingDataSetAsync(
    requestTransitionContext,
    "dbo.FindHostedApplicationLayersByIdentifiers",
    true,
    parameter);
```

The source `IDataTable` should have a schema compatible with the generated SQL TVP type. Missing generated adapter metadata `__ClientKey` and `__ChangeState` is handled by the adapter.


## Event-based SQL type schema loading

`SqlDataAdapter.CreateTableValuedParameterAsync(...)` now uses `SqlServerTableValuedParameterCreator`, which raises `LoadTableValuedParameterSchemaRequest`. The adapter handles that event and loads the SQL user-defined table type schema, then the builder creates the ADO.NET `DataTable` with the exact SQL column order. The static builder does not open database connections.
