using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;
using PocoDataSet.SqlServerDataAdapter;

using PocoDataRowState = PocoDataSet.IData.DataRowState;
using PocoDataSetModel = PocoDataSet.Data.DataSet;
using PocoDataRow = PocoDataSet.Data.DataRow;
using PocoDataTable = PocoDataSet.Data.DataTable;
using PocoSqlDataAdapter = PocoDataSet.SqlServerDataAdapter.SqlDataAdapter;
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

            AdoDataTable adoDataTable = SqlServerTableValuedParameterBuilder.CreateDataTable(table, true);

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
        public void CreateDataTable_SearchMode_IncludesUnchangedRowsAndAdapterMetadataColumns()
        {
            PocoDataTable table = CreateHostedApplicationLayerTable();

            IDataRow row = CreateHostedApplicationLayerRow("HR", "Accounting", "DAL", "http://localhost:5075");
            table.AddLoadedRow(row);

            AdoDataTable adoDataTable = SqlServerTableValuedParameterBuilder.CreateDataTable(table, false);

            Assert.Equal("HostedApplicationLayer", adoDataTable.TableName);
            Assert.True(adoDataTable.Columns.Contains("DomainName"));
            Assert.True(adoDataTable.Columns.Contains("UseCaseName"));
            Assert.True(adoDataTable.Columns.Contains("ApplicationLayerName"));
            Assert.True(adoDataTable.Columns.Contains("Url"));
            Assert.True(adoDataTable.Columns.Contains(SqlServerClientKeyColumn.ColumnName));
            Assert.True(adoDataTable.Columns.Contains(SqlServerChangeStateColumn.ColumnName));

            Assert.Equal(1, adoDataTable.Rows.Count);
            Assert.Equal("HR", adoDataTable.Rows[0]["DomainName"]);
            Assert.Equal("Accounting", adoDataTable.Rows[0]["UseCaseName"]);
            Assert.Equal("DAL", adoDataTable.Rows[0]["ApplicationLayerName"]);
            Assert.Equal((int)SqlServerChangeState.Unchanged, adoDataTable.Rows[0][SqlServerChangeStateColumn.ColumnName]);
            Assert.NotEqual(DBNull.Value, adoDataTable.Rows[0][SqlServerClientKeyColumn.ColumnName]);
        }

        [Fact]
        public void AdapterCreateTableValuedParameter_FullEntityTable_CreatesStructuredParameter()
        {
            PocoSqlDataAdapter adapter = new PocoSqlDataAdapter("Server=(fake);Database=(fake);Trusted_Connection=True;");
            PocoDataTable table = CreateHostedApplicationLayerTable();
            table.AddLoadedRow(CreateHostedApplicationLayerRow("HR", "Accounting", "DPL", "http://localhost:5075"));

            SqlParameter parameter = adapter.CreateTableValuedParameter(
                "@HostedApplicationLayer",
                "dbo.HostedApplicationLayer",
                table);

            Assert.Equal("@HostedApplicationLayer", parameter.ParameterName);
            Assert.Equal(SqlDbType.Structured, parameter.SqlDbType);
            Assert.Equal("dbo.HostedApplicationLayer", parameter.TypeName);

            AdoDataTable adoDataTable = Assert.IsType<AdoDataTable>(parameter.Value);
            Assert.True(adoDataTable.Columns.Contains("DomainName"));
            Assert.True(adoDataTable.Columns.Contains("UseCaseName"));
            Assert.True(adoDataTable.Columns.Contains("ApplicationLayerName"));
            Assert.True(adoDataTable.Columns.Contains(SqlServerClientKeyColumn.ColumnName));
            Assert.True(adoDataTable.Columns.Contains(SqlServerChangeStateColumn.ColumnName));
            Assert.Equal(1, adoDataTable.Rows.Count);
        }

        [Fact]
        public void SqlDataAdapter_ExposesFillIntoExistingDataSetAsync_WithSqlParameterArrayOverload()
        {
            Type adapterType = typeof(PocoSqlDataAdapter);

            Type[] parameterTypes = new Type[]
            {
                typeof(IDataSet),
                typeof(string),
                typeof(bool),
                typeof(SqlParameter[])
            };

            System.Reflection.MethodInfo? method = adapterType.GetMethod(
                "FillIntoExistingDataSetAsync",
                parameterTypes);

            Assert.NotNull(method);
        }

        [Fact]
        public void CreateDataTable_WithSqlTypeSchema_AddsMissingSystemColumnsAndIgnoresExtraColumns()
        {
            PocoDataTable table = CreateHostedApplicationLayerRequestTable();

#pragma warning disable CS0618
            IDataRow row = new PocoDataRow();
#pragma warning restore CS0618
            row["ApplicationLayerName"] = "DAL";
            row["DomainName"] = "HR";
            row["Url"] = "http://localhost:5075";
            row["UseCaseName"] = "Accounting";
            row["IsApplicationUseCaseLayer"] = true;
            table.AddLoadedRow(row);

            AdoDataTable adoDataTable = SqlServerTableValuedParameterBuilder.CreateDataTable(
                table,
                CreateHostedApplicationLayerSqlTypeColumns(),
                false);

            Assert.Equal(18, adoDataTable.Columns.Count);
            Assert.Equal("Id", adoDataTable.Columns[0]!.ColumnName);
            Assert.Equal("ApplicationLayerName", adoDataTable.Columns[1]!.ColumnName);
            Assert.Equal("DomainName", adoDataTable.Columns[2]!.ColumnName);
            Assert.Equal("Guid", adoDataTable.Columns[3]!.ColumnName);
            Assert.Equal("Url", adoDataTable.Columns[4]!.ColumnName);
            Assert.Equal("UseCaseName", adoDataTable.Columns[5]!.ColumnName);
            Assert.Equal("__ClientKey", adoDataTable.Columns[16]!.ColumnName);
            Assert.Equal("__ChangeState", adoDataTable.Columns[17]!.ColumnName);
            Assert.False(adoDataTable.Columns.Contains("IsApplicationUseCaseLayer"));

            Assert.Single(adoDataTable.Rows);
            Assert.Equal("DAL", adoDataTable.Rows[0]["ApplicationLayerName"]);
            Assert.Equal("HR", adoDataTable.Rows[0]["DomainName"]);
            Assert.Equal("http://localhost:5075", adoDataTable.Rows[0]["Url"]);
            Assert.Equal("Accounting", adoDataTable.Rows[0]["UseCaseName"]);
            Assert.Equal(DBNull.Value, adoDataTable.Rows[0]["Id"]);
            Assert.Equal(DBNull.Value, adoDataTable.Rows[0]["Guid"]);
            Assert.IsType<Guid>(adoDataTable.Rows[0]["__ClientKey"]);
            Assert.Equal((int)SqlServerChangeState.Unchanged, adoDataTable.Rows[0]["__ChangeState"]);
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


        static PocoDataTable CreateHostedApplicationLayerTable()
        {
            PocoDataTable table = new PocoDataTable();
            table.TableName = "HostedApplicationLayer";
            table.AddColumn("Id", "bigint", true, true, false);
            table.AddColumn("ApplicationLayerName", "varchar", true, false, false);
            table.AddColumn("DomainName", "varchar", true, false, false);
            table.AddColumn("Guid", "uniqueidentifier", true, false, false);
            table.AddColumn("Url", "varchar", true, false, false);
            table.AddColumn("UseCaseName", "varchar", true, false, false);
            table.AddColumn("BusinessGuid", "uniqueidentifier", true, false, false);
            table.AddColumn("BusinessStringRepresentation", "nvarchar", true, false, false);
            table.AddColumn("CreatedByUserGuid", "uniqueidentifier", true, false, false);
            table.AddColumn("CreatedByUserName", "varchar", true, false, false);
            table.AddColumn("DateOfCreation", "datetime2", true, false, false);
            table.AddColumn("DateOfModification", "datetime2", true, false, false);
            table.AddColumn("IsArchived", "bit", true, false, false);
            table.AddColumn("IsDeleted", "bit", true, false, false);
            table.AddColumn("ModifiedByUserGuid", "uniqueidentifier", true, false, false);
            table.AddColumn("ModifiedByUserName", "varchar", true, false, false);
            return table;
        }

        static IDataRow CreateHostedApplicationLayerRow(string domainName, string useCaseName, string applicationLayerName, string url)
        {
#pragma warning disable CS0618
            IDataRow row = new PocoDataRow();
#pragma warning restore CS0618
            row["DomainName"] = domainName;
            row["UseCaseName"] = useCaseName;
            row["ApplicationLayerName"] = applicationLayerName;
            row["Url"] = url;
            return row;
        }

        static PocoDataTable CreateHostedApplicationLayerRequestTable()
        {
            PocoDataTable table = new PocoDataTable();
            table.TableName = "IHostedApplicationLayer";
            table.AddColumn("ApplicationLayerName", "varchar", true, false, false);
            table.AddColumn("DomainName", "varchar", true, false, false);
            table.AddColumn("Url", "varchar", true, false, false);
            table.AddColumn("UseCaseName", "varchar", true, false, false);
            table.AddColumn("IsApplicationUseCaseLayer", "bit", false, false, false);
            return table;
        }

        static List<SqlServerTableValuedParameterColumn> CreateHostedApplicationLayerSqlTypeColumns()
        {
            List<SqlServerTableValuedParameterColumn> columns = new List<SqlServerTableValuedParameterColumn>();
            columns.Add(new SqlServerTableValuedParameterColumn("Id", "bigint"));
            columns.Add(new SqlServerTableValuedParameterColumn("ApplicationLayerName", "varchar"));
            columns.Add(new SqlServerTableValuedParameterColumn("DomainName", "varchar"));
            columns.Add(new SqlServerTableValuedParameterColumn("Guid", "uniqueidentifier"));
            columns.Add(new SqlServerTableValuedParameterColumn("Url", "varchar"));
            columns.Add(new SqlServerTableValuedParameterColumn("UseCaseName", "varchar"));
            columns.Add(new SqlServerTableValuedParameterColumn("BusinessGuid", "uniqueidentifier"));
            columns.Add(new SqlServerTableValuedParameterColumn("BusinessStringRepresentation", "nvarchar"));
            columns.Add(new SqlServerTableValuedParameterColumn("CreatedByUserGuid", "uniqueidentifier"));
            columns.Add(new SqlServerTableValuedParameterColumn("CreatedByUserName", "varchar"));
            columns.Add(new SqlServerTableValuedParameterColumn("DateOfCreation", "datetime2"));
            columns.Add(new SqlServerTableValuedParameterColumn("DateOfModification", "datetime2"));
            columns.Add(new SqlServerTableValuedParameterColumn("IsArchived", "bit"));
            columns.Add(new SqlServerTableValuedParameterColumn("IsDeleted", "bit"));
            columns.Add(new SqlServerTableValuedParameterColumn("ModifiedByUserGuid", "uniqueidentifier"));
            columns.Add(new SqlServerTableValuedParameterColumn("ModifiedByUserName", "varchar"));
            columns.Add(new SqlServerTableValuedParameterColumn("__ClientKey", "uniqueidentifier"));
            columns.Add(new SqlServerTableValuedParameterColumn("__ChangeState", "int"));
            return columns;
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
