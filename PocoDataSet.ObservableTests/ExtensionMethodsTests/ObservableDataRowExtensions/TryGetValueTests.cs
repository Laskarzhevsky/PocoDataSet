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
        public void TryGetValue()
        {
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("T");
            t.AddColumn("Name", DataTypeNames.STRING, true, false);

            IObservableDataRow row = t.AddNewRow();
            row["Name"] = "X";

            object? value;
            bool found = row.TryGetValue("naME", out value);

            Assert.True(found);
            Assert.Equal("X", value);
        }
    }
}
