using System.Collections.Generic;
using System.Linq;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetToPocoApplier_NoOpAndCompositeKeyPatch_Tests
{
    [Fact]
    public void ApplyTableAndSave_NoOpModifiedRow_DoesNotChangeEntity()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.Departments.Add(new Department { Id = 1, Name = "Sales", Description = "Keep" });
        db.SaveChanges();

        IDataSet cs = DataSetFactory.CreateDataSet();
        IDataTable t = cs.AddNewTable("Department");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);

        IFloatingDataRow patchRow = DataRowExtensions.CreateFloatingRow();
        patchRow["Id"] = 1;
        patchRow.SetDataRowState(DataRowState.Modified);
        t.AddLoadedRow(patchRow);

        // Act
        EfChangesetCopyToPocoApplier.ApplyTableAndSave(db, db.Departments, t);

        // Assert
        Department d = db.Departments.Single(x => x.Id == 1);
        Assert.Equal("Sales", d.Name);
        Assert.Equal("Keep", d.Description);
    }

    [Fact]
    public void ApplyTableAndSave_CompositeKeyPatch_DoesNotOverwriteMissingField()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.OrderLines.Add(new OrderLine { OrderId = 10, LineNo = 1, Sku = "OLD", Description = "Keep" });
        db.SaveChanges();

        IDataSet cs = DataSetFactory.CreateDataSet();
        IDataTable t = cs.AddNewTable("OrderLine");
        t.AddColumn("OrderId", DataTypeNames.INT32, false, true);
        t.AddColumn("LineNo", DataTypeNames.INT32, false, true);
        t.AddColumn("Sku", DataTypeNames.STRING);
        t.AddColumn("Description", DataTypeNames.STRING);

        IFloatingDataRow patchRow = DataRowExtensions.CreateFloatingRow();
        patchRow["OrderId"] = 10;
        patchRow["LineNo"] = 1;
        patchRow["Sku"] = "NEW";
        // NOTE: Description intentionally NOT provided (floating semantics)
        patchRow.SetDataRowState(DataRowState.Modified);
        t.AddLoadedRow(patchRow);

        // Act
        EfChangesetCopyToPocoApplier.ApplyTableAndSave(db, db.OrderLines, t);

        // Assert
        OrderLine line = db.OrderLines.Single(x => x.OrderId == 10 && x.LineNo == 1);
        Assert.Equal("NEW", line.Sku);
        Assert.Equal("Keep", line.Description);
    }
}
