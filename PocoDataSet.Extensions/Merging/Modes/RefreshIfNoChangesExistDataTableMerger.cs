using System;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.Modes
{
    /// <summary>
    /// Performs refresh merge only if the current table contains no pending changes; otherwise throws.
    /// </summary>
    public sealed class RefreshIfNoChangesExistDataTableMerger
    {
        public void MergeRefreshIfNoChangesExist(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            // Fail fast if the table is dirty.
            for (int i = 0; i < currentDataTable.Rows.Count; i++)
            {
                if (currentDataTable.Rows[i].DataRowState != DataRowState.Unchanged)
                {
                    throw new InvalidOperationException(
                        "MergeMode.RefreshIfNoChangesExist cannot be used when current table '" + currentDataTable.TableName + "' contains pending changes.");
                }
            }

            DataTableDefaultMergeHandler handler = new DataTableDefaultMergeHandler();
            handler.MergeRefresh(currentDataTable, refreshedDataTable, mergeOptions);
        }
    }
}
