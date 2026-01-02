using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

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

            ObservableDataTable? concreteTable = observableDataTable as ObservableDataTable;
            if (concreteTable != null)
            {
                concreteTable.AcceptChanges();
                return;
            }

            observableDataTable.InnerDataTable.AcceptChanges();
        }
        #endregion
    }
}
