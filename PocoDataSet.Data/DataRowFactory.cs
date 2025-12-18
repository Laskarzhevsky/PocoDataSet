using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides DataRowFactory functionality
    /// </summary>
    public static class DataRowFactory
    {
        #region Public Methods
        /// <summary>
        /// Create an empty row (no defaults), optionally pre-sized for performance.
        /// </summary>
        public static IDataRow CreateEmpty(int initialCapacity = 0)
        {
            if (initialCapacity > 0)
            {
                return new DataRow(initialCapacity);
            }

            return new DataRow();
        }

        /// <summary>
        /// Create a row and initialize each column with its default value
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns></returns>
        public static IDataRow CreateFromColumnsWithDefaults(IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            DataRow row = new DataRow(listOfColumnMetadata.Count);
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                IColumnMetadata columnMetadata = listOfColumnMetadata[i];
                row[columnMetadata.ColumnName] = MetadataDefaults.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
            }

            return row;
        }
        #endregion
    }
}
