# Test Coverage Map

This file maps architectural areas to test projects.

| Area | Expected test project |
|---|---|
| Core row/table/dataset behavior | `PocoDataSet.DataTests`, `PocoDataSet.Tests` |
| Extension methods and changesets | `PocoDataSet.ExtensionsTests` |
| Serialization round-trips | `PocoDataSet.SerializerTests` |
| Observable wrappers and events | `PocoDataSet.ObservableDataTests`, `PocoDataSet.ObservableTests` |
| Observable merge behavior | `PocoDataSet.ObservableExtensionsTests` |
| EF Core bridge | `PocoDataSet.EfCoreBridge.Tests` |
| SQL Server adapter | `PocoDataSet.SqlServerDataAdapterTests` |

## Recent lifecycle tests to keep

The latest lifecycle review added or required tests around:

```text
DataRow detach after RemoveRow
DataRow detach after RemoveRowAt
DataRow detach after RemoveAllRows
DataSet.RemoveTable removes Tables entry
DataSet.RemoveTable removes TablesJson entry
DataSet.RemoveTable removes related relations
DataSet.RemoveTable preserves unrelated relations
remove/re-add table with same name
```

These tests are important because they protect memory-safety and stale-metadata invariants.
