using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data.Internal
{
    /// <summary>
    /// Provides functionality for managing primary keys in a dataset.
    /// </summary>
    class PrimaryKeys
    {
        #region Data Fields
        /// <summary>
        /// "Items" property data field
        /// </summary>
        private readonly List<string> _items = new();
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a primary key column name to the table.
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="columnName">Column name</param>
        /// <param name="tableName">Table name</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddPrimaryKey(List<IColumnMetadata> listOfColumnMetadata, string columnName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Primary key column name cannot be empty.", nameof(columnName));
            }

            EnsureColumnExists(listOfColumnMetadata, columnName, tableName);

            for (int i = 0; i < _items.Count; i++)
            {
                if (string.Equals(_items[i], columnName, StringComparison.OrdinalIgnoreCase))
                {
                    MarkColumnAsPrimaryKey(listOfColumnMetadata, columnName);
                    return;
                }
            }

            _items.Add(columnName);
            MarkColumnAsPrimaryKey(listOfColumnMetadata, columnName);
        }

        /// <summary>
        /// Clears primary keys
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        public void ClearPrimaryKeys(List<IColumnMetadata> listOfColumnMetadata)
        {
            _items.Clear();

            if (listOfColumnMetadata != null)
            {
                for (int i = 0; i < listOfColumnMetadata.Count; i++)
                {
                    IColumnMetadata columnMetadata = listOfColumnMetadata[i];
                    if (columnMetadata != null)
                    {
                        columnMetadata.IsPrimaryKey = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets primary key column names for the table.
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <param name="tableName">Table name</param>
        public void SetPrimaryKeys(List<IColumnMetadata> listOfColumnMetadata, IList<string> primaryKeyColumnNames, string tableName)
        {
            _items.Clear();

            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                IColumnMetadata columnMetadata = listOfColumnMetadata[i];
                if (columnMetadata != null)
                {
                    columnMetadata.IsPrimaryKey = false;
                }
            }

            if (primaryKeyColumnNames == null || primaryKeyColumnNames.Count == 0)
            {
                return;
            }

            for (int i = 0; i < primaryKeyColumnNames.Count; i++)
            {
                string columnName = primaryKeyColumnNames[i];
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    continue;
                }

                AddPrimaryKey(listOfColumnMetadata, columnName, tableName);
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets primary key column names for the table
        /// </summary>
        public IReadOnlyList<string> Items
        {
            get
            {
                return _items;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Ensures that column exists
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="columnName">Column name</param>
        /// <param name="tableName">Table name</param>
        void EnsureColumnExists(List<IColumnMetadata> listOfColumnMetadata, string columnName, string tableName)
        {
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                IColumnMetadata columnMetadata = listOfColumnMetadata[i];
                if (columnMetadata != null && string.Equals(columnMetadata.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Primary key column '{columnName}' does not exist in table '{tableName}'.");
        }

        /// <summary>
        /// Marks column as primary key
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="columnName">Column name</param>
        void MarkColumnAsPrimaryKey(List<IColumnMetadata> listOfColumnMetadata, string columnName)
        {
            if (listOfColumnMetadata == null)
            {
                return;
            }

            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                IColumnMetadata columnMetadata = listOfColumnMetadata[i];
                if (columnMetadata != null && string.Equals(columnMetadata.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    columnMetadata.IsPrimaryKey = true;
                    return;
                }
            }
        }
        #endregion
    }
}
