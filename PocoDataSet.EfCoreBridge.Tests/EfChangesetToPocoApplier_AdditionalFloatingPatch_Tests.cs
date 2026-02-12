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

public class EfChangesetToPocoApplier_AdditionalFloatingPatch_Tests
{
    [Fact]
    public void ApplyTableAndSave_ModifiedRow_ConcurrencyTokenOmitted_DoesNotThrow_AndAppliesPatch()
    {
        // Arrange: create a shared in-memory database name so two contexts see the same data.
        string sharedDbName = "PocoDataSet_EfCoreBridge_Concurrency_Omitted_" + Guid.NewGuid().ToString("N");

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

        // Build PocoDataSet baseline WITHOUT RowVersion column (token omitted from patch)
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);

        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "Old";
        r["Description"] = "Keep";
        t.AddLoadedRow(r);

        // Patch Name (no concurrency token provided)
        r["Name"] = "Patched";

        IDataSet cs = ds.CreateChangeset();

        // Act / Assert: should NOT throw because we are not enforcing an optimistic concurrency predicate.
        using (TestDbContext db = new TestDbContext(options))
        {
            EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

            Department saved = db.Departments.Single(x => x.Id == 1);
            Assert.Equal("Patched", saved.Name);
            Assert.Equal("Keep", saved.Description);
            Assert.True(saved.RowVersion != null && saved.RowVersion.Length == 1);
            Assert.Equal(2, saved.RowVersion[0]); // should not be overwritten by any provided token (none was provided)
        }
    }

    [Fact]
    public void ApplyTableAndSave_ModifiedRow_ConcurrencyTokenProvided_Matching_DoesNotOverwriteTokenValue()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(
            new Department
            {
                Id = 1,
                Name = "Old",
                Description = "Keep",
                RowVersion = new byte[] { 7 }
            });
        db.SaveChanges();

        // Build PocoDataSet baseline WITH RowVersion
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);
        t.AddColumn("RowVersion", DataTypeNames.BINARY);

        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "Old";
        r["Description"] = "Keep";
        r["RowVersion"] = new byte[] { 7 };
        t.AddLoadedRow(r);

        // Patch Name and provide concurrency token (must be used as OriginalValue only)
        r["Name"] = "Patched";
        r["RowVersion"] = new byte[] { 7 };

        // Act
        IDataSet cs = ds.CreateChangeset();
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert
        Department saved = db.Departments.Single(x => x.Id == 1);
        Assert.Equal("Patched", saved.Name);
        Assert.Equal("Keep", saved.Description);
        Assert.NotNull(saved.RowVersion);
        Assert.Single(saved.RowVersion!);
        Assert.Equal(7, saved.RowVersion![0]); // token must not be overwritten by the patch payload
    }

    [Fact]
    public void ApplyTableAndSave_DeletedRow_WhenEntityAlreadyTracked_DeletesSuccessfully()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "ToDelete", Description = "X" });
        db.SaveChanges();

        // Force entity to be tracked
        Department tracked = db.Departments.Single(x => x.Id == 1);
        Assert.NotNull(tracked);

        // Build PocoDataSet baseline
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);

        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 1;
        r["Name"] = "ToDelete";
        r["Description"] = "X";
        t.AddLoadedRow(r);

        // Delete the row (changeset should include PK only for deletion)
        t.DeleteRow(r);

        // Act
        IDataSet cs = ds.CreateChangeset();
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert
        Assert.Empty(db.Departments.Where(x => x.Id == 1));
    }
}
