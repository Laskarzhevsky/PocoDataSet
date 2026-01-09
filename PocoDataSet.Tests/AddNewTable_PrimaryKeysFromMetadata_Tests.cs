using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    public  class AddNewTable_PrimaryKeysFromMetadata_Tests
    {
        [Fact]
        public void AddNewTable_Fills_PrimaryKeys_From_ColumnMetadata_IsPrimaryKey_Flags()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            List<IColumnMetadata> columns = new List<IColumnMetadata>();

            ColumnMetadata id1 = new ColumnMetadata();
            id1.ColumnName = "TenantId";
            id1.DataType = DataTypeNames.INT32;
            id1.IsPrimaryKey = true;
            id1.IsNullable = false;
            columns.Add(id1);

            ColumnMetadata id2 = new ColumnMetadata();
            id2.ColumnName = "Code";
            id2.DataType = DataTypeNames.STRING;
            id2.IsPrimaryKey = true;
            id2.IsNullable = false;
            columns.Add(id2);

            ColumnMetadata name = new ColumnMetadata();
            name.ColumnName = "Name";
            name.DataType = DataTypeNames.STRING;
            name.IsNullable = true;
            columns.Add(name);

            IDataTable table = dataSet.AddNewTable("TenantEntity", columns);

            Assert.NotNull(table.PrimaryKeys);
            Assert.Equal(2, table.PrimaryKeys.Count);
            Assert.Equal("TenantId", table.PrimaryKeys[0]);
            Assert.Equal("Code", table.PrimaryKeys[1]);
        }
    }
}
