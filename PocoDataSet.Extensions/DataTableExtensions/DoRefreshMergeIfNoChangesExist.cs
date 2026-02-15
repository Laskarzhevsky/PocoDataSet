using PocoDataSet.Extensions.Merging.RefreshMergeIfNoChangesExist;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataTableExtensions
    {
        public static void DoRefreshMergeIfNoChangesExist(this IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            DataTableMerger merger = new DataTableMerger();
            merger.Merge(currentDataTable, refreshedDataTable, mergeOptions);
        }
    }
}
