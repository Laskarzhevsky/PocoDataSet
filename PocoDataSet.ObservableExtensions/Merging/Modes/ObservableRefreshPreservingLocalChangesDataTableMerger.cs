using System;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.Modes
{
    /// <summary>
    /// Reconciles refreshed data with the observable table using primary keys, while preserving local pending changes.
    /// </summary>
    public sealed class ObservableRefreshPreservingLocalChangesDataTableMerger
    {
        public void MergeRefreshPreservingLocalChanges(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataTableDefaultMergeHandler handler = new ObservableDataTableDefaultMergeHandler();
            IObservableMergePolicy policy = ObservableMergePolicyFactory.Create(MergeMode.RefreshPreservingLocalChanges);
            handler.MergeKeyed(currentObservableDataTable, refreshedDataTable, observableMergeOptions, policy);
        }
    }
}
