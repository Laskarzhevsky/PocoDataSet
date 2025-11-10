using System.Text.Json;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Serializer
{
    public static class DataSetSerializer
    {
        #region Public Methods
        /// <summary>
        /// Restores data set from JSON string
        /// </summary>
        /// <param name="serializedDataSet"></param>
        /// <returns>Restored data set from JSON string</returns>
        public static IDataSet? FromJsonString(string? serializedDataSet)
        {
            if (string.IsNullOrEmpty(serializedDataSet))
            {
                return null;
            }

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;

            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IDataSet, DataSet>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IDataTable, DataTable>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IDataRow, DataRow>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IColumnMetadata, ColumnMetadata>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IDataRelation, DataRelation>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IForeignKeyData, ForeignKeyData>());

            DataSet? deserializedDataSet = System.Text.Json.JsonSerializer.Deserialize<DataSet>(serializedDataSet, jsonSerializerOptions);
            return deserializedDataSet;
        }

        /// <summary>
        /// Gets data set JSON string representation
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <returns>Data set JSON string representation</returns>
        public static string? ToJsonString(IDataSet dataSet)
        {
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.PropertyNamingPolicy = null;
            jsonSerializerOptions.WriteIndented = true;

            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IDataSet, DataSet>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IDataTable, DataTable>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IDataRow, DataRow>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IColumnMetadata, ColumnMetadata>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IDataRelation, DataRelation>());
            jsonSerializerOptions.Converters.Add(new ConcreteTypeConverter<IForeignKeyData, ForeignKeyData>());

            string serializedDataSet = System.Text.Json.JsonSerializer.Serialize(dataSet, dataSet.GetType(), jsonSerializerOptions);
            return serializedDataSet;
        }
        #endregion
    }
}
