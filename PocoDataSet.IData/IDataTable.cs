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
        /// Gets list of column metadata that defines the table schema
        /// </summary>
        IReadOnlyList<IColumnMetadata> Columns
        {
            get;
        }

        /// <summary>
        /// Gets the names of the columns that form the primary key.
        /// This list is derived from IColumnMetadata.IsPrimaryKey flags.
        /// </summary>
        IReadOnlyList<string> PrimaryKeys
        {
            get;
        }

        /// <summary>
        /// Gets a read-only view of the data rows contained in the table.
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

        #region Indexers
        /// <summary>
        /// Gets the metadata for the column with the specified name
        /// </summary>
        /// <returns>An instance of IColumnMetadata that contains the metadata for the specified column,
        /// or null if the column does not exist.</returns>
        IColumnMetadata this[string columnName]
        {
            get;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isNullable">Flag indicating whether column is nullable</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        /// <returns>Added column</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if a table contains a column with specified name already</exception>
        IColumnMetadata AddColumn(string columnName, string dataType, bool? isNullable = null, bool? isPrimaryKey = null, bool? isForeignKey = null);

        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="columnMetadata">Column metadata</param>
        void AddColumn(IColumnMetadata columnMetadata);

        /// <summary>
        /// Adds columns
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        void AddColumns(IReadOnlyList<IColumnMetadata> listOfColumnMetadata);

        /// <summary>
        /// Adds loaded row from data storage
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="dataRow">Data row</param>
        /// <exception cref="InvalidOperationException">Exception is thrown if row in Deleted state</exception>
        void AddLoadedRow(IDataRow dataRow);

        /// <summary>
        /// Adds row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        void AddRow(IDataRow dataRow);

        /// <summary>
        /// Gets flag indicating whether data table contains column with specified name
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Flag indicating whether data table contains column with specified name</returns>
        bool ContainsColumn(string columnName);

        /// <summary>
        /// Gets flag indicating whether table contains specified row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>True if table contains specified row, otherwise false</returns>
        bool ContainsRow(IDataRow dataRow);

        /// <summary>
        /// Ensures  that ClientKey column exists
        /// </summary>
        void EnsureClientKeyColumnExists();

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
        /// Attempts to retrieve the metadata for a column with the specified name
        /// </summary>
        /// <param name="columnName">The name of the column for which to retrieve metadata</param>
        /// <param name="columnMetadata">When this method returns, contains the metadata for the specified column if found, otherwise null</param>
        /// <returns>True if the metadata for the specified column is found; otherwise, false.</returns>
        bool TryGetColumn(string columnName, out IColumnMetadata? columnMetadata);
        #endregion
    }
}
