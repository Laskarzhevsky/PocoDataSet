using System;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides default values based on POCO DataSet column metadata.
    /// </summary>
    public static class MetadataDefaults
    {
        /// <summary>
        /// Gets default value by data type token (for example, DataTypeNames.INT32) and nullability.
        /// </summary>
        /// <param name="dataType">Data type token (CLR/logical), typically from DataTypeNames.</param>
        /// <param name="isNullable">True if column allows null values; otherwise false.</param>
        /// <returns>Default value for a new row cell.</returns>
        public static object? GetDefaultValue(string? dataType, bool isNullable)
        {
            if (string.IsNullOrEmpty(dataType))
            {
                return null;
            }

            // Strings: prefer empty string rather than null/default.
            if (string.Equals(dataType, DataTypeNames.STRING, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            // Nullable columns: default is null (except string handled above).
            if (isNullable)
            {
                return null;
            }

            // Binary (byte[]). If non-nullable, prefer empty array.
            if (string.Equals(dataType, DataTypeNames.BINARY, StringComparison.Ordinal))
            {
                return Array.Empty<byte>();
            }

            // Boolean
            if (string.Equals(dataType, DataTypeNames.BOOL, StringComparison.Ordinal))
            {
                return default(bool);
            }

            // Byte
            if (string.Equals(dataType, DataTypeNames.BYTE, StringComparison.Ordinal))
            {
                return default(byte);
            }

            // Int16
            if (string.Equals(dataType, DataTypeNames.INT16, StringComparison.Ordinal))
            {
                return default(short);
            }

            // Int32
            if (string.Equals(dataType, DataTypeNames.INT32, StringComparison.Ordinal))
            {
                return default(int);
            }

            // Int64
            if (string.Equals(dataType, DataTypeNames.INT64, StringComparison.Ordinal))
            {
                return default(long);
            }

            // Single (float)
            if (string.Equals(dataType, DataTypeNames.SINGLE, StringComparison.Ordinal))
            {
                return default(float);
            }

            // Double
            if (string.Equals(dataType, DataTypeNames.DOUBLE, StringComparison.Ordinal))
            {
                return default(double);
            }

            // Decimal
            if (string.Equals(dataType, DataTypeNames.DECIMAL, StringComparison.Ordinal))
            {
                return default(decimal);
            }

            // DateTime
            if (string.Equals(dataType, DataTypeNames.DATE_TIME, StringComparison.Ordinal))
            {
                return default(DateTime);
            }

            // DateOnly
            if (string.Equals(dataType, DataTypeNames.DATE, StringComparison.Ordinal))
            {
                return default(DateOnly);
            }

            // TimeOnly
            if (string.Equals(dataType, DataTypeNames.TIME, StringComparison.Ordinal))
            {
                return default(TimeOnly);
            }

            // Guid
            if (string.Equals(dataType, DataTypeNames.GUID, StringComparison.Ordinal))
            {
                return default(Guid);
            }

            // Logical types: Json / Spatial / Object are left as null by default.
            if (string.Equals(dataType, DataTypeNames.JSON, StringComparison.Ordinal))
            {
                return null;
            }

            if (string.Equals(dataType, DataTypeNames.SPATIAL, StringComparison.Ordinal))
            {
                return null;
            }

            if (string.Equals(dataType, DataTypeNames.OBJECT, StringComparison.Ordinal))
            {
                return null;
            }

            // Unknown token: no default.
            return null;
        }
    }
}
