# PocoDataSet.EfCoreBridge

This package is an **optional** bridge between **Entity Framework Core** and **PocoDataSet**.

It provides convenience helpers to materialize an EF Core `IQueryable<T>` (sync/async) and project results into a PocoDataSet `IDataTable`.

## Key dependency

This bridge intentionally reuses your existing extensions:

- `IDataSet.PocoListToDataTable<T>(...)`
- `IDataTable.CopyFromPocoList<T>(...)`
- `IDataRow.CopyFromPoco(...)`
- `IDataRow.CopyToPoco(...)`
- `IDataRow.ToPoco<T>()`

## Example (sync)

```csharp
using (var db = new AppDbContext())
{
    IDataSet ds = DataSetFactory.CreateDataSet();

    List<IColumnMetadata> cols = new List<IColumnMetadata>();
    cols.Add(new ColumnMetadata("Id", DataTypeNames.INT32));
    cols.Add(new ColumnMetadata("Name", DataTypeNames.STRING));

    IDataTable t = db.Departments
        .Where(d => d.IsActive)
        .OrderBy(d => d.Name)
        .ToDataTable(ds, "Department", cols);

    // UI edits happen against ds / t ...
}
```

## Example (async)

```csharp
IDataTable t = await db.Departments
    .Where(d => d.IsActive)
    .ToDataTableAsync(ds, "Department", cols, cancellationToken);
```

> Note: `listOfColumnMetadata` is explicit to keep schema deterministic and avoid accidental inclusion of navigation/complex properties.
