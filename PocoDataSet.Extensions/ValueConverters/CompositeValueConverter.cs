using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    internal class CompositeFieldValueConverter : IValueConverter
    {
        readonly IValueConverter[] _converters;

        public CompositeFieldValueConverter(IValueConverter[] converters)
        {
            if (converters == null)
            {
                throw new ArgumentNullException(nameof(converters));
            }

            _converters = converters;
        }

        /// <summary>
        /// Gets flag indicating whether converter can convert value into target type
        /// </summary>
        /// <param name="underlyingTargetType">Target type</param>
        /// <returns>Flag indicating whether converter can convert value into target type</returns>
        public bool CanConvert(Type underlyingTargetType)
        {
            return true;
        }

        public bool TryConvert(object value, Type underlyingTargetType, IFormatProvider formatProvider, out object? result)
        {
            object? conversionResult = null;
            if (value == null)
            {
                result = null;
                return false;
            }

            if (underlyingTargetType == null)
            {
                throw new ArgumentNullException(nameof(underlyingTargetType));
            }

            if (formatProvider == null)
            {
                throw new ArgumentNullException(nameof(formatProvider));
            }

            Type? underlying = Nullable.GetUnderlyingType(underlyingTargetType);
            if (underlying == null)
            {
                underlying = underlyingTargetType;
            }

            int i = 0;
            while (i < _converters.Length)
            {
                IValueConverter converter = _converters[i];
                if (converter != null)
                {
                    if (converter.CanConvert(underlying))
                    {
                        object? convertedValue = null;
                        if (converter.TryConvert(value, underlying, formatProvider, out convertedValue))
                        {
                            conversionResult = convertedValue;
                        }
                    }
                }

                i = i + 1;
            }

            try
            {
                conversionResult = System.Convert.ChangeType(value, underlying, formatProvider);
            }
            catch
            {
            }

            result = conversionResult;
            return true;
        }

        public T? Convert<T>(object value, IFormatProvider formatProvider)
        {
            object? obj;
            TryConvert(value, typeof(T), formatProvider, out obj);
            if (obj == null)
            {
                return default(T);
            }

            try
            {
                return (T)obj;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
