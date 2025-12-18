using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.IDataMerge
{
    /// <summary>
    /// Defines data sets merge result
    /// </summary>
    public interface IDataSetsMergeResult
    {
        #region Properties
        /// <summary>
        /// Gets or sets list of added data rows
        /// </summary>
        List<IDataRow> ListOfAddedDataRows
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets list of deleted data rows
        /// </summary>
        List<IDataRow> ListOfDeletedDataRows
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets list of updated data rows
        /// </summary>
        List<IDataRow> ListOfUpdatedDataRows
        {
            get; set;
        }
        #endregion
    }
}
