using PocoDataSet.Extensions.Merging.RefreshMergePreservingLocalChanges;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataSetExtensions
    {
        public static void DoRefreshMergePreservingLocalChanges(this IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions)
        {
            DataSetMerger merger = new DataSetMerger();
            merger.Merge(currentDataSet, refreshedDataSet, mergeOptions);
        }
    }
}
