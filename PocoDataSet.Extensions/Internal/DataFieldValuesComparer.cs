using System;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data field values comparer functionality
    /// </summary>
    public static partial class DataFieldValuesComparer
    {
        /// <summary>
        /// Checks whether field values are equal
        /// </summary>
        /// <param name="firstValue">First value</param>
        /// <param name="secondValue">second value</param>
        /// <returns>Action result</returns>
        public static bool FieldValuesEqual(object? firstValue, object? secondValue)
        {
            if (firstValue == null && secondValue == null)
            {
                return true;
            }

            if (firstValue == null || secondValue == null)
            {
                return false;
            }

            if (Convert.IsDBNull(firstValue))
            {
                firstValue = null;
            }

            if (Convert.IsDBNull(secondValue))
            {
                secondValue = null;
            }

            if (firstValue == null && secondValue == null)
            {
                return true;
            }

            if (firstValue == null || secondValue == null)
            {
                return false;
            }

            return firstValue.Equals(secondValue);
        }
    }
}
