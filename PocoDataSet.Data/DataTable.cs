using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        /// "PrimaryKeys" property data field
        /// </summary>
        readonly List<string> _primaryKeys = new();

        /// <summary>
        /// "Rows" property data field
        /// </summary>
        readonly List<IDataRow> _rows = new();
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets columns
        /// IDataTable interface implementation
        /// </summary>
        public List<IColumnMetadata> Columns
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets primary keys
        /// IDataTable interface implementation
        /// </summary>
        public IReadOnlyList<string> PrimaryKeys
        {
            get
            {
                return _primaryKeys;
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
        public List<IDataRow> RowsJson
        {
            get
            {
                return _rows;
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
            }
        }

        /// <summary>
        /// Gets or sets table name
        /// IDataTable interface implementation
        /// </summary>
        public string TableName
        {
            get; set;
        } = string.Empty;
        #endregion

        #region Public Methods
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
                for (int i = 0; i < Columns.Count; i++)
                {
                    string columnName = Columns[i].ColumnName;
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
                r.SetPrimaryKeyColumns(_primaryKeys);
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
        /// Adds a primary key column name to the table.
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="columnName">Column name</param>
        public void AddPrimaryKey(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Primary key column name cannot be empty.", nameof(columnName));
            }

            EnsureColumnExists(columnName);

            for (int i = 0; i < PrimaryKeys.Count; i++)
            {
                if (string.Equals(PrimaryKeys[i], columnName, StringComparison.OrdinalIgnoreCase))
                {
                    MarkColumnAsPrimaryKey(columnName);
                    return;
                }
            }

            _primaryKeys.Add(columnName);
            MarkColumnAsPrimaryKey(columnName);
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

            // Ensure all columns exist in the row (non-floating rows only).
            if (!(dataRow is IFloatingDataRow))
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    string columnName = Columns[i].ColumnName;
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
                r.SetPrimaryKeyColumns(_primaryKeys);
            }

            _rows.Add(dataRow);
        }

        /// <summary>
        /// Clears table primary keys.
        /// IDataTable interface implementation
        /// </summary>
        public void ClearPrimaryKeys()
        {
            _primaryKeys.Clear();

            if (Columns != null)
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    IColumnMetadata columnMetadata = Columns[i];
                    if (columnMetadata != null)
                    {
                        columnMetadata.IsPrimaryKey = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets flag indicating whether table contains specified row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>True if table contains specified row, otherwise false</returns>
        public bool ContainsRow(IDataRow dataRow)
        {
            return _rows.Contains(dataRow);
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
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            _rows.RemoveAt(rowIndex);
        }

        /// <summary>
        /// Sets primary key column names for the table.
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        public void SetPrimaryKeys(IList<string> primaryKeyColumnNames)
        {
            _primaryKeys.Clear();

            if (Columns != null)
            {
                for (int i = 0; i < Columns.Count; i++)
                {
                    IColumnMetadata columnMetadata = Columns[i];
                    if (columnMetadata != null)
                    {
                        columnMetadata.IsPrimaryKey = false;
                    }
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

        #region Private Methods
        /// <summary>
        /// Ensures that column exists
        /// </summary>
        /// <param name="columnName">Column name</param>
        private void EnsureColumnExists(string columnName)
        {
            if (Columns == null)
            {
                throw new InvalidOperationException("Table columns are not initialized.");
            }

            for (int i = 0; i < Columns.Count; i++)
            {
                IColumnMetadata columnMetadata = Columns[i];
                if (columnMetadata != null && string.Equals(columnMetadata.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            throw new InvalidOperationException($"Primary key column '{columnName}' does not exist in table '{TableName}'.");
        }

        /// <summary>
        /// Marks column as primary key
        /// </summary>
        /// <param name="columnName">Column name</param>
        private void MarkColumnAsPrimaryKey(string columnName)
        {
            if (Columns == null)
            {
                return;
            }

            for (int i = 0; i < Columns.Count; i++)
            {
                IColumnMetadata columnMetadata = Columns[i];
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
