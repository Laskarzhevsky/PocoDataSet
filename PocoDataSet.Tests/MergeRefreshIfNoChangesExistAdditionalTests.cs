using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Additional coverage for RefreshIfNoChangesExist after the "no MergeMode / no policies" refactor.
    /// Focus: dirty-detection matrix + PK-null behavior lock-in.
    /// </summary>
    public class MergeRefreshIfNoChangesExistAdditionalTests
    {
        [Fact]
        public void RefreshIfNoChangesExist_Throws_WhenCurrentHasAddedRow()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();
            IDataRow added = current.Tables["T"].AddNewRow();
            added["Id"] = 2;
            added["Name"] = "Added";

            IDataSet refreshed = BuildRefreshedWithSameSchemaAndRow(1, "One");

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergeIfNoChangesExist(refreshed, options));
        }

        [Fact]
        public void RefreshIfNoChangesExist_Throws_WhenCurrentHasModifiedRow()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();

            // Modify a loaded row to make the table dirty.
            current.Tables["T"].Rows[0]["Name"] = "One_Modified";
            Assert.Equal(DataRowState.Modified, current.Tables["T"].Rows[0].DataRowState);

            IDataSet refreshed = BuildRefreshedWithSameSchemaAndRow(1, "One");

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergeIfNoChangesExist(refreshed, options));
        }

        [Fact]
        public void RefreshIfNoChangesExist_Throws_WhenCurrentHasDeletedRow()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();

            IDataRow row = current.Tables["T"].Rows[0];
            current.Tables["T"].DeleteRow(row);
            Assert.Equal(DataRowState.Deleted, row.DataRowState);

            IDataSet refreshed = BuildRefreshedWithSameSchemaAndRow(1, "One");

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergeIfNoChangesExist(refreshed, options));
        }

        [Fact]
        public void RefreshIfNoChangesExist_DoesNotThrow_WhenCurrentHasNoPendingChanges()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();
            IDataSet refreshed = BuildRefreshedWithSameSchemaAndRow(1, "One_Refreshed");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            Assert.Equal(1, current.Tables["T"].Rows.Count);
            Assert.Equal("One_Refreshed", current.Tables["T"].Rows[0]["Name"]);
            Assert.Equal(DataRowState.Unchanged, current.Tables["T"].Rows[0].DataRowState);
        }

        [Fact]
        public void RefreshPreservingLocalChanges_Throws_WhenPrimaryKeyContainsNull_InRefreshedRow()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            refreshedRow["Id"] = null;
            refreshedRow["Name"] = "Bad";
            t.AddLoadedRow(refreshedRow);

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }

        private static IDataSet BuildCurrentWithPrimaryKey()
        {
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "One";
            t.AddLoadedRow(row);

            return current;
        }

        private static IDataSet BuildRefreshedWithSameSchemaAndRow(object? id, string name)
        {
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = id;
            row["Name"] = name;
            t.AddLoadedRow(row);

            return refreshed;
        }
    }
}
