using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides data row functionality
    /// </summary>
    public class DataRow : Dictionary<string, object?>, IDataRow
    {
        #region Public Methods
        /// <summary>
        /// Gets data field value. On conversion failure you get default(T)
        /// IDataRow interface implementation
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="columnName">Column name</param>
        /// <returns>Data field value</returns>
        public T? GetDataFieldValue<T>(string columnName)
        {
            if (!TryGetValue(columnName, out var value) || value == null)
            {
                return default;
            }

            try
            {
                var targetType = typeof(T);

                if (targetType == typeof(Guid) || targetType == typeof(Guid?))
                {
                    return (T)(object)Guid.Parse(value.ToString()!);
                }

                if (targetType.IsEnum)
                {
                    if (value is string str)
                    {
                        return (T)Enum.Parse(targetType, str, true);
                    }

                    return (T)Enum.ToObject(targetType, value);
                }

                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                return (T)Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Updates data field value
        /// If the column does not yet exist and value is null, no entry is added
        /// IDataRow interface implementation
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value for data field</param>
        /// <returns>Flag indicating whether data field value was updated</returns>
        public bool UpdateDataFieldValue(string columnName, object? value)
        {
            object? dataFieldCurrentValue;
            bool dataFieldHasValue = TryGetValue(columnName, out dataFieldCurrentValue);

            bool dataFieldValueUpdated = false;
            if (dataFieldCurrentValue == null)
            {
                if (value == null)
                {
                    // Nothing to do
                }
                else
                {
                    this[columnName] = value;
                    dataFieldValueUpdated = true;
                }
            }
            else
            {
                if (value == null)
                {
                    this[columnName] = value;
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
                        this[columnName] = value;
                        dataFieldValueUpdated = true;
                    }
                }
            }

            return dataFieldValueUpdated;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets data row state
        /// IDataRow interface implementation
        /// </summary>
        public DataRowState DataRowState
        {
            get; set;
        } = DataRowState.Detached;

        /// <summary>
        /// Gets or sets flag indicating whether data row is selected
        /// IDataRow interface implementation
        /// </summary>
        public bool Selected
        {
            get; set;
        }
        #endregion
    }
}
