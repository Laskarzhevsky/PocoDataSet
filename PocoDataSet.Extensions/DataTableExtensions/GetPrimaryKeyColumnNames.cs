using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Gets primary key column names
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <returns>Primary keys</returns>
        public static List<string> GetPrimaryKeyColumnNames(this IDataTable dataTable, IMergeOptions? mergeOptions)
        {
            List<string> listOfPrimaryKeys = new List<string>();
            if (dataTable == null)
            {
                return listOfPrimaryKeys;
            }

            // 1) Explicit overrides win
            if (mergeOptions != null)
            {
                List<string>? overrideKeys;
                if (mergeOptions.OverriddenPrimaryKeyNames.TryGetValue(dataTable.TableName, out overrideKeys))
                {
                    if (overrideKeys != null && overrideKeys.Count > 0)
                    {
                        return overrideKeys;
                    }
                }
            }

            // 2) Discover composite primary key from schema
            foreach (IColumnMetadata columnMetadata in dataTable.Columns)
            {
                if (columnMetadata != null && columnMetadata.IsPrimaryKey)
                {
                    listOfPrimaryKeys.Add(columnMetadata.ColumnName);
                }
            }

            return listOfPrimaryKeys;
        }
        #endregion
    }
}
