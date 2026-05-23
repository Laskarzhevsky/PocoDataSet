# POCO DataSet Maintainer Documentation

This documentation set is intended to live in the solution root under `/docs`.
It combines two sources:

1. **Current code reality** from the supplied `PocoDataSetCode` folder.
2. **Design intent and public terminology** from the supplied `PocoDataSetDocumentation` website pages.

The goal is not to replace the public website. The goal is to preserve the internal rules that maintainers must not accidentally break.

## Recommended reading order

1. `Architecture/ArchitecturalBoundaries.md`
2. `Architecture/ObjectGraph.md`
3. `Architecture/OwnershipRules.md`
4. `Architecture/RowStateLifecycle.md`
5. `Architecture/SchemaAndPrimaryKeys.md`
6. `Architecture/ChangesetsAndCorrelation.md`
7. `Architecture/MergeSemantics.md`
8. `Architecture/SerializationFlow.md`
9. `Architecture/ObservableLifecycle.md`
10. `Testing/Invariants.md`

## Most important invariant

`POCO DataSet` is a lightweight, schema-aware, interface-first data container. It should keep ownership explicit and avoid hidden object-retention chains.

```text
DataSet owns DataTable references.
DataTable owns DataRow references and schema metadata.
DataRow owns field values and row-state snapshots only.
DataRow must not own DataTable resources.
Relations are DataSet metadata and reference tables/columns by name, not by object reference.
```

## File extension note

The correct extension is `.md` for Markdown files.
