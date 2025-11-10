using System;

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
        /// Gets flag indicating whether data table contains column with specified name
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Flag indicating whether data table contains column with specified name</returns>
        public static bool ContainsColumn(this IDataTable dataTable, string columnName)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }

            string columnNameToLowerInvariant = columnName.ToLowerInvariant();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                IColumnMetadata columnMetadata = dataTable.Columns[i];
                if (columnMetadata.ColumnName.ToLowerInvariant() == columnNameToLowerInvariant)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
