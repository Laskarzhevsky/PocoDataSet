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
        /// Builds primary key index
        /// </summary>
        /// <param name="dataTable">Current data table</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <returns>Built primary key index</returns>
        public static Dictionary<string, IDataRow> BuildPrimaryKeyIndex(this IDataTable? dataTable, IReadOnlyList<string> primaryKeyColumnNames)
        {
            Dictionary<string, IDataRow> dataRowIndex = new Dictionary<string, IDataRow>(StringComparer.Ordinal);
            if (dataTable == null)
            {
                return dataRowIndex;
            }

            foreach (IDataRow dataRow in dataTable.Rows)
            {
                string primaryKeyValue;
                bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(dataRow, primaryKeyColumnNames, out primaryKeyValue);
                if (!ok)
                {
                    primaryKeyValue = string.Empty;
                }

                // First wins (matches historic behavior)
                if (!dataRowIndex.ContainsKey(primaryKeyValue))
                {
                    dataRowIndex.Add(primaryKeyValue, dataRow);
                }
            }

            return dataRowIndex;
        }
    }
}
