using System;
using System.Globalization;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal class EnumValueConverter : IValueConverter
    {
        #region Static Methods
        public static bool CanHandle(Type underlyingTargetType)
        {
            return TypeKind.IsEnum(underlyingTargetType);
        }

        public static bool TryHandle(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
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
                    try
                    {
                        object byName = Enum.Parse(underlyingTargetType, s, true);
                        result = byName;
                        return true;
                    }
                    catch
                    {
                        long num;
                        bool okNum = long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out num);
                        if (okNum)
                        {
                            object boxed = Enum.ToObject(underlyingTargetType, num);
                            result = boxed;
                            return true;
                        }
                    }
                }
            }

            if (value is IConvertible)
            {
                try
                {
                    long num = System.Convert.ToInt64(value, formatProvider);
                    object boxed = Enum.ToObject(underlyingTargetType, num);
                    result = boxed;
                    return true;
                }
                catch
                {
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
