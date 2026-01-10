using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public sealed class EfChangesetCopyToPocoApplier_FloatingSemantics_Tests
{
    [Fact]
    public void ApplyTableAndSave_ModifiedRow_MissingField_DoesNotOverwriteDatabaseValue()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "Old", Description = "KeepMe" });
        db.SaveChanges();

        // Build a loaded PocoDataSet row (baseline)
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "Old";
        r["Description"] = "KeepMe";
        t.AddLoadedRow(r);

        // Modify ONLY Name (Description is missing from delta)
        r["Name"] = "UpdatedName";

        // Act: create changeset (should be floating/sparse) and apply via EF bridge
        IDataSet cs = ds.CreateChangeset();
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert: Description remains untouched
        Department saved = db.Departments.Single(x => x.Id == 1);
        Assert.Equal("UpdatedName", saved.Name);
        Assert.Equal("KeepMe", saved.Description);
    }

    [Fact]
    public void ApplyTableAndSave_ModifiedRow_ExplicitNullField_SetsDatabaseValueToNull()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "Old", Description = "WillBeCleared" });
        db.SaveChanges();

        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "Old";
        r["Description"] = "WillBeCleared";
        t.AddLoadedRow(r);

        // Explicitly set Description to null (must be included in delta)
        r["Description"] = null;

        // Act
        IDataSet cs = ds.CreateChangeset();
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert
        Department saved = db.Departments.Single(x => x.Id == 1);
        Assert.Null(saved.Description);
    }

    [Fact]
    public void ApplyTableAndSave_ModifiedRow_UnknownColumns_AreIgnored()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "Old", Description = "Keep" });
        db.SaveChanges();

        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");

        // Note: "UnknownColumn" exists in PocoDataSet schema but NOT on EF entity.
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);
        t.AddColumn("UnknownColumn", DataTypeNames.STRING);

        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "Old";
        r["Description"] = "Keep";
        r["UnknownColumn"] = "Ignored";
        t.AddLoadedRow(r);

        // Patch: change Name and set unknown column as well.
        r["Name"] = "Updated";
        r["UnknownColumn"] = "StillIgnored";

        // Act (must not throw)
        IDataSet cs = ds.CreateChangeset();
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert: known fields updated, unknown ignored
        Department saved = db.Departments.Single(x => x.Id == 1);
        Assert.Equal("Updated", saved.Name);
        Assert.Equal("Keep", saved.Description);
    }

    [Fact]
    public void ApplyTableAndSave_ModifiedRow_WithConcurrencyTokenProvided_ThrowsOnMismatch()
    {
        // Arrange: create a shared in-memory database name so two contexts see the same data.
        string sharedDbName = "PocoDataSet_EfCoreBridge_Concurrency_" + Guid.NewGuid().ToString("N");

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(sharedDbName)
            .EnableSensitiveDataLogging()
            .Options;

        // Seed V1
        using (TestDbContext seed = new TestDbContext(options))
        {
            seed.Departments.Add(
                new Department
                {
                    Id = 1,
                    Name = "Old",
                    Description = "Keep",
                    RowVersion = new byte[] { 1 }
                });
            seed.SaveChanges();
        }

        // Update DB to V2 (simulates concurrent update)
        using (TestDbContext concurrent = new TestDbContext(options))
        {
            Department d = concurrent.Departments.Single(x => x.Id == 1);
            d.Name = "SomeoneElse";
            d.RowVersion = new byte[] { 2 };
            concurrent.SaveChanges();
        }

        // Build PocoDataSet baseline with RowVersion V1
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);
        t.AddColumn("RowVersion", DataTypeNames.BINARY);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "Old";
        r["Description"] = "Keep";
        r["RowVersion"] = new byte[] { 1 };
        t.AddLoadedRow(r);

        // Patch Name and include concurrency token V1
        r["Name"] = "MyUpdate";
        r["RowVersion"] = new byte[] { 1 };

        IDataSet cs = ds.CreateChangeset();

        // Act / Assert
        using (TestDbContext db = new TestDbContext(options))
        {
            Assert.Throws<DbUpdateConcurrencyException>(
                () => EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]));
        }
    }
}
