using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Tries to get field key by column name
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="foundFieldKey">Found field key</param>
        /// <returns>True if field key found, otherwise false</returns>
        public static bool TryGetFieldKeyByColumnName(this IDataRow? dataRow, string columnName, out string? foundFieldKey)
        {
            if (dataRow == null)
            {
                foundFieldKey = null;
                return false;
            }

            // Fast path (exact match)
            if (dataRow.ContainsKey(columnName))
            {
                foundFieldKey = columnName;
                return true;
            }

            // Scan existing keys ignoring case
            foreach (string key in dataRow.Values.Keys)
            {
                if (string.Equals(key, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    foundFieldKey = key;
                    return true;
                }
            }

            foundFieldKey = columnName;
            return false;
        }
        #endregion
    }
}
