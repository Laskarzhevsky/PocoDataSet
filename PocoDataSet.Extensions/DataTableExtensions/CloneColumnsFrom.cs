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
                IColumnMetadata clonedColumnMetadata = new ColumnMetadata();
                clonedColumnMetadata.ColumnName = columnMetadata.ColumnName;
                clonedColumnMetadata.DataType = columnMetadata.DataType;
                clonedColumnMetadata.Description = columnMetadata.Description;
                clonedColumnMetadata.DisplayName = columnMetadata.DisplayName;
                clonedColumnMetadata.DisplayOrder = columnMetadata.DisplayOrder;
                clonedColumnMetadata.IsForeignKey = columnMetadata.IsForeignKey;
                clonedColumnMetadata.IsNullable = columnMetadata.IsNullable;
                clonedColumnMetadata.IsPrimaryKey = columnMetadata.IsPrimaryKey;
                clonedColumnMetadata.MaxLength = columnMetadata.MaxLength;
                clonedColumnMetadata.Precision = columnMetadata.Precision;
                clonedColumnMetadata.ReferencedColumnName = columnMetadata.ReferencedColumnName;
                clonedColumnMetadata.ReferencedTableName = columnMetadata.ReferencedTableName;
                clonedColumnMetadata.Scale = columnMetadata.Scale;

                dataTable.Columns.Add(clonedColumnMetadata);
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
