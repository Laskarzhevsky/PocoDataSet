using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.SqlServerDataAdapter;
using Xunit;

namespace PocoDataSet.SqlServerDataAdapterTests
{
    public class SqlDataAdapterRelationsSchemaTests
    {
        [Fact]
        public void ApplyForeignKeyGroupsToDataSetRelations_AlternateKeyStyleRelation_AddsRelation()
        {
            // Arrange
            DataSet dataSet = new DataSet();

            // Create principal table with alternate key column 'Code'
            IDataTable customerTable = dataSet.AddNewTable("Customer");
            customerTable.AddColumn("Id", DataTypeNames.INT32);
            customerTable.AddColumn("Code", DataTypeNames.STRING);

            // Create dependent table with FK column 'CustomerCode'
            IDataTable saleTable = dataSet.AddNewTable("Sale");
            saleTable.AddColumn("Id", DataTypeNames.INT32);
            saleTable.AddColumn("CustomerCode", DataTypeNames.STRING);

            ForeignKeyGroup group = new ForeignKeyGroup();
            group.ForeignKeyName = "FK_Sale_Customer_Code";

            // NOTE:
            // In ForeignKeyGroup:
            // - ParentTableName/ParentColumnNames = dependent (child) side (Sale.CustomerCode)
            // - ReferencedTableName/ReferencedColumnNames = principal (parent) side (Customer.Code)
            group.ParentTableName = "Sale";
            group.ReferencedTableName = "Customer";
            group.ParentColumnNames.Add("CustomerCode");
            group.ReferencedColumnNames.Add("Code");

            List<ForeignKeyGroup> groups = new List<ForeignKeyGroup>();
            groups.Add(group);

            // Act
            SqlDataAdapter.ApplyForeignKeyGroupsToDataSetRelations(dataSet, groups);

            // Assert (test real contract: IDataSet.Relations is List<IDataRelation>)
            Assert.NotNull(dataSet.Relations);
            Assert.Single(dataSet.Relations);

            IDataRelation relation = dataSet.Relations[0];

            Assert.Equal("FK_Sale_Customer_Code", relation.RelationName);

            // Parent = referenced table (Customer)
            Assert.Equal("Customer", relation.ParentTableName);
            Assert.Single(relation.ParentColumnNames);
            Assert.Equal("Code", relation.ParentColumnNames[0]);

            // Child = dependent table (Sale)
            Assert.Equal("Sale", relation.ChildTableName);
            Assert.Single(relation.ChildColumnNames);
            Assert.Equal("CustomerCode", relation.ChildColumnNames[0]);
        }
    }
}
