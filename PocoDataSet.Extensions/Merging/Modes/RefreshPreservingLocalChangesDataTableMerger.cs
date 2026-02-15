using System;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.Modes
{
    /// <summary>
    /// Reconciles refreshed data with the current table using primary keys, while preserving local pending changes.
    /// </summary>
    public sealed class RefreshPreservingLocalChangesDataTableMerger
    {
        public void MergeRefreshPreservingLocalChanges(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            DataTableDefaultMergeHandler handler = new DataTableDefaultMergeHandler();
            handler.MergeRefresh(currentDataTable, refreshedDataTable, mergeOptions);
        }
    }
}
