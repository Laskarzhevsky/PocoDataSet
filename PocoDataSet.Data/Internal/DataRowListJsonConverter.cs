using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using PocoDataSet.IData;

namespace PocoDataSet.Data.Internal
{
    /// <summary>
    /// Serializes and deserializes a list of IDataRow preserving runtime types (DataRow vs FloatingDataRow)
    /// using the "$rowKind" discriminator that already exists in the row JSON.
    /// </summary>
    internal sealed class DataRowListJsonConverter : JsonConverter<List<IDataRow>>
    {
        public override List<IDataRow>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected StartArray for Rows.");
            }

            List<IDataRow> rows = new List<IDataRow>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return rows;
                }

                // Deserialize each row object into a JsonDocument to inspect $rowKind.
                using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                {
                    JsonElement root = doc.RootElement;

                    string? rowKind = null;
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("$rowKind", out JsonElement kindEl))
                    {
                        if (kindEl.ValueKind == JsonValueKind.String)
                        {
                            rowKind = kindEl.GetString();
                        }
                    }

                    // Decide concrete type. Default to DataRow.
                    Type rowType = typeof(DataRow);
                    if (!string.IsNullOrWhiteSpace(rowKind) &&
                        string.Equals(rowKind, "Floating", StringComparison.OrdinalIgnoreCase))
                    {
                        rowType = typeof(FloatingDataRow);
                    }

                    IDataRow? row = (IDataRow?)JsonSerializer.Deserialize(root.GetRawText(), rowType, options);
                    if (row != null)
                    {
                        rows.Add(row);
                    }
                }
            }

            throw new JsonException("Unexpected end of JSON while reading Rows.");
        }

        public override void Write(Utf8JsonWriter writer, List<IDataRow> value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();

            for (int i = 0; i < value.Count; i++)
            {
                IDataRow row = value[i];
                if (row == null)
                {
                    writer.WriteNullValue();
                    continue;
                }

                // Serialize using runtime type to preserve $rowKind and subtype-specific JSON.
                JsonSerializer.Serialize(writer, row, row.GetType(), options);
            }

            writer.WriteEndArray();
        }
    }
}
