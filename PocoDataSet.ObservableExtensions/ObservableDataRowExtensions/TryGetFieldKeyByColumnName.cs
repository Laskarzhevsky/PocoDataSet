using System;

using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods.
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Tries to get field key by column name
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="foundFieldKey">Found field key</param>
        /// <returns>True if field key found, otherwise false</returns>
        public static bool TryGetFieldKeyByColumnName(this IObservableDataRow? observableDataRow, string columnName, out string foundFieldKey)
        {
            if (observableDataRow == null)
            {
                foundFieldKey = columnName;
                return false;
            }

            // Fast path (exact match)
            if (observableDataRow.InnerDataRow.ContainsKey(columnName))
            {
                foundFieldKey = columnName;
                return true;
            }


            // Scan existing keys ignoring case
            foreach (string key in observableDataRow.InnerDataRow.Values.Keys)
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
