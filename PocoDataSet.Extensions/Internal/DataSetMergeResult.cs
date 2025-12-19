using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public class DataSetMergeResult : IDataSetMergeResult
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="listOfAddedDataRows">List of added data rows</param>
        /// <param name="listOfDeletedDataRows">List of deleted data rows</param>
        /// <param name="listOfUpdatedDataRows">List of updated data rows</param>
        public DataSetMergeResult(List<IDataRow> listOfAddedDataRows, List<IDataRow> listOfDeletedDataRows, List<IDataRow> listOfUpdatedDataRows)
        {
            ListOfAddedDataRows = listOfAddedDataRows;
            ListOfDeletedDataRows = listOfDeletedDataRows;
            ListOfUpdatedDataRows = listOfUpdatedDataRows;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets list of added data rows
        /// IDataSetsMergeResult interface implementation
        /// </summary>
        public List<IDataRow> ListOfAddedDataRows
        {
            get; private set;
        }

        /// <summary>
        /// Gets list of deleted data rows
        /// IDataSetsMergeResult interface implementation
        /// </summary>
        public List<IDataRow> ListOfDeletedDataRows
        {
            get; private set;
        }

        /// <summary>
        /// Gets list of updated data rows
        /// IDataSetsMergeResult interface implementation
        /// </summary>
        public List<IDataRow> ListOfUpdatedDataRows
        {
            get; private set;
        }
        #endregion
    }
}
