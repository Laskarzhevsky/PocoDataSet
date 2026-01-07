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
        /// If a caller calls AcceptChanges() on unsaved changes that include Deleted rows then Deleted rows will be removed from table.
        /// The caller must understand and accept the consequences.
        /// </summary>
        /// <param name="dataTable">Data table</param>
        public static void AcceptChanges(this IDataTable? dataTable)
        {
            if (dataTable == null)
            {
                return;
            }

            for (int i = dataTable.Rows.Count - 1; i >= 0; i--)
            {
                IDataRow dataRow = dataTable.Rows[i];

                switch (dataRow.DataRowState)
                {
                    case DataRowState.Deleted:
                        // Commit deletion: row must disappear
                        dataTable.RemoveRowAt(i);
                        break;

                    case DataRowState.Added:
                    case DataRowState.Modified:
                        dataRow.AcceptChanges();
                        break;

                    case DataRowState.Unchanged:
                    case DataRowState.Detached:
                    default:
                        break;
                }
            }
        }
        #endregion
    }
}
