using PocoDataSet.IData;

namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides SelectAll filter functionality
    /// </summary>
    public class SelectAllFilter : IRowFilter
    {
        #region Static properties
        /// <summary>
        /// Gets filter instance
        /// </summary>
        public static SelectAllFilter Instance
        {
            get; set;
        } = new SelectAllFilter();
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        SelectAllFilter()
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Calculates flag indicating that passed data row needs to be included into the selection result
        /// IRowFilter interface implementation
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <returns>Flag indicating that passed data row needs to be included into the selection result</returns>
        public bool Include(IDataRow dataRow)
        {
            return true;
        }
        #endregion
    }
}
