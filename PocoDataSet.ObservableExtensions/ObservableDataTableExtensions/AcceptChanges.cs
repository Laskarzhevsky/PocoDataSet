using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data table extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Accepts changes for observable table (delegates to inner table)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        public static void AcceptChanges(this IObservableDataTable? observableDataTable)
        {
            if (observableDataTable == null)
            {
                return;
            }

            for (int i = observableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IObservableDataRow observableDataRow = observableDataTable.Rows[i];

                switch (observableDataRow.DataRowState)
                {
                    case DataRowState.Deleted:
                        // Commit deletion: row must disappear
                        observableDataTable.RemoveRowAt(i);
                        break;

                    case DataRowState.Added:
                    case DataRowState.Modified:
                        observableDataRow.AcceptChanges();
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
