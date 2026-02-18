using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies the CURRENT observed behavior when the refreshed snapshot provides a value whose runtime type does
        /// not match the declared column type in the CURRENT schema.
        ///
        /// Observed contract:
        /// - RefreshPreservingLocalChanges does **not throw** on type mismatch during value application.
        /// - Values are copied as-is (object assignment), leaving it up to consumers to validate types if needed.
        ///
        /// How the test proves the contract:
        /// 1) CURRENT schema declares Name as STRING; CURRENT row is Unchanged.
        /// 2) REFRESHED has the same row (same PK) but Name is an INT value (123).
        /// 3) Act: merge.
        /// 4) Assert: merge does not throw and CURRENT row's Name becomes 123 (boxed int).
        /// </summary>
        [Fact]
        public void SchemaDrift_TypeMismatch_DoesNotThrow()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r["Id"] = 1;
            r["Name"] = "A";
            t.AddLoadedRow(r);

            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // Arrange (REFRESHED) - type mismatch for Name (int instead of string)
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("Department");
            rt.AddColumn("Id", DataTypeNames.INT32, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr["Id"] = 1;
            rr["Name"] = 123;
            rt.AddLoadedRow(rr);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal(123, t.Rows[0]["Name"]);
        }
    }
}
