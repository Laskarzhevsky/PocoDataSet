using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;
using PocoDataSet.SqlServerDataAdapter;

using PocoDataTable = PocoDataSet.Data.DataTable;
using PocoSqlDataAdapter = PocoDataSet.SqlServerDataAdapter.SqlDataAdapter;

namespace PocoDataSet.SqlServerDataAdapter.IntegrationTests
{
    public class HostedApplicationLayerSynchronizationIntegrationTests : IClassFixture<SqlServerIntegrationTestFixture>
    {
        const string TableValuedParameterName = "@HostedApplicationLayer";
        const string TableValuedParameterTypeName = "dbo.HostedApplicationLayer";
        const string SynchronizeStoredProcedureName = "[HostedApplicationLayer].[SynchronizeHostedApplicationLayers]";
        const string FindStoredProcedureName = "[HostedApplicationLayer].[FindHostedApplicationLayersByIdentifiers]";

        readonly SqlServerIntegrationTestFixture Fixture;

        public HostedApplicationLayerSynchronizationIntegrationTests(SqlServerIntegrationTestFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public async Task ExecuteNonQueryAsync_WithHostedApplicationLayerTvp_SynchronizesInsertRefreshDeleteAndReactivate()
        {
            string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 12);
            string domainName = "TVPTest";
            string useCaseName = "Accounting" + uniqueSuffix;
            string url = "http://integration-test/" + uniqueSuffix;
            Guid testUserGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");

            await DeleteTestRowsAsync(domainName, useCaseName, url).ConfigureAwait(false);

            try
            {
                await SynchronizeAsync(domainName, useCaseName, url, new string[] { "DAL", "DPL" }, testUserGuid, "IntegrationTest_Insert")
                    .ConfigureAwait(false);

                int activeCount = await CountRowsAsync(domainName, useCaseName, null).ConfigureAwait(false);
                Assert.Equal(2, activeCount);

                int deletedCount = await CountRowsAsync(domainName, useCaseName, true).ConfigureAwait(false);
                Assert.Equal(0, deletedCount);

                DateTime firstDalModificationDate = await GetDateOfModificationAsync(domainName, useCaseName, "DAL").ConfigureAwait(false);

                await Task.Delay(50).ConfigureAwait(false);

                await SynchronizeAsync(domainName, useCaseName, url, new string[] { "DAL", "DPL" }, testUserGuid, "IntegrationTest_Refresh")
                    .ConfigureAwait(false);

                DateTime secondDalModificationDate = await GetDateOfModificationAsync(domainName, useCaseName, "DAL").ConfigureAwait(false);
                Assert.True(secondDalModificationDate > firstDalModificationDate);

                string modifiedByUserName = await GetModifiedByUserNameAsync(domainName, useCaseName, "DAL").ConfigureAwait(false);
                Assert.Equal("IntegrationTest_Refresh", modifiedByUserName);

                await SynchronizeAsync(domainName, useCaseName, url, new string[] { "DPL" }, testUserGuid, "IntegrationTest_DeleteMissing")
                    .ConfigureAwait(false);

                bool dalIsDeleted = await GetIsDeletedAsync(domainName, useCaseName, "DAL").ConfigureAwait(false);
                bool dplIsDeleted = await GetIsDeletedAsync(domainName, useCaseName, "DPL").ConfigureAwait(false);

                Assert.True(dalIsDeleted);
                Assert.False(dplIsDeleted);

                await SynchronizeAsync(domainName, useCaseName, url, new string[] { "DAL", "DPL" }, testUserGuid, "IntegrationTest_Reactivate")
                    .ConfigureAwait(false);

                dalIsDeleted = await GetIsDeletedAsync(domainName, useCaseName, "DAL").ConfigureAwait(false);
                dplIsDeleted = await GetIsDeletedAsync(domainName, useCaseName, "DPL").ConfigureAwait(false);

                Assert.False(dalIsDeleted);
                Assert.False(dplIsDeleted);

                int totalCount = await CountAllRowsAsync(domainName, useCaseName).ConfigureAwait(false);
                Assert.Equal(2, totalCount);

                int logCount = await CountLogRowsAsync(url).ConfigureAwait(false);
                Assert.True(logCount >= 2);
            }
            finally
            {
                await DeleteTestRowsAsync(domainName, useCaseName, url).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task FillAsync_WithHostedApplicationLayerTvp_ReturnsActiveRows()
        {
            string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 12);
            string domainName = "TVPTest";
            string useCaseName = "Read" + uniqueSuffix;
            string url = "http://integration-test/" + uniqueSuffix;
            Guid testUserGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");

            await DeleteTestRowsAsync(domainName, useCaseName, url).ConfigureAwait(false);

            try
            {
                await SynchronizeAsync(domainName, useCaseName, url, new string[] { "DAL", "DPL" }, testUserGuid, "IntegrationTest_Fill")
                    .ConfigureAwait(false);

                PocoDataTable requestTable = CreateRequestTable(domainName, useCaseName, url, new string[] { "DAL", "DPL" });

                SqlTableValuedParameterInfo[] tableValuedParameters = CreateTableValuedParameterInfos(requestTable);

                List<string> returnedTableNames = new List<string>();
                returnedTableNames.Add("HostedApplicationLayer");

                PocoSqlDataAdapter adapter = new PocoSqlDataAdapter(Fixture.Connection);
                adapter.PopulateRelationsFromSchema = false;

                IDataSet resultDataSet = await adapter.FillAsync(
                    FindStoredProcedureName,
                    true,
                    tableValuedParameters,
                    returnedTableNames,
                    null,
                    null).ConfigureAwait(false);

                IDataTable resultTable = resultDataSet.Tables["HostedApplicationLayer"];

                Assert.Equal(2, resultTable.Rows.Count);
            }
            finally
            {
                await DeleteTestRowsAsync(domainName, useCaseName, url).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task ExecuteNonQueryAsync_WithTvpAndScalarParameters_PassesBothParameterTypes()
        {
            string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 12);
            string domainName = "TVPTest";
            string useCaseName = "Scalar" + uniqueSuffix;
            string url = "http://integration-test/" + uniqueSuffix;
            Guid testUserGuid = Guid.Parse("33333333-3333-3333-3333-333333333333");

            await DeleteTestRowsAsync(domainName, useCaseName, url).ConfigureAwait(false);

            try
            {
                await SynchronizeAsync(domainName, useCaseName, url, new string[] { "DAL" }, testUserGuid, "IntegrationTest_Scalar")
                    .ConfigureAwait(false);

                string modifiedByUserName = await GetModifiedByUserNameAsync(domainName, useCaseName, "DAL").ConfigureAwait(false);
                Guid modifiedByUserGuid = await GetModifiedByUserGuidAsync(domainName, useCaseName, "DAL").ConfigureAwait(false);

                Assert.Equal("IntegrationTest_Scalar", modifiedByUserName);
                Assert.Equal(testUserGuid, modifiedByUserGuid);
            }
            finally
            {
                await DeleteTestRowsAsync(domainName, useCaseName, url).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task ExecuteNonQueryAsync_WithUnknownTvpType_ThrowsClearException()
        {
            string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 12);
            string domainName = "TVPTest";
            string useCaseName = "UnknownType" + uniqueSuffix;
            string url = "http://integration-test/" + uniqueSuffix;

            PocoDataTable requestTable = CreateRequestTable(domainName, useCaseName, url, new string[] { "DAL" });

            SqlTableValuedParameterInfo[] tableValuedParameters = new SqlTableValuedParameterInfo[1];
            tableValuedParameters[0] = new SqlTableValuedParameterInfo(
                TableValuedParameterName,
                "dbo.HostedApplicationLayerTypeThatDoesNotExist",
                requestTable);

            PocoSqlDataAdapter adapter = new PocoSqlDataAdapter(Fixture.Connection);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await adapter.ExecuteNonQueryAsync(
                    SynchronizeStoredProcedureName,
                    true,
                    tableValuedParameters,
                    null).ConfigureAwait(false);
            }).ConfigureAwait(false);

            Assert.Contains("HostedApplicationLayerTypeThatDoesNotExist", exception.Message);
        }

        async Task SynchronizeAsync(string domainName, string useCaseName, string url, string[] applicationLayerNames, Guid userGuid, string userName)
        {
            PocoDataTable requestTable = CreateRequestTable(domainName, useCaseName, url, applicationLayerNames);
            SqlTableValuedParameterInfo[] tableValuedParameters = CreateTableValuedParameterInfos(requestTable);

            Dictionary<string, object?> parameters = new Dictionary<string, object?>();
            parameters.Add("@UserGuid", userGuid);
            parameters.Add("@UserName", userName);

            PocoSqlDataAdapter adapter = new PocoSqlDataAdapter(Fixture.Connection);

            await adapter.ExecuteNonQueryAsync(
                SynchronizeStoredProcedureName,
                true,
                tableValuedParameters,
                parameters,
                null).ConfigureAwait(false);
        }

        static PocoDataTable CreateRequestTable(string domainName, string useCaseName, string url, string[] applicationLayerNames)
        {
            PocoDataTable table = HostedApplicationLayerTestData.CreateHostedApplicationLayerRequestTable();

            for (int i = 0; i < applicationLayerNames.Length; i++)
            {
                HostedApplicationLayerTestData.AddHostedApplicationLayer(
                    table,
                    domainName,
                    useCaseName,
                    applicationLayerNames[i],
                    url);
            }

            return table;
        }

        static SqlTableValuedParameterInfo[] CreateTableValuedParameterInfos(PocoDataTable requestTable)
        {
            SqlTableValuedParameterInfo[] tableValuedParameters = new SqlTableValuedParameterInfo[1];
            tableValuedParameters[0] = new SqlTableValuedParameterInfo(
                TableValuedParameterName,
                TableValuedParameterTypeName,
                requestTable);

            return tableValuedParameters;
        }

        async Task DeleteTestRowsAsync(string domainName, string useCaseName, string url)
        {
            await using (SqlCommand command = Fixture.Connection.CreateCommand())
            {
                    command.CommandText =
@"DELETE FROM [dbo].[HostedApplicationLayerLog]
WHERE [Url] = @Url
   OR [LoggedEntityId] IN
      (
          SELECT [Id]
          FROM [dbo].[HostedApplicationLayer]
          WHERE [DomainName] = @DomainName
            AND [UseCaseName] = @UseCaseName
      );

DELETE FROM [dbo].[HostedApplicationLayer]
WHERE [DomainName] = @DomainName
  AND [UseCaseName] = @UseCaseName;

DELETE FROM [dbo].[HostedApplicationLayerLog]
WHERE [Url] = @Url;";
                    command.Parameters.AddWithValue("@DomainName", domainName);
                    command.Parameters.AddWithValue("@UseCaseName", useCaseName);
                    command.Parameters.AddWithValue("@Url", url);

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        async Task<int> CountRowsAsync(string domainName, string useCaseName, bool? isDeleted)
        {
            int count = 0;

            await using (SqlCommand command = Fixture.Connection.CreateCommand())
            {
                    command.CommandText =
@"SELECT COUNT(*)
FROM [dbo].[HostedApplicationLayer]
WHERE [DomainName] = @DomainName
  AND [UseCaseName] = @UseCaseName";

                    if (isDeleted.HasValue)
                    {
                        command.CommandText = command.CommandText + "\n  AND [IsDeleted] = @IsDeleted";
                        command.Parameters.AddWithValue("@IsDeleted", isDeleted.Value);
                    }

                    command.Parameters.AddWithValue("@DomainName", domainName);
                    command.Parameters.AddWithValue("@UseCaseName", useCaseName);

                    object? value = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    count = Convert.ToInt32(value);
            }

            return count;
        }

        async Task<int> CountAllRowsAsync(string domainName, string useCaseName)
        {
            return await CountRowsAsync(domainName, useCaseName, null).ConfigureAwait(false);
        }

        async Task<int> CountLogRowsAsync(string url)
        {
            int count = 0;

            await using (SqlCommand command = Fixture.Connection.CreateCommand())
            {
                    command.CommandText =
@"SELECT COUNT(*)
FROM [dbo].[HostedApplicationLayerLog]
WHERE [Url] = @Url";
                    command.Parameters.AddWithValue("@Url", url);

                    object? value = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    count = Convert.ToInt32(value);
            }

            return count;
        }

        async Task<DateTime> GetDateOfModificationAsync(string domainName, string useCaseName, string applicationLayerName)
        {
            object? value = await GetSingleValueAsync(
                "[DateOfModification]",
                domainName,
                useCaseName,
                applicationLayerName).ConfigureAwait(false);

            return Convert.ToDateTime(value);
        }

        async Task<bool> GetIsDeletedAsync(string domainName, string useCaseName, string applicationLayerName)
        {
            object? value = await GetSingleValueAsync(
                "[IsDeleted]",
                domainName,
                useCaseName,
                applicationLayerName).ConfigureAwait(false);

            return Convert.ToBoolean(value);
        }

        async Task<string> GetModifiedByUserNameAsync(string domainName, string useCaseName, string applicationLayerName)
        {
            object? value = await GetSingleValueAsync(
                "[ModifiedByUserName]",
                domainName,
                useCaseName,
                applicationLayerName).ConfigureAwait(false);

            return Convert.ToString(value)!;
        }

        async Task<Guid> GetModifiedByUserGuidAsync(string domainName, string useCaseName, string applicationLayerName)
        {
            object? value = await GetSingleValueAsync(
                "[ModifiedByUserGuid]",
                domainName,
                useCaseName,
                applicationLayerName).ConfigureAwait(false);

            return (Guid)value!;
        }

        async Task<object?> GetSingleValueAsync(string columnName, string domainName, string useCaseName, string applicationLayerName)
        {
            object? value = null;

            await using (SqlCommand command = Fixture.Connection.CreateCommand())
            {
                    command.CommandText =
"SELECT " + columnName + @"
FROM [dbo].[HostedApplicationLayer]
WHERE [DomainName] = @DomainName
  AND [UseCaseName] = @UseCaseName
  AND [ApplicationLayerName] = @ApplicationLayerName";
                    command.Parameters.AddWithValue("@DomainName", domainName);
                    command.Parameters.AddWithValue("@UseCaseName", useCaseName);
                    command.Parameters.AddWithValue("@ApplicationLayerName", applicationLayerName);

                    value = await command.ExecuteScalarAsync().ConfigureAwait(false);
            }

            return value;
        }
    }
}
