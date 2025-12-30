using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class AcceptRejectEdgeTests
    {
        [Fact]
        public void RejectChanges_OnTable_RemovesAddedRows()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;

            table.AddRow(row);
            Assert.Equal(DataRowState.Added, row.DataRowState);
            Assert.Equal(1, table.Rows.Count);

            table.RejectChanges();

            Assert.Equal(0, table.Rows.Count);
        }

        [Fact]
        public void AcceptChanges_OnTable_RemovesDeletedRows()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            table.AddLoadedRow(row);

            Assert.Equal(1, table.Rows.Count);
            row.Delete();
            Assert.Equal(DataRowState.Deleted, row.DataRowState);

            table.AcceptChanges();

            Assert.Equal(0, table.Rows.Count);
        }

        [Fact]
        public void MultipleEdits_PreserveOriginalValuesFromFirstBaseline()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Name"] = "A";
            table.AddLoadedRow(row);

            row["Name"] = "B";
            row["Name"] = "C";

            Assert.Equal(DataRowState.Modified, row.DataRowState);
            Assert.True(row.HasOriginalValues);
            Assert.Equal("A", row.OriginalValues["Name"]);
            Assert.Equal("C", row["Name"]);

            row.RejectChanges();

            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
            Assert.Equal("A", row["Name"]);
            Assert.False(row.HasOriginalValues);
        }
    }
}
