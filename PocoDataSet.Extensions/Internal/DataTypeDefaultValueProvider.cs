using System;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides default values for column data types defined as strings.
    /// Used during merge when creating new rows.
    /// </summary>
    public static class DataTypeDefaultValueProvider
    {
        #region Public Methods

        /// <summary>
        /// Gets default value for the specified data type and nullability.
        /// </summary>
        /// <param name="dataType">Column data type name (e.g. "string", "int", "datetime")</param>
        /// <param name="isNullable">True if column allows null values</param>
        /// <returns>Default value</returns>
        public static object? GetDefaultValue(string? dataType, bool isNullable)
        {
            if (isNullable)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(dataType))
            {
                return null;
            }

            string normalized = dataType.Trim().ToLowerInvariant();

            // string-like
            if (normalized == "string"
                || normalized == "nvarchar"
                || normalized == "varchar"
                || normalized == "text")
            {
                return string.Empty;
            }

            // boolean
            if (normalized == "bool"
                || normalized == "boolean")
            {
                return false;
            }

            // integers
            if (normalized == "byte")
            {
                return (byte)0;
            }

            if (normalized == "short"
                || normalized == "int16")
            {
                return (short)0;
            }

            if (normalized == "int"
                || normalized == "int32")
            {
                return 0;
            }

            if (normalized == "long"
                || normalized == "int64")
            {
                return 0L;
            }

            // floating point
            if (normalized == "float"
                || normalized == "single")
            {
                return 0f;
            }

            if (normalized == "double")
            {
                return 0d;
            }

            if (normalized == "decimal")
            {
                return 0m;
            }

            // date / time
            if (normalized == "datetime")
            {
                return DateTime.MinValue;
            }

            if (normalized == "datetimeoffset")
            {
                return DateTimeOffset.MinValue;
            }

            // guid
            if (normalized == "guid"
                || normalized == "uniqueidentifier")
            {
                return Guid.Empty;
            }

            // unknown / custom types
            return null;
        }

        #endregion
    }
}
