using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides data row functionality
    /// </summary>
    public class DataRow : IDataRow
    {
        #region Data Fields
        /// <summary>
        /// Holds current values
        /// </summary>
        readonly Dictionary<string, object?> _values;

        /// <summary>
        /// "OriginalValues" property data field
        /// </summary>
        Dictionary<string, object?>? _originalValues;

        /// <summary>
        /// Holds empty original values
        /// </summary>
        static readonly IReadOnlyDictionary<string, object?> _emptyOriginalValues = new Dictionary<string, object?>(StringComparer.Ordinal);

        /// <summary>
        /// Holds data row state before its deletion
        /// </summary>
        DataRowState _stateBeforeDelete = DataRowState.Unchanged;
        #endregion

        #region Constructors
        /// <summary>
        /// Block "new DataRow()" outside this assembly
        /// </summary>
        [JsonConstructor]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [System.Obsolete("Use DataRowExtensions.CreateRowFromColumns, DataRowExtensions.CreateRowFromColumnsWithDefaultValues, or DataTableExtension.AddNewRow instead.", false)]
        public DataRow()
        {
            _values = new Dictionary<string, object?>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Efficient pre-sizing for adapters (optional but nice)
        /// </summary>
        /// <param name="capacity"></param>
        internal DataRow(int capacity)
        {
            _values = new Dictionary<string, object?>(capacity, StringComparer.Ordinal);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets data row state
        /// IDataRow interface implementation
        /// </summary>
        [JsonInclude]
        public DataRowState DataRowState
        {
            get; private set;
        } = DataRowState.Detached;

        /// <summary>
        /// Gets flag indicating whether snapshot of original values exists
        /// IDataRow interface implementation
        /// </summary>
        public bool HasOriginalValues
        {
            get
            {
                return _originalValues != null;
            }
        }

        internal bool IsLoadedRow
        {
            get; set;
        }

        /// <summary>
        /// Gets read-only view of original values (baseline)
        /// IDataRow interface implementation
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, object?> OriginalValues
        {
            get
            {
                if (_originalValues == null)
                {
                    return _emptyOriginalValues;
                }

                return _originalValues;
            }
        }

        /// <summary>
        /// Gets original values (baseline) for JSON serialization / deserialization
        /// IDataRow interface implementation
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("OriginalValues")]
        public Dictionary<string, object?> OriginalValuesJson
        {
            get
            {
                return _originalValues ?? new Dictionary<string, object?>(StringComparer.Ordinal);
            }
            private set
            {
                _originalValues = value;
            }
        }

        internal List<string> PrimaryKeyColumns
        {
            get; private set;
        } = new List<string>();

        /// <summary>
        /// Gets or sets flag indicating whether data row is selected
        /// IDataRow interface implementation
        /// </summary>
        public bool Selected
        {
            get; set;
        }

        /// <summary>
        /// Gets row values
        /// IDataRow interface implementation
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, object?> Values
        {
            get
            {
                return _values;
            }
        }

        /// <summary>
        /// JSON-only bridge for values population.
        /// System.Text.Json cannot populate IReadOnlyDictionary, so we expose a writable Dictionary
        /// for serialization/deserialization while keeping the public IDataRow.Values read-only.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("Values")]
        public Dictionary<string, object?> ValuesJson
        {
            get
            {
                return _values;
            }
            private set
            {
                _values.Clear();

                if (value == null)
                {
                    return;
                }

                foreach (KeyValuePair<string, object?> pair in value)
                {
                    _values[pair.Key] = pair.Value;
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Accepts current values as baseline and sets state to Unchanged
        /// IDataRow interface implementation
        /// </summary>
        public void AcceptChanges()
        {
            switch (DataRowState)
            {
                case DataRowState.Added:
                case DataRowState.Modified:
                    // Accepting an Add makes the current values the baseline.
                    _originalValues = null;
                    DataRowState = DataRowState.Unchanged;
                    break;
                case DataRowState.Deleted:
                    // Accepting a Delete must remove the row from the table,
                    // which is a table-level responsibility.
                    throw new InvalidOperationException(
                        "AcceptChanges for Deleted rows must be performed at table level, " +
                        "because accepting a deletion removes the row from the table."
                    );

                case DataRowState.Detached:
                case DataRowState.Unchanged:
                default:
                    // Nothing to do
                    break;
            }
        }

        /// <summary>
        /// Gets flag indicating whether column exists
        /// IDataRow interface implementation
        /// </summary>
        public bool ContainsKey(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return false;
            }

            return _values.ContainsKey(columnName);
        }

        /// <summary>
        /// Creates original values snapshot. Do not call this method. This is for internal framework only.
        /// </summary>
        public void CreateOriginalValuesSnapshot()
        {
            if (_originalValues != null)
            {
                return;
            }

            _originalValues = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, object?> pair in _values)
            {
                _originalValues[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Marks row as Deleted but does not remove it from table
        /// IDataRow interface implementation
        /// </summary>
        public void Delete()
        {
            if (DataRowState == DataRowState.Deleted || DataRowState == DataRowState.Detached)
            {
                return;
            }

            if (DataRowState == DataRowState.Added)
            {
                throw new InvalidOperationException(
                    "Delete cannot be called on Added rows. " +
                    "Undoing an add must be performed at table level by removing the row."
                );
            }

            // Capture baseline for undelete / reject
            if (_originalValues == null)
            {
                CreateOriginalValuesSnapshot();
            }

            _stateBeforeDelete = DataRowState;
            DataRowState = DataRowState.Deleted;
        }
/*
        /// <summary>
        /// Returns values enumerator
        /// </summary>
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }
*/
        /// <summary>
        /// Reverts to baseline rows which are in Deleted or Modified state
        /// It does nothing for rows in Detached or Unchanged state
        /// It throws and exception for rows in Added state
        /// IDataRow interface implementation
        /// </summary>
        /// <exception cref="InvalidOperationException">Reject changes cannot be executed on rows in Added state</exception>
        public void RejectChanges()
        {
            if (DataRowState == DataRowState.Added)
            {
                throw new InvalidOperationException(
                    "RejectChanges for Added rows must be performed at table level (IDataTable.RejectChanges), " +
                    "because rejecting requires the row removal from the table."
                );
            }

            if (DataRowState == DataRowState.Detached || DataRowState == DataRowState.Unchanged)
            {
                return;
            }

            if (_originalValues != null)
            {
                _values.Clear();
                foreach (KeyValuePair<string, object?> pair in _originalValues)
                {
                    _values[pair.Key] = pair.Value;
                }
            }

            _originalValues = null;
            DataRowState = DataRowState.Unchanged;
        }

        /// <summary>
        /// Sets row state internally for framework operations (e.g., changesets, merges).
        /// Not intended for business code.
        /// IDataRow interface implementation
        /// </summary>
        /// <param name="dataRowState">Data row state</param>
        public void SetDataRowState(DataRowState dataRowState)
        {
            DataRowState = dataRowState;
        }

        internal void SetPrimaryKeyColumns(List<string> primaryKeys)
        {
            PrimaryKeyColumns.Clear();
            if (primaryKeys == null)
                return;

            for (int i = 0; i < primaryKeys.Count; i++)
            {
                PrimaryKeyColumns.Add(primaryKeys[i]);
            }
        }

        /// <summary>
        /// Tries to get original value by column name
        /// IDataRow interface implementation
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="originalValue">Original value</param>
        /// <returns>True if original value returned, otherwise false</returns>
        public bool TryGetOriginalValue(string columnName, out object? originalValue)
        {
            if (_originalValues == null)
            {
                originalValue = null;
                return false;
            }

            return _originalValues.TryGetValue(columnName, out originalValue);
        }

        /// <summary>
        /// Tries to get value by column name
        /// IDataRow interface implementation
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value if found</param>
        /// <returns>True if value returned, otherwise false</returns>
        public bool TryGetValue(string columnName, out object? value)
        {
            return _values.TryGetValue(columnName, out value);
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets or sets value by column name
        /// IDataRow interface implementation
        /// </summary>
        public object? this[string columnName]
        {
            get
            {
                if (string.IsNullOrEmpty(columnName))
                {
                    return null;
                }

                object? value;
                bool hasValue = _values.TryGetValue(columnName, out value);
                if (!hasValue)
                {
                    return null;
                }

                return value;
            }
            set
            {
                if (string.IsNullOrEmpty(columnName))
                {
                    return;
                }

                // Disallow edits to deleted rows (consistent with UpdateDataFieldValue behavior)
                if (DataRowState == DataRowState.Deleted)
                {
                    return;
                }

                object? oldValue;
                _values.TryGetValue(columnName, out oldValue);

                if (IsLoadedRow)
                {
                    bool isPrimaryKeyColumn = false;
                    for (int i = 0; i < PrimaryKeyColumns.Count; i++)
                    {
                        if (PrimaryKeyColumns[i] == columnName)
                        {
                            isPrimaryKeyColumn = true;
                            break;
                        }
                    }

                    if (isPrimaryKeyColumn)
                    {
                        // Allow PK edits only for Added rows
                        if (DataRowState != DataRowState.Added)
                        {
                            bool isDifferent;

                            if (oldValue == null && value == null)
                            {
                                isDifferent = false;
                            }
                            else if (oldValue == null || value == null)
                            {
                                isDifferent = true;
                            }
                            else
                            {
                                isDifferent = !oldValue.Equals(value);
                            }

                            if (isDifferent)
                            {
                                throw new InvalidOperationException(
                                    "Cannot change primary key column '" + columnName + "' for a loaded row.");
                            }
                        }
                    }
                }

                // If unchanged and this is the first change, capture baseline
                if (DataRowState == DataRowState.Unchanged)
                {
                    if (_originalValues == null)
                    {
                        CreateOriginalValuesSnapshot();
                    }
                }

                // Assign value
                _values[columnName] = value;

                // Mark as modified (except Added remains Added; Deleted stays Deleted)
                MarkAsModified(oldValue, value);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Marks Unchanged rows as Modified
        /// </summary>
        void MarkAsModified(object? oldValue, object? newValue)
        {
            if (Equals(oldValue, newValue))
            {
                return;
            }

            if (DataRowState == DataRowState.Unchanged)
            {
                DataRowState = DataRowState.Modified;
            }
        }
        #endregion
    }
}
