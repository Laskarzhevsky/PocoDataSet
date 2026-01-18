using System;
using System.Collections.Generic;

using Xunit;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

namespace PocoDataSet.ObservableTests
{
    public sealed class ObservableDataSetReloadFromTests
    {
        [Fact]
        public void ReloadFrom_ReplacesRows_AndAssignsClientKey_AndLeavesRowsUnchanged()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true, false);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID, false, false, false);

            IObservableDataRow existing = t.AddNewRow();
            existing["Id"] = 99;
            existing["Name"] = "Old";
            existing[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
            existing.AcceptChanges();

            // Refreshed snapshot
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.PrimaryKeys = new List<string> { "Id" };

            IDataRow r1 = PocoDataSet.Extensions.DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "A";
            rt.AddLoadedRow(r1);

            IDataRow r2 = PocoDataSet.Extensions.DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r2["Id"] = 2;
            r2["Name"] = "B";
            rt.AddLoadedRow(r2);

            // Act
            current.ReloadFrom(refreshed);

            // Assert
            Assert.Equal(2, t.Rows.Count);

            for (int i = 0; i < t.Rows.Count; i++)
            {
                IObservableDataRow row = t.Rows[i];

                Assert.Equal(DataRowState.Unchanged, row.DataRowState);

                IDataRow innerRow = row.InnerDataRow;

                object? idObj;
                innerRow.TryGetValue("Id", out idObj);
                Assert.NotNull(idObj);

                object? ckObj;
                innerRow.TryGetValue(SpecialColumnNames.CLIENT_KEY, out ckObj);
                Assert.NotNull(ckObj);
            }
        }
    }
}
