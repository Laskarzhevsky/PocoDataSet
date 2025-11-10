using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Represents data table
    /// </summary>
    public class DataTable : IDataTable
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets columns
        /// IDataTable interface implementation
        /// </summary>
        public List<IColumnMetadata> Columns
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets rows
        /// IDataTable interface implementation
        /// </summary>
        public List<IDataRow> Rows
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets table name
        /// IDataTable interface implementation
        /// </summary>
        public string TableName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets primary key
        /// IDataTable interface implementation
        /// </summary>
        public List<string> PrimaryKey
        {
            get; set;
        } = new();
        #endregion
    }
}
