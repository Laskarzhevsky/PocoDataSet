using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataRowExtensions
{
    public partial class ObservableDataRowExtensionsTests
    {

        [Fact]
        public void CopyToPoco()
        {
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("Department");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING, true, false);

            IObservableDataRow row = t.AddNewRow();
            row["Id"] = 2;
            row["Name"] = "Finance";
            row.AcceptChanges();

            Department poco = new Department();

            row.CopyToPoco(poco);

            Assert.Equal(2, poco.id);
            Assert.Equal("Finance", poco.NAME);
        }
    }
}
