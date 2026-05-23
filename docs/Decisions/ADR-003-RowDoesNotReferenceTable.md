# Row does not reference table

Status: Accepted

## Decision

`DataRow` does not keep a reference to its owning `DataTable`.

## Reason

This prevents ownership cycles and keeps row-level operations independent. Physical removal remains table-level.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
