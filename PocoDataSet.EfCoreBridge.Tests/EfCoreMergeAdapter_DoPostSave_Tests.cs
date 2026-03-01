using System;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.IData;
using PocoDataSet.Extensions;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests
{
    public class EfCoreMergeAdapter_DoPostSave_Tests
    {
        [Fact]
        public void DoPostSave_ReturnsDelta_WithClientKey_AndGeneratedIdentity_ForAddedRow()
        {
            using TestDbContext db = DbTestHelpers.CreateContext();

            // Arrange changeset with one Added row.
            IDataSet changeset = new DataSet();
            DataTable t = new DataTable();
            t.TableName = "Department";
            t.AddColumn(new ColumnMetadata { ColumnName = "Id", DataType = "System.Int32", IsPrimaryKey = true });
            t.AddColumn(new ColumnMetadata { ColumnName = "Name", DataType = "System.String" });
            t.AddColumn(new ColumnMetadata { ColumnName = "__ClientKey", DataType = "System.Guid" });            Guid clientKey = Guid.NewGuid();
            IDataRow added = t.AddNewRow();
            added["Id"] = 0;
            added["Name"] = "HR";
            added["__ClientKey"] = clientKey;
changeset.AddTable(t);

            // Act
            IDataSet response = EfCoreMergeAdapter.SaveData(db, changeset);

            // Assert: response contains Department table with one row and echoes client key + generated Id.
            IDataTable responseTable = response["Department"];
            Assert.Single(responseTable.Rows);

            IDataRow responseRow = responseTable.Rows[0];

            Assert.True(responseRow.TryGetValue("__ClientKey", out object? ck));
            Assert.Equal(clientKey, ck);

            Assert.True(responseRow.TryGetValue("Id", out object? idObj));
            Assert.NotNull(idObj);
            Assert.True((int)idObj! > 0);
        }
    }
}
