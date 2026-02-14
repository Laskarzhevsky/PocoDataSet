using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// System.Text.Json cannot deserialize interface-typed collections.
    /// This converter round-trips IDataRow items and preserves FloatingDataRow semantics
    /// via the $rowKind discriminator.
    /// </summary>
    internal sealed class DataRowListJsonConverter : JsonConverter<List<IDataRow>>
    {
        public override List<IDataRow> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new List<IDataRow>();
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected JSON array for Rows.");
            }

            List<IDataRow> rows = new List<IDataRow>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return rows;
                }

                // Parse the row object so we can inspect $rowKind.
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

                    IDataRow row;
                    string raw = root.GetRawText();

                    if (string.Equals(rowKind, "floating", StringComparison.OrdinalIgnoreCase))
                    {
                        row = JsonSerializer.Deserialize<FloatingDataRow>(raw, options)
                              ?? throw new JsonException("Failed to deserialize FloatingDataRow.");
                    }
                    else
                    {
                        row = JsonSerializer.Deserialize<DataRow>(raw, options)
                              ?? throw new JsonException("Failed to deserialize DataRow.");
                    }

                    rows.Add(row);
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
                Type runtimeType = row.GetType();
                JsonSerializer.Serialize(writer, row, runtimeType, options);
            }

            writer.WriteEndArray();
        }
    }
}
