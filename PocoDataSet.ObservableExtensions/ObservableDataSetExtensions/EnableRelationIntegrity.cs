using PocoDataSet.Extensions.Relations;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Enables relation integrity validation during editing for an observable data set.
        /// Subscribe to subscription.RelationValidationFailed to receive violations.
        /// Dispose the returned subscription to detach handlers and prevent leaks.
        /// </summary>
        /// <param name="observableDataSet">Observable data set.</param>
        /// <param name="relationValidationOptions">Relation validation options (null uses defaults).</param>
        /// <returns>Subscription (IDisposable) that must be disposed when no longer needed.</returns>
        public static ObservableRelationIntegritySubscription EnableRelationIntegrity(this IObservableDataSet? observableDataSet, RelationValidationOptions? relationValidationOptions)
        {
            if (observableDataSet == null)
            {
                return default!;
            }

            RelationValidationOptions effectiveOptions;

            if (relationValidationOptions == null)
            {
                effectiveOptions = new RelationValidationOptions();
            }
            else
            {
                effectiveOptions = relationValidationOptions;
            }

            return new ObservableRelationIntegritySubscription(observableDataSet, effectiveOptions);
        }
        #endregion
    }
}
