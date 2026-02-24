using System;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Defines value converter functionality
    /// </summary>
    internal interface IValueConverter
    {
        #region Methods
        /// <summary>
        /// Gets flag indicating whether converter can convert value into target type
        /// </summary>
        /// <param name="underlyingTargetType">Target type</param>
        /// <returns>Flag indicating whether converter can convert value into target type</returns>
        bool CanConvert(Type underlyingTargetType);

        /// <summary>
        /// Tries to convert
        /// </summary>
        /// <param name="value">Value for conversion</param>
        /// <param name="underlyingTargetType">Target type</param>
        /// <param name="formatProvider">Format provider</param>
        /// <param name="result">Converted value</param>
        /// <returns>Flag indicating whether conversion was successful</returns>
        bool TryConvert(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result);
        #endregion
    }
}
