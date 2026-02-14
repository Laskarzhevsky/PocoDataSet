using System;
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
        /// Compiles primary key value.
        /// </summary>
        /// <remarks>
        /// This method is kept for backward compatibility. New code should prefer <see cref="RowIdentityResolver.TryGetPrimaryKeyValue"/>.
        /// </remarks>
        /// <param name="dataRow">Data row</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <returns>Compiled primary key value</returns>
        [Obsolete("Use RowIdentityResolver.TryGetPrimaryKeyValue(...) instead. This extension is kept for backward compatibility.")]
        public static string CompilePrimaryKeyValue(this IDataRow? dataRow, IReadOnlyList<string> primaryKeyColumnNames)
        {
            if (dataRow == null)
            {
                return string.Empty;
            }

            string primaryKeyValue;
            bool hasPrimaryKeyValue = RowIdentityResolver.TryGetPrimaryKeyValue(dataRow, primaryKeyColumnNames, out primaryKeyValue);

            return hasPrimaryKeyValue ? primaryKeyValue : string.Empty;
        }
        #endregion
    }
}
