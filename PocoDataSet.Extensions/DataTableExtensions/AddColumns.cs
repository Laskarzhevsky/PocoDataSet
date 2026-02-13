/*
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
        #region Public Methods
        /// <summary>
        /// Adds columns
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        public static void AddColumns(this IDataTable? dataTable, List<IColumnMetadata> listOfColumnMetadata)
        {
            if (dataTable == null)
            {
                return;
            }

            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                IColumnMetadata c = listOfColumnMetadata[i];
                if (!dataTable.ContainsColumn(c.ColumnName))
                {
                    dataTable.AddColumn(c.ColumnName, c.DataType, c.IsNullable, c.IsPrimaryKey, c.IsForeignKey);
                }
            }

            // Reuse the same invariant logic as AddColumn
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                string columnName = listOfColumnMetadata[i].ColumnName;
                EnsureExistingRowsHaveColumn(dataTable, columnName);
            }


            // If PrimaryKeys list was not provided, rebuild it from column metadata.
            if (dataTable.PrimaryKeys == null || dataTable.PrimaryKeys.Count == 0)
            {
                List<string> primaryKeys = new List<string>();

                for (int i = 0; i < listOfColumnMetadata.Count; i++)
                {
                    IColumnMetadata column = listOfColumnMetadata[i];
                    if (column != null && column.IsPrimaryKey)
                    {
                        bool exists = false;
                        for (int k = 0; k < primaryKeys.Count; k++)
                        {
                            if (string.Equals(primaryKeys[k], column.ColumnName, StringComparison.OrdinalIgnoreCase))
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            primaryKeys.Add(column.ColumnName);
                        }
                    }
                }

                dataTable.SetPrimaryKeys(primaryKeys);
            }
        }
        #endregion
    }
}
*/