using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.SqlServerDataAdapter;
using Xunit;

namespace PocoDataSet.SqlServerDataAdapterTests
{
    public class SqlDataAdapterRelationsInMemoryTests
    {
        [Fact]
        public void ApplyForeignKeyGroupsToDataSetRelations_DoesNotAdd_WhenReferencedTableMissing()
        {
            // Arrange
            DataSet dataSet = new DataSet();

            IDataTable child = dataSet.AddNewTable("Sale");
            child.AddColumn("Id", DataTypeNames.INT32);
            child.AddColumn("CustomerCode", DataTypeNames.STRING);

            ForeignKeyGroup group = new ForeignKeyGroup();
            group.ForeignKeyName = "FK_Sale_Customer_Code";
            group.ParentTableName = "Sale";
            group.ReferencedTableName = "Customer";
            group.ParentColumnNames.Add("CustomerCode");
            group.ReferencedColumnNames.Add("Code");

            List<ForeignKeyGroup> groups = new List<ForeignKeyGroup>();
            groups.Add(group);

            // Act
            SqlDataAdapter.ApplyForeignKeyGroupsToDataSetRelations(dataSet, groups);

            // Assert
            Assert.NotNull(dataSet.Relations);
            Assert.Empty(dataSet.Relations);
        }

        [Fact]
        public void ApplyForeignKeyGroupsToDataSetRelations_AddsRelation_WhenBothTablesExist()
        {
            // Arrange
            DataSet dataSet = new DataSet();

            IDataTable customer = dataSet.AddNewTable("Customer");
            customer.AddColumn("Id", DataTypeNames.INT32);
            customer.AddColumn("Code", DataTypeNames.STRING);

            IDataTable sale = dataSet.AddNewTable("Sale");
            sale.AddColumn("Id", DataTypeNames.INT32);
            sale.AddColumn("CustomerCode", DataTypeNames.STRING);

            ForeignKeyGroup group = new ForeignKeyGroup();
            group.ForeignKeyName = "FK_Sale_Customer_Code";
            group.ParentTableName = "Sale";
            group.ReferencedTableName = "Customer";
            group.ParentColumnNames.Add("CustomerCode");
            group.ReferencedColumnNames.Add("Code");

            List<ForeignKeyGroup> groups = new List<ForeignKeyGroup>();
            groups.Add(group);

            // Act
            SqlDataAdapter.ApplyForeignKeyGroupsToDataSetRelations(dataSet, groups);

            // Assert (real contract)
            Assert.NotNull(dataSet.Relations);
            Assert.Single(dataSet.Relations);

            IDataRelation relation = dataSet.Relations[0];
            Assert.Equal("FK_Sale_Customer_Code", relation.RelationName);
            Assert.Equal("Customer", relation.ParentTableName);
            Assert.Equal("Sale", relation.ChildTableName);
            Assert.Single(relation.ParentColumnNames);
            Assert.Single(relation.ChildColumnNames);
            Assert.Equal("Code", relation.ParentColumnNames[0]);
            Assert.Equal("CustomerCode", relation.ChildColumnNames[0]);
        }

        [Fact]
        public void ApplyForeignKeyGroupsToDataSetRelations_CompositeForeignKey_PreservesColumnOrder()
        {
            // Arrange
            DataSet dataSet = new DataSet();

            IDataTable order = dataSet.AddNewTable("Order");
            order.AddColumn("CompanyId", DataTypeNames.INT32);
            order.AddColumn("OrderNo", DataTypeNames.INT32);

            IDataTable line = dataSet.AddNewTable("OrderLine");
            line.AddColumn("CompanyId", DataTypeNames.INT32);
            line.AddColumn("OrderNo", DataTypeNames.INT32);
            line.AddColumn("LineNo", DataTypeNames.INT32);

            ForeignKeyGroup group = new ForeignKeyGroup();
            group.ForeignKeyName = "FK_OrderLine_Order";
            group.ParentTableName = "OrderLine";
            group.ReferencedTableName = "Order";
            group.ParentColumnNames.Add("CompanyId");
            group.ParentColumnNames.Add("OrderNo");
            group.ReferencedColumnNames.Add("CompanyId");
            group.ReferencedColumnNames.Add("OrderNo");

            List<ForeignKeyGroup> groups = new List<ForeignKeyGroup>();
            groups.Add(group);

            // Act
            SqlDataAdapter.ApplyForeignKeyGroupsToDataSetRelations(dataSet, groups);

            // Assert
            Assert.NotNull(dataSet.Relations);
            Assert.Single(dataSet.Relations);

            IDataRelation relation = dataSet.Relations[0];
            Assert.Equal("Order", relation.ParentTableName);
            Assert.Equal("OrderLine", relation.ChildTableName);

            Assert.Equal(2, relation.ParentColumnNames.Count);
            Assert.Equal(2, relation.ChildColumnNames.Count);

            Assert.Equal("CompanyId", relation.ParentColumnNames[0]);
            Assert.Equal("OrderNo", relation.ParentColumnNames[1]);

            Assert.Equal("CompanyId", relation.ChildColumnNames[0]);
            Assert.Equal("OrderNo", relation.ChildColumnNames[1]);
        }

        [Fact]
        public void ApplyForeignKeyGroupsToDataSetRelations_DoesNotAddDuplicateRelations_WhenCalledTwice()
        {
            // Arrange
            DataSet dataSet = new DataSet();

            IDataTable customer = dataSet.AddNewTable("Customer");
            customer.AddColumn("Code", DataTypeNames.STRING);

            IDataTable sale = dataSet.AddNewTable("Sale");
            sale.AddColumn("CustomerCode", DataTypeNames.STRING);

            ForeignKeyGroup group = new ForeignKeyGroup();
            group.ForeignKeyName = "FK_Sale_Customer_Code";
            group.ParentTableName = "Sale";
            group.ReferencedTableName = "Customer";
            group.ParentColumnNames.Add("CustomerCode");
            group.ReferencedColumnNames.Add("Code");

            List<ForeignKeyGroup> groups = new List<ForeignKeyGroup>();
            groups.Add(group);

            // Act
            SqlDataAdapter.ApplyForeignKeyGroupsToDataSetRelations(dataSet, groups);
            SqlDataAdapter.ApplyForeignKeyGroupsToDataSetRelations(dataSet, groups);

            // Assert
            Assert.NotNull(dataSet.Relations);
            Assert.Single(dataSet.Relations);
        }

        [Fact]
        public async Task FillIntoExistingDataSetAsync_AppendsTables_AndCanPopulateRelations_WithoutSqlServer()
        {
            // Arrange
            string connectionString = "Server=(fake);Database=(fake);Trusted_Connection=True;";
            SqlDataAdapter adapter = new SqlDataAdapter(connectionString);

            adapter.PopulateRelationsFromSchema = true;

            // Foreign key schema that the adapter would have loaded from SQL Server.
            // Parent = dependent (child) side, Referenced = principal (parent) side.
            ForeignKeyGroup fk = new ForeignKeyGroup();
            fk.ForeignKeyName = "FK_Employee_Department";
            fk.ParentTableName = "Employee";
            fk.ReferencedTableName = "Department";
            fk.ParentColumnNames.Add("DepartmentId");
            fk.ReferencedColumnNames.Add("Id");

            List<ForeignKeyGroup> fkGroups = new List<ForeignKeyGroup>();
            fkGroups.Add(fk);

            // In-memory FillOverride: emulates what FillAsync would do (add tables) without DB access.
            adapter.FillOverride = async (baseQuery, isStoredProcedure, parameters, returnedTableNames, cs, existing) =>
            {
                await Task.Yield();

                IDataSet ds = existing ?? DataSetFactory.CreateDataSet();

                if (baseQuery.Contains("Department", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ds.Tables.ContainsKey("Department"))
                    {
                        IDataTable t = ds.AddNewTable("Department");
                        t.AddColumn("Id", DataTypeNames.INT32);
                        t.AddColumn("Name", DataTypeNames.STRING);
                    }
                }

                if (baseQuery.Contains("Employee", StringComparison.OrdinalIgnoreCase))
                {
                    if (!ds.Tables.ContainsKey("Employee"))
                    {
                        IDataTable t = ds.AddNewTable("Employee");
                        t.AddColumn("Id", DataTypeNames.INT32);
                        t.AddColumn("DepartmentId", DataTypeNames.INT32);
                        t.AddColumn("Name", DataTypeNames.STRING);
                    }
                }

                if (adapter.PopulateRelationsFromSchema)
                {
                    SqlDataAdapter.ApplyForeignKeyGroupsToDataSetRelations(ds, fkGroups);
                }

                return ds;
            };

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // Act: two independent queries, same dataset
            await adapter.FillIntoExistingDataSetAsync(dataSet, "SELECT * FROM Department", false, null, null, null);
            await adapter.FillIntoExistingDataSetAsync(dataSet, "SELECT * FROM Employee", false, null, null, null);

            // Assert: both tables exist in the same dataset
            Assert.True(dataSet.Tables.ContainsKey("Department"));
            Assert.True(dataSet.Tables.ContainsKey("Employee"));

            // Relation should exist after both tables are present
            Assert.NotNull(dataSet.Relations);
            Assert.Single(dataSet.Relations);

            IDataRelation relation = dataSet.Relations[0];
            Assert.Equal("FK_Employee_Department", relation.RelationName);
            Assert.Equal("Department", relation.ParentTableName);
            Assert.Equal("Employee", relation.ChildTableName);
            Assert.Single(relation.ParentColumnNames);
            Assert.Single(relation.ChildColumnNames);
            Assert.Equal("Id", relation.ParentColumnNames[0]);
            Assert.Equal("DepartmentId", relation.ChildColumnNames[0]);
        }

        [Fact]
        public async Task FillIntoExistingDataSetAsync_ThrowsArgumentNullException_WhenDataSetIsNull()
        {
            // Arrange
            SqlDataAdapter adapter = new SqlDataAdapter("Server=(fake);Database=(fake);Trusted_Connection=True;");

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await adapter.FillIntoExistingDataSetAsync(null!, "SELECT 1", false, null, null, null);
            });
        }
    }
}
