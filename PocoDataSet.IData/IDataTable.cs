using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data table functionality
    /// </summary>
    public interface IDataTable
    {
        #region Properties
        /// <summary>
        /// Gets or sets list of columns
        /// </summary>
        List<IColumnMetadata> Columns
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets rows
        /// </summary>
        List<IDataRow> Rows
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets table name
        /// </summary>
        string TableName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets List of primary keys
        /// </summary>
        List<string> PrimaryKey
        {
            get; set;
        }
        #endregion
    }
}
