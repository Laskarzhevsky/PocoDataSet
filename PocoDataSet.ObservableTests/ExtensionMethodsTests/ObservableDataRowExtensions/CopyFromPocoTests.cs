using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ObservableTests.ObservableDataRowExtensions
{
    public partial class ObservableDataRowExtensionsTests
    {
        [Fact]
        public void CopyFromPoco()
        {
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("Department");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING, true, false);

            IObservableDataRow row = t.AddNewRow();
            row["Id"] = 1;
            row["Name"] = "Sales";
            row.AcceptChanges();

            DataFieldValueChangedCounter fieldCounter = new DataFieldValueChangedCounter();
            RowStateChangedCounter stateCounter = new RowStateChangedCounter();

            row.DataFieldValueChanged += fieldCounter.Handler;
            row.RowStateChanged += stateCounter.Handler;

            DepartmentPoco poco = new DepartmentPoco();
            poco.id = 1;
            poco.NAME = "Marketing";

            row.CopyFromPoco(poco);

            Assert.Equal("Marketing", row["Name"]);
            Assert.True(fieldCounter.Count >= 1);
            Assert.True(stateCounter.Count >= 1);
            Assert.Equal(PocoDataSet.IData.DataRowState.Modified, row.DataRowState);
        }
    }
}
