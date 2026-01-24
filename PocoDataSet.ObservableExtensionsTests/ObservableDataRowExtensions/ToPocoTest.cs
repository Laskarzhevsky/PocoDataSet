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
        public void ToPoco()
        {
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("Department");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING, true, false);

            IObservableDataRow row = t.AddNewRow();
            row["Id"] = 3;
            row["Name"] = "HR";
            row.AcceptChanges();

            Department poco = row.ToPoco<Department>();

            Assert.Equal(3, poco.id);
            Assert.Equal("HR", poco.NAME);
        }
    }
}
