using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions.Merging.DoReplaceMerge;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods.
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        public static void DoReplaceMerge(this IDataRow currentDataRow, IDataRow refreshedDataRow, string currentObservableDataTableName, IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            if (currentDataRow == null)
            {
                throw new ArgumentNullException(nameof(currentDataRow));
            }

            if (refreshedDataRow == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataRow));
            }

            if (currentObservableDataTableName == null)
            {
                throw new ArgumentNullException(nameof(currentObservableDataTableName));
            }

            if (listOfColumnMetadata == null)
            {
                throw new ArgumentNullException(nameof(listOfColumnMetadata));
            }

            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataRowMerger merger = new ObservableDataRowMerger();
            merger.MergeDataRow(currentObservableDataTableName, currentDataRow, refreshedDataRow, listOfColumnMetadata, observableMergeOptions);
        }

        public static void DoReplaceMerge(this IObservableDataRow currentObservableDataRow, IDataRow refreshedDataRow, string currentObservableDataTableName, IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            if (currentObservableDataRow == null)
            {
                throw new ArgumentNullException(nameof(currentObservableDataRow));
            }

            if (refreshedDataRow == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataRow));
            }

            if (currentObservableDataTableName == null)
            {
                throw new ArgumentNullException(nameof(currentObservableDataTableName));
            }

            if (listOfColumnMetadata == null)
            {
                throw new ArgumentNullException(nameof(listOfColumnMetadata));
            }

            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataRowMerger merger = new ObservableDataRowMerger();
            merger.MergeObservableRow(currentObservableDataTableName, currentObservableDataRow, refreshedDataRow, listOfColumnMetadata, observableMergeOptions);
        }
    }
}
