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
        // Keys
        modelBuilder.Entity<Department>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<OrderLine>()
            .HasKey(x => new { x.OrderId, x.LineNo });

        // Concurrency token used by PATCH semantics tests.
        // IMPORTANT: Do NOT use IsRowVersion() here, because we set RowVersion explicitly in tests
        // and we also want provider-agnostic optimistic concurrency checks (e.g., SQLite in-memory).
        modelBuilder.Entity<Department>()
            .Property(x => x.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedNever();
    }
}

public sealed class Department
{
    public int Id { get; set; }
    public string? Name { get; set; }

    // Extra field to validate sparse (floating) updates do NOT overwrite values
    // when the field is not provided in the changeset row.
    public string? Description { get; set; }

    public byte[]? RowVersion { get; set; }
}

public sealed class OrderLine
{
    public int OrderId { get; set; }
    public int LineNo { get; set; }
    public string? Sku { get; set; }

    // Extra field to validate sparse (floating) updates for composite primary keys.
    public string? Description { get; set; }
}
