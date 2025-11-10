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
        /// Adds column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        public static void ClearSelection(this IDataTable? dataTable)
        {
            if (dataTable == null)
            {
                return;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i].Selected = false;
            }
        }
        #endregion
    }
}
