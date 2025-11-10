using System;

namespace PocoDataSet.Extensions
{
    public static class Defaults
    {
        /// <summary>
        /// Returns the runtime default for a System.Type (null for refs, default(T) for value types).
        /// </summary>
        public static object? GetDefault(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            return ForType(t);
        }

        /// <summary>
        /// Same as GetDefault(Type). Kept for callers that already use ForType.
        /// </summary>
        public static object? ForType(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (t.IsValueType)
            {
                try
                {
                    return Activator.CreateInstance(t);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        #region Methods

        /// <summary>
        /// Gets default value by data type name (e.g., "Int32") and nullability.
        /// </summary>
        public static object? GetDefaultValue(string dataType, bool isNullable)
        {
            Type? type = null;

            if (string.Equals(dataType, "String", StringComparison.Ordinal))
            {
                type = typeof(string);
            }
            else if (string.Equals(dataType, "Int32", StringComparison.Ordinal))
            {
                type = typeof(int);
            }
            else if (string.Equals(dataType, "Int64", StringComparison.Ordinal))
            {
                type = typeof(long);
            }
            else if (string.Equals(dataType, "Boolean", StringComparison.Ordinal))
            {
                type = typeof(bool);
            }
            else if (string.Equals(dataType, "DateTime", StringComparison.Ordinal))
            {
                type = typeof(DateTime);
            }
            else if (string.Equals(dataType, "Decimal", StringComparison.Ordinal))
            {
                type = typeof(decimal);
            }
            else if (string.Equals(dataType, "Double", StringComparison.Ordinal))
            {
                type = typeof(double);
            }
            else if (string.Equals(dataType, "Guid", StringComparison.Ordinal))
            {
                type = typeof(Guid);
            }

            if (type == null)
            {
                return null;
            }

            // Strings: prefer empty string rather than null/default
            if (type == typeof(string))
            {
                return string.Empty;
            }

            // For value types or when asked to produce a nullable default, create default instance
            if (isNullable)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch
                {
                    return null;
                }
            }

            if (type.IsValueType)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch
                {
                    return null;
                }
            }

            // Non-nullable reference type → null
            return null;
        }

        #endregion
    }
}
