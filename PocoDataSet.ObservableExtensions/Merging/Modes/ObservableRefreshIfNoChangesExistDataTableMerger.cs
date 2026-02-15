using System;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.Modes
{
    /// <summary>
    /// Performs refresh merge only if the current observable table contains no pending changes; otherwise throws.
    /// </summary>
    public sealed class ObservableRefreshIfNoChangesExistDataTableMerger
    {
        public void MergeRefreshIfNoChangesExist(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataTableDefaultMergeHandler handler = new ObservableDataTableDefaultMergeHandler();
            IObservableMergePolicy policy = ObservableMergePolicyFactory.Create(MergeMode.RefreshIfNoChangesExist);
            handler.MergeKeyed(currentObservableDataTable, refreshedDataTable, observableMergeOptions, policy);
        }
    }
}
