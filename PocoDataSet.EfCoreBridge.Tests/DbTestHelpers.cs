using System;
using Microsoft.EntityFrameworkCore;

namespace PocoDataSet.EfCoreBridge.Tests;

internal static class DbTestHelpers
{
    internal static TestDbContext CreateContext()
    {
        // Unique in-memory database per test
        string dbName = "PocoDataSet_EfCoreBridge_" + Guid.NewGuid().ToString("N");

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new TestDbContext(options);
    }
}
