using System;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Provides DataFieldValueChangedEventArgs functionality
    /// </summary>
    public class DataFieldValueChangedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Backward-compatible constructor.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="requestor">Object which requests update</param>
        public DataFieldValueChangedEventArgs(string columnName, object? requestor)
        {
            ColumnName = columnName;
            Requestor = requestor;
        }

        /// <summary>
        /// Detailed constructor.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="oldValue">Previous value</param>
        /// <param name="newValue">New value</param>
        /// <param name="requestor">Object which requests update</param>
        /// <param name="tableName">Optional table name (typically populated when forwarded by table/dataset)</param>
        public DataFieldValueChangedEventArgs(string columnName, object? oldValue, object? newValue, object? requestor, string? tableName)
        {
            ColumnName = columnName;
            OldValue = oldValue;
            NewValue = newValue;
            Requestor = requestor;
            TableName = tableName;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets column name
        /// </summary>
        public string ColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets object which requests update
        /// </summary>
        public object? Requestor
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets table name (populated by table/dataset forwarding handlers).
        /// </summary>
        public string? TableName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets previous value.
        /// </summary>
        public object? OldValue
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets new value.
        /// </summary>
        public object? NewValue
        {
            get; set;
        }
        #endregion
    }
}