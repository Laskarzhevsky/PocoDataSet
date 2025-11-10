using System;
using System.Globalization;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public class DateTimeValueConverter : IValueConverter
    {
        #region Static Methods
        public static bool CanHandle(Type underlyingTargetType)
        {
            return TypeKind.IsDateTime(underlyingTargetType);
        }

        public static bool TryHandle(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
            if (value is DateTime)
            {
                result = (DateTime)value;
                return true;
            }

            string? s = null;
            if (value is string)
            {
                s = ((string)value).Trim();
            }
            else
            {
                if (value != null)
                {
                    try
                    {
                        s = Convert.ToString(value, formatProvider);
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
            }

            if (s != null)
            {
                if (s.Length > 0)
                {
                    DateTime parsed;
                    bool ok = DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed);
                    if (ok)
                    {
                        result = parsed;
                        return true;
                    }
                }
            }

            if (value != null)
            {
                try
                {
                    object via = Convert.ChangeType(value, typeof(DateTime), formatProvider);
                    result = (DateTime)via;
                    return true;
                }
                catch
                {
                }
            }

            result = default(DateTime);
            return false;
        }

        public static bool TryNullable(object? value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
            if (value == null)
            {
                result = null;
                return true;
            }

            object dt;
            bool ok = TryHandle(value, underlyingTargetType, formatProvider, out dt);
            if (ok)
            {
                result = dt;
                return true;
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
