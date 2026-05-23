using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;
using PocoDataSet.SqlServerDataAdapter;

using PocoDataRowState = PocoDataSet.IData.DataRowState;
using PocoDataSetModel = PocoDataSet.Data.DataSet;
using PocoDataRow = PocoDataSet.Data.DataRow;
using PocoDataTable = PocoDataSet.Data.DataTable;
using AdoDataTable = System.Data.DataTable;

namespace PocoDataSet.SqlServerDataAdapterTests
{
    public class SqlServerTableValuedParameterBuilderTests
    {
        [Fact]
        public void CreateDataTable_AddedModifiedDeletedRows_AddsStableChangeStateColumn()
        {
            PocoDataTable table = CreateCustomerTable();

            IDataRow addedRow = CreateRow(0, "Added customer");
            table.AddRow(addedRow);

            IDataRow modifiedRow = CreateRow(10, "Original customer");
            table.AddLoadedRow(modifiedRow);
            modifiedRow["Name"] = "Modified customer";

            IDataRow deletedRow = CreateRow(11, "Deleted customer");
            table.AddLoadedRow(deletedRow);
            deletedRow.Delete();

            IDataRow unchangedRow = CreateRow(12, "Unchanged customer");
            table.AddLoadedRow(unchangedRow);

            AdoDataTable adoDataTable = SqlServerTableValuedParameterBuilder.CreateDataTable(table);

            Assert.Equal("Customer", adoDataTable.TableName);
            Assert.True(adoDataTable.Columns.Contains("Id"));
            Assert.True(adoDataTable.Columns.Contains("Name"));
            Assert.True(adoDataTable.Columns.Contains(SqlServerChangeStateColumn.ColumnName));
            Assert.Equal(typeof(int), adoDataTable.Columns[SqlServerChangeStateColumn.ColumnName]!.DataType);

            Assert.Equal(3, adoDataTable.Rows.Count);
            Assert.Equal((int)SqlServerChangeState.Added, adoDataTable.Rows[0][SqlServerChangeStateColumn.ColumnName]);
            Assert.Equal((int)SqlServerChangeState.Modified, adoDataTable.Rows[1][SqlServerChangeStateColumn.ColumnName]);
            Assert.Equal((int)SqlServerChangeState.Deleted, adoDataTable.Rows[2][SqlServerChangeStateColumn.ColumnName]);
        }

        [Fact]
        public void CreateDataTables_MultipleTables_ReturnsOnlyTablesWithSaveableRows()
        {
            PocoDataSetModel dataSet = new PocoDataSetModel();

            PocoDataTable customerTable = CreateCustomerTable();
            customerTable.AddRow(CreateRow(0, "Added customer"));
            dataSet.AddTable(customerTable);

            PocoDataTable orderTable = new PocoDataTable();
            orderTable.TableName = "Order";
            orderTable.AddColumn("Id", "int", false, true, false);
            orderTable.AddColumn("CustomerId", "int", false, false, true);
            orderTable.AddColumn("Amount", "decimal", false, false, false);
            IDataRow orderRow = CreateOrderRow(100, 10, 25.50m);
            orderTable.AddLoadedRow(orderRow);
            orderRow["Amount"] = 30.00m;
            dataSet.AddTable(orderTable);

            PocoDataTable unchangedTable = CreateCustomerTable();
            unchangedTable.TableName = "UnchangedCustomer";
            unchangedTable.AddLoadedRow(CreateRow(20, "Unchanged customer"));
            dataSet.AddTable(unchangedTable);

            Dictionary<string, AdoDataTable> adoTables = SqlServerTableValuedParameterBuilder.CreateDataTables(dataSet);

            Assert.Equal(2, adoTables.Count);
            Assert.True(adoTables.ContainsKey("Customer"));
            Assert.True(adoTables.ContainsKey("Order"));
            Assert.False(adoTables.ContainsKey("UnchangedCustomer"));

            Assert.Equal((int)SqlServerChangeState.Added, adoTables["Customer"].Rows[0][SqlServerChangeStateColumn.ColumnName]);
            Assert.Equal((int)SqlServerChangeState.Modified, adoTables["Order"].Rows[0][SqlServerChangeStateColumn.ColumnName]);
        }

        [Fact]
        public void CreateStructuredParameter_CreatesSqlServerStructuredParameter()
        {
            PocoDataTable table = CreateCustomerTable();
            table.AddRow(CreateRow(0, "Added customer"));

            SqlParameter parameter = SqlServerTableValuedParameterBuilder.CreateStructuredParameter(
                "@Customer",
                "dbo.CustomerTableType",
                table);

            Assert.Equal("@Customer", parameter.ParameterName);
            Assert.Equal(SqlDbType.Structured, parameter.SqlDbType);
            Assert.Equal("dbo.CustomerTableType", parameter.TypeName);
            Assert.IsType<AdoDataTable>(parameter.Value);

            AdoDataTable adoDataTable = (AdoDataTable)parameter.Value;
            Assert.True(adoDataTable.Columns.Contains(SqlServerChangeStateColumn.ColumnName));
            Assert.Equal((int)SqlServerChangeState.Added, adoDataTable.Rows[0][SqlServerChangeStateColumn.ColumnName]);
        }

        [Fact]
        public void GetChangeState_UsesSqlServerContractValues_NotPocoEnumValues()
        {
            Assert.Equal(SqlServerChangeState.Added, SqlServerTableValuedParameterBuilder.GetChangeState(PocoDataRowState.Added));
            Assert.Equal(SqlServerChangeState.Modified, SqlServerTableValuedParameterBuilder.GetChangeState(PocoDataRowState.Modified));
            Assert.Equal(SqlServerChangeState.Deleted, SqlServerTableValuedParameterBuilder.GetChangeState(PocoDataRowState.Deleted));
            Assert.Equal(SqlServerChangeState.Unchanged, SqlServerTableValuedParameterBuilder.GetChangeState(PocoDataRowState.Unchanged));
            Assert.Equal(SqlServerChangeState.Unchanged, SqlServerTableValuedParameterBuilder.GetChangeState(PocoDataRowState.Detached));

            Assert.Equal(1, (int)SqlServerChangeState.Added);
            Assert.Equal(2, (int)SqlServerChangeState.Modified);
            Assert.Equal(3, (int)SqlServerChangeState.Deleted);
        }

        [Fact]
        public void CreateDataTable_ReservedChangeStateColumnName_Throws()
        {
            PocoDataTable table = CreateCustomerTable();
            table.AddColumn(SqlServerChangeStateColumn.ColumnName, "int", false, false, false);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                SqlServerTableValuedParameterBuilder.CreateDataTable(table);
            });

            Assert.Contains(SqlServerChangeStateColumn.ColumnName, exception.Message);
        }

        static PocoDataTable CreateCustomerTable()
        {
            PocoDataTable table = new PocoDataTable();
            table.TableName = "Customer";
            table.AddColumn("Id", "int", false, true, false);
            table.AddColumn("Name", "nvarchar", true, false, false);
            return table;
        }

        static IDataRow CreateRow(int id, string name)
        {
#pragma warning disable CS0618
            IDataRow row = new PocoDataRow();
#pragma warning restore CS0618
            row["Id"] = id;
            row["Name"] = name;
            return row;
        }

        static IDataRow CreateOrderRow(int id, int customerId, decimal amount)
        {
#pragma warning disable CS0618
            IDataRow row = new PocoDataRow();
#pragma warning restore CS0618
            row["Id"] = id;
            row["CustomerId"] = customerId;
            row["Amount"] = amount;
            return row;
        }
    }
}
