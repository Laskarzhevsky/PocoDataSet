using System;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class SqlServerIntegration_DeleteChangesetTests
    {
        [Fact(Skip = "Temporarily disabled feature")]
        public async Task SaveChangesAsync_DeletedRow_IssuesDeleteInDatabase()
        {
            string? masterConnectionString = Environment.GetEnvironmentVariable("POCODATASET_TEST_SQLSERVER");
            if (string.IsNullOrWhiteSpace(masterConnectionString))
            {
                // Optional integration test.
                return;
            }

            string dbName = "PocoDataSet_Test_" + Guid.NewGuid().ToString("N");
            string dbConnectionString = BuildDatabaseConnectionString(masterConnectionString, dbName);

            try
            {
                await CreateDatabaseAsync(masterConnectionString, dbName);
                await CreateDepartmentTableAsync(dbConnectionString);

                // Insert one row directly
                int insertedId = await InsertDepartmentAsync(dbConnectionString, "ToDelete");
                Assert.True(insertedId > 0);

                // Build dataset that represents loaded row
                IDataSet dataSet = DataSetFactory.CreateDataSet();
                IDataTable table = dataSet.AddNewTable("Department");
                table.AddColumn("Id", DataTypeNames.INT32);
                table.AddColumn("Name", DataTypeNames.STRING);

                IDataRow loaded = DataRowExtensions.CreateRowFromColumns(table.Columns);
                loaded["Id"] = insertedId;
                loaded["Name"] = "ToDelete";
                table.AddLoadedRow(loaded);

                // Mark as deleted and create changeset
                table.DeleteRow(loaded);
                IDataSet? changeset = dataSet.CreateChangeset();
                Assert.NotNull(changeset);

                // Act: save changeset -> should delete
                SqlServerDataAdapter.SqlDataAdapter adapter = new SqlServerDataAdapter.SqlDataAdapter(dbConnectionString);
                int affected = await adapter.SaveChangesAsync(changeset!, dbConnectionString);

                // Assert
                Assert.Equal(1, affected);

                int count = await CountDepartmentByIdAsync(dbConnectionString, insertedId);
                Assert.Equal(0, count);
            }
            finally
            {
                try
                {
                    await DropDatabaseAsync(masterConnectionString, dbName);
                }
                catch { }
            }
        }

        private static string BuildDatabaseConnectionString(string masterConnectionString, string databaseName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(masterConnectionString);
            builder.InitialCatalog = databaseName;
            return builder.ToString();
        }

        private static async Task CreateDatabaseAsync(string masterConnectionString, string dbName)
        {
            using (SqlConnection cn = new SqlConnection(masterConnectionString))
            {
                await cn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("CREATE DATABASE [" + dbName + "]", cn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static async Task CreateDepartmentTableAsync(string dbConnectionString)
        {
            string sql =
@"CREATE TABLE [dbo].[Department](
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Department] PRIMARY KEY,
    [Name] NVARCHAR(50) NULL,
    [RowVersion] ROWVERSION NOT NULL
);";

            using (SqlConnection cn = new SqlConnection(dbConnectionString))
            {
                await cn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static async Task<int> InsertDepartmentAsync(string dbConnectionString, string name)
        {
            string sql = "INSERT INTO [dbo].[Department]([Name]) OUTPUT INSERTED.[Id] VALUES(@name);";

            using (SqlConnection cn = new SqlConnection(dbConnectionString))
            {
                await cn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@name", name);

                    object? value = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(value);
                }
            }
        }

        private static async Task<int> CountDepartmentByIdAsync(string dbConnectionString, int id)
        {
            using (SqlConnection cn = new SqlConnection(dbConnectionString))
            {
                await cn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM [dbo].[Department] WHERE [Id] = @id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    object? value = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(value);
                }
            }
        }

        private static async Task DropDatabaseAsync(string masterConnectionString, string dbName)
        {
            using (SqlConnection cn = new SqlConnection(masterConnectionString))
            {
                await cn.OpenAsync();

                string sql =
@"IF DB_ID(@db) IS NOT NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'ALTER DATABASE [' + @db + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [' + @db + N'];';
    EXEC(@sql);
END";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@db", dbName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
