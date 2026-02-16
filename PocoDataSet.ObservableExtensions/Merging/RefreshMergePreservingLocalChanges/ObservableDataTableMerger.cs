using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.RefreshMergePreservingLocalChanges
{
    /// <summary>
    /// Performs RefreshPreservingLocalChanges merge for an observable table.
    /// </summary>
    internal sealed class ObservableDataTableMerger
    {
        public void Merge(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataTableMergeEngine engine = new ObservableDataTableMergeEngine();
            ObservableDataRowMerger rowMerger = new ObservableDataRowMerger();

            engine.MergeKeyed(
                currentObservableDataTable,
                refreshedDataTable,
                observableMergeOptions,
                "RefreshPreservingLocalChanges",
                false,
                false,
                true,
                PreserveRowWhenMissingFromRefreshed,
                ValidateAfterPrimaryKeyDiscovery,
                rowMerger);
        }

        static bool PreserveRowWhenMissingFromRefreshed(DataRowState state)
        {
            return state != DataRowState.Unchanged;
        }

        static void ValidateAfterPrimaryKeyDiscovery(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions, int primaryKeyCount)
        {
            if (primaryKeyCount <= 0)
            {
                throw new InvalidOperationException(
                    "RefreshPreservingLocalChanges requires a primary key on table '" + currentObservableDataTable.TableName + "'.");
            }

            // When we merge keyed, every refreshed row must have a fully populated primary key.
            // If we allow null/DBNull primary key parts, the merge may silently map the row to an empty key,
            // causing collisions and incorrect reconciliation.
            ValidateRefreshedPrimaryKeyValuesAreNotNull(currentObservableDataTable, refreshedDataTable, observableMergeOptions);
        }

        static void ValidateRefreshedPrimaryKeyValuesAreNotNull(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions)
        {
            if (currentObservableDataTable == null)
            {
                return;
            }

            if (refreshedDataTable == null)
            {
                return;
            }

            System.Collections.Generic.List<string> primaryKeyColumnNames = PocoDataSet.ObservableExtensions.ObservableDataTableExtensions.GetPrimaryKeyColumnNames(currentObservableDataTable, observableMergeOptions);
            if (primaryKeyColumnNames == null || primaryKeyColumnNames.Count == 0)
            {
                return;
            }

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedDataTable.Rows[i];

                for (int k = 0; k < primaryKeyColumnNames.Count; k++)
                {
                    string columnName = primaryKeyColumnNames[k];

                    object? value;
                    refreshedRow.TryGetValue(columnName, out value);

                    if (value == null || value == DBNull.Value)
                    {
                        throw new InvalidOperationException(
                            "RefreshPreservingLocalChanges requires non-null primary key values in refreshed table '" + refreshedDataTable.TableName +
                            "'. Column '" + columnName + "' was null in refreshed row index " + i + ".");
                    }
                }
            }
        }
    }
}
