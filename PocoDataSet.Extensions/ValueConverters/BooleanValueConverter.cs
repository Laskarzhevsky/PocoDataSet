using System;
using System.Globalization;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal class BooleanValueConverter : IValueConverter
    {
        #region Static Methods
        public static bool CanHandle(Type underlyingTargetType)
        {
            return TypeKind.IsBoolean(underlyingTargetType);
        }

        public static bool TryHandle(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
            if (value is bool)
            {
                result = (bool)value;
                return true;
            }

            if (value is sbyte || value is byte || value is short || value is ushort ||
                value is int || value is uint || value is long || value is ulong)
            {
                try
                {
                    long n = System.Convert.ToInt64(value, formatProvider);
                    result = n != 0;
                    return true;
                }
                catch
                {
                }
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
                    bool parsedBool;
                    bool okBool = bool.TryParse(s, out parsedBool);
                    if (okBool)
                    {
                        result = parsedBool;
                        return true;
                    }

                    long n2;
                    bool okNum2 = long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out n2);
                    if (okNum2)
                    {
                        result = n2 != 0;
                        return true;
                    }
                }
            }

            try
            {
                bool viaConvert = System.Convert.ToBoolean(value, formatProvider);
                result = viaConvert;
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
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
