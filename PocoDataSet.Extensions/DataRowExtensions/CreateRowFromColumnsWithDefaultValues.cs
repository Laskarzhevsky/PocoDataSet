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
        /// Creates row from columns with default values
        /// </summary>
        /// <param name="columnsMetadata">Columns metadata</param>
        /// <returns>Created row</returns>
        public static IDataRow CreateRowFromColumnsWithDefaultValues(List<IColumnMetadata> columnsMetadata)
        {
            IDataRow dataRow = DataRowFactory.CreateEmpty(columnsMetadata.Count);
            foreach (IColumnMetadata columnMetadata in columnsMetadata)
            {
                dataRow[columnMetadata.ColumnName] = MetadataDefaults.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
            }

            return dataRow;
        }
        #endregion
    }
}
