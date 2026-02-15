using PocoDataSet.Extensions.Merging.DoReplaceMerge;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataSetExtensions
    {
        public static void DoReplaceMerge(this IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions)
        {
            DataSetMerger merger = new DataSetMerger();
            merger.Merge(currentDataSet, refreshedDataSet, mergeOptions);
        }
    }
}
