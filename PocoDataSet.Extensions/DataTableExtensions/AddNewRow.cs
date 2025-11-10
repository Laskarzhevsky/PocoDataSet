using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Creates new data row in data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>New data row created in data table</returns>
        public static IDataRow AddNewRow(this IDataTable dataTable)
        {
            IDataRow dataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);
            dataTable.Rows.Add(dataRow);

            return dataRow;
        }
        #endregion
    }
}
