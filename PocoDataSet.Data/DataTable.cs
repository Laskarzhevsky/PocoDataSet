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
        readonly DataTableSchema _dataTableSchema = new ();

        /// <summary>
        /// "Rows" property data field
        /// </summary>
        readonly List<IDataRow> _rows = new();
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets columns
        /// IDataTable interface implementation
        /// </summary>
        public IReadOnlyList<IColumnMetadata> Columns
        {
            get
            {
                return _dataTableSchema.Items;
            }
        }

        /// <summary>
        /// Gets primary keys
        /// IDataTable interface implementation
        /// </summary>
        public IReadOnlyList<string> PrimaryKeys
        {
            get
            {
                return _dataTableSchema.PrimaryKeys.Items;
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
        public void AddColumns(List<IColumnMetadata> listOfColumnMetadata)
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
                r.SetPrimaryKeyColumns(_dataTableSchema.PrimaryKeys.Items);
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
                r.SetPrimaryKeyColumns(_dataTableSchema.PrimaryKeys.Items);
            }

            _rows.Add(dataRow);
        }

        /// <summary>
        /// Clears table primary keys.
        /// IDataTable interface implementation
        /// </summary>
        public void ClearPrimaryKeys()
        {
            _dataTableSchema.PrimaryKeys.ClearPrimaryKeys(_dataTableSchema.Items);
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
        /// IDataTable interface implementation
        /// </summary>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        public void SetPrimaryKeys(IList<string> primaryKeyColumnNames)
        {
            _dataTableSchema.PrimaryKeys.SetPrimaryKeys(Columns, primaryKeyColumnNames, TableName);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Ensures that existing rows have column
        /// </summary>
        /// <param name="columnName">column name</param>
        void EnsureExistingRowsHaveColumn(string columnName)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                IDataRow row = Rows[i];

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
