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
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <param name="scopeFunction">Scope function</param>
        /// <returns>Built data row index</returns>
        public static Dictionary<string, IDataRow> BuildDataRowIndex(this IDataTable currentDataTable, List<string> primaryKeyColumnNames, Func<IDataRow, bool>? scopeFunction)
        {
            Dictionary<string, IDataRow> dataRowIndex = new Dictionary<string, IDataRow>(StringComparer.Ordinal);
            if (currentDataTable == null)
            {
                return dataRowIndex;
            }

            foreach (IDataRow dataRow in currentDataTable.Rows)
            {
                if (scopeFunction != null && !scopeFunction(dataRow))
                {
                    continue;
                }

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
