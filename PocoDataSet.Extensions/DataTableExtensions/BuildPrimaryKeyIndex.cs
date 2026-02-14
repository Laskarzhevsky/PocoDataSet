using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        /// <summary>
        /// Builds primary key index.
        /// NOTE: Primary key identity is now schema-authoritative; this method delegates to the centralized index builder.
        /// </summary>
        /// <param name="dataTable">Current data table</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <returns>Built primary key index</returns>
        public static Dictionary<string, IDataRow> BuildPrimaryKeyIndex(this IDataTable? dataTable, IReadOnlyList<string> primaryKeyColumnNames)
        {
            return RowIndexBuilder.BuildRowIndex(dataTable, primaryKeyColumnNames);
        }
    }
}
