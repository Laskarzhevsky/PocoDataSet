using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides DataRow selectors
    /// </summary>
    public partial class DataRowSelectors
    {
        #region Public Methods
        /// <summary>
        /// Selects all data rows
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>Flag indicating that passed data row needs to be included into the selection result</returns>
        public static bool SelectAllRows(IDataRow dataRow)
        {
            return true;
        }
        #endregion
    }
}
