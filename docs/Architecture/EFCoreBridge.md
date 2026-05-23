# EF Core Bridge

## Purpose

`PocoDataSet.EfCoreBridge` connects POCO DataSet changesets to EF Core without making the core model depend on EF Core.

## Main responsibilities

```text
Load EF query results into IDataSet/IDataTable
Build column metadata from EF model
Apply changeset rows to EF entities
Save changes
Return post-save response dataset for client reconciliation
Apply EF model keys and relations to a dataset
```

## Important classes

| Class | Responsibility |
|---|---|
| `EfCoreMergeAdapter` | High-level load/save adapter. |
| `EfChangesetDataSetApplier` | Applies an entire changeset dataset. |
| `EfChangesetToPocoApplier` | Applies changeset rows to POCO entities. |
| `EfChangesetCopyToPocoApplier` | Alternate copy-based apply behavior. |
| `EfColumnMetadataBuilder` | Builds column metadata from EF model. |
| `EfModelEntityTypeResolver` | Resolves dataset table names to EF entity types. |
| `RelationTableSorter` | Orders tables by relation dependencies. |

## Boundary rule

EF Core-specific reflection and entity resolution belongs in the bridge, not in `PocoDataSet.Data` or `PocoDataSet.Extensions`.

## Post-save rule

When saving a changeset, the response should support `DoPostSaveMerge` by returning enough data to reconcile client rows.
