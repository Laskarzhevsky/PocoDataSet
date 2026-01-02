using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Rejects changes for observable data set (delegates to inner data set)
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        public static void RejectChanges(this IObservableDataSet? observableDataSet)
        {
            if (observableDataSet == null)
            {
                return;
            }

            ObservableDataSet? concreteDataSet = observableDataSet as ObservableDataSet;
            if (concreteDataSet != null)
            {
                concreteDataSet.RejectChanges();
                return;
            }

            observableDataSet.InnerDataSet.RejectChanges();
        }
        #endregion
    }
}
