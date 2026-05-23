# Detach clears attachment metadata

Status: Accepted

## Decision

Physical row removal clears copied table attachment metadata and sets the row to `Detached`.

## Reason

Rows may outlive their table through external references. They should not keep stale table-owned metadata after removal.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
