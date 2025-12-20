using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides data set functionality
    /// </summary>
    public class DataSet : IDataSet
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets name
        /// IDataSet interface implementation
        /// </summary>
        public string? Name
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets relations
        /// IDataSet interface implementation
        /// </summary>
        public List<IDataRelation> Relations
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets tables
        /// IDataSet interface implementation
        /// </summary>
        public Dictionary<string, IDataTable> Tables
        {
            get; set;
        } = new();
        #endregion
    }
}
