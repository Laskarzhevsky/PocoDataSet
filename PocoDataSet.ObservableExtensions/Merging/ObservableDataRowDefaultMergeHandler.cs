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
            IObservableMergePolicy policy = ObservableMergePolicyFactory.Create(observableMergeOptions.MergeMode);
            if (!policy.CanOverwriteRow(currentDataRow.DataRowState))
            {
                return false;
            }

            bool rowValueChanged = ApplyValues(currentDataRow, refreshedDataRow, listOfColumnMetadata);
            if (policy.ShouldAcceptChangesAfterMerge)
            {
                currentDataRow.AcceptChanges();
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
        public bool Merge(string currentObservableDataTableName, IObservableDataRow currentObservableDataRow, IDataRow refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions) {
            IObservableMergePolicy policy = ObservableMergePolicyFactory.Create(observableMergeOptions.MergeMode);
            if (!policy.CanOverwriteRow(currentObservableDataRow.InnerDataRow.DataRowState))
            {
                return false;
            }

            bool rowValueChanged = ApplyValues(currentObservableDataRow, refreshedDataRow, listOfColumnMetadata);
            if (policy.ShouldAcceptChangesAfterMerge)
            {
                currentObservableDataRow.AcceptChanges();
            }

            return rowValueChanged;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Applies values
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>True if at least one value had changed, otherwise false</returns>
        static bool ApplyValues(IDataRow currentDataRow, IDataRow refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata)
        {
            bool rowValueChanged = false;

            foreach (IColumnMetadata columnMetadata in listOfColumnMetadata)
            {
                string columnName = columnMetadata.ColumnName;

                object? oldValue;
                currentDataRow.TryGetValue(columnName, out oldValue);

                object? newValue;
                refreshedDataRow.TryGetValue(columnName, out newValue);

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
        /// Applies values
        /// </summary>
        /// <param name="currentObservableDataRow">Current observable data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>True if at least one value had changed, otherwise false</returns>
        static bool ApplyValues(IObservableDataRow currentObservableDataRow, IDataRow refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata)
        {
            bool rowValueChanged = false;

            foreach (IColumnMetadata columnMetadata in listOfColumnMetadata)
            {
                string columnName = columnMetadata.ColumnName;

                object? oldValue;
                currentObservableDataRow.InnerDataRow.TryGetValue(columnName, out oldValue);

                object? newValue;
                refreshedDataRow.TryGetValue(columnName, out newValue);

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
