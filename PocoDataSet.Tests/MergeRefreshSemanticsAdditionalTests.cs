using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class MergeRefreshSemanticsAdditionalTests
    {
        [Fact]
        public void Merge_Refresh_Updates_UnchangedRow_AndKeepsItUnchanged()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.PrimaryKeys = new List<string> { "Id" };

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "Old";
            t.AddLoadedRow(row);

            Assert.Equal(DataRowState.Unchanged, row.DataRowState);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.PrimaryKeys = new List<string> { "Id" };

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "New";
            rt.AddLoadedRow(r1);

            // Act
            current.MergeWith(refreshed, MergeMode.Refresh);

            // Assert
            Assert.Equal("New", row["Name"]);
            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
        }

        [Fact]
        public void Merge_Refresh_Preserves_AddedRow_WhenServerSnapshotDoesNotContainIt()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.PrimaryKeys = new List<string> { "Id" };

            IDataRow added = t.AddNewRow();
            added["Id"] = 100;
            added["Name"] = "ClientOnly";

            Assert.Equal(DataRowState.Added, added.DataRowState);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.PrimaryKeys = new List<string> { "Id" };

            // Act
            current.MergeWith(refreshed, MergeMode.Refresh);

            // Assert: Added row preserved
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal("ClientOnly", t.Rows[0]["Name"]);
            Assert.Equal(DataRowState.Added, t.Rows[0].DataRowState);
        }

        [Fact]
        public void Merge_Refresh_Preserves_DeletedRow_WhenServerSnapshotDoesNotContainIt()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.PrimaryKeys = new List<string> { "Id" };

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "ToDelete";
            t.AddLoadedRow(row);

            t.DeleteRow(row);
            Assert.Equal(DataRowState.Deleted, row.DataRowState);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.PrimaryKeys = new List<string> { "Id" };

            // Act
            current.MergeWith(refreshed, MergeMode.Refresh);

            // Assert: Deleted row preserved in Refresh mode
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal(DataRowState.Deleted, t.Rows[0].DataRowState);
        }

        [Fact]
        public void Merge_Refresh_DoesNotThrow_WhenColumnAddedAfterRowsExist_AndRowDoesNotContainKey()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.PrimaryKeys = new List<string> { "Id" };

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "Old";
            t.AddLoadedRow(row);

            // Add a column after rows already exist (row may not contain this key internally)
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumns(t.Columns);
            rt.PrimaryKeys = new List<string> { "Id" };

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "New";
            r1[SpecialColumnNames.CLIENT_KEY] = System.Guid.NewGuid();
            rt.AddLoadedRow(r1);

            // Act + Assert: must not throw
            current.MergeWith(refreshed, MergeMode.Refresh);

            // And Name must be updated (row is Unchanged during refresh)
            Assert.Equal("New", row["Name"]);
            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
        }
    }
}
