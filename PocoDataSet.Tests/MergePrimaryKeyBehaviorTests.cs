using System;
using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class MergePrimaryKeyBehaviorTests
    {
        [Fact]
        public void Merge_WithPrimaryKey_UpdatesMatchingRows_AndTracksUpdated()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            currentRow["Id"] = 2;
            currentRow["Name"] = "Financial";
            currentTable.AddLoadedRow(currentRow);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);
            refreshedTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            refreshedRow["Id"] = 2;
            refreshedRow["Name"] = "Reception";
            refreshedTable.AddLoadedRow(refreshedRow);

            // Act
            IDataSetMergeResult result = current.MergeWith(refreshed, MergeMode.Refresh);

            // Assert
            Assert.Single(currentTable.Rows);
            Assert.Equal(2, currentTable.Rows[0]["Id"]);
            Assert.Equal("Reception", currentTable.Rows[0]["Name"]);

            // Refresh merge treats refreshed data as baseline
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[0].DataRowState);

            // The merge result should record an update
            Assert.Single(result.UpdatedDataRows);
            Assert.Empty(result.AddedDataRows);
            Assert.Empty(result.DeletedDataRows);
        }

        [Fact]
        public void Merge_WithPrimaryKey_DeletesMissingRows_AndTracksDeleted()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r1["Id"] = 1;
            r1["Name"] = "A";
            currentTable.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r2["Id"] = 2;
            r2["Name"] = "B";
            currentTable.AddLoadedRow(r2);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);
            refreshedTable.PrimaryKeys = new List<string> { "Id" };

            // refreshed contains only Id=1; Id=2 is missing -> should be treated as deleted in refresh merge
            IDataRow rr1 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            rr1["Id"] = 1;
            rr1["Name"] = "A";
            refreshedTable.AddLoadedRow(rr1);

            // Act
            IDataSetMergeResult result = current.MergeWith(refreshed, MergeMode.Refresh);

            // Assert
            // Refresh merge treats refreshed data as authoritative snapshot,
            // so missing rows are removed from the current table.
            Assert.Single(currentTable.Rows);
            Assert.Equal(1, currentTable.Rows[0]["Id"]);
            Assert.Equal("A", currentTable.Rows[0]["Name"]);

            // But merge result should still report that a row was deleted (Id=2).
            Assert.Single(result.DeletedDataRows);
        }

        [Fact]
        public void Merge_WithPrimaryKey_DoesNotDelete_LocalAddedRow_WhenRefreshingSnapshot()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.PrimaryKeys = new List<string> { "Id" };

            // Baseline row (loaded from server earlier)
            IDataRow loaded = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            loaded["Id"] = 1;
            loaded["Name"] = "A";
            currentTable.AddLoadedRow(loaded);

            // Local client-added row (must not be removed by Refresh merge)
            IDataRow added = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            added["Id"] = 999;
            added["Name"] = "LOCAL";
            currentTable.AddRow(added);

            Assert.Equal(DataRowState.Added, added.DataRowState);

            // Refreshed snapshot contains only the baseline row
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);
            refreshedTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow rr1 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            rr1["Id"] = 1;
            rr1["Name"] = "A";
            refreshedTable.AddLoadedRow(rr1);

            // Act
            IDataSetMergeResult result = current.MergeWith(refreshed, MergeMode.Refresh);

            // Assert
            // Baseline row remains, local Added row remains too
            Assert.Equal(2, currentTable.Rows.Count);

            bool hasId1 = false;
            bool hasId999 = false;

            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                IDataRow row = currentTable.Rows[i];
                if ((int)row["Id"]! == 1)
                {
                    hasId1 = true;
                }
                if ((int)row["Id"]! == 999)
                {
                    hasId999 = true;
                }
            }

            Assert.True(hasId1);
            Assert.True(hasId999);

            // Refresh should not report the local Added row as deleted
            Assert.Empty(result.DeletedDataRows);
        }

        [Fact]
        public void Merge_WithPrimaryKey_DeletionDetection_RemovesMissingUnchangedRow()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One";
            currentTable.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r2["Id"] = 2;
            r2["Name"] = "Two";
            currentTable.AddLoadedRow(r2);

            IDataRow r3 = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
            r3["Id"] = 3;
            r3["Name"] = "Three";
            currentTable.AddLoadedRow(r3);

            // Refreshed snapshot is missing Id=2
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);
            refreshedTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow rr1 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            rr1["Id"] = 1;
            rr1["Name"] = "One";
            refreshedTable.AddLoadedRow(rr1);

            IDataRow rr3 = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
            rr3["Id"] = 3;
            rr3["Name"] = "Three";
            refreshedTable.AddLoadedRow(rr3);

            // Act
            IDataSetMergeResult result = current.MergeWith(refreshed, MergeMode.Refresh);

            // Assert: row 2 must be removed (or counted as deleted in merge result)
            Assert.Equal(2, currentTable.Rows.Count);

            bool has1 = false;
            bool has2 = false;
            bool has3 = false;

            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                IDataRow row = currentTable.Rows[i];
                int id = (int)row["Id"]!;
                if (id == 1)
                {
                    has1 = true;
                }
                if (id == 2)
                {
                    has2 = true;
                }
                if (id == 3)
                {
                    has3 = true;
                }
            }

            Assert.True(has1);
            Assert.False(has2);
            Assert.True(has3);

            Assert.Single(result.DeletedDataRows);
        }
    }
}
