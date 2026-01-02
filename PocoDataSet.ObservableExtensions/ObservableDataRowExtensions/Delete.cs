using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Deletes observable data row
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        public static void Delete(this IObservableDataRow? observableDataRow)
        {
            if (observableDataRow == null)
            {
                return;
            }

            ObservableDataRow? concreteRow = observableDataRow as ObservableDataRow;
            if (concreteRow != null)
            {
                concreteRow.Delete();
                return;
            }

            observableDataRow.InnerDataRow.Delete();
        }
        #endregion
    }
}
