# Client key for post-save correlation

Status: Accepted

## Decision

Use `__ClientKey` for client-created row correlation.

## Reason

Server-generated primary keys are unknown before save. A client-only key allows post-save responses to map generated values back to local rows.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
