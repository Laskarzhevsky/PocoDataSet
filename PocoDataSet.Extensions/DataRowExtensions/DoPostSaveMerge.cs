using System.Collections.Generic;

using PocoDataSet.Extensions.Merging.PostSaveMerge;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataRowExtensions
    {
        public static void DoPostSaveMerge(this IDataRow currentDataRow, IDataRow refreshedDataRow, string tableName, IReadOnlyList<IColumnMetadata> columns, IMergeOptions mergeOptions)
        {
            DataRowMerger merger = new DataRowMerger();
            merger.Merge(currentDataRow, refreshedDataRow, columns);
        }
    }
}
