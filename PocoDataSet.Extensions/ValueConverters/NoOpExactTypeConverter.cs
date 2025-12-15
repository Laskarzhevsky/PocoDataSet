using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// If the value already matches the underlying target type, just return it.
    /// </summary>
    public class NoOpExactTypeConverter : IValueConverter
    {
        public bool CanConvert(Type underlyingTargetType)
        {
            // Applicable to all to allow short-circuit; we still check type equality in TryConvert.
            return true;
        }

        public bool TryConvert(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
            if (value != null && value.GetType() == underlyingTargetType)
            {
                result = value;
                return true;
            }

            result = null;
            return false;
        }
    }
}
