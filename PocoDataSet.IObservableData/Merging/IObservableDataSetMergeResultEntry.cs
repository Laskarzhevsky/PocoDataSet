namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable data set merge result entry functionalty
    /// </summary>
    public interface IObservableDataSetMergeResultEntry
    {
        #region Properties
        /// <summary>
        /// Gets observable data row
        /// </summary>
        IObservableDataRow ObservableDataRow
        {
            get;
        }

        /// <summary>
        /// Gets table name
        /// </summary>
        string TableName
        {
            get;
        }
        #endregion
    }
}
