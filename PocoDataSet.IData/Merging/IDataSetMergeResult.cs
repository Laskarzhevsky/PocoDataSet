using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data set merge result functionalty
    /// </summary>
    public interface IDataSetMergeResult
    {
        #region Methods
        /// <summary>
        /// Clears data set merge result
        /// </summary>
        void Clear();
        #endregion

        #region Properties
        /// <summary>
        /// Gets added data rows
        /// </summary>
        List<IDataSetMergeResultEntry> AddedDataRows
        {
            get;
        }

        /// <summary>
        /// Gets deleted data rows
        /// </summary>
        List<IDataSetMergeResultEntry> DeletedDataRows
        {
            get;
        }

        /// <summary>
        /// Gets updated data rows
        /// </summary>
        List<IDataSetMergeResultEntry> UpdatedDataRows
        {
            get;
        }
        #endregion
    }
}
