using PocoDataSet.Extensions.Merging.RefreshMergeIfNoChangesExist;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataSetExtensions
    {
        public static void DoRefreshMergeIfNoChangesExist(this IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions)
        {
            DataSetMerger merger = new DataSetMerger();
            merger.Merge(currentDataSet, refreshedDataSet, mergeOptions);
        }
    }
}
