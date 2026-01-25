using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests
{
    public class EfColumnMetadataBuilder_SchemaBirth_Tests
    {
        [Fact]
        public void ToDataTable_Sets_PrimaryKey_Flags_And_TablePrimaryKeys_For_SingleKey()
        {
            using SingleKeyDbContext db = CreateSingleKeyContext();

            db.Departments.Add(new Department { DepartmentId = 1, Name = "Sales" });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = db.Departments.AsQueryable().ToDataTable(db, dataSet, "Department");

            Assert.Equal("Department", table.TableName);

            IColumnMetadata? pkColumn = FindColumn(table, "DepartmentId");
            Assert.NotNull(pkColumn);
            Assert.True(pkColumn!.IsPrimaryKey);

            Assert.NotNull(table.PrimaryKeys);
            Assert.Equal(1, table.PrimaryKeys.Count);
            Assert.Equal("DepartmentId", table.PrimaryKeys[0]);

            Assert.Equal(1, table.Rows.Count);
            Assert.Equal(1, (int)table.Rows[0]["DepartmentId"]!);
        }

        [Fact]
        public void ToDataTable_Sets_PrimaryKey_Flags_And_TablePrimaryKeys_For_CompositeKey()
        {
            using CompositeKeyDbContext db = CreateCompositeKeyContext();

            db.Tenants.Add(new TenantEntity { TenantId = 7, Code = "A1", Name = "Alpha" });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = db.Tenants.AsQueryable().ToDataTable(db, dataSet, "TenantEntity");

            IColumnMetadata? pk1 = FindColumn(table, "TenantId");
            IColumnMetadata? pk2 = FindColumn(table, "Code");

            Assert.NotNull(pk1);
            Assert.NotNull(pk2);

            Assert.True(pk1!.IsPrimaryKey);
            Assert.True(pk2!.IsPrimaryKey);

            Assert.NotNull(table.PrimaryKeys);
            Assert.Equal(2, table.PrimaryKeys.Count);
            Assert.Equal("TenantId", table.PrimaryKeys[0]);
            Assert.Equal("Code", table.PrimaryKeys[1]);
        }

        [Fact]
        public void ToDataTable_Sets_ForeignKey_Metadata()
        {
            using ForeignKeyDbContext db = CreateForeignKeyContext();

            db.Departments.Add(new Department { DepartmentId = 1, Name = "Sales" });
            db.Employees.Add(new Employee { EmployeeId = 10, DepartmentId = 1, Name = "John" });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = db.Employees.AsQueryable().ToDataTable(db, dataSet, "Employee");

            IColumnMetadata? fk = FindColumn(table, "DepartmentId");
            Assert.NotNull(fk);

            Assert.True(fk!.IsForeignKey);
            Assert.Equal("Department", fk.ReferencedTableName);
            Assert.Equal("DepartmentId", fk.ReferencedColumnName);
        }

        [Fact]
        public void ToDataTable_Sets_Nullability_From_EF_Model()
        {
            using NullabilityDbContext db = CreateNullabilityContext();

            db.People.Add(new Person { PersonId = 1, RequiredName = "A", OptionalNick = null });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = db.People.AsQueryable().ToDataTable(db, dataSet, "Person");

            IColumnMetadata? required = FindColumn(table, "RequiredName");
            IColumnMetadata? optional = FindColumn(table, "OptionalNick");

            Assert.NotNull(required);
            Assert.NotNull(optional);

            Assert.False(required!.IsNullable);
            Assert.True(optional!.IsNullable);
        }

        [Fact]
        public void ToDataTable_Throws_For_ShadowPrimaryKey()
        {
            using ShadowKeyDbContext db = CreateShadowKeyContext();

            ShadowEntity entity = new ShadowEntity { Name = "X" };
            db.ShadowEntities.Add(entity);

            // Set the shadow PK value explicitly.
            db.Entry(entity).Property("ShadowId").CurrentValue = 1;

            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            Assert.Throws<InvalidOperationException>(() =>
            {
                db.ShadowEntities.AsQueryable().ToDataTable(db, dataSet, "ShadowEntity");
            });
        }

        [Fact]
        public void ToDataTable_Allows_Keyless_Entity_And_DoesNotSet_PrimaryKeys()
        {
            using KeylessDbContext db = CreateKeylessContext();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // Keyless types are query-only; do not Add/SaveChanges.
            IDataTable table = db.Reports
                .AsNoTracking()
                .AsQueryable()
                .ToDataTable(db, dataSet, "ReportRow");

            Assert.NotNull(table);
            Assert.NotNull(table.PrimaryKeys);
            Assert.Equal(0, table.PrimaryKeys.Count);

            // Optional: still has columns
            Assert.True(table.Columns.Count > 0);
        }

        [Fact]
        public async Task ToDataTableAsync_Sets_PrimaryKeys()
        {
            using SingleKeyDbContext db = CreateSingleKeyContext();

            db.Departments.Add(new Department { DepartmentId = 1, Name = "Sales" });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = await db.Departments.AsQueryable().ToDataTableAsync(db, dataSet, "Department");

            Assert.NotNull(table.PrimaryKeys);
            Assert.Equal(1, table.PrimaryKeys.Count);
            Assert.Equal("DepartmentId", table.PrimaryKeys[0]);
        }

        [Fact]
        public void ToDataTable_Populates_MaxLength_And_PrecisionScale_When_Relational_Metadata_Available()
        {
            using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            DbContextOptions<RelationalDbContext> options = new DbContextOptionsBuilder<RelationalDbContext>()
                .UseSqlite(connection)
                .Options;

            using RelationalDbContext db = new RelationalDbContext(options);
            db.Database.EnsureCreated();

            db.RelationalEntities.Add(new RelationalEntity { RelationalEntityId = 1, Name = "ABCDE", Amount = 12.34m });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = db.RelationalEntities.AsQueryable().ToDataTable(db, dataSet, "RelationalEntity");

            IColumnMetadata? name = FindColumn(table, "Name");
            Assert.NotNull(name);
            Assert.Equal(50, name!.MaxLength);

            IColumnMetadata? amount = FindColumn(table, "Amount");
            Assert.NotNull(amount);
            Assert.Equal((byte)18, amount!.Precision);
            Assert.Equal((byte)2, amount.Scale);
        }

        private static IColumnMetadata? FindColumn(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                IColumnMetadata c = table.Columns[i];
                if (string.Equals(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return c;
                }
            }

            return null;
        }

        private static SingleKeyDbContext CreateSingleKeyContext()
        {
            DbContextOptions<SingleKeyDbContext> options = new DbContextOptionsBuilder<SingleKeyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new SingleKeyDbContext(options);
        }

        private static CompositeKeyDbContext CreateCompositeKeyContext()
        {
            DbContextOptions<CompositeKeyDbContext> options = new DbContextOptionsBuilder<CompositeKeyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CompositeKeyDbContext(options);
        }

        private static ForeignKeyDbContext CreateForeignKeyContext()
        {
            DbContextOptions<ForeignKeyDbContext> options = new DbContextOptionsBuilder<ForeignKeyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ForeignKeyDbContext(options);
        }

        private static NullabilityDbContext CreateNullabilityContext()
        {
            DbContextOptions<NullabilityDbContext> options = new DbContextOptionsBuilder<NullabilityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new NullabilityDbContext(options);
        }

        private static ShadowKeyDbContext CreateShadowKeyContext()
        {
            DbContextOptions<ShadowKeyDbContext> options = new DbContextOptionsBuilder<ShadowKeyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ShadowKeyDbContext(options);
        }

        private static KeylessDbContext CreateKeylessContext()
        {
            DbContextOptions<KeylessDbContext> options = new DbContextOptionsBuilder<KeylessDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new KeylessDbContext(options);
        }

        private class SingleKeyDbContext : DbContext
        {
            public SingleKeyDbContext(DbContextOptions<SingleKeyDbContext> options)
                : base(options)
            {
            }

            public DbSet<Department> Departments => Set<Department>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Department>(b =>
                {
                    b.ToTable("Department");
                    b.HasKey(x => x.DepartmentId);
                    b.Property(x => x.Name).HasMaxLength(50);
                });
            }
        }

        private class CompositeKeyDbContext : DbContext
        {
            public CompositeKeyDbContext(DbContextOptions<CompositeKeyDbContext> options)
                : base(options)
            {
            }

            public DbSet<TenantEntity> Tenants => Set<TenantEntity>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TenantEntity>(b =>
                {
                    b.ToTable("TenantEntity");
                    b.HasKey(x => new { x.TenantId, x.Code });
                    b.Property(x => x.Code).HasMaxLength(10);
                });
            }
        }

        private class ForeignKeyDbContext : DbContext
        {
            public ForeignKeyDbContext(DbContextOptions<ForeignKeyDbContext> options)
                : base(options)
            {
            }

            public DbSet<Department> Departments => Set<Department>();
            public DbSet<Employee> Employees => Set<Employee>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Department>(b =>
                {
                    b.ToTable("Department");
                    b.HasKey(x => x.DepartmentId);
                });

                modelBuilder.Entity<Employee>(b =>
                {
                    b.ToTable("Employee");
                    b.HasKey(x => x.EmployeeId);

                    b.HasOne<Department>()
                        .WithMany()
                        .HasForeignKey(x => x.DepartmentId);
                });
            }
        }

        private class NullabilityDbContext : DbContext
        {
            public NullabilityDbContext(DbContextOptions<NullabilityDbContext> options)
                : base(options)
            {
            }

            public DbSet<Person> People => Set<Person>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Person>(b =>
                {
                    b.ToTable("Person");
                    b.HasKey(x => x.PersonId);

                    b.Property(x => x.RequiredName).IsRequired();
                    b.Property(x => x.OptionalNick).IsRequired(false);
                });
            }
        }

        private class ShadowKeyDbContext : DbContext
        {
            public ShadowKeyDbContext(DbContextOptions<ShadowKeyDbContext> options)
                : base(options)
            {
            }

            public DbSet<ShadowEntity> ShadowEntities => Set<ShadowEntity>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ShadowEntity>(b =>
                {
                    b.ToTable("ShadowEntity");
                    b.Property<int>("ShadowId");
                    b.HasKey("ShadowId");
                    b.Property(x => x.Name).HasMaxLength(10);
                });
            }
        }

        private class KeylessDbContext : DbContext
        {
            public KeylessDbContext(DbContextOptions<KeylessDbContext> options)
                : base(options)
            {
            }

            public DbSet<ReportRow> Reports => Set<ReportRow>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ReportRow>(b =>
                {
                    b.ToTable("ReportRow");
                    b.HasNoKey();
                    b.Property(x => x.Name);
                });
            }
        }

        private class RelationalDbContext : DbContext
        {
            public RelationalDbContext(DbContextOptions<RelationalDbContext> options)
                : base(options)
            {
            }

            public DbSet<RelationalEntity> RelationalEntities => Set<RelationalEntity>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<RelationalEntity>(b =>
                {
                    b.ToTable("RelationalEntity");
                    b.HasKey(x => x.RelationalEntityId);

                    b.Property(x => x.Name).HasMaxLength(50);

                    b.Property(x => x.Amount).HasPrecision(18, 2);
                });
            }
        }

        private class Department
        {
            public int DepartmentId { get; set; }
            public string? Name { get; set; }
        }

        private class Employee
        {
            public int EmployeeId { get; set; }
            public int DepartmentId { get; set; }
            public string? Name { get; set; }
        }

        private class TenantEntity
        {
            public int TenantId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string? Name { get; set; }
        }

        private class Person
        {
            public int PersonId { get; set; }
            public string RequiredName { get; set; } = string.Empty;
            public string? OptionalNick { get; set; }
        }

        private class ShadowEntity
        {
            public string? Name { get; set; }
        }

        private class ReportRow
        {
            public string? Name { get; set; }
        }

        private class RelationalEntity
        {
            public int RelationalEntityId { get; set; }
            public string? Name { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
