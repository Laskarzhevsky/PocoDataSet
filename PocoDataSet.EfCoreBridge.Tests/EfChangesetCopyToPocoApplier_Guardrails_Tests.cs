using System.Collections.Generic;
using System.Linq;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetCopyToPocoApplier_Guardrails_Tests
{
    [Fact]
    public void ApplyTable_Throws_WhenPrimaryKeyColumnIsMissingFromRowValues()
    {
        using TestDbContext db = DbTestHelpers.CreateContext();

        // Build a changeset table that claims PK = "Id" but does not contain an "Id" column at all.
        // This is a safe way to test the applier guardrails without violating PocoDataSet rules
        // about changing PK columns for loaded rows.
        IDataSet cs = DataSetFactory.CreateDataSet();
        IDataTable ct = cs.AddNewTable("Department");
        ct.AddColumn("Code", DataTypeNames.STRING);
        ct.AddColumn("Name", DataTypeNames.STRING);

        ct.PrimaryKeys = new List<string> { "Id" };

        // Add a loaded row and mark it Deleted so the applier will try to resolve key values.
        IDataRow row = DataRowExtensions.CreateRowFromColumns(ct.Columns);
        row["Code"] = "D1";
        row["Name"] = "Ghost";
        ct.AddLoadedRow(row);

        ct.DeleteRow(row);

        Assert.Throws<System.InvalidOperationException>(
            () => EfChangesetCopyToPocoApplier.ApplyTable(db, db.Departments, ct));
    }

    [Fact]
    public void ApplyTableAndSave_DoesNotThrow_WhenDeletedRowDoesNotExistInDatabase()
    {
        using TestDbContext db = DbTestHelpers.CreateContext();

        // Seed only Id=1
        db.Departments.Add(new Department { Id = 1, Name = "Keep" });
        db.SaveChanges();

        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        // Loaded baseline row Id=999 (not in DB)
        IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r["Id"] = 999;
        r["Name"] = "Ghost";
        t.AddLoadedRow(r);

        // Delete it
        t.DeleteRow(r);

        IDataSet cs = ds.CreateChangeset();
        IDataTable ct = cs.Tables["Department"];

        // Should be idempotent (delete-if-exists)
        EfChangesetCopyToPocoApplier.ApplyTableAndSave(db, db.Departments, ct);

        // Ensure seeded row still exists
        Assert.Equal(1, db.Departments.Count());
    }
}
