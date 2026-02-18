using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Additional Replace schema contract tests: - Replace preserves current column metadata (PK
    /// flag, nullability, max length). - Refreshed column metadata does not overwrite current
    /// schema.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute Replace merge (replace rows while enforcing current schema contract).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
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
