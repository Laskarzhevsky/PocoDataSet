namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines row filter functionality
    /// </summary>
    public interface IDataRowFilter
    {
        #region Public Methods
        /// <summary>
        /// Gets flag indicating whether filter includes data row for selection
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>Flag indicating whether filter includes data row for selection</returns>
        bool Includes(IDataRow dataRow);
        #endregion
    }
}
