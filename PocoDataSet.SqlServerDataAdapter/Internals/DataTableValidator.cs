using System;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides data table validator functionality
    /// </summary>
    static internal class DataTableValidator
    {
        #region Public Methods
        /// <summary>
        /// Validates table exists in SQL Server
        /// </summary>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <param name="tableName">Table name</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ValidateTableExistsInSqlServer(TableWriteMetadata tableWriteMetadata, string tableName)
        {
            if (tableWriteMetadata.ColumnNames.Count == 0)
            {
                throw new InvalidOperationException("Table '" + tableName + "' does not exist in SQL Server or is not accessible.");
            }
        }

        /// <summary>
        /// Validates table for save
        /// </summary>
        /// <param name="dataTable">Data table</param>
        public static void ValidateTableForSave(IDataTable dataTable)
        {
            if (string.IsNullOrEmpty(dataTable.TableName))
            {
                throw new InvalidOperationException("TableName is not specified.");
            }

            if (dataTable.Columns == null)
            {
                throw new InvalidOperationException("Columns collection is null.");
            }

            if (dataTable.Rows == null)
            {
                throw new InvalidOperationException("Rows collection is null.");
            }
        }
        #endregion
    }
}
