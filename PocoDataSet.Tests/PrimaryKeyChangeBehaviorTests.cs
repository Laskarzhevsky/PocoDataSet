using System;
using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class PrimaryKeyChangeBehaviorTests
    {
        [Fact]
        public void ChangingPrimaryKey_OnLoadedRow_Throws()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.PrimaryKeys = new List<string> { "Id" };

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "A";
            table.AddLoadedRow(row);

            Assert.Throws<InvalidOperationException>(() =>
            {
                row["Id"] = 2;
            });
        }

        [Fact]
        public void ChangingPrimaryKey_OnAddedRow_IsAllowed()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.PrimaryKeys = new List<string> { "Id" };

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "A";
            table.AddRow(row);

            row["Id"] = 2;

            Assert.Equal(2, (int)row["Id"]!);
            Assert.Equal(DataRowState.Added, row.DataRowState);
        }
    }
}
