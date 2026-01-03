using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static partial class DataTableExtensions
    {
        /// <summary>
        /// Copies column metadata from the source table into the target table.
        /// </summary>
        public static void CopyColumnsFrom(this IDataTable targetTable, IDataTable sourceTable)
        {
            if (targetTable == null)
            {
                throw new ArgumentNullException(nameof(targetTable));
            }

            if (sourceTable == null)
            {
                throw new ArgumentNullException(nameof(sourceTable));
            }

            for (int i = 0; i < sourceTable.Columns.Count; i++)
            {
                IColumnMetadata column = sourceTable.Columns[i];

                targetTable.AddColumn(
                    column.ColumnName,
                    column.DataType,
                    column.IsNullable,
                    column.IsPrimaryKey,
                    column.IsForeignKey);
            }
        }
    }
}
