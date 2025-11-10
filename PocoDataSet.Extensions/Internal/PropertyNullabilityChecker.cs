using System;
using System.Reflection;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides property nullability checker functionality
    /// </summary>
    public static class PropertyNullabilityChecker
    {
        #region Public Methods
        /// <summary>
        /// Check whether property is nullable
        /// </summary>
        /// <param name="propertyInfo">Property info</param>
        /// <returns>Flag indicating whether property is nullable</returns>
        public static bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            Type propertyType = propertyInfo.PropertyType;
            if (propertyType.IsValueType)
            {
                Type? underlyingType = Nullable.GetUnderlyingType(propertyType);
                if (underlyingType == null)
                {
                    return false;
                }

                return true;
            }

            // Reference types: treat as nullable by default (matches your interfaces' string? usage).
            return true;
        }
        #endregion
    }
}
