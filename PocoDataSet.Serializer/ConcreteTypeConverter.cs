using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocoDataSet.Serializer
{
    /// <summary>
    /// Provides concrete type converter functionality
    /// </summary>
    /// <typeparam name="TInterface">Interface which concrete type implements</typeparam>
    /// <typeparam name="TConcrete">Concrete type</typeparam>
    public class ConcreteTypeConverter<TInterface, TConcrete> : JsonConverter<TInterface> where TConcrete : TInterface
    {
        #region Overridden Methods
        /// <summary>
        /// Deserializes concrete type
        /// </summary>
        /// <param name="reader">UTF8 JSON reader</param>
        /// <param name="typeToConvert">Type to convert</param>
        /// <param name="options">JSON serializer options </param>
        /// <returns>Deserialized concrete type</returns>
        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<TConcrete>(ref reader, options);
        }

        /// <summary>
        /// Serializes concrete type
        /// </summary>
        /// <param name="writer">UTF8 JSON writer </param>
        /// <param name="value">Value to serialize</param>
        /// <param name="options">JSON serializer options </param>
        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (TConcrete)value!, options);
        }
        #endregion
    }
}