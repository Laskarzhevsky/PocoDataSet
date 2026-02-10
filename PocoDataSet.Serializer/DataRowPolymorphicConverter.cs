using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Serializer
{
    /// <summary>
    /// Polymorphic IDataRow converter that round-trips FloatingDataRow.
    /// This converter relies on the "$rowKind" discriminator property:
    /// - missing or "regular" -> DataRow
    /// - "floating" -> FloatingDataRow
    /// </summary>
    public sealed class DataRowPolymorphicConverter : JsonConverter<IDataRow>
    {
        #region Constants
        /// <summary>
        /// 
        /// </summary>
        private const string RowKindPropertyName = "$rowKind";

        /// <summary>
        /// 
        /// </summary>
        private const string RowKindFloating = "floating";
        #endregion

        #region Public Methods
        /// <summary>
        /// Reads data row information
        /// </summary>
        /// <param name="reader">UTF8 JSON reader</param>
        /// <param name="typeToConvert">Type to convert</param>
        /// <param name="options">JSON serializer options</param>
        /// <returns>Data row</returns>
        /// <exception cref="JsonException"></exception>
        public override IDataRow? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected JSON object when deserializing IDataRow.");
            }

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = document.RootElement;

                string? rowKind = null;
                JsonElement rowKindElement;

                if (root.TryGetProperty(RowKindPropertyName, out rowKindElement))
                {
                    if (rowKindElement.ValueKind == JsonValueKind.String)
                    {
                        rowKind = rowKindElement.GetString();
                    }
                }

                string json = root.GetRawText();

                if (StringComparer.OrdinalIgnoreCase.Equals(rowKind, RowKindFloating))
                {
                    FloatingDataRow? floatingRow = JsonSerializer.Deserialize<FloatingDataRow>(json, options);
                    if (floatingRow == null)
                    {
                        throw new JsonException("Failed to deserialize FloatingDataRow.");
                    }

                    return floatingRow;
                }

                DataRow? row = JsonSerializer.Deserialize<DataRow>(json, options);
                if (row == null)
                {
                    throw new JsonException("Failed to deserialize DataRow.");
                }

                return row;
            }
        }

        /// <summary>
        /// Writes string representation of data row
        /// </summary>
        /// <param name="writer">UTF8 JSON writer</param>
        /// <param name="value">Data row</param>
        /// <param name="options">JSON serializer options</param>
        public override void Write(Utf8JsonWriter writer, IDataRow value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            // Serialize using the runtime type so derived RowKind is preserved.
            if (value is FloatingDataRow floatingRow)
            {
                JsonSerializer.Serialize<FloatingDataRow>(writer, floatingRow, options);
                return;
            }

            if (value is DataRow dataRow)
            {
                JsonSerializer.Serialize<DataRow>(writer, dataRow, options);
                return;
            }

            // Fallback: serialize as DataRow shape if a different implementation appears.
            // (This maintains backwards compatibility with the IDataRow -> DataRow contract.)
            JsonSerializer.Serialize<object>(writer, value, options);
        }
        #endregion
    }
}
