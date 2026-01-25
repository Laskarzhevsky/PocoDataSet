using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetCopyToPocoApplier_SingleKey_Tests
{
    [Fact]
    public void ApplyTableAndSave_InsertsUpdatesDeletes_SingleKey()
    {
        // Arrange: seed EF with one row that we will modify, and another that we will delete
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "Old", Description = "Keep" });
        db.Departments.Add(new Department { Id = 2, Name = "ToDelete", Description = "WillBeRemoved" });
        db.SaveChanges();

        // Arrange: build PocoDataSet with same baseline (loaded rows)
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        // Loaded row Id=1
        IDataRow r1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r1["Id"] = 1;
        r1["Name"] = "Old";
        t.AddLoadedRow(r1);

        // Loaded row Id=2 (will be deleted)
        IDataRow r2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r2["Id"] = 2;
        r2["Name"] = "ToDelete";
        t.AddLoadedRow(r2);

        // New row Id=3
        IDataRow r3 = t.AddNewRow();
        r3["Id"] = 3;
        r3["Name"] = "New";

        // Modify Id=1
        r1["Name"] = "Updated";

        // Delete Id=2
        t.DeleteRow(r2);

        // changeset
        IDataSet cs = ds.CreateChangeset();
        cs.Tables["Department"].PrimaryKeys = new List<string> { "Id" };

        // Act
        EfChangesetCopyToPocoApplier.ApplyTableAndSave(db, db.Departments, cs.Tables["Department"]);

        // Assert
        List<Department> all = db.Departments.OrderBy(x => x.Id).ToList();

        Assert.Equal(2, all.Count);
        Assert.Equal(1, all[0].Id);
        Assert.Equal("Updated", all[0].Name);
        Assert.Equal("Keep", all[0].Description);
        Assert.Equal(3, all[1].Id);
        Assert.Equal("New", all[1].Name);
    }

    [Fact]
    public void ApplyTableAndSave_DoesNotOverwrite_UnprovidedColumns_WhenRowIsFloating()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "Old", Description = "Keep" });
        db.SaveChanges();

        // Build baseline table that DOES NOT include Description.
        // If EF applier incorrectly uses dense update semantics, Description would be overwritten.
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow loaded = DataRowExtensions.CreateRowFromColumns(t.Columns);
        loaded["Id"] = 1;
        loaded["Name"] = "Old";
        t.AddLoadedRow(loaded);

        // Modify only Name (floating changeset should include only Id + Name)
        loaded["Name"] = "Updated";

        IDataSet cs = ds.CreateChangeset();
        IDataTable ct = cs.Tables["Department"];
        ct.PrimaryKeys = new List<string> { "Id" };

        // Act
        EfChangesetCopyToPocoApplier.ApplyTableAndSave(db, db.Departments, ct);

        // Assert
        Department? saved = db.Departments.Find(1);
        Assert.NotNull(saved);
        Assert.Equal("Updated", saved!.Name);
        Assert.Equal("Keep", saved.Description);
    }

    [Fact]
    public void ApplyTable_DoesNotThrow_WhenPrimaryKeysAreInferredFromIdColumn()
    {
        using TestDbContext db = DbTestHelpers.CreateContext();

        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        // Do not set t.PrimaryKeys.
        // Your AddColumn implementation infers PrimaryKeys when the column name is "Id".
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);

        IDataRow r = t.AddNewRow();
        r["Id"] = 1;
        r["Name"] = "X";

        IDataSet cs = ds.CreateChangeset();
        IDataTable ct = cs.Tables["Department"];

        // Assert: should not throw because PrimaryKeys are inferred.
        EfChangesetCopyToPocoApplier.ApplyTable(db, db.Departments, ct);
    }

    [Fact]
    public void ApplyTable_Throws_WhenNoPrimaryKeysAndNoInferableIdColumn()
    {
        using TestDbContext db = DbTestHelpers.CreateContext();

        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        // Intentionally avoid "Id" column to avoid PK inference.
        t.AddColumn("Code", DataTypeNames.STRING);
        t.AddColumn("Name", DataTypeNames.STRING);

        IDataRow r = t.AddNewRow();
        r["Code"] = "D1";
        r["Name"] = "X";

        IDataSet cs = ds.CreateChangeset();
        IDataTable ct = cs.Tables["Department"];

        // Ensure PrimaryKeys is empty
        if (ct.PrimaryKeys != null)
        {
            ct.PrimaryKeys.Clear();
        }
        ct.PrimaryKeys = new List<string>();

        Assert.Throws<System.InvalidOperationException>(
            () => EfChangesetCopyToPocoApplier.ApplyTable(db, db.Departments, ct));
    }
}
