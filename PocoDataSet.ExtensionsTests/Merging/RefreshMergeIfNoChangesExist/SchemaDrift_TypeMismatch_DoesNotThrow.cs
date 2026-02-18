using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the current schema/type-mismatch behavior for RefreshIfNoChangesExist.
    ///
    /// Meaning:
    /// - If CURRENT and REFRESHED contain a column with the same name but different declared types,
    ///   RefreshIfNoChangesExist does NOT throw.
    /// - CURRENT schema is preserved; REFRESHED schema differences do not block the merge.
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT has Name declared as STRING and one row (Id=1, Name="One").
    /// - Arrange: REFRESHED has Name declared as INT32 and a matching row (Id=1, Name=123).
    /// - Act: run RefreshIfNoChangesExist merge.
    /// - Assert: no exception is thrown and the CURRENT row receives the REFRESHED value (123) for Name.
    ///
    /// Notes:
    /// - This test deliberately locks what the code does today, even if the policy might be debated.
    ///   If you later choose to enforce type safety, change this test (and likely the implementation) intentionally.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void SchemaDrift_TypeMismatch_DoesNotThrow()
        {
            // Arrange CURRENT schema.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            // Add a single CURRENT row.
            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            // Arrange REFRESHED schema with a type mismatch for "Name".
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.INT32);

            // Add a matching REFRESHED row with an INT32 value for Name.
            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = 123;
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // Act: merge should not throw even though column types differ.
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: find the CURRENT row and confirm the REFRESHED value was applied.
            IDataRow merged = null;

            foreach (IDataRow row in t.Rows)
            {
                object idValue = row["Id"];

                if (idValue is int && (int)idValue == 1)
                {
                    merged = row;
                    break;
                }
            }

            Assert.NotNull(merged);

            // The value is copied as-is (INT32), even though CURRENT declares Name as STRING.
            Assert.Equal(123, merged["Name"]);
        }
    }
}
