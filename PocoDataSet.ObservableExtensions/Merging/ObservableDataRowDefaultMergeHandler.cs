using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides default row merge handler functionality
    /// </summary>
    public class ObservableDataRowDefaultMergeHandler : IObservableDataRowMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current data row with refreshed data row
        /// IObservableDataRowMergeHandler interface implementation
        /// </summary>
        /// <param name="currentObservableDataTableName">Observable table name current observable data row belongs to</param>
        /// <param name="currentDataRow">Current row to update</param>
        /// <param name="refreshedDataRow">Refreshed data row providing values</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <returns>True if any value of the current data row changed, otherwise false</returns>
        public bool Merge(string currentObservableDataTableName, IDataRow currentDataRow, IDataRow refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            PocoDataSet.IData.DataRowState dataRowState = currentDataRow.DataRowState;
            switch (observableMergeOptions.MergeMode)
            {
                case MergeMode.Refresh:
                    // Never overwrite user edits
                    if (dataRowState != DataRowState.Unchanged)
                    {
                        return false;
                    }

                    break;
                case MergeMode.PostSave:
                    // Allow updates for Added / Modified, but not Deleted
                    if (dataRowState == DataRowState.Deleted)
                    {
                        return false;
                    }

                    break;
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

            return rowValueChanged;
        }

        /// <summary>
        /// Merges current observable data row with refreshed data row
        /// IObservableDataRowMergeHandler interface implementation
        /// </summary>
        /// <param name="currentObservableDataTableName">Observable table name current observable data row belongs to</param>
        /// <param name="currentObservableDataRow">Current observable row to update</param>
        /// <param name="refreshedDataRow">Refreshed data row providing values</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <returns>True if any value of the current data row changed, otherwise false</returns>
        public bool Merge(string currentObservableDataTableName, IObservableDataRow currentObservableDataRow, IDataRow refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            PocoDataSet.IData.DataRowState dataRowState = currentObservableDataRow.InnerDataRow.DataRowState;
            switch (observableMergeOptions.MergeMode)
            {
                case MergeMode.Refresh:
                    // Never overwrite user edits
                    if (dataRowState != DataRowState.Unchanged)
                    {
                        return false;
                    }

                    break;
                case MergeMode.PostSave:
                    // Allow updates for Added / Modified, but not Deleted
                    if (dataRowState == DataRowState.Deleted)
                    {
                        return false;
                    }

                    break;
            }

            bool rowValueChanged = false;
            foreach (IColumnMetadata columnMetadata in listOfColumnMetadata)
            {
                string columnName = columnMetadata.ColumnName;
                object? oldValue = currentObservableDataRow.InnerDataRow.GetDataFieldValue<object?>(columnName);
                object? newValue = refreshedDataRow.GetDataFieldValue<object?>(columnName);
                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                currentObservableDataRow[columnName] = newValue;
                rowValueChanged = true;
            }

            return rowValueChanged;
        }
        #endregion
    }
}
