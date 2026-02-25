using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetDataSetApplier_EdgeCases_Tests
{
    [Fact]
    public void ApplyChangesetAndSave_DoesNotThrow_WhenRelationsContainCycle_AndForeignKeysAreNullable_Sqlite()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        DbContextOptions<CycleNullableFkDbContext> options = new DbContextOptionsBuilder<CycleNullableFkDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        using CycleNullableFkDbContext db = new CycleNullableFkDbContext(options);
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");

        // Arrange: two tables with nullable foreign keys pointing to each other.
        // This produces a cycle in the relation graph, which RelationTableSorter must handle deterministically.
        IDataSet ds = DataSetFactory.CreateDataSet();

        IDataTable a = ds.AddNewTable("A");
        a.AddColumn("Id", DataTypeNames.INT32, false, true);
        a.AddColumn("BId", DataTypeNames.INT32, true, false);

        IDataTable b = ds.AddNewTable("B");
        b.AddColumn("Id", DataTypeNames.INT32, false, true);
        b.AddColumn("AId", DataTypeNames.INT32, true, false);

        // Cycle: A depends on B, and B depends on A.
        // Note: ParentColumns should be the referenced key, ChildColumns the FK.
        AddRelation(ds, "B_to_A", "B", new List<string> { "Id" }, "A", new List<string> { "BId" });
        AddRelation(ds, "A_to_B", "A", new List<string> { "Id" }, "B", new List<string> { "AId" });

        IDataRow a1 = a.AddNewRow();
        a1["Id"] = 1;
        a1["BId"] = null;

        IDataRow b1 = b.AddNewRow();
        b1["Id"] = 10;
        b1["AId"] = null;

        IDataSet cs = ds.CreateChangeset();

        // Act
        EfChangesetDataSetApplier.ApplyChangesetAndSave(db, cs);

        // Assert
        Assert.Equal(1, db.As.Count());
        Assert.Equal(1, db.Bs.Count());
    }

    [Fact]
    public void ApplyChangesetAndSave_DeletesParentAfterNullingChildForeignKey_WhenForeignKeyIsNullable_Sqlite()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        DbContextOptions<NullableFkDbContext> options = new DbContextOptionsBuilder<NullableFkDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        using NullableFkDbContext db = new NullableFkDbContext(options);
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");

        // Seed baseline: child references parent.
        db.Parents.Add(new NullableFkParentEntity { Id = 1, Name = "P1" });
        db.Children.Add(new NullableFkChildEntity { Id = 10, ParentId = 1, Name = "C1" });
        db.SaveChanges();

        // Arrange: dataset baseline (loaded rows)
        IDataSet ds = DataSetFactory.CreateDataSet();

        IDataTable parent = ds.AddNewTable("Parent");
        parent.AddColumn("Id", DataTypeNames.INT32, false, true);
        parent.AddColumn("Name", DataTypeNames.STRING);

        IDataTable child = ds.AddNewTable("Child");
        child.AddColumn("Id", DataTypeNames.INT32, false, true);
        child.AddColumn("ParentId", DataTypeNames.INT32, true, false);
        child.AddColumn("Name", DataTypeNames.STRING);

        AddRelation(ds, "ParentChild", "Parent", new List<string> { "Id" }, "Child", new List<string> { "ParentId" });

        IDataRow pLoaded = DataRowExtensions.CreateRowFromColumns(parent.Columns);
        pLoaded["Id"] = 1;
        pLoaded["Name"] = "P1";
        parent.AddLoadedRow(pLoaded);

        IDataRow cLoaded = DataRowExtensions.CreateRowFromColumns(child.Columns);
        cLoaded["Id"] = 10;
        cLoaded["ParentId"] = 1;
        cLoaded["Name"] = "C1";
        child.AddLoadedRow(cLoaded);

        // Child FK is nulled (Modified) and then parent is deleted.
        // If deletes are applied before the child update, SQLite would reject the delete under Restrict.
        cLoaded["ParentId"] = null;
        parent.DeleteRow(pLoaded);

        IDataSet cs = ds.CreateChangeset();

        // Act
        EfChangesetDataSetApplier.ApplyChangesetAndSave(db, cs);

        // Assert: parent deleted; child remains with NULL FK.
        Assert.Empty(db.Parents.ToList());
        NullableFkChildEntity? savedChild = db.Children.SingleOrDefault(x => x.Id == 10);
        Assert.NotNull(savedChild);
        Assert.Null(savedChild!.ParentId);
    }

    [Fact]
    public void RelationTableSorter_FallsBackToOriginalOrder_WhenCycleDetected()
    {
        // This test targets the deterministic cycle behavior in the sorter.
        // It uses reflection because RelationTableSorter is internal to EfCoreBridge.

        IDataSet ds = DataSetFactory.CreateDataSet();

        IDataTable a = ds.AddNewTable("A");
        a.AddColumn("Id", DataTypeNames.INT32, false, true);

        IDataTable b = ds.AddNewTable("B");
        b.AddColumn("Id", DataTypeNames.INT32, false, true);

        AddRelation(ds, "B_to_A", "B", new List<string> { "Id" }, "A", new List<string> { "Id" });
        AddRelation(ds, "A_to_B", "A", new List<string> { "Id" }, "B", new List<string> { "Id" });

        List<string> original = new List<string> { "B", "A" };

        Type? sorterType = typeof(EfChangesetDataSetApplier).Assembly.GetType("PocoDataSet.EfCoreBridge.RelationTableSorter");
        Assert.NotNull(sorterType);

        MethodInfo? method = sorterType!.GetMethod("SortTablesByRelations", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);

        object? resultObj = method!.Invoke(null, new object[] { ds, original });
        Assert.NotNull(resultObj);

        List<string> result = (List<string>)resultObj!;
        Assert.Equal(original, result);
    }

    private static SqliteConnection CreateOpenSqliteConnection()
    {
        SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static void AddRelation(
        IDataSet dataSet,
        string relationName,
        string parentTable,
        List<string> parentColumns,
        string childTable,
        List<string> childColumns)
    {
        if (dataSet == null)
        {
            throw new ArgumentNullException(nameof(dataSet));
        }

        dataSet.AddRelation(relationName, parentTable, parentColumns, childTable, childColumns);
    }

    private sealed class CycleNullableFkDbContext : DbContext
    {
        public CycleNullableFkDbContext(DbContextOptions<CycleNullableFkDbContext> options) : base(options)
        {
        }

        public DbSet<CycleAEntity> As => Set<CycleAEntity>();
        public DbSet<CycleBEntity> Bs => Set<CycleBEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CycleAEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<CycleBEntity>().HasKey(x => x.Id);

            modelBuilder.Entity<CycleAEntity>()
                .HasOne<CycleBEntity>()
                .WithMany()
                .HasForeignKey(x => x.BId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CycleBEntity>()
                .HasOne<CycleAEntity>()
                .WithMany()
                .HasForeignKey(x => x.AId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    [Table("A")]
    private sealed class CycleAEntity
    {
        public int Id { get; set; }
        public int? BId { get; set; }
    }

    [Table("B")]
    private sealed class CycleBEntity
    {
        public int Id { get; set; }
        public int? AId { get; set; }
    }

    private sealed class NullableFkDbContext : DbContext
    {
        public NullableFkDbContext(DbContextOptions<NullableFkDbContext> options) : base(options)
        {
        }

        public DbSet<NullableFkParentEntity> Parents => Set<NullableFkParentEntity>();
        public DbSet<NullableFkChildEntity> Children => Set<NullableFkChildEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NullableFkParentEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<NullableFkChildEntity>().HasKey(x => x.Id);

            modelBuilder.Entity<NullableFkChildEntity>()
                .HasOne<NullableFkParentEntity>()
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    [Table("Parent")]
    private sealed class NullableFkParentEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    [Table("Child")]
    private sealed class NullableFkChildEntity
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string? Name { get; set; }
    }
}
