using System;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable DataRow functionality
    /// </summary>
    public interface IObservableDataRow
    {
        #region Events
        /// <summary>
        /// Cell changed notification
        /// </summary>
        event EventHandler<DataFieldValueChangedEventArgs>? DataFieldValueChanged;

        /// <summary>
        /// Row state changed notification
        /// </summary>
        event EventHandler<RowStateChangedEventArgs>? RowStateChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Gets data field value
        /// </summary>
        /// <typeparam name="T"><Value type/typeparam>
        /// <param name="columnName">Column name</param>
        /// <returns>Data field value</returns>
        T? GetDataFieldValue<T>(string columnName);

        /// <summary>
        /// Updates data field value
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value for setting into data row</param>
        /// <param name="requestor">Object which requests update</param>
        /// <returns>Flag indicating whether value was set</returns>
        [Obsolete("Use the indexer (observableDataRow[columnName]) which raises observable events.", false)]

        bool UpdateDataFieldValue(string columnName, object? value, object? requestor);

        /// <summary>
        /// Updates Selected data field value
        /// </summary>
        /// <param name="value">Value for setting into data row</param>
        /// <param name="requestor">Object which requests update</param>
        /// <returns>Flag indicating whether value was set</returns>
        bool UpdateSelectedDataFieldValue(object? value, object? requestor);
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets data row state
        /// </summary>
        public DataRowState DataRowState
        {
            get;
        }

        /// <summary>
        /// Gets inner data row
        /// </summary>
        IDataRow InnerDataRow
        {
            get;
        }


        /// <summary>
        /// Gets or sets a data field value by column name.
        /// The setter must raise observable events.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Data field value</returns>
        object? this[string columnName]
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets key
        /// </summary>
        string Key
        {
            get; set;
        }
        #endregion
    }
}
