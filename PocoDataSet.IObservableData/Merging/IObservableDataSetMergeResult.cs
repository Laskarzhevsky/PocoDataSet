using System.Collections.Generic;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable data set merge result functionalty
    /// </summary>
    public interface IObservableDataSetMergeResult
    {
        #region Methods
        /// <summary>
        /// Clears observable data set merge result
        /// </summary>
        void Clear();
        #endregion

        #region Properties
        /// <summary>
        /// Gets added observable data rows
        /// </summary>
        List<IObservableDataSetMergeResultEntry> AddedObservableDataRows
        {
            get;
        }

        /// <summary>
        /// Gets deleted observable data rows
        /// </summary>
        List<IObservableDataSetMergeResultEntry> DeletedObservableDataRows
        {
            get;
        }

        /// <summary>
        /// Gets updated observable data rows
        /// </summary>
        List<IObservableDataSetMergeResultEntry> UpdatedObservableDataRows
        {
            get;
        }
        #endregion
    }
}
