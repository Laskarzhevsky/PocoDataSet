using System.Collections.Generic;
using System.Linq;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public sealed class EfChangesetCopyToPocoApplier_CompositeKey_Tests
{
    [Fact]
    public void ApplyTableAndSave_UpdatesAndDeletes_CompositeKey()
    {
        // Arrange
        using TestDbContext db = DbTestHelpers.CreateContext();

        db.OrderLines.Add(new OrderLine { OrderId = 10, LineNo = 1, Sku = "OLD" });
        db.OrderLines.Add(new OrderLine { OrderId = 10, LineNo = 2, Sku = "DEL" });
        db.SaveChanges();

        // Build PocoDataSet baseline
        IDataSet ds = DataSetFactory.CreateDataSet();
        IDataTable t = ds.AddNewTable("OrderLine");
        t.AddColumn("OrderId", DataTypeNames.INT32);
        t.AddColumn("LineNo", DataTypeNames.INT32);
        t.AddColumn("Sku", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "OrderId", "LineNo" };

        // Loaded row (10,1)
        IDataRow r1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r1["OrderId"] = 10;
        r1["LineNo"] = 1;
        r1["Sku"] = "OLD";
        t.AddLoadedRow(r1);

        // Loaded row (10,2) -> delete
        IDataRow r2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
        r2["OrderId"] = 10;
        r2["LineNo"] = 2;
        r2["Sku"] = "DEL";
        t.AddLoadedRow(r2);

        // Modify (10,1)
        r1["Sku"] = "NEW";

        // Delete (10,2)
        t.DeleteRow(r2);

        // Add (11,1)
        IDataRow r3 = t.AddNewRow();
        r3["OrderId"] = 11;
        r3["LineNo"] = 1;
        r3["Sku"] = "ADD";

        IDataSet cs = ds.CreateChangeset();
        cs.Tables["OrderLine"].PrimaryKeys = new List<string> { "OrderId", "LineNo" };

        // Act
        EfChangesetCopyToPocoApplier.ApplyTableAndSave(db, db.OrderLines, cs.Tables["OrderLine"]);

        // Assert
        List<OrderLine> lines = db.OrderLines
            .OrderBy(x => x.OrderId)
            .ThenBy(x => x.LineNo)
            .ToList();

        Assert.Equal(2, lines.Count);

        Assert.Equal(10, lines[0].OrderId);
        Assert.Equal(1, lines[0].LineNo);
        Assert.Equal("NEW", lines[0].Sku);

        Assert.Equal(11, lines[1].OrderId);
        Assert.Equal(1, lines[1].LineNo);
        Assert.Equal("ADD", lines[1].Sku);
    }
}
