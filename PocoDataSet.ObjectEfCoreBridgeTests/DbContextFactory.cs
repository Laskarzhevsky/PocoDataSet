using System;
using Microsoft.EntityFrameworkCore;

namespace PocoDataSet.ObjectEfCoreBridgeTests
{
    internal static class DbContextFactory
    {
        internal static TestDbContext CreateDbContext()
        {
            DbContextOptionsBuilder<TestDbContext> optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            TestDbContext dbContext = new TestDbContext(optionsBuilder.Options);
            Seed(dbContext);
            return dbContext;
        }

        static void Seed(TestDbContext dbContext)
        {
            dbContext.Departments.Add(new Department { Id = 1, Name = "Sales" });
            dbContext.Departments.Add(new Department { Id = 2, Name = "HR" });

            dbContext.Employees.Add(new Employee { Id = 1, Name = "Alice", DepartmentId = 1 });
            dbContext.Employees.Add(new Employee { Id = 2, Name = "Bob", DepartmentId = 1 });
            dbContext.Employees.Add(new Employee { Id = 3, Name = "Carol", DepartmentId = 2 });

            dbContext.SaveChanges();
        }
    }
}
