using System;

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
        /// Creates new data row in data table with default values taken from columns metadata
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>New data row created in data table</returns>
        public static IDataRow AddNewRow(this IDataTable? dataTable)
        {
            if (dataTable == null)
            {
                throw new System.ArgumentNullException(nameof(dataTable));
            }

            EnsureClientKeyColumnExists(dataTable);

            IDataRow dataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);
            dataRow[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
            dataRow.DataRowState = DataRowState.Added;
            dataTable.Rows.Add(dataRow);

            return dataRow;
        }

        static void EnsureClientKeyColumnExists(IDataTable dataTable)
        {
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (string.Equals(dataTable.Columns[i].ColumnName, SpecialColumnNames.CLIENT_KEY, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            ColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = SpecialColumnNames.CLIENT_KEY;
            columnMetadata.DataType = DataTypeNames.GUID;
            columnMetadata.IsNullable = false;
            columnMetadata.IsPrimaryKey = false;
            columnMetadata.DisplayName = null;
            columnMetadata.Description = "Client-only key for changeset correlation.";
            dataTable.Columns.Add(columnMetadata);
        }
        #endregion
    }
}
