using PocoDataSet.Extensions.Merging.PostSaveMerge;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataTableExtensions
    {
        public static void DoPostSaveMerge(this IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            DataTableMerger merger = new DataTableMerger();
            merger.Merge(currentDataTable, refreshedDataTable, mergeOptions);
        }
    }
}
