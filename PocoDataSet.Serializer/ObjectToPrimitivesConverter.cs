using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocoDataSet.Serializer
{
    public sealed class ObjectToPrimitivesConverter : JsonConverter<object?>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Number:
                    // favor integers when exact
                    if (reader.TryGetInt64(out long l))
                        return l;
                    if (reader.TryGetDecimal(out decimal dec))
                        return dec;
                    return reader.GetDouble();

                case JsonTokenType.String:
                    string? s = reader.GetString();
                    if (s is null)
                        return null;

                    // common strong types you use
                    if (Guid.TryParse(s, out var g))
                        return g;

                    // Round-trip ISO 8601 (the default JsonSerializer writes)
                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                            DateTimeStyles.RoundtripKind, out var dt))
                        return dt;

                    return s;

                case JsonTokenType.StartArray:
                    var list = new List<object?>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        list.Add(JsonSerializer.Deserialize<object?>(ref reader, options));
                    }
                    return list;

                case JsonTokenType.StartObject:
                    var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException("Expected property name.");
                        string name = reader.GetString()!;
                        reader.Read();
                        dict[name] = JsonSerializer.Deserialize<object?>(ref reader, options);
                    }
                    return dict;

                default:
                    throw new JsonException($"Unsupported token {reader.TokenType} for object.");
            }
        }

        public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            // Delegate back to the runtime to serialize with the actual runtime type
            JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
        }
    }
}