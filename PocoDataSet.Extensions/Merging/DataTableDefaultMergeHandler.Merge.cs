using System;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public partial class DataTableDefaultMergeHandler
    {
        public void Merge(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            if (mergeOptions == null)
            {
                throw new ArgumentNullException(nameof(mergeOptions));
            }

            if (mergeOptions.MergeMode == MergeMode.PostSave)
            {
                MergePostSave(currentDataTable, refreshedDataTable, mergeOptions);
                return;
            }

            // Default / Refresh / Replace all use Refresh-style table merge.
            // Replace-specific behavior (if any) is handled by row merge handlers / options.
            MergeRefresh(currentDataTable, refreshedDataTable, mergeOptions);
        }
    }
}
