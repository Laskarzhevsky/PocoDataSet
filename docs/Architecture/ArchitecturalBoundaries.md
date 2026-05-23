# Architectural Boundaries

## Purpose

POCO DataSet provides an ADO.NET-like data container without depending on `System.Data.DataSet` behavior. Its design favors plain interfaces, schema metadata, row state, JSON serialization, and explicit merge operations.

## Projects and responsibilities

| Project | Responsibility |
|---|---|
| `PocoDataSet.IData` | Core interfaces: `IDataSet`, `IDataTable`, `IDataRow`, metadata, relations, merge result contracts. |
| `PocoDataSet.Data` | Concrete in-memory implementation of dataset, table, row, column metadata, relation metadata, and internal schema helpers. |
| `PocoDataSet.Extensions` | Convenience and workflow operations: table creation, interface projection, changesets, accept/reject changes, relation validation, and merge methods. |
| `PocoDataSet.Serializer` | JSON serialization/deserialization using `System.Text.Json`, concrete type converters, row polymorphism, and value normalization. |
| `PocoDataSet.IObservableData` | Observable interfaces and event contracts. |
| `PocoDataSet.ObservableData` | Observable wrappers around core data objects with row/table/view events. |
| `PocoDataSet.ObservableExtensions` | Observable-aware merge and relation helper workflows. |
| `PocoDataSet.EfCoreBridge` | EF Core loading, schema extraction, changeset application, and post-save response generation. |
| `PocoDataSet.SqlServerDataAdapter` | SQL Server load/save adapter, schema metadata loading, transaction-aware save workflows, relation-aware table ordering, and concurrency support. |

## Boundary rule

The core model should remain independent from UI, EF Core, SQL Server, HTTP, BPUA, Blazor, and any persistence-specific technology.

The bridges/adapters may know about external frameworks. The core model should not.

## Public documentation alignment

The website pages describe POCO DataSet as lightweight, schema-driven, serializable, interface-friendly, and suitable for metadata-driven workflows. Internal code should preserve those promises.
