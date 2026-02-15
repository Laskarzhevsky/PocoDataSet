using System.Collections.Generic;

using PocoDataSet.Extensions.Merging.RefreshMergePreservingLocalChanges;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataRowExtensions
    {
        public static bool DoRefreshMergePreservingLocalChanges(this IDataRow currentDataRow, IDataRow refreshedDataRow, string tableName, IReadOnlyList<IColumnMetadata> columns, IMergeOptions mergeOptions)
        {
            DataRowMerger merger = new DataRowMerger();
            return merger.Merge(currentDataRow, refreshedDataRow, columns);
        }
    }
}
