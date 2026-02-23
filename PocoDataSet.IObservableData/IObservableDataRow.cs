using System;
using System.ComponentModel;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable DataRow functionality
    /// </summary>
    public interface IObservableDataRow : INotifyPropertyChanged
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
        /// Accepts changes on the inner data row and raises RowStateChanged if the row state changes.
        /// Note: Deleted rows should typically be handled at table level because accepting a deletion may remove the row from a table.
        /// </summary>
        /// <param name="requestor">Object which requests update</param>
        void AcceptChanges(object? requestor = null);

        /// <summary>
        /// Gets data field value
        /// </summary>
        /// <typeparam name="T"><Value type/typeparam>
        /// <param name="columnName">Column name</param>
        /// <returns>Data field value</returns>
        T? GetDataFieldValue<T>(string columnName);

        /// <summary>
        /// Rejects changes
        /// </summary>
        /// <param name="requestor">Method execution requestor</param>
        void RejectChanges(object? requestor = null);

        /// <summary>
        /// Attempts to retrieve the original value of the specified column before any modifications were made.
        /// </summary>
        /// <remarks>This method is useful for tracking changes to data, allowing comparison between the
        /// current and original values.</remarks>
        /// <param name="columnName">The name of the column for which to retrieve the original value. This parameter cannot be null or empty.</param>
        /// <param name="originalValue">When this method returns <see langword="true"/>, contains the original value of the specified column;
        /// otherwise, is set to <see langword="null"/>.</param>
        /// <returns>True if the original value was successfully retrieved, otherwise false</returns>
        bool TryGetOriginalValue(string columnName, out object? originalValue);

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

        /// <summary>
        /// Gets or sets flag indicating whether data row is selected
        /// </summary>
        bool Selected
        {
            get; set;
        }
        #endregion
    }
}
