using System;
using System.Reflection;

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
        /// Copies public readable properties from POCO object into data row
        /// </summary>
        /// <typeparam name="T">POCO type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <param name="poco">POCO</param>
        public static void CopyFromPoco<T>(this IDataRow? dataRow, T poco)
        {
            if (dataRow == null)
            {
                return;
            }

            Type pocoObjectType = typeof(T);
            foreach (PropertyInfo propertyInfo in pocoObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!propertyInfo.CanRead)
                {
                    continue;
                }

                object? propertyValue = propertyInfo.GetValue(poco);
                dataRow[propertyInfo.Name] = propertyValue;
            }
        }
        #endregion
    }
}
