using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class MergeRefreshConflictRuleTests
    {
        [Fact]
        public void Merge_Refresh_DoesNotOverwrite_LocalModifiedRow_WhenServerAlsoChangedRow()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "ServerOld";
            t.AddLoadedRow(row);

            // Local edit
            row["Name"] = "LocalEdit";
            Assert.Equal(DataRowState.Modified, row.DataRowState);

            // Refreshed snapshot also changed the same row
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "ServerNew";
            rt.AddLoadedRow(r1);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
// Assert: local edit is preserved
            Assert.Equal("LocalEdit", row["Name"]);
            Assert.Equal(DataRowState.Modified, row.DataRowState);
        }
    }
}

