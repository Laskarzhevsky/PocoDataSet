using System.Collections.Generic;

using Xunit;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class RelationsIntegrityNoEnforcementTests
    {
        [Fact]
        public void DeletingParent_DoesNotCascadeOrRestrictChild()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable dept = dataSet.AddNewTable("Department");
            dept.AddColumn("Id", DataTypeNames.INT32);
            dept.AddColumn("Name", DataTypeNames.STRING);

            IDataTable emp = dataSet.AddNewTable("Employee");
            emp.AddColumn("Id", DataTypeNames.INT32);
            emp.AddColumn("DepartmentId", DataTypeNames.INT32);
            emp.AddColumn("Name", DataTypeNames.STRING);

            // Add relation metadata (no behavior enforced today)
            IDataRelation relation = new DataRelation();
            relation.RelationName = "Department_Employee";
            relation.ParentTable = "Department";
            relation.ChildTable = "Employee";
            relation.ParentColumns = new List<string> { "Id" };
            relation.ChildColumns = new List<string> { "DepartmentId" };
            dataSet.Relations.Add(relation);

            IDataRow d1 = DataRowExtensions.CreateRowFromColumns(dept.Columns);
            d1["Id"] = 10;
            d1["Name"] = "HR";
            dept.AddLoadedRow(d1);

            IDataRow e1 = DataRowExtensions.CreateRowFromColumns(emp.Columns);
            e1["Id"] = 1;
            e1["DepartmentId"] = 10;
            e1["Name"] = "Alice";
            emp.AddLoadedRow(e1);

            // Act: delete parent row (soft delete)
            dept.DeleteRow(d1);

            // Assert: parent is deleted, child untouched
            Assert.Equal(DataRowState.Deleted, d1.DataRowState);
            Assert.Equal(1, emp.Rows.Count);
            Assert.Equal(DataRowState.Unchanged, emp.Rows[0].DataRowState);
            Assert.Equal(10, (int)emp.Rows[0]["DepartmentId"]!);

            // Act: accept deletion at table level (physical removal)
            dept.AcceptChanges();

            // Assert: department row removed; employee still present
            Assert.Equal(0, dept.Rows.Count);
            Assert.Equal(1, emp.Rows.Count);
            Assert.Equal(10, (int)emp.Rows[0]["DepartmentId"]!);
        }
    }
}
