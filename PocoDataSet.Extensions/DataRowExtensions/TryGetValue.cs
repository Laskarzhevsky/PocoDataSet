using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        /// <summary>
        /// Tries to get value
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value from field</param>
        /// <returns>True if value returned, otherwise false</returns>
        /// <exception cref="KeyNotFoundException">Exception is thrown if table does not contain the column with specified name</exception>
        public static bool TryGetValue(this IDataRow dataRow, string columnName, out object? value)
        {
            if (dataRow.ContainsKey(columnName))
            {
                throw new KeyNotFoundException(nameof(columnName));
            }

            return dataRow.TryGetValue(columnName, out value);
        }
    }
}
