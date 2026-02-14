using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data
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
        /// PrimaryKeys property data field
        /// </summary>
        readonly PrimaryKeys _primaryKeys = new();
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

            // Clear PK list; flags are on columns that are being cleared anyway.
            _primaryKeys.RebuildFromColumnFlags(_columns);
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

            // Keep PK list in sync with flags.
            _primaryKeys.RebuildFromColumnFlags(_columns);
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

            _primaryKeys.RebuildFromColumnFlags(_columns);
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
            _primaryKeys.RebuildFromColumnFlags(_columns);
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
        /// Gets primary keys
        /// </summary>
        public PrimaryKeys PrimaryKeys
        {
            get
            {
                return _primaryKeys;
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
    }
}
