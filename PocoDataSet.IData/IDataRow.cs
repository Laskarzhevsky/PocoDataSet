using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data row functionality
    /// </summary>
    public interface IDataRow : IDictionary<string, object?>
    {
        #region Properties
        /// <summary>
        /// Gets or sets data row state
        /// </summary>
        DataRowState DataRowState
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets flag indicating whether data row is selected
        /// </summary>
        bool Selected
        {
            get; set;
        }
        #endregion
    }
}
