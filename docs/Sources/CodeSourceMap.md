# Code Source Map

This documentation was prepared against the supplied `PocoDataSetCode` folder.

## Key source files

| Topic | Source files |
|---|---|
| Core dataset | `PocoDataSet.Data/DataSet.cs` |
| Core table | `PocoDataSet.Data/DataTable.cs` |
| Core row | `PocoDataSet.Data/DataRow.cs`, `FloatingDataRow.cs` |
| Schema and primary keys | `PocoDataSet.Data/Internal/DataTableSchema.cs` |
| Core interfaces | `PocoDataSet.IData/IDataSet.cs`, `IDataTable.cs`, `IDataRow.cs` |
| Changesets | `PocoDataSet.Extensions/DataSetExtensions/CreateChangeset.cs` |
| Merge entry points | `PocoDataSet.Extensions/DataSetExtensions/DoPostSaveMerge.cs`, `DoRefreshMergeIfNoChangesExist.cs`, `DoRefreshMergePreservingLocalChanges.cs`, `DoReplaceMerge.cs`, `MergeWith.cs` |
| Merge implementations | `PocoDataSet.Extensions/Merging/...` |
| Accept/reject/delete | `PocoDataSet.Extensions/DataTableExtensions/AcceptChanges.cs`, `RejectChanges.cs`, `DeleteRow.cs` |
| Serialization | `PocoDataSet.Serializer/DataSetSerializer.cs` and converters |
| Observable dataset/table/view | `PocoDataSet.ObservableData/*.cs` |
| EF Core bridge | `PocoDataSet.EfCoreBridge/*.cs` |
| SQL Server adapter | `PocoDataSet.SqlServerDataAdapter/*.cs` |
