using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Updates data field value
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value for data field</param>
        /// <returns>Flag indicating whether data field value was updated</returns>
        public static bool UpdateDataFieldValue(this IDataRow? dataRow, string columnName, object? value)
        {
            if (dataRow == null)
            {
                return false;
            }

            Dictionary<string, object?> dataRowAsDictionary = (Dictionary<string, object?>)dataRow;
            if (!dataRowAsDictionary.ContainsKey(columnName))
            {
                return false;
            }

            bool dataFieldValueUpdated = false;
            object? dataFieldCurrentValue;
            bool dataFieldHasValue = dataRowAsDictionary.TryGetValue(columnName, out dataFieldCurrentValue);
            if (dataFieldCurrentValue == null)
            {
                if (value == null)
                {
                    // Nothing to do
                }
                else
                {
                    dataRow[columnName] = value;
                    dataFieldValueUpdated = true;
                }
            }
            else
            {
                if (value == null)
                {
                    dataRow[columnName] = value;
                    dataFieldValueUpdated = true;
                }
                else
                {
                    if (dataFieldCurrentValue.Equals(value))
                    {
                        // Nothing to do
                    }
                    else
                    {
                        dataRow[columnName] = value;
                        dataFieldValueUpdated = true;
                    }
                }
            }

            return dataFieldValueUpdated;
        }
        #endregion
    }
}
