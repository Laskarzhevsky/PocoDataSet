# Serialization Flow

## Serializer entry point

`PocoDataSet.Serializer.DataSetSerializer` provides:

```text
ToJsonString(IDataSet? dataSet)
FromJsonString(string? serializedDataSet)
```

## Why custom converters exist

The public model is interface-based and read-only in important places:

```text
IDataSet.Tables -> IReadOnlyDictionary<string, IDataTable>
IDataTable.Rows -> IReadOnlyList<IDataRow>
IDataRow.Values -> IReadOnlyDictionary<string, object?>
```

`System.Text.Json` cannot directly instantiate interfaces or populate read-only interface collections. Therefore the concrete implementation provides JSON bridge properties and the serializer registers concrete converters.

## Converters

The serializer registers converters for:

```text
IDataSet -> DataSet
IDataTable -> DataTable
IDataRow -> regular/floating row polymorphic converter
IColumnMetadata -> ColumnMetadata
IDataRelation -> DataRelation
IForeignKeyData -> ForeignKeyData
object? -> primitive values instead of JsonElement
```

## JSON bridge properties

| Runtime property | JSON bridge |
|---|---|
| `DataSet.Tables` | `DataSet.TablesJson` |
| `DataRow.Values` | `DataRow.ValuesJson` |
| `DataRow.OriginalValues` | `DataRow.OriginalValuesJson` |

## Deserialization normalization

After deserialization, `DataSetTypeNormalizer.NormalizeDataSet` is called. This step is important because values may need conversion from JSON primitive forms into expected runtime forms.

## Maintainer rule

Do not remove the JSON bridge properties or converters just because the public interface appears to expose the same data. The bridge properties exist to preserve interface-based public contracts while still supporting JSON round-trips.
