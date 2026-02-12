using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data table functionality
    /// </summary>
    public interface IDataTable
    {
        #region Properties
        /// <summary>
        /// Gets or sets list of columns
        /// </summary>
        List<IColumnMetadata> Columns
        {
            get; set;
        }

        /// <summary>
        /// Gets primary keys
        /// </summary>
        IReadOnlyList<string> PrimaryKeys
        {
            get;
        }

        /// <summary>
        /// Gets rows
        /// </summary>
        IReadOnlyList<IDataRow> Rows
        {
            get;
        }

        /// <summary>
        /// Gets or sets table name
        /// </summary>
        string TableName
        {
            get; set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds loaded row from data storage
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="dataRow">Data row</param>
        /// <exception cref="InvalidOperationException">Exception is thrown if row in Deleted state</exception>
        void AddLoadedRow(IDataRow dataRow);

        /// <summary>
        /// Adds a primary key column name to the table.
        /// </summary>
        /// <param name="columnName">Column name</param>
        void AddPrimaryKey(string columnName);

        /// <summary>
        /// Adds row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        void AddRow(IDataRow dataRow);

        /// <summary>
        /// Clears table primary keys.
        /// </summary>
        void ClearPrimaryKeys();

        /// <summary>
        /// Gets flag indicating whether table contains specified row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>True if table contains specified row, otherwise false</returns>
        bool ContainsRow(IDataRow dataRow);

        /// <summary>
        /// Removes all rows
        /// </summary>
        void RemoveAllRows();

        /// <summary>
        /// Removes row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>True if row was removed, otherwise false</returns>
        bool RemoveRow(IDataRow dataRow);

        /// <summary>
        /// Removes row at position specified by row index
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        void RemoveRowAt(int rowIndex);

        /// <summary>
        /// Sets primary key column names for the table.
        /// </summary>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        void SetPrimaryKeys(IList<string> primaryKeyColumnNames);
        #endregion
    }
}
