using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetToPocoApplier_FloatingPatch_Tests
{
    [Fact]
    public void ApplyTableAndSave_UpdatesTrackedEntity_AndDoesNotOverwriteUnprovidedColumns()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        // Keep this entity TRACKED (no ChangeTracker.Clear())
        Department tracked = new Department { Id = 1, Name = "Old", Description = "Keep" };
        db.Departments.Add(tracked);
        db.SaveChanges();

        // Build dataset table that does NOT include Description
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);

        IDataRow loaded = DataRowExtensions.CreateRowFromColumns(t.Columns);
        loaded["Id"] = 1;
        loaded["Name"] = "Old";
        t.AddLoadedRow(loaded);

        // Modify only Name
        loaded["Name"] = "Updated";

        IDataSet cs = ds.CreateChangeset();

        // Act
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert
        Department? saved = db.Departments.Find(1);
        Assert.NotNull(saved);
        Assert.Equal("Updated", saved!.Name);
        Assert.Equal("Keep", saved.Description);
    }
}
