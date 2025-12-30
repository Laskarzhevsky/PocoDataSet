using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data row functionality
    /// </summary>
    public interface IDataRow
    {
        #region Properties
        /// <summary>
        /// Gets or sets data row state
        /// </summary>
        DataRowState DataRowState
        {
            get;
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

        /// <summary>
        /// Gets row values
        /// </summary>
        IReadOnlyDictionary<string, object?> Values
        {
            get;
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets or sets value by column name
        /// </summary>
        object? this[string columnName]
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
        /// Gets flag indicating whether column exists
        /// </summary>
        bool ContainsKey(string columnName);

        /// <summary>
        /// Marks row as Deleted but does not remove it from table
        /// </summary>
        void Delete();

        /// <summary>
        /// Reverts to baseline rows which are in Deleted or Modified state
        /// It does nothing for rows in Detached or Unchanged state
        /// It throws and exception for rows in Added state
        /// </summary>
        /// <exception cref="InvalidOperationException">Reject changes cannot be executed on rows in Added state</exception>
        void RejectChanges();

        /// <summary>
        /// Sets row state internally for framework operations (e.g., changesets, merges).
        /// Not intended for business code.
        /// </summary>
        /// <param name="dataRowState">Data row state</param>
        void SetDataRowState(DataRowState dataRowState);
        
        /// <summary>
        /// Tries to get original value by column name
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="originalValue">Original value</param>
        /// <returns>True if original value returned, otherwise false</returns>
        bool TryGetOriginalValue(string columnName, out object? originalValue);

        /// <summary>
        /// Tries to get value by column name
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value if found</param>
        /// <returns>True if value returned, otherwise false</returns>
        bool TryGetValue(string columnName, out object? value);
        #endregion
    }
}
