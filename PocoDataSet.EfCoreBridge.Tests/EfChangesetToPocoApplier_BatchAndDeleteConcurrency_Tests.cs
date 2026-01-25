using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetToPocoApplier_BatchAndDeleteConcurrency_Tests
{
    [Fact]
    public void ApplyTableAndSave_ModifiedRows_MultipleEntities_AppliesPatchesForEachRow()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "A", Description = "DA" });
        db.Departments.Add(new Department { Id = 2, Name = "B", Description = "DB" });
        db.SaveChanges();

        // Build PocoDataSet baseline with two loaded rows
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r1["Id"] = 1;
        r1["Name"] = "A";
        r1["Description"] = "DA";
        t.AddLoadedRow(r1);

        IDataRow r2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r2["Id"] = 2;
        r2["Name"] = "B";
        r2["Description"] = "DB";
        t.AddLoadedRow(r2);

        // Patch: update only Name for row 1, and only Description for row 2 (floating rows).
        r1["Name"] = "A2";
        r2["Description"] = "DB2";

        IDataSet cs = ds.CreateChangeset();

        // Act
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert
        Department d1 = db.Departments.Single(x => x.Id == 1);
        Department d2 = db.Departments.Single(x => x.Id == 2);

        Assert.Equal("A2", d1.Name);
        Assert.Equal("DA", d1.Description); // unchanged (missing in patch)

        Assert.Equal("B", d2.Name);         // unchanged (missing in patch)
        Assert.Equal("DB2", d2.Description);
    }

    [Fact]
    public void ApplyTableAndSave_AddedRow_WithUnknownColumn_IgnoresUnknownAndInsertsEntity()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);
        t.AddColumn("UnknownColumn", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = t.AddNewRow();
        r["Id"] = 10;
        r["Name"] = "New";
        r["Description"] = "Desc";
        r["UnknownColumn"] = "Ignored";

        IDataSet cs = ds.CreateChangeset();

        // Act
        EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert
        Department saved = db.Departments.Single(x => x.Id == 10);
        Assert.Equal("New", saved.Name);
        Assert.Equal("Desc", saved.Description);
    }

    [Fact]
    public void ApplyTableAndSave_DeletedRow_WithConcurrencyTokenProvided_ThrowsOnMismatch()
    {
        // Arrange: SQLite in-memory
        using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        using (TestDbContext setup = new TestDbContext(options))
        {
            setup.Database.EnsureCreated();
        }

        // Seed row with RowVersion V1
        using (TestDbContext seed = new TestDbContext(options))
        {
            seed.Departments.Add(new Department { Id = 1, Name = "Old", Description = "Keep", RowVersion = new byte[] { 1 } });
            seed.SaveChanges();
        }

        // Concurrent update to RowVersion V2
        using (TestDbContext concurrent = new TestDbContext(options))
        {
            Department d = concurrent.Departments.Single(x => x.Id == 1);
            d.Name = "SomeoneElse";
            d.RowVersion = new byte[] { 2 };
            concurrent.SaveChanges();
        }

        // Build a CHANGSET TABLE directly (do not use ds.CreateChangeset here)
        IDataSet cs = DataSetFactory.CreateDataSet();
        IDataTable ct = cs.AddNewTable("Department");
        ct.AddColumn("Id", DataTypeNames.INT32);
        ct.AddColumn("RowVersion", DataTypeNames.BINARY);
        ct.PrimaryKeys = new List<string> { "Id" };

        // 1) Add as LOADED (Unchanged)
        IDataRow row = DataRowExtensions.CreateRowFromColumns(ct.Columns);
        row["Id"] = 1;
        row["RowVersion"] = new byte[] { 1 };
        ct.AddLoadedRow(row);

        // 2) Now mark as Deleted (legal because it’s already in the table)
        row.Delete();

        // (Optional but safe) ensure RowVersion is still present after Delete()
        row["RowVersion"] = new byte[] { 1 };

        // Act / Assert
        using (TestDbContext db = new TestDbContext(options))
        {
            Assert.Throws<DbUpdateConcurrencyException>(
                () => EfChangesetToPocoApplier.ApplyTableAndSave(db, db.Departments, ct));
        }
    }
}
