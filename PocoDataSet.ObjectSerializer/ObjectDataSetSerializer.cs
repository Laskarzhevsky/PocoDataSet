using System;
using System.Collections.Generic;
using System.Text.Json;

using PocoDataSet.IObjectData;
using PocoDataSet.ObjectData;

namespace PocoDataSet.ObjectSerializer
{
    /// <summary>
    /// Provides JSON serialization support for ObjectDataSet.
    /// </summary>
    public static class ObjectDataSetSerializer
    {
        #region Public Methods
        /// <summary>
        /// Restores object data set from JSON string.
        /// </summary>
        /// <param name="serializedObjectDataSet">Serialized object data set</param>
        /// <returns>Restored object data set</returns>
        public static IObjectDataSet? FromJsonString(string? serializedObjectDataSet)
        {
            if (string.IsNullOrEmpty(serializedObjectDataSet))
            {
                return null;
            }

            JsonSerializerOptions options = CreateJsonSerializerOptions();
            SerializedObjectDataSet? payload = JsonSerializer.Deserialize<SerializedObjectDataSet>(serializedObjectDataSet, options);
            if (payload == null)
            {
                return null;
            }

            ObjectDataSet objectDataSet = new ObjectDataSet();

            if (payload.Tables == null)
            {
                return objectDataSet;
            }

            for (int i = 0; i < payload.Tables.Count; i++)
            {
                SerializedObjectTable serializedTable = payload.Tables[i];
                if (serializedTable == null || string.IsNullOrWhiteSpace(serializedTable.Name) || string.IsNullOrWhiteSpace(serializedTable.ItemType))
                {
                    continue;
                }

                Type? itemType = Type.GetType(serializedTable.ItemType, throwOnError: false);
                if (itemType == null)
                {
                    throw new InvalidOperationException("Unable to resolve object table item type: " + serializedTable.ItemType);
                }

                Type listType = typeof(List<>).MakeGenericType(itemType);
                object? deserializedList = JsonSerializer.Deserialize(serializedTable.Items.GetRawText(), listType, options);

                Type objectTableType = typeof(ObjectTable<>).MakeGenericType(itemType);
                object? createdTable = Activator.CreateInstance(objectTableType, serializedTable.Name, deserializedList);

                if (createdTable == null)
                {
                    throw new InvalidOperationException("Unable to create object table for type: " + itemType.FullName);
                }

                AddTable(objectDataSet, (IObjectTable)createdTable);
            }

            return objectDataSet;
        }

        /// <summary>
        /// Gets object data set JSON string representation.
        /// </summary>
        /// <param name="objectDataSet">Object data set</param>
        /// <returns>Object data set JSON string representation</returns>
        public static string? ToJsonString(IObjectDataSet objectDataSet)
        {
            if (objectDataSet == null)
            {
                return null;
            }

            JsonSerializerOptions options = CreateJsonSerializerOptions();

            SerializedObjectDataSet payload = new SerializedObjectDataSet();
            payload.Tables = new List<SerializedObjectTable>();

            foreach (KeyValuePair<string, IObjectTable> pair in objectDataSet.Tables)
            {
                IObjectTable table = pair.Value;

                SerializedObjectTable serializedTable = new SerializedObjectTable();
                serializedTable.Name = table.Name;
                serializedTable.ItemType = table.ItemType.AssemblyQualifiedName ?? table.ItemType.FullName ?? table.ItemType.Name;
                serializedTable.Items = JsonSerializer.SerializeToElement(table.UntypedItems, table.UntypedItems.GetType(), options);

                payload.Tables.Add(serializedTable);
            }

            return JsonSerializer.Serialize(payload, options);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectDataSet"></param>
        /// <param name="table"></param>
        static void AddTable(ObjectDataSet objectDataSet, IObjectTable table)
        {
            Type itemType = table.ItemType;
            Type helperType = typeof(ObjectDataSetSerializer);
            var method = helperType.GetMethod(nameof(AddTypedTable), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var genericMethod = method!.MakeGenericMethod(itemType);
            genericMethod.Invoke(null, new object[] { objectDataSet, table });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectDataSet"></param>
        /// <param name="table"></param>
        static void AddTypedTable<T>(ObjectDataSet objectDataSet, IObjectTable table)
        {
            ObjectTable<T> typedTable = (ObjectTable<T>)table;

            ObjectTable<T> createdTable = objectDataSet.AddTable<T>(typedTable.Name);
            createdTable.Items.AddRange(typedTable.Items);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static JsonSerializerOptions CreateJsonSerializerOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = null;
            options.PropertyNameCaseInsensitive = true;
            options.WriteIndented = true;
            return options;
        }
        #endregion
    }
}
