using System;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Refresh-style merge that is only allowed when the current table contains no pending changes.
    /// Intended for read-only / polling / search-result scenarios where the current table is expected to be clean.
    /// </summary>
    internal sealed class RefreshIfNoChangesExistMergePolicy : IObservableMergePolicy
    {
        #region Properties
        public bool IsFullReload
        {
            get { return false; }
        }

        public MergeMode Mode
        {
            get { return MergeMode.RefreshIfNoChangesExist; }
        }

        public bool RejectDuplicateRefreshedPrimaryKeys
        {
            get { return true; }
        }

        public bool RequiresClientKeyCorrelation
        {
            get { return false; }
        }

        public bool RequiresPrimaryKey
        {
            get { return true; }
        }

        public bool ShouldAcceptChangesAfterMerge
        {
            get { return true; }
        }
        #endregion

        #region Methods
        public bool CanOverwriteRow(DataRowState currentState)
        {
            // This mode is only valid when there are no pending changes.
            // Treat only Unchanged rows as overwriteable.
            return currentState == DataRowState.Unchanged;
        }

        public bool PreserveRowWhenMissingFromRefreshed(DataRowState currentState)
        {
            // In refresh-style reconciliation, missing rows are removed if they are Unchanged.
            // Any other state indicates pending local changes and should be preserved (although this mode forbids them).
            return currentState != DataRowState.Unchanged;
        }

        public void ValidateAfterPrimaryKeyDiscovery(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions,
            int primaryKeyCount)
        {
            if (currentObservableDataTable == null) throw new ArgumentNullException(nameof(currentObservableDataTable));
            if (refreshedDataTable == null) throw new ArgumentNullException(nameof(refreshedDataTable));
            if (observableMergeOptions == null) throw new ArgumentNullException(nameof(observableMergeOptions));

            if (RequiresPrimaryKey && primaryKeyCount <= 0)
            {
                throw new InvalidOperationException(
                    $"MergeMode.{Mode} requires a primary key for deterministic matching, but table '{currentObservableDataTable.TableName}' has no primary key.");
            }

            // No further validation here; row-state validation lives in ValidatePreconditions.
        }

        public void ValidatePreconditions(
            IObservableDataTable currentObservableDataTable,
            IDataTable refreshedDataTable,
            IObservableMergeOptions observableMergeOptions)
        {
            if (currentObservableDataTable == null) throw new ArgumentNullException(nameof(currentObservableDataTable));
            if (refreshedDataTable == null) throw new ArgumentNullException(nameof(refreshedDataTable));
            if (observableMergeOptions == null) throw new ArgumentNullException(nameof(observableMergeOptions));

            // Enforce "no pending changes" invariant.
            for (int i = 0; i < currentObservableDataTable.Rows.Count; i++)
            {
                DataRowState state = currentObservableDataTable.Rows[i].DataRowState;
                if (state != DataRowState.Unchanged)
                {
                    throw new InvalidOperationException(
                        $"MergeMode.{Mode} cannot be used when table '{currentObservableDataTable.TableName}' contains pending changes.");
                }
            }
        }
        #endregion
    }
}
