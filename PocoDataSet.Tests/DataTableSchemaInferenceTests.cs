using System;
using Xunit;
using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public class DataTableSchemaInferenceTests
    {
        [Fact]
        public void AddColumn_Id_InfersPrimaryKey_AndNotNullable()
        {
            DataTable table = new DataTable();
            table.TableName = "Employee";

            IColumnMetadata id = table.AddColumn("Id", DataTypeNames.INT32);

            Assert.True(id.IsPrimaryKey);
            Assert.False(id.IsNullable);
            Assert.False(id.IsForeignKey);
        }

        [Fact]
        public void AddColumn_DepartmentId_InfersForeignKey_ToDepartment_Id()
        {
            DataTable table = new DataTable();
            table.TableName = "Employee";

            IColumnMetadata fk = table.AddColumn("DepartmentId", DataTypeNames.INT32);

            Assert.False(fk.IsPrimaryKey);
            Assert.True(fk.IsNullable); // default nullable for non-PK
            Assert.True(fk.IsForeignKey);
            Assert.Equal("Department", fk.ReferencedTableName);
            Assert.Equal("Id", fk.ReferencedColumnName);
        }

        [Fact]
        public void AddColumn_ForeignKeyInference_CanBeDisabledExplicitly()
        {
            DataTable table = new DataTable();
            table.TableName = "Employee";

            IColumnMetadata fk = table.AddColumn("DepartmentId", DataTypeNames.INT32, isForeignKey: false);

            Assert.False(fk.IsPrimaryKey);
            Assert.False(fk.IsForeignKey);
            Assert.Null(fk.ReferencedTableName);
            Assert.Null(fk.ReferencedColumnName);
        }

        [Fact]
        public void AddColumn_NullabilityCanBeForced_ForPrimaryKey()
        {
            DataTable table = new DataTable();
            table.TableName = "Employee";

            // Even though Id infers PK, caller can override nullability explicitly.
            IColumnMetadata id = table.AddColumn("Id", DataTypeNames.INT32, isNullable: true);

            Assert.True(id.IsPrimaryKey);
            Assert.True(id.IsNullable);
        }

        [Fact]
        public void AddColumn_PrimaryKeyInference_CanBeDisabledExplicitly()
        {
            DataTable table = new DataTable();
            table.TableName = "Employee";

            IColumnMetadata id = table.AddColumn("Id", DataTypeNames.INT32, isPrimaryKey: false);

            Assert.False(id.IsPrimaryKey);
            Assert.False(id.IsForeignKey); // still must not infer FK for Id
        }
    }
}
