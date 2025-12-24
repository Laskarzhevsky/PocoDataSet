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
        /// Accepts changes
        /// </summary>
        /// <param name="dataTable">Data table</param>
        public static void AcceptChanges(this IDataTable? dataTable)
        {
            if (dataTable == null)
            {
                return;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i].AcceptChanges();
            }
        }
        #endregion
    }
}
