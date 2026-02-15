using System;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions.Merging.Modes
{
    /// <summary>
    /// Applies server-confirmed changeset back to the observable table after a successful save.
    /// </summary>
    public sealed class ObservablePostSaveDataTableMerger
    {
        public void MergePostSave(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            if (observableMergeOptions == null)
            {
                throw new ArgumentNullException(nameof(observableMergeOptions));
            }

            ObservableDataTableDefaultMergeHandler handler = new ObservableDataTableDefaultMergeHandler();
            IObservableMergePolicy policy = ObservableMergePolicyFactory.Create(MergeMode.PostSave);
            handler.MergeKeyed(currentObservableDataTable, refreshedDataTable, observableMergeOptions, policy);
        }
    }
}
