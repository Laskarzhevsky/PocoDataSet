using System;
using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class DataSetReloadFromTests
    {
        [Fact]
        public void ReloadFrom_ReplacesRows_AndAssignsClientKey_AndLeavesRowsUnchanged()
        {
            // Arrange: current dataset with a table and a tracking column already present
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("_ClientKey", DataTypeNames.GUID, false, false, false);
            t.PrimaryKeys = new List<string> { "Id" };

            IDataRow existing = DataRowExtensions.CreateRowFromColumns(t.Columns);
            existing["Id"] = 99;
            existing["Name"] = "Old";
            existing["_ClientKey"] = Guid.NewGuid();
            t.AddLoadedRow(existing);

            // Refreshed snapshot
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.PrimaryKeys = new List<string> { "Id" };

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "A";
            rt.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r2["Id"] = 2;
            r2["Name"] = "B";
            rt.AddLoadedRow(r2);

            // Act
            current.ReloadFrom(refreshed);

            // Assert
            Assert.Equal(2, t.Rows.Count);

            for (int i = 0; i < t.Rows.Count; i++)
            {
                IDataRow row = t.Rows[i];

                Assert.Equal(DataRowState.Unchanged, row.DataRowState);

                object? idObj;
                row.TryGetValue("Id", out idObj);
                Assert.NotNull(idObj);

                object? ckObj;
                row.TryGetValue("_ClientKey", out ckObj);
                Assert.NotNull(ckObj);
            }
        }
    }
}
