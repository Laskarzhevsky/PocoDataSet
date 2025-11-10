using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Creates row from columns
        /// </summary>
        /// <param name="columnsMetadata">Columns metadata</param>
        /// <returns>Created row</returns>
        public static IDataRow CreateRowFromColumns(List<IColumnMetadata> columnsMetadata)
        {
            IDataRow row = new DataRow();
            foreach (IColumnMetadata column in columnsMetadata)
            {
                row[column.ColumnName] = null;
            }

            return row;
        }
        #endregion
    }
}
