using PocoDataSet.Extensions.Merging.PostSaveMerge;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataSetExtensions
    {
        public static void DoPostSaveMerge(this IDataSet currentDataSet, IDataSet changesetDataSet, IMergeOptions mergeOptions)
        {
            DataSetMerger merger = new DataSetMerger();
            merger.Merge(currentDataSet, changesetDataSet, mergeOptions);
        }
    }
}
