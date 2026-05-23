# Website Documentation Source Map

This documentation also used the supplied `PocoDataSetDocumentation` folder to align internal terms with public documentation.

## Important public pages

| Topic | Website page |
|---|---|
| Architectural boundaries | `Concepts/ArchitectureOverview.htm` |
| Design philosophy | `Concepts/DesignPhilosophy.htm` |
| Data model overview | `Concepts/OverviewOfDataModel.htm` |
| Table and row organization | `Concepts/TableAndRowOrganization.htm` |
| Changesets and correlation | `Concepts/ChangesetsAndCorrelations.htm` |
| Merge semantics | `Concepts/MergeSemanticsContract.htm` |
| Serialization | `Concepts/SerializationAndDeserialization.htm` |
| Observable integration | `Concepts/ObservableDataSetIntegration.htm` |
| Interface projections | `Concepts/InterfaceProjections.htm` |
| EF compatibility | `Concepts/EFCompatibility.htm` |
| Schema-driven relations | `Concepts/SchemaDrivenRelations.htm` |
| Validation and constraints | `Concepts/ValidationAndConstraints.htm` |
| EF quick starts | `QuickStart/EFBridgeQuickStart.htm`, `QuickStart/EFCoreBridgeRelationAwarePersistence.htm` |
| SQL adapter quick starts | `QuickStart/SqlDataAdapterQuickStart.htm`, `QuickStart/SqlDataAdapterTransactionsAndAdvancedUsage.htm` |

## Maintainer use

When code and website documentation disagree, treat it as a documentation drift or implementation drift issue. Do not silently choose one without adding a test or an ADR.
