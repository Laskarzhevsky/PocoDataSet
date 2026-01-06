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
        /// Tries to get value from the field by using both, case sensitive and then case-insensitive cloumn name
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Requested value</param>
        /// <returns>True if requested requested field with column name exists in data row, otherwise false</returns>
        private static bool TryGetValue(this IDataRow? dataRow, string columnName, out object? value)
        {
            if (dataRow == null)
            {
                value = null;
                return false;
            }

            // Using case sensitive column name
            if (dataRow.TryGetValue(columnName, out value))
            {
                return true;
            }

            // Using case insensitive column name
            string? existingKey;
            if (dataRow.TryGetFieldKeyByColumnName(columnName, out existingKey))
            {
                return dataRow.TryGetValue(existingKey!, out value);
            }

            value = null;
            return false;
        }
        #endregion
    }
}
