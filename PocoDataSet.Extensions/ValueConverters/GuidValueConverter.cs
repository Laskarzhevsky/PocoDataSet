using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal class GuidValueConverter : IValueConverter
    {
        #region Static Methods
        public static bool CanHandle(Type underlyingTargetType)
        {
            return TypeKind.IsGuid(underlyingTargetType);
        }

        public static bool TryHandle(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
            if (value is Guid)
            {
                result = (Guid)value;
                return true;
            }

            string? s;
            if (value is string)
            {
                s = ((string)value).Trim();
            }
            else
            {
                try
                {
                    s = System.Convert.ToString(value, formatProvider);
                }
                catch
                {
                    s = null;
                }

                if (s != null)
                {
                    s = s.Trim();
                }
            }

            if (s != null)
            {
                if (s.Length > 0)
                {
                    Guid g;
                    bool ok = Guid.TryParse(s, out g);
                    if (ok)
                    {
                        result = g;
                        return true;
                    }
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
