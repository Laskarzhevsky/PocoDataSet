using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides data row functionality
    /// </summary>
    public class DataRow : Dictionary<string, object?>, IDataRow
    {
        #region Data Fields
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
        public DataRow() : base(StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Efficient pre-sizing for adapters (optional but nice)
        /// </summary>
        /// <param name="capacity"></param>
        internal DataRow(int capacity) : base(capacity, StringComparer.Ordinal) 
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets data row state
        /// IDataRow interface implementation
        /// </summary>
        public DataRowState DataRowState
        {
            get; set;
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

        /// <summary>
        /// Gets read-only view of original values (baseline)
        /// IDataRow interface implementation
        /// </summary>
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
        /// Gets or sets flag indicating whether data row is selected
        /// IDataRow interface implementation
        /// </summary>
        public bool Selected
        {
            get; set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accepts current values as baseline and sets state to Unchanged
        /// IDataRow interface implementation
        /// </summary>
        public void AcceptChanges()
        {
            _originalValues = null;
            DataRowState = DataRowState.Unchanged;
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
            foreach (KeyValuePair<string, object?> pair in this)
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
            if (DataRowState == DataRowState.Deleted)
            {
                return;
            }

            _stateBeforeDelete = DataRowState;
            DataRowState = DataRowState.Deleted;
        }

        /// <summary>
        /// Marks data row state as Modified except the following two cases:
        /// 1. Added remains Added when modified
        /// 2. Deleted stays Deleted unless explicitly Undelete() is called.
        /// </summary>
        public void MarkAsModified()
        {
            if (DataRowState == DataRowState.Unchanged)
            {
                DataRowState = DataRowState.Modified;
                return;
            }
        }

        /// <summary>
        /// Reverts to baseline. If Added, becomes Detached
        /// IDataRow interface implementation
        /// </summary>
        public void RejectChanges()
        {
            if (DataRowState == DataRowState.Added)
            {
                // New row: undo means "row never existed"
                Clear();
                _originalValues = null;
                DataRowState = DataRowState.Detached;
                return;
            }

            if (_originalValues != null)
            {
                Clear();
                foreach (KeyValuePair<string, object?> pair in _originalValues)
                {
                    this[pair.Key] = pair.Value;
                }
            }

            _originalValues = null;
            DataRowState = DataRowState.Unchanged;
        }

        /// <summary>
        /// Reverts deletion to previous state (Unchanged or Modified)
        /// IDataRow interface implementation
        /// </summary>
        public void Undelete()
        {
            if (DataRowState != DataRowState.Deleted)
            {
                _stateBeforeDelete = DataRowState.Unchanged;
            }

            // If it was Added, keep Added; if it was Modified, restore Modified, etc.
            DataRowState = _stateBeforeDelete;
        }
        #endregion
    }
}
