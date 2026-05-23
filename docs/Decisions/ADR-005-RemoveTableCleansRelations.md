# RemoveTable cleans relations

Status: Accepted

## Decision

`DataSet.RemoveTable` removes relations that reference the removed table.

## Reason

Relations store names, not object references, so this is not a classic leak. It is still required to prevent stale metadata.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
