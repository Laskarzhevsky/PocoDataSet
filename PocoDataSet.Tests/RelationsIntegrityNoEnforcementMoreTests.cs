using System.Collections.Generic;

using Xunit;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class RelationsIntegrityNoEnforcementMoreTests
    {
        [Fact]
        public void DeletingChild_DoesNotAffectParent()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable parent = dataSet.AddNewTable("Department");
            parent.AddColumn("Id", DataTypeNames.INT32);

            IDataTable child = dataSet.AddNewTable("Employee");
            child.AddColumn("Id", DataTypeNames.INT32);
            child.AddColumn("DepartmentId", DataTypeNames.INT32);

            dataSet.AddRelation("Department_Employee", "Department", new List<string> { "Id" }, "Employee", new List<string> { "DepartmentId" });

            IDataRow d = DataRowExtensions.CreateRowFromColumns(parent.Columns);
            d["Id"] = 10;
            parent.AddLoadedRow(d);

            IDataRow e = DataRowExtensions.CreateRowFromColumns(child.Columns);
            e["Id"] = 1;
            e["DepartmentId"] = 10;
            child.AddLoadedRow(e);

            child.DeleteRow(e);

            Assert.Equal(DataRowState.Unchanged, d.DataRowState);
            Assert.Equal(DataRowState.Deleted, e.DataRowState);
        }

        [Fact]
        public void OrphanChild_IsAllowed()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable parent = dataSet.AddNewTable("Department");
            parent.AddColumn("Id", DataTypeNames.INT32);

            IDataTable child = dataSet.AddNewTable("Employee");
            child.AddColumn("Id", DataTypeNames.INT32);
            child.AddColumn("DepartmentId", DataTypeNames.INT32);

            dataSet.AddRelation("Department_Employee", "Department", new List<string> { "Id" }, "Employee", new List<string> { "DepartmentId" });

            // No parent row with Id=999 exists, but we can still add child
            IDataRow e = DataRowExtensions.CreateRowFromColumns(child.Columns);
            e["Id"] = 1;
            e["DepartmentId"] = 999;
            child.AddRow(e);

            Assert.Equal(1, child.Rows.Count);
            Assert.Equal(999, (int)child.Rows[0]["DepartmentId"]!);
        }
    }
}
