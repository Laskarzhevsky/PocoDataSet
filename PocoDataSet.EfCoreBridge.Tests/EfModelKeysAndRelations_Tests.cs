using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PocoDataSet.IData;
using PocoDataSet.Extensions;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfModelKeysAndRelations_Tests
{
    [Fact]
    public void ApplyEfModelKeysAndRelations_Adds_PrimaryKeys_And_SingleColumn_Relation()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        using ParentChildDbContext db = CreateContext<ParentChildDbContext>(connection);

        IDataSet ds = DataSetFactory.CreateDataSet();

        // Create tables and columns only (no PKs, no relations).
        IDataTable parent = ds.AddNewTable("Parent");
        parent.AddColumn("Id", DataTypeNames.INT32);
        parent.AddColumn("Name", DataTypeNames.STRING);

        IDataTable child = ds.AddNewTable("Child");
        child.AddColumn("Id", DataTypeNames.INT32);
        child.AddColumn("ParentId", DataTypeNames.INT32);
        child.AddColumn("Name", DataTypeNames.STRING);

        // Act
        ds.ApplyEfModelKeysAndRelations(db);

        // Assert PKs
        Assert.Equal(new List<string> { "Id" }, ds.Tables["Parent"].PrimaryKeys);
        Assert.Equal(new List<string> { "Id" }, ds.Tables["Child"].PrimaryKeys);

        // Assert relation
        IDataRelation? relation = ds.Relations.FirstOrDefault(r =>
            string.Equals(r.ParentTableName, "Parent", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.ChildTableName, "Child", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(relation);
        Assert.Equal(new List<string> { "Id" }, relation!.ParentColumnNames);
        Assert.Equal(new List<string> { "ParentId" }, relation!.ChildColumnNames);
    }

    [Fact]
    public void ApplyEfModelKeysAndRelations_Adds_CompositeKey_Relation()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        using CompositeKeyDbContext db = CreateContext<CompositeKeyDbContext>(connection);

        IDataSet ds = DataSetFactory.CreateDataSet();

        // Principal table
        IDataTable order = ds.AddNewTable("Orders");
        order.AddColumn("OrderId", DataTypeNames.INT32);
        order.AddColumn("Version", DataTypeNames.INT32);

        // Dependent table
        IDataTable line = ds.AddNewTable("OrderLines");
        line.AddColumn("OrderId", DataTypeNames.INT32);
        line.AddColumn("Version", DataTypeNames.INT32);
        line.AddColumn("LineNo", DataTypeNames.INT32);

        ds.ApplyEfModelKeysAndRelations(db);

        Assert.Equal(new List<string> { "OrderId", "Version" }, ds.Tables["Orders"].PrimaryKeys);
        Assert.Equal(new List<string> { "OrderId", "Version", "LineNo" }, ds.Tables["OrderLines"].PrimaryKeys);

        IDataRelation? relation = ds.Relations.FirstOrDefault(r =>
            string.Equals(r.ParentTableName, "Orders", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.ChildTableName, "OrderLines", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(relation);
        Assert.Equal(new List<string> { "OrderId", "Version" }, relation!.ParentColumnNames);
        Assert.Equal(new List<string> { "OrderId", "Version" }, relation!.ChildColumnNames);
    }

    [Fact]
    public void ApplyEfModelKeysAndRelations_DoesNotThrow_When_CycleExists_And_FKsNullable()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        using CycleDbContext db = CreateContext<CycleDbContext>(connection);

        IDataSet ds = DataSetFactory.CreateDataSet();

        IDataTable a = ds.AddNewTable("A");
        a.AddColumn("Id", DataTypeNames.INT32);
        a.AddColumn("BId", DataTypeNames.INT32);

        IDataTable b = ds.AddNewTable("B");
        b.AddColumn("Id", DataTypeNames.INT32);
        b.AddColumn("AId", DataTypeNames.INT32);

        ds.ApplyEfModelKeysAndRelations(db);

        // Should add two relations: A->B and B->A (order does not matter)
        Assert.Equal(2, ds.Relations.Count);

        Assert.Contains(ds.Relations, r =>
            string.Equals(r.ParentTableName, "A", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.ChildTableName, "B", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(ds.Relations, r =>
            string.Equals(r.ParentTableName, "B", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.ChildTableName, "A", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApplyEfModelKeysAndRelations_Adds_Relation_To_AlternateKey_PrincipalKey()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        using AlternateKeyDbContext db = CreateContext<AlternateKeyDbContext>(connection);

        IDataSet ds = DataSetFactory.CreateDataSet();

        // Principal table (PK = Id, Alternate Key = Code)
        IDataTable customer = ds.AddNewTable("Customers");
        customer.AddColumn("Id", DataTypeNames.INT32);
        customer.AddColumn("Code", DataTypeNames.STRING);

        // Dependent table (FK points to principal alternate key)
        IDataTable sale = ds.AddNewTable("Sales");
        sale.AddColumn("Id", DataTypeNames.INT32);
        sale.AddColumn("CustomerCode", DataTypeNames.STRING);

        // Act
        ds.ApplyEfModelKeysAndRelations(db);

        // Assert PKs
        Assert.Equal(new List<string> { "Id" }, ds.Tables["Customers"].PrimaryKeys);
        Assert.Equal(new List<string> { "Id" }, ds.Tables["Sales"].PrimaryKeys);

        // Assert relation uses principal alternate key columns
        IDataRelation? relation = ds.Relations.FirstOrDefault(r =>
            string.Equals(r.ParentTableName, "Customers", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.ChildTableName, "Sales", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(relation);
        Assert.Equal(new List<string> { "Code" }, relation!.ParentColumnNames);
        Assert.Equal(new List<string> { "CustomerCode" }, relation!.ChildColumnNames);
    }

    private static SqliteConnection CreateOpenSqliteConnection()
    {
        SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static TContext CreateContext<TContext>(SqliteConnection connection)
        where TContext : DbContext
    {
        DbContextOptionsBuilder<TContext> builder = new DbContextOptionsBuilder<TContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging();

        TContext db = (TContext)Activator.CreateInstance(typeof(TContext), builder.Options)!;
        db.Database.EnsureCreated();
        return db;
    }

    private sealed class ParentChildDbContext : DbContext
    {
        public ParentChildDbContext(DbContextOptions<ParentChildDbContext> options) : base(options) { }

        public DbSet<Parent> Parents => Set<Parent>();
        public DbSet<Child> Children => Set<Child>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>()
                .ToTable("Parent")
                .HasKey(x => x.Id);

            modelBuilder.Entity<Child>()
                .ToTable("Child")
                .HasKey(x => x.Id);

            // Required FK (single column)
            modelBuilder.Entity<Child>()
                .HasOne<Parent>()
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .IsRequired();
        }
    }

    private sealed class CompositeKeyDbContext : DbContext
    {
        public CompositeKeyDbContext(DbContextOptions<CompositeKeyDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .ToTable("Orders")
                .HasKey(x => new { x.OrderId, x.Version });

            modelBuilder.Entity<OrderLine>()
                .ToTable("OrderLines")
                .HasKey(x => new { x.OrderId, x.Version, x.LineNo });

            modelBuilder.Entity<OrderLine>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(x => new { x.OrderId, x.Version })
                .IsRequired();
        }
    }

    private sealed class CycleDbContext : DbContext
    {
        public CycleDbContext(DbContextOptions<CycleDbContext> options) : base(options) { }

        public DbSet<AEntity> AEntities => Set<AEntity>();
        public DbSet<BEntity> BEntities => Set<BEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AEntity>()
                .ToTable("A")
                .HasKey(x => x.Id);

            modelBuilder.Entity<BEntity>()
                .ToTable("B")
                .HasKey(x => x.Id);

            // Nullable FKs to allow the model without cascade requirements
            modelBuilder.Entity<AEntity>()
                .HasOne<BEntity>()
                .WithMany()
                .HasForeignKey(x => x.BId)
                .IsRequired(false);

            modelBuilder.Entity<BEntity>()
                .HasOne<AEntity>()
                .WithMany()
                .HasForeignKey(x => x.AId)
                .IsRequired(false);
        }
    }

    private sealed class AlternateKeyDbContext : DbContext
    {
        public AlternateKeyDbContext(DbContextOptions<AlternateKeyDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Sale> Sales => Set<Sale>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .ToTable("Customers")
                .HasKey(x => x.Id);

            // Alternate key (unique) used as FK principal key.
            modelBuilder.Entity<Customer>()
                .HasAlternateKey(x => x.Code);

            modelBuilder.Entity<Sale>()
                .ToTable("Sales")
                .HasKey(x => x.Id);

            // FK references alternate key via HasPrincipalKey.
            modelBuilder.Entity<Sale>()
                .HasOne<Customer>()
                .WithMany()
                .HasForeignKey(x => x.CustomerCode)
                .HasPrincipalKey(x => x.Code)
                .IsRequired();
        }
    }

    private sealed class Parent
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private sealed class Child
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string? Name { get; set; }
    }

    private sealed class Order
    {
        public int OrderId { get; set; }
        public int Version { get; set; }
    }

    private sealed class OrderLine
    {
        public int OrderId { get; set; }
        public int Version { get; set; }
        public int LineNo { get; set; }
    }

    private sealed class AEntity
    {
        public int Id { get; set; }
        public int? BId { get; set; }
    }

    private sealed class BEntity
    {
        public int Id { get; set; }
        public int? AId { get; set; }
    }

    private sealed class Customer
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    private sealed class Sale
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
    }
}
