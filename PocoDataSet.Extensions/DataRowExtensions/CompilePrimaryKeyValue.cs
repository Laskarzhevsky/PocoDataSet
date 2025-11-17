using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
        /// Compiles primary key value
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <returns>Compiled primary key value</returns>
        public static string CompilePrimaryKeyValue(this IDataRow? dataRow, List<string> primaryKeyColumnNames)
        {
            StringBuilder stringBuilder = new System.Text.StringBuilder();
            if (dataRow == null)
            {
                return stringBuilder.ToString();
            }

            for (int i = 0; i < primaryKeyColumnNames.Count; i++)
            {
                object? primaryKeyValue = dataRow.GetDataFieldValue<object?>(primaryKeyColumnNames[i]);
                string stringRepresentationOfValue = GetStringRepresentationOf(primaryKeyValue);
                if (i > 0)
                {
                    stringBuilder.Append('|');
                }

                stringBuilder.Append(stringRepresentationOfValue.Length.ToString(CultureInfo.InvariantCulture));
                stringBuilder.Append('#');
                stringBuilder.Append(stringRepresentationOfValue);
            }

            return stringBuilder.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets string representation of value
        /// </summary>
        /// <param name="value">Value to get string representation of</param>
        /// <returns>String representation of value</returns>
        static string GetStringRepresentationOf(object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (Convert.IsDBNull(value))
            {
                return string.Empty;
            }

            IFormattable? formattable = value as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            string? stringRepresentationOfValue = value.ToString();
            if (stringRepresentationOfValue == null)
            {
                return string.Empty;
            }

            return stringRepresentationOfValue;
        }
        #endregion
    }
}
