using System.Collections.Generic;

using PocoDataSet.Extensions;
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
        /// Merges current data row with refreshed data row by copying refreshed values into current data row
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>True if any value of row sas changed, otherwise false</returns>
        public static bool MergeWith(this IDataRow? currentDataRow, IDataRow? refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata)
        {
            bool rowValueChanged = false;
            if (currentDataRow == null || refreshedDataRow == null)
            {
                return false;
            }

            foreach (IColumnMetadata columnMetadata in listOfColumnMetadata)
            {
                string columnName = columnMetadata.ColumnName;
                object? oldValue = currentDataRow.GetDataFieldValue<object?>(columnName);
                object? newValue = refreshedDataRow.GetDataFieldValue<object?>(columnName);
                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                currentDataRow.UpdateDataFieldValue(columnName, newValue);
                rowValueChanged = true;
            }

            return rowValueChanged;
        }
        #endregion
    }
}
