using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data row functionality
    /// </summary>
    public interface IDataRow : IDictionary<string, object?>
    {
        #region Properties
        /// <summary>
        /// Gets or sets data row state
        /// </summary>
        DataRowState DataRowState
        {
            get; set;
        }

        /// <summary>
        /// Gets flag indicating whether snapshot of original values exists
        /// </summary>
        bool HasOriginalValues
        {
            get;
        }

        /// <summary>
        /// Gets read-only view of original values (baseline)
        /// </summary>
        IReadOnlyDictionary<string, object?> OriginalValues
        {
            get;
        }

        /// <summary>
        /// Gets or sets flag indicating whether data row is selected
        /// </summary>
        bool Selected
        {
            get; set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accepts current values as baseline and sets state to Unchanged
        /// </summary>
        void AcceptChanges();

        /// <summary>
        /// Marks row as Deleted but does not remove it from table
        /// </summary>
        void Delete();

        /// <summary>
        /// Reverts to baseline. If Added, becomes Detached
        /// </summary>
        void RejectChanges();

        /// <summary>
        /// Reverts deletion to previous state (Unchanged or Modified)
        /// </summary>
        void Undelete();
        #endregion
    }
}
