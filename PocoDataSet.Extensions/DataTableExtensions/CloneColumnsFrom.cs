using System.Collections.Generic;

using PocoDataSet.Data;
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
        /// Clones columns from the source data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="sourceDataTable">Source data table</param>
        public static void CloneColumnsFrom(this IDataTable? dataTable, IDataTable? sourceDataTable)
        {
            if (dataTable == null || sourceDataTable == null)
            {
                return;
            }

            List<IColumnMetadata> clonedListOfColumnMetadata = new List<IColumnMetadata>();
            for (int i = 0; i < sourceDataTable.Columns.Count; i++)
            {
                IColumnMetadata columnMetadata = sourceDataTable.Columns[i];
                IColumnMetadata clonedColumnMetadata = sourceDataTable.Columns[i].Clone();
                dataTable.AddColumn(clonedColumnMetadata);
            }

            // Preserve primary keys from source table
            if (sourceDataTable.PrimaryKeys != null && sourceDataTable.PrimaryKeys.Count > 0)
            {
                dataTable.SetPrimaryKeys(new List<string>(sourceDataTable.PrimaryKeys));
            }
        }
        #endregion
    }
}
