using System.Collections.Generic;
using System.Linq;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetToPocoApplier_Tests
{
    [Fact]
    public void ApplyTableAndSave_InsertsUpdatesDeletes_Detached()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "Old", Description = "Keep" });
        db.Departments.Add(new Department { Id = 2, Name = "ToDelete", Description = "WillBeRemoved" });
        db.SaveChanges();

        // Clear tracker to keep the test independent from tracking behavior.
        db.ChangeTracker.Clear();

        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r1["Id"] = 1;
        r1["Name"] = "Old";
        t.AddLoadedRow(r1);

        IDataRow r2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r2["Id"] = 2;
        r2["Name"] = "ToDelete";
        t.AddLoadedRow(r2);

        // modify + delete + add
        r1["Name"] = "Updated";
        t.DeleteRow(r2);

        IDataRow r3 = t.AddNewRow();
        r3["Id"] = 3;
        r3["Name"] = "New";

        IDataSet cs = ds.CreateChangeset();

        // Act
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert
        List<Department> all = db.Departments.OrderBy(x => x.Id).ToList();

        Assert.Equal(2, all.Count);
        Assert.Equal(1, all[0].Id);
        Assert.Equal("Updated", all[0].Name);
        Assert.Equal("Keep", all[0].Description);
        Assert.Equal(3, all[1].Id);
        Assert.Equal("New", all[1].Name);
    }
}
