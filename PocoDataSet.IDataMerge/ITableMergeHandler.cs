using PocoDataSet.IData;

namespace PocoDataSet.IDataMerge
{
    /// <summary>
    /// Defines table merge handler.
    /// </summary>
    public interface ITableMergeHandler
    {
        /// <summary>
        /// Merges current table with refreshed table.
        /// </summary>
        /// <param name="currentTable">Current table.</param>
        /// <param name="refreshedTable">Refreshed table.</param>
        /// <param name="dataSetContext">Merge context.</param>
        void Merge(IDataTable currentTable, IDataTable refreshedTable, IMergeContext dataSetContext);
    }
}
