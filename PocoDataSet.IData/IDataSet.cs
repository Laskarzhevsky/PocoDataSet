using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data set functionality
    /// </summary>
    public interface IDataSet
    {
        #region Properties
        /// <summary>
        /// Gets or sets relations
        /// </summary>
        List<IDataRelation> Relations
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets tables
        /// </summary>
        Dictionary<string, IDataTable> Tables
        {
            get; set;
        }
        #endregion
    }
}
