using System;
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
        /// Builds data row index
        /// </summary>
        /// <param name="dataTable">Current data table</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <returns>Built data row index</returns>
        public static Dictionary<string, IDataRow> BuildDataRowIndex(this IDataTable? dataTable, List<string> primaryKeyColumnNames)
        {
            Dictionary<string, IDataRow> dataRowIndex = new Dictionary<string, IDataRow>(StringComparer.Ordinal);
            if (dataTable == null)
            {
                return dataRowIndex;
            }

            foreach (IDataRow dataRow in dataTable.Rows)
            {
                string primaryKeyValue = dataRow.CompilePrimaryKeyValue(primaryKeyColumnNames);
                if (!dataRowIndex.ContainsKey(primaryKeyValue))
                {
                    dataRowIndex.Add(primaryKeyValue, dataRow);
                }
            }

            return dataRowIndex;
        }
    }
}
