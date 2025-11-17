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
        /// Clones columns from data table
        /// </summary>
        /// <param name="clonedDataTable">Cloned data table</param>
        /// <param name="dataTable">Data table</param>
        public static void CloneColumnsFrom(this IDataTable? clonedDataTable, IDataTable? dataTable)
        {
            if (clonedDataTable == null || dataTable == null)
            {
                return;
            }

            List<IColumnMetadata> clonedListOfColumnMetadata = new List<IColumnMetadata>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                IColumnMetadata columnMetadata = dataTable.Columns[i];
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

                clonedDataTable.Columns.Add(clonedColumnMetadata);
            }
        }
        #endregion
    }
}
