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
        /// Deletes row from table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="dataRow">Data row</param>
        public static void DeleteRow(this IDataTable? dataTable, IDataRow? dataRow)
        {
            if (dataTable == null || dataRow == null)
            {
                return;
            }

            if (!dataTable.ContainsRow(dataRow))
            {
                return;
            }

            switch (dataRow.DataRowState)
            {
                case DataRowState.Added:
                    // Undo creation: row never existed
                    dataTable.RemoveRow(dataRow);
                    break;

                case DataRowState.Unchanged:
                case DataRowState.Modified:
                    // Soft delete: keep row for backend + undelete
                    dataRow.Delete();
                    break;

                case DataRowState.Deleted:
                case DataRowState.Detached:
                default:
                    // Nothing to do
                    break;
            }
        }
        #endregion
    }
}
