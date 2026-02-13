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
        /// Creates data row from list of column metadata
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>Created data row</returns>
        public static IDataRow CreateRowFromColumns(IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            IDataRow dataRow = DataRowFactory.CreateEmpty(listOfColumnMetadata.Count);
            foreach (IColumnMetadata columnMetadata in listOfColumnMetadata)
            {
                dataRow[columnMetadata.ColumnName] = null;
            }

            return dataRow;
        }
        #endregion
    }
}
