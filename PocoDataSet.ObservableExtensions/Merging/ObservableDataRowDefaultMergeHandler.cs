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
            DataRowState dataRowState = currentDataRow.DataRowState;

            // Apply the same policy as the non-observable merge:
            // - Refresh: update only Unchanged rows (never overwrite user edits).
            // - PostSave: allow updating Added/Modified/Unchanged, but never Deleted.
            switch (observableMergeOptions.MergeMode)
            {
                case MergeMode.Replace:
                    // Replace discards local changes; refreshed values always win.
                    // Commit baseline the same way as Refresh/PostSave.
                    break;

                case MergeMode.Refresh:
                    if (dataRowState != DataRowState.Unchanged)
                    {
                        return false;
                    }

                    break;
                case MergeMode.PostSave:
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

                // Be tolerant to schema evolution / special columns (e.g., __ClientKey) that may not exist yet on older rows.
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

            // In both Refresh and PostSave, the server snapshot becomes baseline.
            if (observableMergeOptions.MergeMode == MergeMode.Refresh || observableMergeOptions.MergeMode == MergeMode.PostSave || observableMergeOptions.MergeMode == MergeMode.Replace)
            {
                // AcceptChanges on an Unchanged row is a no-op; on Added/Modified it commits the post-save baseline.
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
        public bool Merge(string currentObservableDataTableName, IObservableDataRow currentObservableDataRow, IDataRow refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            DataRowState dataRowState = currentObservableDataRow.InnerDataRow.DataRowState;
            switch (observableMergeOptions.MergeMode)
            {
                case MergeMode.Refresh:
                    if (dataRowState != DataRowState.Unchanged)
                    {
                        return false;
                    }

                    break;
                case MergeMode.PostSave:
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

                object? oldValue;
                currentObservableDataRow.InnerDataRow.TryGetValue(columnName, out oldValue);

                object? newValue;
                refreshedDataRow.TryGetValue(columnName, out newValue);

                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                // Use observable indexer so events fire.
                currentObservableDataRow[columnName] = newValue;
                rowValueChanged = true;
            }

            if (observableMergeOptions.MergeMode == MergeMode.Refresh || observableMergeOptions.MergeMode == MergeMode.PostSave || observableMergeOptions.MergeMode == MergeMode.Replace)
            {
                // Commit baseline (also clears Modified/Added after a successful post-save reconciliation).
                currentObservableDataRow.AcceptChanges();
            }

            return rowValueChanged;
        }
        #endregion
    }
}
