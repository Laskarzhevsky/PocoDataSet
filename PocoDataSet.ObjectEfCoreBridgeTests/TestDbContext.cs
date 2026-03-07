using Microsoft.EntityFrameworkCore;

namespace PocoDataSet.ObjectEfCoreBridgeTests
{
    public sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees
        {
            get;
            set;
        }

        public DbSet<Department> Departments
        {
            get;
            set;
        }
    }
}
