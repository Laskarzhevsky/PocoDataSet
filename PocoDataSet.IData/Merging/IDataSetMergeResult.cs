using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data set merge result functionalty
    /// </summary>
    public interface IDataSetMergeResult
    {
        #region Properties
        /// <summary>
        /// Gets list of added data rows
        /// </summary>
        List<IDataRow> ListOfAddedDataRows
        {
            get;
        }

        /// <summary>
        /// Gets list of deleted data rows
        /// </summary>
        List<IDataRow> ListOfDeletedDataRows
        {
            get;
        }

        /// <summary>
        /// Gets list of updated data rows
        /// </summary>
        List<IDataRow> ListOfUpdatedDataRows
        {
            get;
        }
        #endregion
    }
}
