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
        /// Gets or sets primary key
        /// IDataTable interface implementation
        /// </summary>
        public List<string> PrimaryKeys
        {
            get; set;
        } = new();

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

        #region Methods
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

            // Ensure the row contains all columns
            for (int i = 0; i < Columns.Count; i++)
            {
                string columnName = Columns[i].ColumnName;
                if (!dataRow.ContainsKey(columnName))
                {
                    // Detached + indexer assignment is safe: it does not snapshot originals.
                    dataRow[columnName] = null;
                }
            }

            // Mark row metadata: loaded rows have immutable PK
            DataRow? r = dataRow as DataRow;
            if (r != null)
            {
                r.IsLoadedRow = true;
                r.SetPrimaryKeyColumns(this.PrimaryKeys);
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

            // Ensure all columns exist in the row.
            for (int i = 0; i < Columns.Count; i++)
            {
                string columnName = Columns[i].ColumnName;
                if (!dataRow.ContainsKey(columnName))
                {
                    // This uses your indexer; for Detached rows this won't snapshot originals.
                    dataRow[columnName] = null;
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
                r.SetPrimaryKeyColumns(this.PrimaryKeys);
            }

            _rows.Add(dataRow);
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
        #endregion
    }
}
