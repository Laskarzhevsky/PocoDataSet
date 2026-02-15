using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions.Merging.RefreshMergePreservingLocalChanges;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data table extension methods.
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        public static void DoRefreshMergePreservingLocalChanges(this IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            if (currentObservableDataTable == null)
            {
                throw new ArgumentNullException(nameof(currentObservableDataTable));
            }

            if (refreshedDataTable == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataTable));
            }

            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataTableMerger merger = new ObservableDataTableMerger();
            merger.Merge(currentObservableDataTable, refreshedDataTable, observableMergeOptions);
        }
    }
}
