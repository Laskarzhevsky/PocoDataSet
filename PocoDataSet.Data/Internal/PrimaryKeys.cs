using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data.Internal
{
    /// <summary>
    /// Owns and enforces primary key invariants for a table.
    /// This is NOT a raw list: callers must go through these methods to keep
    /// <see cref="IDataTable.PrimaryKeys"/> and <see cref="IColumnMetadata.IsPrimaryKey"/> in sync.
    /// </summary>
    internal sealed class PrimaryKeys
    {
        #region Data Fields
        /// <summary>
        /// "Items" property data field
        /// </summary>
        private readonly List<string> _items = new();
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a primary key column name to the table (idempotent).
        /// Also sets the corresponding <see cref="IColumnMetadata.IsPrimaryKey"/> flag to true.
        /// </summary>
        public void AddPrimaryKey(IReadOnlyList<IColumnMetadata> columns, string columnName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Column name is null or empty.", nameof(columnName));
            }

            IColumnMetadata column = EnsureColumnExists(columns, columnName, tableName);

            // Canonicalize to schema's actual casing.
            string canonicalName = column.ColumnName;

            if (ContainsIgnoreCase(canonicalName))
            {
                // idempotent - do not create duplicates
                column.IsPrimaryKey = true;
                return;
            }

            _items.Add(canonicalName);
            column.IsPrimaryKey = true;
        }

        /// <summary>
        /// Clears primary keys and unsets <see cref="IColumnMetadata.IsPrimaryKey"/> for all columns.
        /// </summary>
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
        /// Rebuilds <see cref="Items"/> from <see cref="IColumnMetadata.IsPrimaryKey"/> flags.
        /// This is useful after deserialization when column flags are present.
        /// </summary>
        /// <param name="listOfColumnMetadata"></param>
        public void RebuildFromColumnFlags(IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            _items.Clear();

            if (listOfColumnMetadata == null)
            {
                return;
            }

            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                IColumnMetadata column = listOfColumnMetadata[i];
                if (column == null)
                {
                    continue;
                }

                if (!column.IsPrimaryKey)
                {
                    continue;
                }

                if (!ContainsIgnoreCase(column.ColumnName))
                {
                    _items.Add(column.ColumnName);
                }
            }
        }

        /// <summary>
        /// Sets primary keys for the table. This is authoritative: it clears existing PKs first.
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <param name="tableName">Table name</param>
        public void SetPrimaryKeys(IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IList<string> primaryKeyColumnNames, string tableName)
        {
            ClearPrimaryKeys(listOfColumnMetadata);

            if (primaryKeyColumnNames == null || primaryKeyColumnNames.Count == 0)
            {
                return;
            }

            for (int i = 0; i < primaryKeyColumnNames.Count; i++)
            {
                string name = primaryKeyColumnNames[i];
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                AddPrimaryKey(listOfColumnMetadata, name, tableName);
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

        #region Private methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        bool ContainsIgnoreCase(string columnName)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (string.Equals(_items[i], columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="columnName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        IColumnMetadata EnsureColumnExists(IReadOnlyList<IColumnMetadata> columns, string columnName, string tableName)
        {
            if (columns == null)
            {
                throw new InvalidOperationException($"Table '{tableName}' has no columns.");
            }

            for (int i = 0; i < columns.Count; i++)
            {
                IColumnMetadata column = columns[i];
                if (column == null)
                {
                    continue;
                }

                if (string.Equals(column.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return column;
                }
            }

            throw new InvalidOperationException($"Column '{columnName}' does not exist in table '{tableName}'.");
        }
        #endregion
    }
}
