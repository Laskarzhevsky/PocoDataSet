using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.Extensions.Relations;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides observable relation integrity enforcer functionaltiy
    /// </summary>
    internal class ObservableRelationIntegrityEnforcer : IDisposable
    {
        #region Data Fields
        /// <summary>
        /// Holds referenct to observable data set
        /// </summary>
        private readonly IObservableDataSet _observableDataSet;

        /// <summary>
        /// Holds referenct to relation validation options
        /// </summary>
        private readonly RelationValidationOptions _relationValidationOptions;
        #endregion

        #region Events
        /// <summary>
        /// RelationValidationFailed event
        /// </summary>
        public event EventHandler<ObservableRelationValidationFailedEventArgs>? RelationValidationFailed;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="observableDataSet">Observable data set.</param>
        /// <param name="relationValidationOptions">Relation validation options</param>
        public ObservableRelationIntegrityEnforcer(IObservableDataSet observableDataSet, RelationValidationOptions relationValidationOptions)
        {
            _observableDataSet = observableDataSet;
            _relationValidationOptions = relationValidationOptions;

            _observableDataSet.DataFieldValueChanged += ObservableDataSet_DataFieldValueChanged;
            _observableDataSet.RowAdded += ObservableDataSet_RowAdded;
            _observableDataSet.RowRemoved += ObservableDataSet_RowRemoved;
            _observableDataSet.TableAdded += ObservableDataSet_TableAdded;
            _observableDataSet.TableRemoved += ObservableDataSet_TableRemoved;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validates the integrity of data relations and raises the RelationValidationFailed event if any violations are detected.
        /// </summary>
        /// <remarks>This method checks for relation integrity violations in the underlying data set. If
        /// violations are found and there are subscribers to the RelationValidationFailed event, the event is raised
        /// with details of the violations. If no violations are found or there are no subscribers, the method returns
        /// without raising the event.</remarks>
        /// <param name="handledEventName">The name of the event being handled, used to identify the context in which relation validation is performed.</param>
        private void ValidateAndRaiseRelationValidationFailedEvent(string handledEventName)
        {
            IReadOnlyList<RelationIntegrityViolation> violations = _observableDataSet.InnerDataSet.ValidateRelations(_relationValidationOptions);

            if (violations == null)
            {
                return;
            }

            if (violations.Count == 0)
            {
                return;
            }

            if (RelationValidationFailed == null)
            {
                return;
            }

            RelationValidationFailed(this, new ObservableRelationValidationFailedEventArgs(handledEventName, violations));
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles ObservableDataSet.DataFieldValueChanged event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event srguments</param>
        private void ObservableDataSet_DataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
        {
            ValidateAndRaiseRelationValidationFailedEvent("DataFieldValueChanged");
        }

        /// <summary>
        /// Handles ObservableDataSet.RowAdded event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event srguments</param>
        private void ObservableDataSet_RowAdded(object? sender, RowsChangedEventArgs e)
        {
            ValidateAndRaiseRelationValidationFailedEvent("RowAdded");
        }

        /// <summary>
        /// Handles ObservableDataSet.RowRemoved event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event srguments</param>
        private void ObservableDataSet_RowRemoved(object? sender, RowsChangedEventArgs e)
        {
            ValidateAndRaiseRelationValidationFailedEvent("RowRemoved");
        }

        /// <summary>
        /// Handles ObservableDataSet.TableAdded event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event srguments</param>
        private void ObservableDataSet_TableAdded(object? sender, TablesChangedEventArgs e)
        {
            ValidateAndRaiseRelationValidationFailedEvent("TableAdded");
        }

        /// <summary>
        /// Handles ObservableDataSet.TableRemoved event
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event srguments</param>
        private void ObservableDataSet_TableRemoved(object? sender, TablesChangedEventArgs e)
        {
            ValidateAndRaiseRelationValidationFailedEvent("TableRemoved");
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Disposes ObservableRelationIntegrityEnforcer instance
        /// </summary>
        public void Dispose()
        {
            _observableDataSet.DataFieldValueChanged -= ObservableDataSet_DataFieldValueChanged;
            _observableDataSet.RowAdded -= ObservableDataSet_RowAdded;
            _observableDataSet.RowRemoved -= ObservableDataSet_RowRemoved;
            _observableDataSet.TableAdded -= ObservableDataSet_TableAdded;
            _observableDataSet.TableRemoved -= ObservableDataSet_TableRemoved;
        }
        #endregion
    }
}
