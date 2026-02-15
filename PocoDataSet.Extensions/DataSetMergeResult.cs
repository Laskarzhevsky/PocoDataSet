using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data set merge result functionality
    /// </summary>
    public class DataSetMergeResult : IDataSetMergeResult
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="addedDataRows">Added data rows</param>
        /// <param name="deletedDataRows">Deleted data rows</param>
        /// <param name="updatedDataRows">Updated data rows</param>
        public DataSetMergeResult(List<IDataSetMergeResultEntry> addedDataRows, List<IDataSetMergeResultEntry> deletedDataRows, List<IDataSetMergeResultEntry> updatedDataRows)
        {
            AddedDataRows = addedDataRows;
            DeletedDataRows = deletedDataRows;
            UpdatedDataRows = updatedDataRows;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Clears data set merge result
        /// IDataSetsMergeResult interface implementation
        /// </summary>
        public void Clear()
        {
            AddedDataRows.Clear();
            DeletedDataRows.Clear();
            UpdatedDataRows.Clear();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets added data rows
        /// IDataSetsMergeResult interface implementation
        /// </summary>
        public List<IDataSetMergeResultEntry> AddedDataRows
        {
            get; private set;
        }

        /// <summary>
        /// Gets deleted data rows
        /// IDataSetsMergeResult interface implementation
        /// </summary>
        public List<IDataSetMergeResultEntry> DeletedDataRows
        {
            get; private set;
        }

        /// <summary>
        /// Gets updated data rows
        /// IDataSetsMergeResult interface implementation
        /// </summary>
        public List<IDataSetMergeResultEntry> UpdatedDataRows
        {
            get; private set;
        }
        #endregion
    }
}
