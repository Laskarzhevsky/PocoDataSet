using PocoDataSet.IData;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Xml.Linq;

namespace PocoDataSet.Data.Internal
{
    /// <summary>
    /// Owns schema for a table: columns, lookup, and primary key tracking.
    /// </summary>
    internal sealed class DataTableSchema
    {
        #region Data Fields
        /// <summary>
        /// ClientKeyColumn property data field
        /// </summary>
        IColumnMetadata? _clientKeyColumn;

        /// <summary>
        /// Columns property data field
        /// </summary>
        List<IColumnMetadata> _columns = new List<IColumnMetadata>();

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, IColumnMetadata> _columnsByName = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Cached PK column names derived from <see cref="IColumnMetadata.IsPrimaryKey"/> flags.
        /// Source of truth is always the column flags.
        /// </summary>
        readonly List<string> _primaryKeyColumnNames = new();
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            _columnsByName.Clear();
            _columns.Clear();
            _clientKeyColumn = null;

            _primaryKeyColumnNames.Clear();
        }

        /// <summary>
        /// Creates column
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isNullable">Flag indicating whether column is nullable</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        /// <returns>Added column</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if a table contains a column with specified name already</exception>
        public IColumnMetadata CreateColumn(string columnName, string dataType, bool? isNullable = null, bool? isPrimaryKey = null, bool? isForeignKey = null)
        {
            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = columnName;
            columnMetadata.DataType = dataType;

            // Primary key
            if (isPrimaryKey.HasValue)
            {
                columnMetadata.IsPrimaryKey = isPrimaryKey.Value;
            }
            else
            {
                if (columnName == "Id")
                {
                    columnMetadata.IsPrimaryKey = true;
                }
            }

            // Nullability
            if (isNullable.HasValue)
            {
                columnMetadata.IsNullable = isNullable.Value;
            }
            else
            {
                if (columnMetadata.IsPrimaryKey)
                {
                    columnMetadata.IsNullable = false;
                }
                else
                {
                    if (columnName == "Id")
                    {
                        columnMetadata.IsNullable = false;
                    }
                    else
                    {
                        columnMetadata.IsNullable = true;
                    }
                }
            }

            // Foreign key
            if (isForeignKey.HasValue)
            {
                columnMetadata.IsForeignKey = isForeignKey.Value;
            }
            else
            {
                // Do not infer FK for primary keys
                if (!columnMetadata.IsPrimaryKey)
                {
                    if (columnName != "Id" && columnName.EndsWith("Id", StringComparison.Ordinal))
                    {
                        columnMetadata.IsForeignKey = true;
                    }
                }
            }

            // Referenced table/column
            if (columnMetadata.IsForeignKey)
            {
                if (columnName != "Id" && columnName.EndsWith("Id", StringComparison.Ordinal))
                {
                    columnMetadata.ReferencedColumnName = "Id";
                    columnMetadata.ReferencedTableName = columnName.Substring(0, columnName.Length - 2);
                }
            }

            return columnMetadata;
        }

        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="columnMetadata">Column metadata</param>
        public void AddColumn(IColumnMetadata columnMetadata)
        {
            // Keep table.PrimaryKeys as a single source of truth
            if (_columnsByName.ContainsKey(columnMetadata.ColumnName))
            {
                throw new InvalidOperationException($"Column '{columnMetadata.ColumnName}' already exists in table '{TableName}'.");
            }

            _columnsByName.Add(columnMetadata.ColumnName, columnMetadata);
            _columns.Add(columnMetadata);

            if (string.Equals(columnMetadata.ColumnName, SpecialColumnNames.CLIENT_KEY, StringComparison.OrdinalIgnoreCase))
            {
                _clientKeyColumn = columnMetadata;
            }

            RebuildPrimaryKeysFromColumnFlags();
        }

        /// <summary>
        /// Gets flag indicating whether data table contains column with specified name
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Flag indicating whether data table contains column with specified name</returns>
        public bool ContainsColumn(string columnName)
        {
            return _columnsByName.ContainsKey(columnName);
        }

        /// <summary>
        /// Replaces columns
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        public void ReplaceColumns(IList<IColumnMetadata>? listOfColumnMetadata)
        {
            _columnsByName.Clear();
            _columns.Clear();
            _clientKeyColumn = null;

            if (listOfColumnMetadata != null)
            {
                for (int i = 0; i < listOfColumnMetadata.Count; i++)
                {
                    IColumnMetadata column = listOfColumnMetadata[i];
                    if (column == null)
                    {
                        continue;
                    }

                    // Use internal add without rebuilding PK list on each add (perf & determinism).
                    if (!_columnsByName.ContainsKey(column.ColumnName))
                    {
                        _columnsByName.Add(column.ColumnName, column);
                        _columns.Add(column);

                        if (string.Equals(column.ColumnName, SpecialColumnNames.CLIENT_KEY, StringComparison.OrdinalIgnoreCase))
                        {
                            _clientKeyColumn = column;
                        }
                    }
                }
            }

            RebuildPrimaryKeysFromColumnFlags();
        }

        /// <summary>
        /// Ensures  that ClientKey column exists
        /// </summary>
        public void EnsureClientKeyColumnExists()
        {
            if (_clientKeyColumn != null)
            {
                return;
            }

            if (_columnsByName.TryGetValue(SpecialColumnNames.CLIENT_KEY, out IColumnMetadata existing))
            {
                _clientKeyColumn = existing;
                return;
            }

            IColumnMetadata clientKeyColumn = new ColumnMetadata();
            clientKeyColumn.ColumnName = SpecialColumnNames.CLIENT_KEY;
            clientKeyColumn.DataType = DataTypeNames.GUID;
            clientKeyColumn.IsNullable = false;
            clientKeyColumn.IsPrimaryKey = false;
            clientKeyColumn.DisplayName = null;
            clientKeyColumn.Description = "Client-only key for changeset correlation.";

            _columnsByName.Add(clientKeyColumn.ColumnName, clientKeyColumn);
            _columns.Add(clientKeyColumn);
            _clientKeyColumn = clientKeyColumn;

            // PK list unchanged, but keep it consistent with flags.
            RebuildPrimaryKeysFromColumnFlags();
        }

        /// <summary>
        /// Gets a flag indicating whether this schema currently has any primary key columns.
        /// </summary>
        public bool HasPrimaryKey
        {
            get
            {
                return _primaryKeyColumnNames.Count > 0;
            }
        }

        /// <summary>
        /// Gets primary key column names derived from <see cref="IColumnMetadata.IsPrimaryKey"/> flags.
        /// </summary>
        public IReadOnlyList<string> PrimaryKeyColumnNames
        {
            get
            {
                return _primaryKeyColumnNames;
            }
        }

        /// <summary>
        /// Clears primary key flags on all columns.
        /// </summary>
        public void ClearPrimaryKeyFlags()
        {
            for (int i = 0; i < _columns.Count; i++)
            {
                IColumnMetadata column = _columns[i];
                if (column != null)
                {
                    column.IsPrimaryKey = false;
                }
            }

            RebuildPrimaryKeysFromColumnFlags();
        }

        /// <summary>
        /// Sets primary keys by column name. This is authoritative: it clears existing PK flags first.
        /// </summary>
        public void SetPrimaryKeysByName(IList<string> primaryKeyColumnNames)
        {
            ClearPrimaryKeyFlags();

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

                if (!_columnsByName.TryGetValue(name, out IColumnMetadata column))
                {
                    throw new InvalidOperationException($"Column '{name}' does not exist in table '{TableName}'.");
                }

                column.IsPrimaryKey = true;
            }

            RebuildPrimaryKeysFromColumnFlags();
        }

        /// <summary>
        /// Attempts to retrieve the metadata for a column with the specified name
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="columnName">The name of the column for which to retrieve metadata</param>
        /// <param name="columnMetadata">When this method returns, contains the metadata for the specified column if found, otherwise null</param>
        /// <returns>True if the metadata for the specified column is found; otherwise, false.</returns>
        public bool TryGetColumn(string columnName, out IColumnMetadata? columnMetadata)
        {
            if (_columnsByName.TryGetValue(columnName, out IColumnMetadata? foundColumnMetadata))
            {
                columnMetadata = foundColumnMetadata;
                return true;
            }
            else
            {
                columnMetadata = null;
                return false;
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets client key column
        /// </summary>
        public IColumnMetadata? ClientKeyColumn
        {
            get
            {
                return _clientKeyColumn;
            }
        }

        /// <summary>
        /// Gets data table columns
        /// </summary>
        public IReadOnlyList<IColumnMetadata> Items
        {
            get
            {
                return _columns;
            }
        }

        /// <summary>
        /// The owning table name (for error messages).
        /// </summary>
        public string TableName
        {
            get; set;
        } = string.Empty;
        #endregion

        #region Indexers
        /// <summary>
        /// Gets the metadata for the column with the specified name.
        /// IDataTable interface implementation
        /// </summary>
        /// <returns>An instance of IColumnMetadata that contains the metadata for the specified column,
        /// or null if the column does not exist</returns>
        public IColumnMetadata this[string columnName]
        {
            get
            {
                if (ContainsColumn(columnName))
                {
                    return _columnsByName[columnName];
                }

                throw new KeyNotFoundException($"Column '{columnName}' does not exist in table '{TableName}'.");
            }
        }
        #endregion

        #region Private Methods
        void RebuildPrimaryKeysFromColumnFlags()
        {
            _primaryKeyColumnNames.Clear();

            for (int i = 0; i < _columns.Count; i++)
            {
                IColumnMetadata column = _columns[i];
                if (column == null)
                {
                    continue;
                }

                if (!column.IsPrimaryKey)
                {
                    continue;
                }

                // Keep order stable (schema order). Avoid duplicates.
                bool alreadyAdded = false;
                for (int j = 0; j < _primaryKeyColumnNames.Count; j++)
                {
                    if (string.Equals(_primaryKeyColumnNames[j], column.ColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                {
                    _primaryKeyColumnNames.Add(column.ColumnName);
                }
            }
        }
        #endregion
    }
}
