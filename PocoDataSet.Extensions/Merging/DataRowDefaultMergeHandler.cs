using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data row default merge handler functionality
    /// </summary>
    public class DataRowDefaultMergeHandler : IRowMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current data row with refreshed data row by copying refreshed values into current data row
        /// </summary>
        /// <param name="tableName">Table name current data row belongs to</param>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <returns>True if any value of row has changed, otherwise false</returns>
        public bool MergeRow(string tableName, IDataRow currentDataRow, IDataRow refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata, IMergeOptions mergeOptions)
        {
            // Never overwrite user work
            if (currentDataRow.DataRowState == DataRowState.Modified || currentDataRow.DataRowState == DataRowState.Added || currentDataRow.DataRowState == DataRowState.Deleted)
            {
                return false;
            }

            bool rowValueChanged = false;
            foreach (IColumnMetadata columnMetadata in listOfColumnMetadata)
            {
                string columnName = columnMetadata.ColumnName;
                object? oldValue = currentDataRow.GetDataFieldValue<object?>(columnName);
                object? newValue = refreshedDataRow.GetDataFieldValue<object?>(columnName);
                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                currentDataRow[columnName] = newValue;
                rowValueChanged = true;
            }

            if (rowValueChanged)
            {
                // Backend data becomes baseline
                currentDataRow.AcceptChanges();
            }

            return rowValueChanged;
        }        
        #endregion
    }
}
