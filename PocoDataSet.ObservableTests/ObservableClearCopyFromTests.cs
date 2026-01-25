using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableClearCopyFromTests
    {
        [Fact]
        public void Clear_RemovesAllRows_ButKeepsTablesAndColumns()
        {
            // Arrange
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true, false);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r1 = t.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "A";
            r1.AcceptChanges();

            IObservableDataRow r2 = t.AddNewRow();
            r2["Id"] = 2;
            r2["Name"] = "B";
            r2.AcceptChanges();

            int before = t.Columns.Count;

            // Act
            ds.Clear();

            // Assert
            Assert.True(ds.Tables.ContainsKey("T"));
            Assert.Equal(before, t.Columns.Count);   // schema unchanged (includes _ClientKey)
            Assert.Equal(0, t.Rows.Count);
        }

        [Fact]
        public void CopyFrom_CopiesValues_ForMatchingColumns()
        {
            // Arrange
            IObservableDataSet ods = new ObservableDataSet();
            IObservableDataTable t = ods.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true, false);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable srcTable = ds.AddNewTable("T");
            srcTable.AddColumn("Id", DataTypeNames.INT32);
            srcTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow src = PocoDataSet.Extensions.DataRowExtensions.CreateRowFromColumns(srcTable.Columns);
            src["Id"] = 7;
            src["Name"] = "X";

            IObservableDataRow dst = t.AddNewRow();

            // Act
            dst.CopyFrom(src, srcTable.Columns);

            // Assert
            Assert.Equal(7, dst["Id"]);
            Assert.Equal("X", dst["Name"]);
        }
    }
}
