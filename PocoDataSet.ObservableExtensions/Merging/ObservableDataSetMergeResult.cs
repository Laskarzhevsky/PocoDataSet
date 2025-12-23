using System.Collections.Generic;

using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides observable data set merge result functionality
    /// </summary>
    public class ObservableDataSetMergeResult : IObservableDataSetMergeResult
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="addedObservableDataRows">Added observable data rows</param>
        /// <param name="deletedObservableDataRows">Deleted observable data rows</param>
        /// <param name="updatedObservableDataRows">Updated observable data rows</param>
        public ObservableDataSetMergeResult(List<IObservableDataSetMergeResultEntry> addedObservableDataRows, List<IObservableDataSetMergeResultEntry> deletedObservableDataRows, List<IObservableDataSetMergeResultEntry> updatedObservableDataRows)
        {
            AddedObservableDataRows = addedObservableDataRows;
            DeletedObservableDataRows = deletedObservableDataRows;
            UpdatedObservableDataRows = updatedObservableDataRows;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Clears observable data set merge result
        /// IObservableDataSetsMergeResult interface implementation
        /// </summary>
        public void Clear()
        {
            AddedObservableDataRows.Clear();
            DeletedObservableDataRows.Clear();
            UpdatedObservableDataRows.Clear();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets added observable data rows
        /// IObservableDataSetsMergeResult interface implementation
        /// </summary>
        public List<IObservableDataSetMergeResultEntry> AddedObservableDataRows
        {
            get; private set;
        }

        /// <summary>
        /// Gets deleted observable data rows
        /// IObservableDataSetsMergeResult interface implementation
        /// </summary>
        public List<IObservableDataSetMergeResultEntry> DeletedObservableDataRows
        {
            get; private set;
        }

        /// <summary>
        /// Gets updated observable data rows
        /// IObservableDataSetsMergeResult interface implementation
        /// </summary>
        public List<IObservableDataSetMergeResultEntry> UpdatedObservableDataRows
        {
            get; private set;
        }
        #endregion
    }
}
