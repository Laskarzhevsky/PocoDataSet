using System;
using System.Globalization;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        /// <summary>
        /// Gets data field value
        /// </summary>
        /// <typeparam name="T">Data field value type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Data field value</returns>
        public static T? GetDataFieldValue<T>(this IDataRow? dataRow, string columnName)
        {
            if (dataRow == null)
            {
                return default(T);
            }

            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            object? raw;
            bool found = dataRow.TryGetValue(columnName, out raw);
            if (!found)
            {
                return default(T);
            }

            if (raw == null)
            {
                return default(T);
            }

            Type targetType = typeof(T);
            Type? tmp = Nullable.GetUnderlyingType(targetType);
            Type underlying;
            if (tmp == null)
            {
                underlying = targetType;
            }
            else
            {
                underlying = tmp;
            }

            var converter = CompositeValueConverterFactory.Default;

            object? converted;
            bool ok = converter.TryConvert(raw, underlying, CultureInfo.InvariantCulture, out converted);
            if (!ok)
            {
                return default(T);
            }

            if (converted == null)
            {
                return default(T);
            }

            try
            {
                return (T)converted;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
