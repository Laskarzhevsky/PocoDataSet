using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Defines row filter functionality
    /// </summary>
    public interface IRowFilter
    {
        #region Methods
        /// <summary>
        /// Calculates flag indicating that passed data row needs to be included into the selection result
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>Flag indicating that passed data row needs to be included into the selection result</returns>
        bool Include(IDataRow dataRow);
        #endregion
    }
}
