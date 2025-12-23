namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable data set merge result entry functionalty
    /// </summary>
    public class ObservableDataSetMergeResultEntry : IObservableDataSetMergeResultEntry
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="observableDataRow">Observable data row</param>
        public ObservableDataSetMergeResultEntry(string tableName, IObservableDataRow observableDataRow)
        {
            TableName = tableName;
            ObservableDataRow = observableDataRow;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets observable data row
        /// </summary>
        public IObservableDataRow ObservableDataRow
        {
            get;
        }

        /// <summary>
        /// Gets table name
        /// </summary>
        public string TableName
        {
            get;
        }
        #endregion
    }
}
