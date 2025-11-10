using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data row functionality
    /// </summary>
    public interface IDataRow : IDictionary<string, object?>
    {
        #region Methods
        /// <summary>
        /// Gets data field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="columnName">Column name</param>
        /// <returns>Data field value</returns>
        T? GetDataFieldValue<T>(string columnName);

        /// <summary>
        /// Sets value into data row
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value for setting into data row</param>
        /// <returns>Flag indicating whether value was set</returns>
        bool UpdateDataFieldValue(string columnName, object? value);
        #endregion

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
