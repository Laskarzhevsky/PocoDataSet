using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class CreateChangesetTests
    {
        [Fact]
        public void CreateChangeset_IncludesOnlyChangedRows_AndOnlyTablesWithChanges()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // Table A: has changes
            IDataTable a = dataSet.AddNewTable("A");
            a.AddColumn("Id", DataTypeNames.INT32);
            a.AddColumn("Name", DataTypeNames.STRING);

            // Unchanged row (loaded)
            IDataRow a1 = DataRowExtensions.CreateRowFromColumns(a.Columns);
            a1["Id"] = 1;
            a1["Name"] = "U";
            a.AddLoadedRow(a1);

            // Added row
            IDataRow a2 = DataRowExtensions.CreateRowFromColumns(a.Columns);
            a2["Id"] = 2;
            a2["Name"] = "Added";
            a.AddRow(a2);

            // Modified row (loaded then edited)
            IDataRow a3 = DataRowExtensions.CreateRowFromColumns(a.Columns);
            a3["Id"] = 3;
            a3["Name"] = "Before";
            a.AddLoadedRow(a3);
            a3["Name"] = "After";

            // Table B: no changes -> must not appear in changeset
            IDataTable b = dataSet.AddNewTable("B");
            b.AddColumn("Id", DataTypeNames.INT32);

            IDataRow b1 = DataRowExtensions.CreateRowFromColumns(b.Columns);
            b1["Id"] = 10;
            b.AddLoadedRow(b1);

            // Act
            IDataSet? changeset = dataSet.CreateChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.Single(changeset!.Tables);

            Assert.True(changeset.Tables.ContainsKey("A"));
            Assert.False(changeset.Tables.ContainsKey("B"));

            IDataTable changedA = changeset.Tables["A"];
            Assert.Equal(2, changedA.Rows.Count);

            bool hasAdded = false;
            bool hasModified = false;

            for (int i = 0; i < changedA.Rows.Count; i++)
            {
                IDataRow row = changedA.Rows[i];
                if ((int)row["Id"]! == 2)
                {
                    hasAdded = true;
                    Assert.Equal(DataRowState.Added, row.DataRowState);
                }
                if ((int)row["Id"]! == 3)
                {
                    hasModified = true;
                    Assert.Equal(DataRowState.Modified, row.DataRowState);
                    Assert.Equal("After", row["Name"]);
                }
            }

            Assert.True(hasAdded);
            Assert.True(hasModified);
        }
    }
}
