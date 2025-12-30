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
        /// Rejects changes
        /// </summary>
        /// <param name="dataTable">Data table</param>
        public static void RejectChanges(this IDataTable? dataTable)
        {
            if (dataTable == null)
            {
                return;
            }

            for (int i = dataTable.Rows.Count - 1; i >= 0; i--)
            {
                IDataRow row = dataTable.Rows[i];
                switch (row.DataRowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Detached:
                        dataTable.RemoveRowAt(i);
                        break;
                    case DataRowState.Deleted:
                    case DataRowState.Modified:
                        row.RejectChanges();
                        break;
                    default:
                        // Unchanged → nothing to do
                        break;
                }
            }
        }
        #endregion
    }
}
