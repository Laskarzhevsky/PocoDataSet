using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.RefreshMergeIfNoChangesExist
{
    /// <summary>
    /// Performs RefreshIfNoChangesExist merge for a single row.
    /// </summary>
    internal sealed class ObservableDataRowMerger : IObservableRowMerger
    {
        public bool MergeDataRow(string currentObservableDataTableName, IDataRow currentDataRow, IDataRow refreshedDataRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            if (!CanOverwrite(currentDataRow.DataRowState))
            {
                return false;
            }

            bool changed = ApplyValues(currentDataRow, refreshedDataRow, listOfColumnMetadata);

            if (ShouldAcceptChangesAfterMerge())
            {
                currentDataRow.AcceptChanges();
            }

            return changed;
        }

        public bool MergeObservableRow(string currentObservableDataTableName, IObservableDataRow currentObservableDataRow, IDataRow refreshedDataRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            if (!CanOverwrite(currentObservableDataRow.InnerDataRow.DataRowState))
            {
                return false;
            }

            bool changed;

            bool suppressRowStateChanged = ShouldSuppressRowStateChanged(currentObservableDataRow);

            if (suppressRowStateChanged && currentObservableDataRow is ObservableDataRow observableDataRow)
            {
                using (observableDataRow.SuppressRowStateChanged())
                {
                    changed = ApplyValues(currentObservableDataRow, refreshedDataRow, listOfColumnMetadata);

                    if (ShouldAcceptChangesAfterMerge())
                    {
                        currentObservableDataRow.AcceptChanges();
                    }
                }
            }
            else
            {
                changed = ApplyValues(currentObservableDataRow, refreshedDataRow, listOfColumnMetadata);

                if (ShouldAcceptChangesAfterMerge())
                {
                    currentObservableDataRow.AcceptChanges();
                }
            }

            return changed;
        }

        static bool ApplyValues(IDataRow currentDataRow, IDataRow refreshedDataRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
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

        static bool ApplyValues(IObservableDataRow currentObservableDataRow, IDataRow refreshedDataRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
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

        static bool ShouldAcceptChangesAfterMerge()
        {
            return true;
        }

        static bool CanOverwrite(DataRowState currentState)
        {
            return currentState == DataRowState.Unchanged;
        }

        static bool ShouldSuppressRowStateChanged(IObservableDataRow row)
        {
            return true;
        }
    }
}
