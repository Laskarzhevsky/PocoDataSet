using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal class ConvertibleValueConverter : IValueConverter
    {
        #region Static Methods
        public static bool CanHandle(Type underlyingTargetType)
        {
            // We act as a general fallback for IConvertible targets.
            return true;
        }

        public static bool TryHandle(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
            if (value is IConvertible)
            {
                try
                {
                    object converted = System.Convert.ChangeType(value, underlyingTargetType, formatProvider);
                    result = converted;
                    return true;
                }
                catch
                {
                    result = null;
                    return false;
                }
            }

            result = null;
            return false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets flag indicating whether converter can convert value into target type
        /// </summary>
        /// <param name="underlyingTargetType">Target type</param>
        /// <returns>Flag indicating whether converter can convert value into target type</returns>
        public bool CanConvert(Type underlyingTargetType)
        {
            return CanHandle(underlyingTargetType);
        }

        /// <summary>
        /// Tries to convert
        /// </summary>
        /// <param name="value">Value for conversion</param>
        /// <param name="underlyingTargetType">Target type</param>
        /// <param name="formatProvider">Format provider</param>
        /// <param name="result">Converted value</param>
        /// <returns>Flag indicating whether conversion was successful</returns>
        public bool TryConvert(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
            return TryHandle(value, underlyingTargetType, formatProvider, out result);
        }
        #endregion
    }
}
