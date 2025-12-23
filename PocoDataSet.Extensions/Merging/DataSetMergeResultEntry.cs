using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides data set merge result entry functionalty
    /// </summary>
    public class DataSetMergeResultEntry : IDataSetMergeResultEntry
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="dataRow">Data row</param>
        public DataSetMergeResultEntry(string tableName, IDataRow dataRow)
        {
            TableName = tableName;
            DataRow = dataRow;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets data row
        /// IDataSetMergeResultEntry interface implementation
        /// </summary>
        public IDataRow DataRow
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets table name
        /// IDataSetMergeResultEntry interface implementation
        /// </summary>
        public string TableName
        {
            get; private set;
        }
        #endregion
    }
}
