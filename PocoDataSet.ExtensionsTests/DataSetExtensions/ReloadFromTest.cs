using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void ReloadFromTest()
        {
            // 1. Current data set shown in the UI
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable department = current.AddNewTable("Department");
            department.AddColumn("Id", DataTypeNames.INT32, false, true);
            department.AddColumn("Name", DataTypeNames.STRING);

            // 2. Refreshed snapshot from server (for example, new search results)
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshed.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32, false, true);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow a = refreshedDepartment.AddNewRow();
            a["Id"] = 1;
            a["Name"] = "Sales";

            IDataRow b = refreshedDepartment.AddNewRow();
            b["Id"] = 2;
            b["Name"] = "Engineering";

            // Act
            // 3. Replace current rows with refreshed rows (discard local edits)
            current.ReloadFrom(refreshed);

            // Assert
            // After ReloadFrom, current tables contain only refreshed loaded rows.
            Assert.Equal(2, department.Rows.Count);
        }
    }
}
