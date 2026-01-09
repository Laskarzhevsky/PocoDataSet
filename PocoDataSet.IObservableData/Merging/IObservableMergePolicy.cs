using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Encapsulates merge-mode-specific policy decisions so merge handlers do not need switch statements.
    /// </summary>
    public interface IObservableMergePolicy
    {
        #region Properties
        /// <summary>
        /// Whether merge should perform full table reload (discard all local state).
        /// </summary>
        bool IsFullReload
        {
            get;
        }

        /// <summary>
        /// Merge mode this policy represents.
        /// </summary>
        MergeMode Mode
        {
            get;
        }

        /// <summary>
        /// Whether duplicate primary keys in refreshed data should be rejected (determinism).
        /// </summary>
        bool RejectDuplicateRefreshedPrimaryKeys
        {
            get;
        }

        /// <summary>
        /// Whether this merge mode requires __ClientKey correlation to be present.
        /// </summary>
        bool RequiresClientKeyCorrelation
        {
            get;
        }

        /// <summary>
        /// Whether this merge mode requires the current table to have a primary key for deterministic matching.
        /// </summary>
        bool RequiresPrimaryKey
        {
            get;
        }

        /// <summary>
        /// Whether the merge should call AcceptChanges after applying refreshed values.
        /// </summary>
        bool ShouldAcceptChangesAfterMerge
        {
            get;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets flag indicating whether a row can be overwritten / updated in-place
        /// </summary>
        /// <param name="currentState">Current state</param>
        /// <returns>Flag indicating whether a row can be overwritten / updated in-place</returns>
        bool CanOverwriteRow(DataRowState currentState);

        /// <summary>
        /// Gets flag indicating whether a current row should be preserved if it is missing from refreshed data.
        /// </summary>
        /// <param name="currentState">Current state</param>
        /// <returns>Flag indicating whether a current row should be preserved if it is missing from refreshed data</returns>
        bool PreserveRowWhenMissingFromRefreshed(DataRowState currentState);

        /// <summary>
        /// Validates table-level preconditions that depend on primary-key discovery.
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <param name="primaryKeyCount">Primary key count</param>
        void ValidateAfterPrimaryKeyDiscovery(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions, int primaryKeyCount);

        /// <summary>
        /// Validates any table-level preconditions that are independent of primary-key computation.
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        void ValidatePreconditions(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions);
        #endregion
    }
}