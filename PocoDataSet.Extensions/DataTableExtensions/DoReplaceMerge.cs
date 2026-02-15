using PocoDataSet.Extensions.Merging.DoReplaceMerge;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataTableExtensions
    {
        public static void DoReplaceMerge(this IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            DataTableMerger merger = new DataTableMerger();
            merger.Merge(currentDataTable, refreshedDataTable, mergeOptions);
        }
    }
}
