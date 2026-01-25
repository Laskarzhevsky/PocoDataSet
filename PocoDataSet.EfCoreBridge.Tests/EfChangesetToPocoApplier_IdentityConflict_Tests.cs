using System.Collections.Generic;
using System.Linq;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetToPocoApplier_IdentityConflict_Tests
{
    [Fact]
    public void ApplyTableAndSave_PatchesTrackedEntity_WhenDbContextAlreadyTracksSameKey()
    {
        using TestDbContext db = DbTestHelpers.CreateContext();

        // Seed and KEEP TRACKED
        Department tracked = new Department { Id = 1, Name = "Old" };
        db.Departments.Add(tracked);
        db.SaveChanges();

        // Build a changeset that modifies the same entity.
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        // Baseline loaded row matches DB
        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "Old";
        t.AddLoadedRow(r);

        // Modify baseline row
        r["Name"] = "Updated";

        IDataSet cs = ds.CreateChangeset();

        Assert.True(cs.Tables.ContainsKey("Department"));

        IDataTable ct = cs.Tables["Department"];

        // Act/Assert: floating PATCH applies to the already-tracked entity (no identity conflict).
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, ct);

        Assert.Equal("Updated", tracked.Name);

        Department? saved = db.Departments.Find(1);
        Assert.NotNull(saved);
        Assert.Equal("Updated", saved!.Name);

        Assert.Equal(1, db.Departments.Count());
    }

    [Fact]
    public void ApplyTableAndSave_Succeeds_WhenTrackerIsCleared()
    {
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "Old" });
        db.SaveChanges();

        // Detach all tracked entities
        db.ChangeTracker.Clear();

        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "Old";
        t.AddLoadedRow(r);

        r["Name"] = "Updated";

        IDataSet cs = ds.CreateChangeset();

        Assert.True(cs.Tables.ContainsKey("Department"));

        IDataTable ct = cs.Tables["Department"];

        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, ct);

        Department? saved = db.Departments.Find(1);
        Assert.NotNull(saved);
        Assert.Equal("Updated", saved!.Name);

        Assert.Equal(1, db.Departments.Count());
    }
}
