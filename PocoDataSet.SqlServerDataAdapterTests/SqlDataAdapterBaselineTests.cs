using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PocoDataSet.Data;
using PocoDataSet.SqlServerDataAdapter;
using Xunit;

namespace PocoDataSet.SqlServerDataAdapterTests
{
    public sealed class SqlDataAdapterBaselineTests
    {
        [Fact]
        public async Task FillAsync_InvalidConnectionString_Throws()
        {
            SqlDataAdapter adapter = new SqlDataAdapter(null);

            // Intentionally invalid. We just want to ensure exceptions are not swallowed.
            string connectionString = "NotAValidConnectionString";

            await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                await adapter.FillAsync(
                    baseQuery: "select 1",
                    isStoredProcedure: false,
                    parameters: null,
                    returnedTableNames: new List<string> { "T" },
                    connectionString: connectionString,
                    dataSet: new DataSet());
            });
        }

        [Fact]
        public async Task SaveChangesAsync_EmptyChangeset_IsNoOp_ReturnsZero()
        {
            SqlDataAdapter adapter = new SqlDataAdapter(null);

            PocoDataSet.IData.IDataSet emptyChangeset = new PocoDataSet.Data.DataSet();

            // Must not throw even if ConnectionString is not specified,
            // because empty changeset should be a no-op.
            int affected = await adapter.SaveChangesAsync(emptyChangeset);

            Assert.Equal(0, affected);
        }
    }
}
