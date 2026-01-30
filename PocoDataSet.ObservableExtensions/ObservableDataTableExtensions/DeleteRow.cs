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
        /// Deletes row from table
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="observableDataRow">Observable data row</param>
        public static void DeleteRow(this IObservableDataTable? observableDataTable, IObservableDataRow? observableDataRow)
        {
            if (observableDataTable == null || observableDataRow == null)
            {
                return;
            }

            if (!observableDataTable.ContainsRow(observableDataRow))
            {
                return;
            }

            switch (observableDataRow.DataRowState)
            {
                case DataRowState.Added:
                    // Undo creation: row never existed
                    observableDataTable.RemoveRow(observableDataRow);
                    break;

                case DataRowState.Unchanged:
                case DataRowState.Modified:
                    // Soft delete: keep row for backend + undelete
                    observableDataRow.Delete();
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
