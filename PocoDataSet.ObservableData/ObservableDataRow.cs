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
        /// Updates data field value
        /// IObservableDataRow interface implementation
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value for data field</param>
        /// <param name="requestor">Object which requests update</param>
        /// <returns>Flag indicating whether value was set</returns>
        public bool UpdateDataFieldValue(string columnName, object? value, object? requestor)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Column name must be provided.", nameof(columnName));
            }

            bool dataFieldValueUpdated = _innerDataRow.UpdateDataFieldValue(columnName, value);
            if (dataFieldValueUpdated)
            {
                RaiseDataFieldValueChangedEvent(columnName, requestor);
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
        #endregion
    }
}
