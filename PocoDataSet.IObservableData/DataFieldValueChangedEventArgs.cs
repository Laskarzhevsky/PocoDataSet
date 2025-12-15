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
        /// Default constructor
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="requestor">Object which requests update</param>
        public DataFieldValueChangedEventArgs(string columnName, object? requestor)
        {
            ColumnName = columnName;
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
        #endregion
    }
}
