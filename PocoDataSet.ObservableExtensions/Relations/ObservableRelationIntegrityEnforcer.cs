using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.Extensions.Relations;
using PocoDataSet.IData;
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
        /// Guards against re-entrant validation (validation may indirectly trigger more events).
        /// </summary>
        private bool _isValidating;

        /// <summary>
        /// Holds reference to observable data set
        /// </summary>
        private readonly IObservableDataSet _observableDataSet;

        /// <summary>
        /// Cache of column names participating in any relation (parent or child columns).
        /// Used to reduce validation noise.
        /// </summary>
        private readonly HashSet<string> _relationColumnNames;

        /// <summary>
        /// Cache of table names participating in any relation (parent or child).
        /// Used to reduce validation noise.
        /// </summary>
        private readonly HashSet<string> _relationTableNames;

        /// <summary>
        /// Holds relation validation options
        /// </summary>
        private readonly RelationValidationOptions _relationValidationOptions;
        #endregion

        #region Events
        /// <summary>
        /// Raised when relation validation detects any violations during editing.
        /// </summary>
        public event EventHandler<ObservableRelationValidationFailedEventArgs>? RelationValidationFailed;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of ObservableRelationIntegrityEnforcer class
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="relationValidationOptions">Relation validation options</param>
        public ObservableRelationIntegrityEnforcer(IObservableDataSet observableDataSet, RelationValidationOptions relationValidationOptions)
        {
            if (observableDataSet == null)
            {
                throw new ArgumentNullException(nameof(observableDataSet));
            }

            if (relationValidationOptions == null)
            {
                throw new ArgumentNullException(nameof(relationValidationOptions));
            }

            _observableDataSet = observableDataSet;
            _relationValidationOptions = relationValidationOptions;

            _relationTableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _relationColumnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            BuildRelationCaches();

            _observableDataSet.DataFieldValueChanged += ObservableDataSet_DataFieldValueChanged;
            _observableDataSet.RowAdded += ObservableDataSet_RowAdded;
            _observableDataSet.RowRemoved += ObservableDataSet_RowRemoved;
            _observableDataSet.TableAdded += ObservableDataSet_TableAdded;
            _observableDataSet.TableRemoved += ObservableDataSet_TableRemoved;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Builds caches of relation table/column names for noise reduction.
        /// </summary>
        private void BuildRelationCaches()
        {
            IDataSet innerDataSet = _observableDataSet.InnerDataSet;
            if (innerDataSet == null)
            {
                return;
            }

            IReadOnlyList<IDataRelation> relations = innerDataSet.Relations;
            if (relations == null)
            {
                return;
            }

            foreach (IDataRelation relation in relations)
            {
                if (relation == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(relation.ParentTableName))
                {
                    _relationTableNames.Add(relation.ParentTableName);
                }

                if (!string.IsNullOrWhiteSpace(relation.ChildTableName))
                {
                    _relationTableNames.Add(relation.ChildTableName);
                }

                AddColumnsToCache(relation.ParentColumnNames);
                AddColumnsToCache(relation.ChildColumnNames);
            }
        }

        /// <summary>
        /// Adds column names to column cache.
        /// </summary>
        /// <param name="columns">List of column names</param>
        private void AddColumnsToCache(IReadOnlyList<string>? columns)
        {
            if (columns == null)
            {
                return;
            }

            foreach (string columnName in columns)
            {
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    continue;
                }

                _relationColumnNames.Add(columnName);
            }
        }

        /// <summary>
        /// Validates the integrity of data relations and raises RelationValidationFailed event if any violations are detected.
        /// </summary>
        /// <remarks>
        /// This method checks for relation integrity violations in the underlying data set. If violations are found and there
        /// are subscribers to the RelationValidationFailed event, the event is raised with details of the violations.
        /// </remarks>
        /// <param name="handledEventName">The name of the event that triggered validation.</param>
        private void ValidateAndRaiseRelationValidationFailedEvent(string handledEventName)
        {
            if (_isValidating)
            {
                return;
            }

            _isValidating = true;

            try
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

                EventHandler<ObservableRelationValidationFailedEventArgs>? handler = RelationValidationFailed;
                if (handler == null)
                {
                    return;
                }

                handler(this, new ObservableRelationValidationFailedEventArgs(handledEventName, violations));
            }
            finally
            {
                _isValidating = false;
            }
        }

        /// <summary>
        /// Handles ObservableDataSet.DataFieldValueChanged event
        /// Noise reduction: validates only when a relation column changes.
        /// </summary>
        private void ObservableDataSet_DataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            string columnName = e.ColumnName;
            if (string.IsNullOrWhiteSpace(columnName))
            {
                return;
            }

            if (!_relationColumnNames.Contains(columnName))
            {
                return;
            }

            ValidateAndRaiseRelationValidationFailedEvent("RelationColumnChanged: " + columnName);
        }

        /// <summary>
        /// Handles ObservableDataSet.RowAdded event
        /// Noise reduction: validates only when the table participates in relations.
        /// </summary>
        private void ObservableDataSet_RowAdded(object? sender, RowsChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            string tableName = e.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return;
            }

            if (!_relationTableNames.Contains(tableName))
            {
                return;
            }

            ValidateAndRaiseRelationValidationFailedEvent("RowAdded: " + tableName);
        }

        /// <summary>
        /// Handles ObservableDataSet.RowRemoved event
        /// Noise reduction: validates only when the table participates in relations.
        /// </summary>
        private void ObservableDataSet_RowRemoved(object? sender, RowsChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            string tableName = e.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return;
            }

            if (!_relationTableNames.Contains(tableName))
            {
                return;
            }

            ValidateAndRaiseRelationValidationFailedEvent("RowRemoved: " + tableName);
        }

        /// <summary>
        /// Handles ObservableDataSet.TableAdded event
        /// Noise reduction: validates only when the table participates in relations.
        /// </summary>
        private void ObservableDataSet_TableAdded(object? sender, TablesChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            string tableName = e.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return;
            }

            if (!_relationTableNames.Contains(tableName))
            {
                return;
            }

            ValidateAndRaiseRelationValidationFailedEvent("TableAdded: " + tableName);
        }

        /// <summary>
        /// Handles ObservableDataSet.TableRemoved event
        /// Noise reduction: validates only when the table participates in relations.
        /// </summary>
        private void ObservableDataSet_TableRemoved(object? sender, TablesChangedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            string tableName = e.TableName;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return;
            }

            if (!_relationTableNames.Contains(tableName))
            {
                return;
            }

            ValidateAndRaiseRelationValidationFailedEvent("TableRemoved: " + tableName);
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
