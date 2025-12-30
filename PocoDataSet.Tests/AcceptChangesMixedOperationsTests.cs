using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class AcceptChangesMixedOperationsTests
    {
        [Fact]
        public void AcceptChanges_AfterAddedModifiedDeleted_LeavesOnlyUnchangedRows()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            // Loaded -> Modified
            IDataRow m = DataRowExtensions.CreateRowFromColumns(table.Columns);
            m["Id"] = 1;
            m["Name"] = "Before";
            table.AddLoadedRow(m);
            m["Name"] = "After";

            // Loaded -> Deleted
            IDataRow d = DataRowExtensions.CreateRowFromColumns(table.Columns);
            d["Id"] = 2;
            d["Name"] = "ToDelete";
            table.AddLoadedRow(d);
            table.DeleteRow(d);

            // Added
            IDataRow a = DataRowExtensions.CreateRowFromColumns(table.Columns);
            a["Id"] = 3;
            a["Name"] = "Added";
            table.AddRow(a);

            Assert.Equal(DataRowState.Modified, m.DataRowState);
            Assert.Equal(DataRowState.Deleted, d.DataRowState);
            Assert.Equal(DataRowState.Added, a.DataRowState);

            // Act
            table.AcceptChanges();

            // Assert:
            // - Deleted row physically removed
            // - Added row becomes Unchanged
            // - Modified row becomes Unchanged and keeps new value
            Assert.Equal(2, table.Rows.Count);

            bool has1 = false;
            bool has2 = false;
            bool has3 = false;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                IDataRow row = table.Rows[i];
                int id = (int)row["Id"]!;

                if (id == 1)
                {
                    has1 = true;
                    Assert.Equal("After", row["Name"]);
                    Assert.Equal(DataRowState.Unchanged, row.DataRowState);
                }
                else if (id == 2)
                {
                    has2 = true;
                }
                else if (id == 3)
                {
                    has3 = true;
                    Assert.Equal("Added", row["Name"]);
                    Assert.Equal(DataRowState.Unchanged, row.DataRowState);
                }
            }

            Assert.True(has1);
            Assert.False(has2);
            Assert.True(has3);
        }
    }
}
