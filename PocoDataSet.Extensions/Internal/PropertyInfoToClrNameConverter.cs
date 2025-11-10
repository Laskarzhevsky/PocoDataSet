using System;
using System.Reflection;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data type name to CLR name converter functionality
    /// </summary>
    public static class PropertyInfoToClrNameConverter
    {
        #region Public Methods
        /// <summary>
        /// Gets CLR name
        /// </summary>
        /// <param name="dataTypeName">Data type name</param>
        /// <returns>CLR name</returns>
        public static string GetClrTypeName(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            Type propertyType = property.PropertyType;
            Type? underlyingType = Nullable.GetUnderlyingType(propertyType);

            if (underlyingType != null)
            {
                propertyType = underlyingType;
            }

            // Map common CLR types to ClrTypeNames
            if (propertyType == typeof(bool))
            {
                return ClrTypeNames.BOOLEAN;
            }
            else if (propertyType == typeof(byte))
            {
                return ClrTypeNames.BYTE;
            }
            else if (propertyType == typeof(byte[]))
            {
                return ClrTypeNames.BYTE_ARRAY;
            }
            else if (propertyType == typeof(DateTime))
            {
                return ClrTypeNames.DATE_TIME;
            }
            else if (propertyType == typeof(DateTimeOffset))
            {
                return ClrTypeNames.DATE_TIME_OFFSET;
            }
            else if (propertyType == typeof(decimal))
            {
                return ClrTypeNames.DECIMAL;
            }
            else if (propertyType == typeof(double))
            {
                return ClrTypeNames.DOUBLE;
            }
            else if (propertyType == typeof(Guid))
            {
                return ClrTypeNames.GUID;
            }
            else if (propertyType == typeof(short))
            {
                return ClrTypeNames.INT16;
            }
            else if (propertyType == typeof(int))
            {
                return ClrTypeNames.INT32;
            }
            else if (propertyType == typeof(long))
            {
                return ClrTypeNames.INT64;
            }
            else if (propertyType == typeof(object))
            {
                return ClrTypeNames.OBJECT;
            }
            else if (propertyType == typeof(SByte))
            {
                return ClrTypeNames.SBYTE;
            }
            else if (propertyType == typeof(Single))
            {
                return ClrTypeNames.SINGLE;
            }
            else if (propertyType == typeof(string))
            {
                return ClrTypeNames.STRING;
            }
            else if (propertyType == typeof(TimeSpan))
            {
                return ClrTypeNames.TIME_SPAN;
            }
            else if (propertyType == typeof(UInt16))
            {
                return ClrTypeNames.UINT16;
            }
            else if (propertyType == typeof(UInt32))
            {
                return ClrTypeNames.UINT32;
            }
            else if (propertyType == typeof(UInt64))
            {
                return ClrTypeNames.UINT64;
            }

            // Fallback to string if you prefer being permissive; otherwise throw.
            return ClrTypeNames.STRING;
        }
        #endregion
    }
}
