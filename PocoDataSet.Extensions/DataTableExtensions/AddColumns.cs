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

            dataTable.Columns = listOfColumnMetadata;

            // Reuse the same invariant logic as AddColumn
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                string columnName = listOfColumnMetadata[i].ColumnName;
                EnsureExistingRowsHaveColumn(dataTable, columnName);
            }
        }
        #endregion
    }
}
