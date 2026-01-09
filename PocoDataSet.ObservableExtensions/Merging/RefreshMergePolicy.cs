using System;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    sealed class RefreshMergePolicy : IObservableMergePolicy
    {
        #region Properties
        /// <summary>
        /// Whether merge should perform full table reload (discard all local state).
        /// IObservableMergePolicy interface implementation
        /// </summary>
        public bool IsFullReload
        {
            get; private set;
        }

        /// <summary>
        /// Merge mode this policy represents.
        /// IObservableMergePolicy interface implementation
        /// </summary>
        public MergeMode Mode
        {
            get; private set;
        } = MergeMode.Refresh;

        /// <summary>
        /// Whether duplicate primary keys in refreshed data should be rejected (determinism).
        /// IObservableMergePolicy interface implementation
        /// </summary>
        public bool RejectDuplicateRefreshedPrimaryKeys
        {
            get; private set;
        } = true;

        /// <summary>
        /// Whether this merge mode requires __ClientKey correlation to be present.
        /// IObservableMergePolicy interface implementation
        /// </summary>
        public bool RequiresClientKeyCorrelation
        {
            get; private set;
        }

        /// <summary>
        /// Whether this merge mode requires the current table to have a primary key for deterministic matching.
        /// IObservableMergePolicy interface implementation
        /// </summary>
        public bool RequiresPrimaryKey
        {
            get; private set;
        } = true;

        /// <summary>
        /// Whether the merge should call AcceptChanges after applying refreshed values.
        /// IObservableMergePolicy interface implementation
        /// </summary>
        public bool ShouldAcceptChangesAfterMerge
        {
            get; private set;
        } = true;
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets flag indicating whether a row can be overwritten / updated in-place
        /// IObservableMergePolicy interface implementation
        /// </summary>
        /// <param name="currentState">Current state</param>
        /// <returns>Flag indicating whether a row can be overwritten / updated in-place</returns>
        public bool CanOverwriteRow(DataRowState currentState)
        {
            return currentState == DataRowState.Unchanged;
        }

        /// <summary>
        /// Gets flag indicating whether a current row should be preserved if it is missing from refreshed data.
        /// IObservableMergePolicy interface implementation
        /// </summary>
        /// <param name="currentState">Current state</param>
        /// <returns>Flag indicating whether a current row should be preserved if it is missing from refreshed data</returns>
        public bool PreserveRowWhenMissingFromRefreshed(DataRowState currentState)
        {
            return currentState != DataRowState.Unchanged;
        }

        /// <summary>
        /// Validates table-level preconditions that depend on primary-key discovery.
        /// IObservableMergePolicy interface implementation
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <param name="primaryKeyCount">Primary key count</param>
        public void ValidateAfterPrimaryKeyDiscovery(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions, int primaryKeyCount)
        {
            if (primaryKeyCount == 0)
            {
                throw new InvalidOperationException(
                    "MergeMode.Refresh requires a primary key on current table '" + currentObservableDataTable.TableName + "'.");
            }
        }

        /// <summary>
        /// Validates any table-level preconditions that are independent of primary-key computation.
        /// IObservableMergePolicy interface implementation
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public void ValidatePreconditions(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
        }
        #endregion
    }
}