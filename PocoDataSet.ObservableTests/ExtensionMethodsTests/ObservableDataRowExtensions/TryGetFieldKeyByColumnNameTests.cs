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
        public void TryGetFieldKeyByColumnName()
        {
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("T");
            t.AddColumn("Name", DataTypeNames.STRING, true, false);

            IObservableDataRow row = t.AddNewRow();
            row["Name"] = "X";

            string existingKey;
            bool found = row.TryGetFieldKeyByColumnName("naME", out existingKey);

            Assert.True(found);
            Assert.Equal("Name", existingKey);
        }
    }
}
