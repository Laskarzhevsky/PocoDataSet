using System;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions.Merging.Modes
{
    /// <summary>
    /// Applies server-confirmed changeset back to the current table after a successful save.
    /// </summary>
    public sealed class PostSaveDataTableMerger
    {
        public void MergePostSave(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            DataTableDefaultMergeHandler handler = new DataTableDefaultMergeHandler();
            handler.MergePostSave(currentDataTable, refreshedDataTable, mergeOptions);
        }
    }
}
