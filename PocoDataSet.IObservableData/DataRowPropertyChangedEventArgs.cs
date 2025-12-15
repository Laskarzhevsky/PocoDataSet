using System.ComponentModel;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Provides DataRowPropertyChangedEventArgs functionality
    /// </summary>
    public class DataRowPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="table">Data table</param>
        /// <param name="row">Data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        public DataRowPropertyChangedEventArgs(IDataTable table, IDataRow row, string columnName, object? oldValue, object? newValue) : base(columnName)
        {
            Table = table;
            Row = row;
            OldValue = oldValue;
            NewValue = newValue;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets new value
        /// </summary>
        public object? NewValue
        {
            get;
        }

        /// <summary>
        /// Gets old value
        /// </summary>
        public object? OldValue
        {
            get;
        }

        /// <summary>
        /// Gets row
        /// </summary>
        public IDataRow Row
        {
            get;
        }

        /// <summary>
        /// Gets table
        /// </summary>
        public IDataTable Table
        {
            get;
        }
        #endregion
    }
}
