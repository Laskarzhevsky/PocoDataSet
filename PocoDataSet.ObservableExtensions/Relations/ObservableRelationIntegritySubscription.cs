using System;
using PocoDataSet.Extensions.Relations;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Represents an active relation integrity subscription for an observable data set.
    /// </summary>
    public class ObservableRelationIntegritySubscription : IDisposable
    {
        #region Data Fields
        /// <summary>
        /// Holds reference to ObservableRelationIntegrityEnforcer instance
        /// </summary>
        private readonly ObservableRelationIntegrityEnforcer _oObservableRelationIntegrityEnforcer;
        #endregion

        #region Events
        /// <summary>
        /// Raised when relation validation finds violations during editing.
        /// </summary>
        public event EventHandler<ObservableRelationValidationFailedEventArgs>? RelationValidationFailed;
        #endregion

        #region Constructors
        /// <summary>
        /// Default construcor
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="relationValidationOptions">Relation validation options</param>
        internal ObservableRelationIntegritySubscription(IObservableDataSet observableDataSet, RelationValidationOptions relationValidationOptions)
        {
            _oObservableRelationIntegrityEnforcer = new ObservableRelationIntegrityEnforcer(observableDataSet, relationValidationOptions);
            _oObservableRelationIntegrityEnforcer.RelationValidationFailed += ObservableRelationIntegrityEnforcer_RelationValidationFailed;
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Handles ObservableRelationIntegrityEnforcer.RelationValidationFailed event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event arguments</param>
        private void ObservableRelationIntegrityEnforcer_RelationValidationFailed(object? sender, ObservableRelationValidationFailedEventArgs e)
        {
            EventHandler<ObservableRelationValidationFailedEventArgs>? handler = RelationValidationFailed;
            if (handler == null)
            {
                return;
            }

            handler(this, e);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Disposes ObservableRelationIntegritySubscription instance
        /// </summary>
        public void Dispose()
        {
            _oObservableRelationIntegrityEnforcer.RelationValidationFailed -= ObservableRelationIntegrityEnforcer_RelationValidationFailed;
            _oObservableRelationIntegrityEnforcer.Dispose();
        }
        #endregion
    }
}
