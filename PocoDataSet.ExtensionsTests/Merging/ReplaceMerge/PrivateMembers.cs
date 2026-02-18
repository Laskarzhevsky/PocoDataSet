using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Additional Replace schema contract tests:
    /// - Replace preserves current column metadata (PK flag, nullability, max length).
    /// - Refreshed column metadata does not overwrite current schema.
    /// </summary>
    public partial class ReplaceMerge
    {
        static int CountUserColumns(IDataTable table)
        {
            int count = 0;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                string name = table.Columns[i].ColumnName;

                // Ignore system columns that can be auto-added by infrastructure.
                if (name == SpecialColumnNames.CLIENT_KEY)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private static IColumnMetadata GetColumn(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == columnName)
                {
                    return table.Columns[i];
                }
            }

            throw new Xunit.Sdk.XunitException("Column not found: " + columnName);
        }

        static bool HasColumn(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == columnName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
