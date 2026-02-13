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
        /// <param name="columnName">Column name</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddPrimaryKey(string columnName)
        {
            _items.Add(columnName);
        }

        /// <summary>
        /// Clears primary keys
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        public void ClearPrimaryKeys(IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
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
        public void SetPrimaryKeys(IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IList<string> primaryKeyColumnNames, string tableName)
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

                AddPrimaryKey(columnName);
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
    }
}
