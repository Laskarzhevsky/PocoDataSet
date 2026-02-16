using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Locks the invariant that a single MergeOptions instance (and its DataSetMergeResult instance)
    /// flows through the entire merge pipeline without being replaced by inner layers.
    /// </summary>
    public class MergeOptionsPropagationTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotReplace_DataSetMergeResult_Instance()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One_Refreshed";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult expectedResultInstance = options.DataSetMergeResult;

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Same(expectedResultInstance, options.DataSetMergeResult);
            Assert.Equal("One_Refreshed", (string)current.Tables["T"].Rows[0]["Name"]!);
        }

        [Fact]
        public void ReplaceMerge_DoesNotReplace_DataSetMergeResult_Instance()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r2["Id"] = 2;
            r2["Name"] = "Two";
            rt.AddLoadedRow(r2);

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult expectedResultInstance = options.DataSetMergeResult;

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Same(expectedResultInstance, options.DataSetMergeResult);
            Assert.Single(current.Tables["T"].Rows);
            Assert.Equal(2, current.Tables["T"].Rows[0]["Id"]);
        }
    }
}
