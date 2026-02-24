using System;
using System.ComponentModel;

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
        /// Standard .NET property change notification (interop for WPF/WinUI/MAUI, etc.).
        /// For column updates performed through the indexer, ObservableDataRow raises:
        /// - PropertyChanged(columnName)
        /// - PropertyChanged("Item[]") to signal indexer updates.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

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

        int _rowStateChangedSuppressionCount;
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
        /// Attempts to retrieve the original value of the specified column before any modifications were made.
        /// IObservableDataRow interface implementation
        /// </summary>
        /// <remarks>This method is useful for tracking changes to data, allowing comparison between the
        /// current and original values.</remarks>
        /// <param name="columnName">The name of the column for which to retrieve the original value. This parameter cannot be null or empty.</param>
        /// <param name="originalValue">When this method returns <see langword="true"/>, contains the original value of the specified column;
        /// otherwise, is set to <see langword="null"/>.</param>
        /// <returns>True if the original value was successfully retrieved, otherwise false</returns>
        public bool TryGetOriginalValue(string columnName, out object? originalValue)
        {
            return InnerDataRow.TryGetValue(columnName, out originalValue);
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

            object? oldValue = _innerDataRow[columnName];

            // Raise only on real change.
            if (ValueComparer.AreValuesEqual(oldValue, value))
            {
                return false;
            }

            DataRowState oldState = _innerDataRow.DataRowState;

            _innerDataRow[columnName] = value;

            object? newValue = value;

            RaiseDataFieldValueChangedEvent(columnName, oldValue, newValue, requestor);

            // Interop: notify that the "property" named by the column key has changed.
            // Also notify the conventional indexer name used by some binding engines.
            RaisePropertyChanged(columnName);
            RaisePropertyChanged("Item[]");

            DataRowState newState = _innerDataRow.DataRowState;
            RaiseRowStateChangedEvent(oldState, newState, requestor);

            return true;
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

        /// <summary>
        /// Gets or sets flag indicating whether data row is selected
        /// IObservableDataRow interface implementation
        /// </summary>
        public bool Selected
        {
            get
            {
                return _innerDataRow.Selected;
            }
            set
            {
                bool oldValue = _innerDataRow.Selected;
                if (oldValue == value)
                {
                    return;
                }

                _innerDataRow.Selected = value;
                RaisePropertyChanged(nameof(Selected));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Raises DataFieldValueChanged event
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <param name="requestor">Object which requests update</param>
        void RaiseDataFieldValueChangedEvent(string columnName, object? oldValue, object? newValue, object? requestor)
        {
            if (DataFieldValueChanged != null)
            {
                DataFieldValueChanged(this, new DataFieldValueChangedEventArgs(columnName, oldValue, newValue, requestor, null));
            }
        }

        /// <summary>
        /// Raises PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// INTERNAL USE ONLY.
        /// Suppresses RowStateChanged notifications for the lifetime of the returned scope.
        /// This is used by merge operations (e.g., Refresh mode) where values may be updated
        /// and then committed back to Unchanged state, and intermediate state flips are not
        /// semantically meaningful to observers.
        /// </summary>
        /// <returns>Disposable scope that re-enables RowStateChanged when disposed</returns>
        internal System.IDisposable SuppressRowStateChanged()
        {
            _rowStateChangedSuppressionCount++;
            return new RowStateChangedSuppressionScope(this);
        }

        sealed class RowStateChangedSuppressionScope : System.IDisposable
        {
            readonly ObservableDataRow _owner;

            public RowStateChangedSuppressionScope(ObservableDataRow owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner._rowStateChangedSuppressionCount > 0)
                {
                    _owner._rowStateChangedSuppressionCount--;
                }
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

            
            if (_rowStateChangedSuppressionCount > 0)
            {
                return;
            }

            if (RowStateChanged != null)
            {
                RowStateChanged(this, new RowStateChangedEventArgs(oldState, newState, requestor));
            }

            // Interop: notify standard property change listeners.
            RaisePropertyChanged(nameof(DataRowState));
        }
        #endregion
    }
}
