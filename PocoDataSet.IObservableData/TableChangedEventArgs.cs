using System;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Provides TableChanged event arguments functionality
    /// </summary>
    public class TableChangedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tableName">Table name</param>
        public TableChangedEventArgs(string tableName)
        {
            TableName = tableName;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets table name
        /// </summary>
        public string TableName
        {
            get; set;
        }
        #endregion
    }
}
