using Microsoft.EntityFrameworkCore;

namespace PocoDataSet.EfCoreBridge.Tests;

public sealed class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderLine>()
            .HasKey(x => new { x.OrderId, x.LineNo });
    }
}

public sealed class Department
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public sealed class OrderLine
{
    public int OrderId { get; set; }
    public int LineNo { get; set; }
    public string? Sku { get; set; }
}
