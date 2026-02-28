using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using PocoDataSet.Data.Internal;
using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Represents data table
    /// </summary>
    public class DataTable : IDataTable
    {
        #region Data Fields
        /// <summary>
        /// Holds reference to data table schema (columns and primary keys)
        /// </summary>
        readonly DataTableSchema _dataTableSchema = new();

        /// <summary>
        /// "Rows" property data field
        /// </summary>
        readonly List<IDataRow> _rows = new();

        /// <summary>
        /// Holds PrimaryKeys JSON value when it arrives before Columns during deserialization.
        /// </summary>
        List<string>? _pendingPrimaryKeysJson;

        /// <summary>
        /// TableName property data field
        /// </summary>
        string _tableName = string.Empty;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets list of column metadata that defines the table schema
        /// IDataTable interface implementation
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<IColumnMetadata> Columns
        {
            get
            {
                return _dataTableSchema.Items;
            }
        }

        /// <summary>
        /// Gets columns for JSON serialization / deserialization
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("Columns")]
        public List<ColumnMetadata> ColumnsJson
        {
            get
            {
                // System.Text.Json cannot deserialize interfaces. Use the concrete ColumnMetadata.
                // If schema contains other IColumnMetadata implementations, clone/copy into ColumnMetadata.
                List<ColumnMetadata> result = new List<ColumnMetadata>(_dataTableSchema.Items.Count);

                for (int i = 0; i < _dataTableSchema.Items.Count; i++)
                {
                    IColumnMetadata item = _dataTableSchema.Items[i];

                    ColumnMetadata? concrete = item as ColumnMetadata;
                    if (concrete != null)
                    {
                        result.Add(concrete);
                        continue;
                    }

                    IColumnMetadata cloned = item.Clone();
                    ColumnMetadata? clonedConcrete = cloned as ColumnMetadata;
                    if (clonedConcrete != null)
                    {
                        result.Add(clonedConcrete);
                        continue;
                    }

                    // Last resort: copy known fields.
                    ColumnMetadata copy = new ColumnMetadata();
                    copy.ColumnName = item.ColumnName;
                    copy.DataType = item.DataType;
                    copy.Description = item.Description;
                    copy.DisplayName = item.DisplayName;
                    copy.DisplayOrder = item.DisplayOrder;
                    copy.IsForeignKey = item.IsForeignKey;
                    copy.IsNullable = item.IsNullable;
                    copy.IsPrimaryKey = item.IsPrimaryKey;
                    copy.MaxLength = item.MaxLength;
                    copy.Precision = item.Precision;
                    copy.ReferencedColumnName = item.ReferencedColumnName;
                    copy.ReferencedTableName = item.ReferencedTableName;
                    copy.Scale = item.Scale;
                    result.Add(copy);
                }

                return result;
            }
            private set
            {
                // ReplaceColumns expects IColumnMetadata; convert without LINQ.
                List<IColumnMetadata> list = new List<IColumnMetadata>();

                if (value != null)
                {
                    for (int i = 0; i < value.Count; i++)
                    {
                        list.Add(value[i]);
                    }
                }

                _dataTableSchema.ReplaceColumns(list);
                _dataTableSchema.TableName = TableName;

                // Apply pending PrimaryKeys (if PrimaryKeys JSON was read before Columns).
                ApplyPendingPrimaryKeysIfAny();

                // If rows were already deserialized, ensure each row has all columns.
                EnsureExistingRowsHaveAllColumns();
            }
        }

        /// <summary>
        /// Gets primary keys
        /// IDataTable interface implementation
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<string> PrimaryKeys
        {
            get
            {
                return _dataTableSchema.PrimaryKeyColumnNames;
            }
        }

        /// <summary>
        /// Gets rows
        /// IDataTable interface implementation
        /// </summary>
        [JsonIgnore]
        public IReadOnlyList<IDataRow> Rows
        {
            get
            {
                return _rows;
            }
        }

        /// <summary>
        /// Gets rows for JSON serialization / deserialization
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("Rows")]
        [JsonConverter(typeof(DataRowListJsonConverter))]
        public List<IDataRow> RowsJson
        {
            get
            {
                return new List<IDataRow>(_rows);
            }
            private set
            {
                _rows.Clear();

                if (value == null)
                {
                    return;
                }

                for (int i = 0; i < value.Count; i++)
                {
                    _rows.Add(value[i]);
                }

                // If schema was already deserialized, ensure each row has all columns.
                EnsureExistingRowsHaveAllColumns();
            }
        }

        /// <summary>
        /// Gets or sets table name
        /// IDataTable interface implementation
        /// </summary>
        public string TableName
        {
            get
            {
                return _tableName;
            }
            set
            {
                _tableName = value ?? string.Empty;
                _dataTableSchema.TableName = _tableName;
            }
        }
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
                if (columnName == null)
                {
                    throw new KeyNotFoundException($"DataTable does not contain column with name {columnName}");
                }

                return _dataTableSchema[columnName];
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds column
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isNullable">Flag indicating whether column is nullable</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        /// <returns>Added column</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if a table contains a column with specified name already</exception>
        public IColumnMetadata AddColumn(string columnName, string dataType, bool? isNullable = null, bool? isPrimaryKey = null, bool? isForeignKey = null)
        {
            if (_dataTableSchema.ContainsColumn(columnName))
            {
                throw new InvalidOperationException($"Column '{columnName}' already exists in table '{TableName}'.");
            }

            IColumnMetadata columnMetadata = _dataTableSchema.CreateColumn(columnName, dataType, isNullable, isPrimaryKey, isForeignKey);
            AddColumn(columnMetadata);

            return columnMetadata;
        }

        /// <summary>
        /// Adds column
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="columnMetadata">Column metadata</param>
        public void AddColumn(IColumnMetadata columnMetadata)
        {
            if (_dataTableSchema.ContainsColumn(columnMetadata.ColumnName))
            {
                throw new InvalidOperationException($"Column '{columnMetadata.ColumnName}' already exists in table '{TableName}'.");
            }

            EnsureExistingRowsHaveColumn(columnMetadata.ColumnName);
            _dataTableSchema.AddColumn(columnMetadata);
        }

        /// <summary>
        /// Adds columns
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        public void AddColumns(IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                AddColumn(listOfColumnMetadata[i]);
            }
        }

        /// <summary>
        /// Adds loaded row from data storage
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="dataRow">Data row</param>
        /// <exception cref="InvalidOperationException">Exception is thrown if row in Deleted state</exception>
        public void AddLoadedRow(IDataRow dataRow)
        {
            if (dataRow == null)
            {
                return;
            }

            if (dataRow.DataRowState == DataRowState.Deleted)
            {
                throw new InvalidOperationException("Cannot add a Deleted row to a table.");
            }

            // Ensure the row contains all columns (non-floating rows only)
            if (!(dataRow is IFloatingDataRow))
            {
                for (int i = 0; i < _dataTableSchema.Items.Count; i++)
                {
                    string columnName = _dataTableSchema.Items[i].ColumnName;
                    if (!dataRow.ContainsKey(columnName))
                    {
                        // Detached + indexer assignment is safe: it does not snapshot originals.
                        dataRow[columnName] = null;
                    }
                }
            }

            // Mark row metadata: loaded rows have immutable PK
            DataRow? r = dataRow as DataRow;
            if (r != null)
            {
                r.IsLoadedRow = true;
                r.SetPrimaryKeyColumns(_dataTableSchema.PrimaryKeyColumnNames);
            }

            // Physically attach row WITHOUT calling AddRow (AddRow is for client-added rows)
            _rows.Add(dataRow);

            // Normalize to baseline
            if (dataRow.DataRowState == DataRowState.Detached || dataRow.DataRowState == DataRowState.Added)
            {
                dataRow.SetDataRowState(DataRowState.Unchanged);
            }
        }

        /// <summary>
        /// Adds row
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="dataRow">Data row</param>
        public void AddRow(IDataRow dataRow)
        {
            if (dataRow == null)
            {
                return;
            }

            if (dataRow.DataRowState == DataRowState.Deleted)
            {
                throw new InvalidOperationException("Cannot add a Deleted row to a table.");
            }

            if (ContainsRow(dataRow))
            {
                throw new InvalidOperationException("Row already exists in this table.");
            }

            // Ensure all columns exist in the row (non-floating rows only).
            if (!(dataRow is IFloatingDataRow))
            {
                for (int i = 0; i < _dataTableSchema.Items.Count; i++)
                {
                    string columnName = _dataTableSchema.Items[i].ColumnName;
                    if (!dataRow.ContainsKey(columnName))
                    {
                        // This uses your indexer; for Detached rows this won't snapshot originals.
                        dataRow[columnName] = null;
                    }
                }
            }

            if (dataRow.DataRowState == DataRowState.Detached)
            {
                dataRow.SetDataRowState(DataRowState.Added);
            }

            DataRow? r = dataRow as DataRow;
            if (r != null)
            {
                r.IsLoadedRow = false;
                r.SetPrimaryKeyColumns(_dataTableSchema.PrimaryKeyColumnNames);
            }

            _rows.Add(dataRow);
        }

        /// <summary>
        /// Clears table primary keys.
        /// IDataTable interface implementation
        /// </summary>
        internal void ClearPrimaryKeys()
        {
            _dataTableSchema.ClearPrimaryKeyFlags();
        }

        /// <summary>
        /// Gets flag indicating whether data table contains column with specified name
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Flag indicating whether data table contains column with specified name</returns>
        public bool ContainsColumn(string columnName)
        {
            return _dataTableSchema.ContainsColumn(columnName);
        }

        /// <summary>
        /// Gets flag indicating whether table contains specified row
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>True if table contains specified row, otherwise false</returns>
        public bool ContainsRow(IDataRow dataRow)
        {
            return _rows.Contains(dataRow);
        }

        /// <summary>
        /// Ensures  that ClientKey column exists
        /// IDataTable interface implementation
        /// </summary>
        public void EnsureClientKeyColumnExists()
        {
            _dataTableSchema.EnsureClientKeyColumnExists();
        }

        /// <summary>
        /// Removes all rows
        /// IDataTable interface implementation
        /// </summary>
        public void RemoveAllRows()
        {
            _rows.Clear();
        }

        /// <summary>
        /// Removes row
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>True if row was removed, otherwise false</returns>
        public bool RemoveRow(IDataRow dataRow)
        {
            if (dataRow == null)
            {
                return false;
            }

            if (!ContainsRow(dataRow))
            {
                throw new InvalidOperationException("Row does not belong to this table.");
            }

            return _rows.Remove(dataRow);
        }

        /// <summary>
        /// Removes row at position specified by row index
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public void RemoveRowAt(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _rows.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(rowIndex));
            }

            _rows.RemoveAt(rowIndex);
        }

        /// <summary>
        /// Sets primary key column names for the table.
        /// </summary>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        internal void SetPrimaryKeys(IList<string> primaryKeyColumnNames)
        {
            _dataTableSchema.SetPrimaryKeysByName(primaryKeyColumnNames);
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
            if (_dataTableSchema.TryGetColumn(columnName, out IColumnMetadata? foundColumnMetadata))
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

        #region Private Methods
        /// <summary>
        /// Ensures that existing rows have column
        /// </summary>
        /// <param name="columnName">column name</param>

        void ApplyPendingPrimaryKeysIfAny()
        {
            if (_dataTableSchema.Items.Count == 0)
            {
                return;
            }

            if (_pendingPrimaryKeysJson == null)
            {
                return;
            }

            _dataTableSchema.SetPrimaryKeysByName(_pendingPrimaryKeysJson);
            _pendingPrimaryKeysJson = null;
        }

        /// <summary>
        /// Ensures that all existing rows have all current schema columns.
        /// This is important during deserialization where Rows and Columns can be set in either order.
        /// </summary>
        void EnsureExistingRowsHaveAllColumns()
        {
            for (int c = 0; c < _dataTableSchema.Items.Count; c++)
            {
                string columnName = _dataTableSchema.Items[c].ColumnName;
                EnsureExistingRowsHaveColumn(columnName);
            }
        }

        void EnsureExistingRowsHaveColumn(string columnName)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                IDataRow row = Rows[i];

                // Floating (sparse) rows must preserve missing keys.
                if (row is PocoDataSet.IData.IFloatingDataRow)
                {
                    continue;
                }

                // Only safe for our concrete DataRow: we can write into ValuesJson without changing row state.
                PocoDataSet.Data.DataRow? concreteRow = row as PocoDataSet.Data.DataRow;
                if (concreteRow == null)
                {
                    continue;
                }

                if (!concreteRow.ValuesJson.ContainsKey(columnName))
                {
                    concreteRow.ValuesJson.Add(columnName, null);
                }

                // Keep OriginalValues aligned if the row is already tracking baseline.
                if (concreteRow.HasOriginalValues)
                {
                    Dictionary<string, object?> originalValues = concreteRow.OriginalValuesJson;
                    if (!originalValues.ContainsKey(columnName))
                    {
                        originalValues.Add(columnName, null);
                    }
                }
            }
        }
        #endregion
    }
}