# Core model uses interfaces

Status: Accepted

## Decision

The public surface is interface-first (`IDataSet`, `IDataTable`, `IDataRow`) while concrete types live in `PocoDataSet.Data`.

## Reason

This allows extensions, serializers, observable wrappers, and adapters to depend on contracts instead of concrete implementation.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
