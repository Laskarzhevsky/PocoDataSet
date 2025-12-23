namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data set merge result entry functionalty
    /// </summary>
    public interface IDataSetMergeResultEntry
    {
        #region Properties
        /// <summary>
        /// Gets data row
        /// </summary>
        IDataRow DataRow
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
