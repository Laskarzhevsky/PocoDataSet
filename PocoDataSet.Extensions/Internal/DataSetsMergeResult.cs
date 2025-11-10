using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public class DataSetsMergeResult
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="listOfAddedDataRows">List of added data rows</param>
        /// <param name="listOfDeletedDataRows">List of deleted data rows</param>
        /// <param name="listOfUpdatedDataRows">List of updated data rows</param>
        public DataSetsMergeResult(List<IDataRow> listOfAddedDataRows, List<IDataRow> listOfDeletedDataRows, List<IDataRow> listOfUpdatedDataRows)
        {
            ListOfAddedDataRows = listOfAddedDataRows;
            ListOfDeletedDataRows = listOfDeletedDataRows;
            ListOfUpdatedDataRows = listOfUpdatedDataRows;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets list of added data rows
        /// </summary>
        public List<IDataRow> ListOfAddedDataRows
        {
            get; set;
        }

        /// <summary>
        /// Gets list of deleted data rows
        /// </summary>
        public List<IDataRow> ListOfDeletedDataRows
        {
            get; set;
        }

        /// <summary>
        /// Gets list of updated data rows
        /// </summary>
        public List<IDataRow> ListOfUpdatedDataRows
        {
            get; set;
        }
        #endregion
    }
}
