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
        /// Updates data field value and applies row state transitions.
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value for data field</param>
        /// <returns>Flag indicating whether data field value was updated</returns>
        [Obsolete("This method is deprecated. Please use row indexer instead to update value, for example dataRow[\"Id\"] = 1;")]
        public static bool UpdateDataFieldValue(this IDataRow? dataRow, string columnName, object? value)
        {
            if (dataRow == null)
            {
                return false;
            }

            dataRow[columnName] = value;

            bool dataFieldValueUpdated = false;
            object? originalValue;
            dataRow.TryGetOriginalValue(columnName, out originalValue);
            if (originalValue != value)
            {
                dataFieldValueUpdated = true;
            }

            return dataFieldValueUpdated;
        }
        #endregion
    }
}
