using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides observable data row fuctionality
    /// </summary>
    public class ObservableDataRow : IObservableDataRow
    {
        #region Events
        /// <summary>
        /// Cell changed notification
        /// IObservableDataRow interface implementation
        /// </summary>
        public event EventHandler<DataFieldValueChangedEventArgs>? DataFieldValueChanged;

        /// <summary>
        /// Row state changed notification
        /// IObservableDataRow interface implementation
        /// </summary>
        public event EventHandler<RowStateChangedEventArgs>? RowStateChanged;
        #endregion

        #region DataFields
        /// <summary>
        /// Holds reference to IDataRow
        /// </summary>
        readonly IDataRow _innerDataRow;
        #endregion

        #region Constructors
        /// <summary>
        /// Default construcot
        /// </summary>
        /// <param name="dataRow">Data row</param>
        public ObservableDataRow(IDataRow dataRow, string key = "")
        {
            if (dataRow == null)
            {
                throw new ArgumentNullException(nameof(dataRow));
            }

            _innerDataRow = dataRow;
            Key = key;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Accepts changes on the inner data row and raises RowStateChanged if the row state changes.
        /// Note: Deleted rows should typically be handled at table level because accepting a deletion may remove the row from a table.
        /// IObservableDataRow interface implementation
        /// </summary>
        /// <param name="requestor">Object which requests update</param>
        public void AcceptChanges(object? requestor = null)
        {
            DataRowState oldState = _innerDataRow.DataRowState;
            if (oldState == DataRowState.Deleted)
            {
                throw new System.InvalidOperationException("Deleted rows should be accepted at table level. Use IObservableDataTable.AcceptChanges() instead.");
            }

            _innerDataRow.AcceptChanges();

            DataRowState newState = _innerDataRow.DataRowState;
            RaiseRowStateChangedEvent(oldState, newState, requestor);
        }

        /// <summary>
        /// Gets data field value
        /// IObservableDataRow interface implementation
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="columnName">Column name</param>
        /// <returns>Value of specified type</returns>
        public T? GetDataFieldValue<T>(string columnName)
        {
            return _innerDataRow.GetDataFieldValue<T>(columnName);
        }

        /// <summary>
        /// Rejects changes
        /// IObservableDataRow interface implementation
        /// </summary>
        /// <param name="requestor">Method execution requestor</param>
        public void RejectChanges(object? requestor = null)
        {
            DataRowState oldState = _innerDataRow.DataRowState;

            _innerDataRow.RejectChanges();

            DataRowState newState = _innerDataRow.DataRowState;

            // If state changed (e.g., Modified -> Unchanged), raise observable event
            RaiseRowStateChangedEvent(oldState, newState, requestor);
        }

        /// <summary>
        /// Updates data field value
        /// IObservableDataRow interface implementation
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value for data field</param>
        /// <param name="requestor">Object which requests update</param>
        /// <returns>Flag indicating whether value was set</returns>
        [Obsolete("Use the indexer (observableDataRow[columnName]) which raises observable events.", false)]

        public bool UpdateDataFieldValue(string columnName, object? value, object? requestor)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Column name must be provided.", nameof(columnName));
            }

            // If the value is not changing, do nothing.
            // NOTE: Previously, this method only compared against the *original* value, which could
            // raise duplicate events if the same value was assigned repeatedly.
            object? currentValue = _innerDataRow[columnName];
            if (object.Equals(currentValue, value))
            {
                return false;
            }

            DataRowState oldState = _innerDataRow.DataRowState;
            _innerDataRow[columnName] = value;

            bool dataFieldValueUpdated = false;
            object ? originalValue;
            _innerDataRow.TryGetOriginalValue(columnName, out originalValue);
            if (!object.Equals(originalValue, value))
            {
                dataFieldValueUpdated = true;
            }

            if (dataFieldValueUpdated)
            {
                RaiseDataFieldValueChangedEvent(columnName, requestor);
                DataRowState newState = _innerDataRow.DataRowState;
                RaiseRowStateChangedEvent(oldState, newState, requestor);
            }

            return dataFieldValueUpdated;
        }

        /// <summary>
        /// Updates Selected data field value
        /// IObservableDataRow interface implementation
        /// </summary>
        /// <param name="value">Value for setting into data row</param>
        /// <param name="requestor">Object which requests update</param>
        /// <returns>Flag indicating whether value was set</returns>
        public bool UpdateSelectedDataFieldValue(object? value, object? requestor)
        {
            return UpdateDataFieldValue(ColumnNames.SELECTED, value, requestor);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets data row state
        /// IObservableDataRow interface implementation
        /// </summary>
        public DataRowState DataRowState
        {
            get
            {
                return _innerDataRow.DataRowState;
            }
        }

        /// <summary>
        /// Gets inner data row
        /// IObservableDataRow interface implementation
        /// </summary>
        public IDataRow InnerDataRow
        {
            get
            {
                return _innerDataRow;
            }
        }



        /// <summary>
        /// Gets or sets a data field value by column name.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Data field value</returns>
        public object? this[string columnName]
        {
            get
            {
                return _innerDataRow[columnName];
            }
            set
            {
                UpdateDataFieldValue(columnName, value, null);
            }
        }

        /// <summary>
        /// Gets or sets key
        /// IObservableDataRow interface implementation
        /// </summary>
        public string Key
        {
            get; set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Raises DataFieldValueChanged event
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="requestor">Object which requests update</param>
        void RaiseDataFieldValueChangedEvent(string columnName, object? requestor)
        {
            if (DataFieldValueChanged != null)
            {
                DataFieldValueChanged(this, new DataFieldValueChangedEventArgs(columnName, requestor));
            }
        }

        /// <summary>
        /// Raises RowStateChangedEventIfNeeded event
        /// </summary>
        /// <param name="oldState">Old state</param>
        /// <param name="newState">New state</param>
        /// <param name="requestor">Object which requests update</param>
        void RaiseRowStateChangedEvent(DataRowState oldState, DataRowState newState, object? requestor)
        {
            if (oldState == newState)
            {
                return;
            }

            if (RowStateChanged != null)
            {
                RowStateChanged(this, new RowStateChangedEventArgs(oldState, newState, requestor));
            }
        }
        #endregion
    }
}
