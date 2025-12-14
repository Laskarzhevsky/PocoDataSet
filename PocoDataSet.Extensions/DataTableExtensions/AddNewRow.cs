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
        /// Creates new data row in data table with default values taken from columns metadata
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>New data row created in data table</returns>
        public static IDataRow AddNewRow(this IDataTable? dataTable)
        {
            if (dataTable == null)
            {
                throw new System.ArgumentNullException(nameof(dataTable));
            }

            IDataRow dataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);
            dataTable.Rows.Add(dataRow);

            return dataRow;
        }
        #endregion
    }
}
