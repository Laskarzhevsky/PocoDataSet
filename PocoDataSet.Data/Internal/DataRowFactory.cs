using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// INTERNAL USE ONLY.
    /// Provides DataRowFactory functionality
    /// </summary>
    internal static class DataRowFactory
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
        /// Creates an empty floating (sparse) row that can contain only explicitly provided fields.
        /// </summary>
        public static IFloatingDataRow CreateFloating(int initialCapacity = 0)
        {
            if (initialCapacity > 0)
            {
                return new FloatingDataRow(initialCapacity);
            }

            return new FloatingDataRow();
        }

        /// <summary>
        /// Creates data row from list of column metadata
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>Created data row</returns>
        public static IDataRow CreateRowFromColumns(IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            IDataRow dataRow = CreateEmpty(listOfColumnMetadata.Count);
            foreach (IColumnMetadata columnMetadata in listOfColumnMetadata)
            {
                dataRow[columnMetadata.ColumnName] = null;
            }

            return dataRow;
        }

        /// <summary>
        /// Creates data row from list of column metadata and initialize each data field with its default value
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>Created data row</returns>
        public static IDataRow CreateRowFromColumnsWithDefaults(IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            IDataRow dataRow = CreateEmpty(listOfColumnMetadata.Count);
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                IColumnMetadata columnMetadata = listOfColumnMetadata[i];
                dataRow[columnMetadata.ColumnName] = MetadataDefaults.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
            }

            return dataRow;
        }
        #endregion
    }
}
