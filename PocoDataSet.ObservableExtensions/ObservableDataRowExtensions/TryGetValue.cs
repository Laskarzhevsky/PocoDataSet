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
        /// Tries to get value from the field by using both, case sensitive and then case-insensitive column name
        /// </summary>
        /// <param name="observableDataRow">Observable Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Requested value</param>
        /// <returns>True if requested requested field with column name exists in data row, otherwise false</returns>
        public static bool TryGetValue(this IObservableDataRow? observableDataRow, string columnName, out object? value)
        {
            if (observableDataRow == null)
            {
                value = null;
                return false;
            }

            // Using case sensitive column name
            if (observableDataRow.InnerDataRow.TryGetValue(columnName, out value))
            {
                return true;
            }

            // Using case insensitive column name
            string? existingKey;
            if (observableDataRow.TryGetFieldKeyByColumnName(columnName, out existingKey))
            {
                return observableDataRow.InnerDataRow.TryGetValue(existingKey!, out value);
            }

            value = null;
            return false;
        }
        #endregion
    }
}
