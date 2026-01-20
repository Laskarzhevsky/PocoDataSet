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
        /// Rejects changes for an observable data table in an observable-aware way.
        /// Added (and Detached) rows are removed from the observable table (raising RowsRemoved).
        /// Modified and Deleted rows are reverted and RowStateChanged is raised.
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        public static void RejectChanges(this IObservableDataTable? observableDataTable)
        {
            if (observableDataTable == null)
            {
                return;
            }

            // Walk backwards so row removals are safe.
            for (int i = observableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IObservableDataRow observableRow = observableDataTable.Rows[i];

                switch (observableRow.DataRowState)
                {
                    case DataRowState.Added:
                    case DataRowState.Detached:
                        observableDataTable.RemoveRowAt(i);
                        break;

                    case DataRowState.Modified:
                    case DataRowState.Deleted:
                        observableRow.RejectChanges();
                        break;

                    case DataRowState.Unchanged:
                    default:
                        // Unchanged -> nothing to do
                        break;
                }
            }
        }
        #endregion
    }
}
