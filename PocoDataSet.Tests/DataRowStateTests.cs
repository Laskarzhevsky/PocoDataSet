using PocoDataSet.Extensions;
using PocoDataSet.IData;
using Xunit;

namespace PocoDataSet.Tests
{
    public sealed class DataRowStateTests
    {
        [Fact]
        public void AddRow_NewRow_BecomesAdded()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "Customer Service";

            table.AddRow(row);

            Assert.Equal(DataRowState.Added, row.DataRowState);
            Assert.False(row.HasOriginalValues);
        }

        [Fact]
        public void AddLoadedRow_LoadedRow_BecomesUnchanged()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "Customer Service";

            table.AddLoadedRow(row);

            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
            Assert.False(row.HasOriginalValues);
        }

        [Fact]
        public void Modify_UnchangedRow_BecomesModified_AndStoresOriginalValues()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "Customer Service";

            table.AddLoadedRow(row);

            row["Name"] = "Reception";

            Assert.Equal(DataRowState.Modified, row.DataRowState);
            Assert.True(row.HasOriginalValues);

            Assert.True(row.OriginalValues.ContainsKey("Name"));
            Assert.Equal("Customer Service", row.OriginalValues["Name"]);
            Assert.Equal("Reception", row["Name"]);
        }

        [Fact]
        public void RejectChanges_ModifiedRow_RevertsValuesAndState()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "Customer Service";

            table.AddLoadedRow(row);

            row["Name"] = "Reception";
            Assert.Equal(DataRowState.Modified, row.DataRowState);

            row.RejectChanges();

            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
            Assert.Equal("Customer Service", row["Name"]);
            Assert.False(row.HasOriginalValues);
        }

        [Fact]
        public void AcceptChanges_ModifiedRow_NormalizesStateAndClearsOriginalValues()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "Customer Service";

            table.AddLoadedRow(row);

            row["Name"] = "Reception";
            Assert.Equal(DataRowState.Modified, row.DataRowState);

            row.AcceptChanges();

            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
            Assert.Equal("Reception", row["Name"]);
            Assert.False(row.HasOriginalValues);
        }

        [Fact]
        public void Delete_UnchangedRow_BecomesDeleted_AndRejectRestores()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "Customer Service";

            table.AddLoadedRow(row);

            row.Delete();
            Assert.Equal(DataRowState.Deleted, row.DataRowState);

            row.RejectChanges();

            Assert.Equal(DataRowState.Unchanged, row.DataRowState);
            Assert.Equal("Customer Service", row["Name"]);
        }
    }
}
