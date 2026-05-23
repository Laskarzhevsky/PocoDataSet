# Observable layer as wrapper

Status: Accepted

## Decision

Keep observable behavior in wrapper projects instead of the core data model.

## Reason

This preserves a lightweight core model and lets UI-focused notifications evolve separately.

## Consequences

- Tests should lock this behavior.
- Public documentation should not promise behavior that contradicts this decision.
- Any future change should update this ADR or add a replacement ADR.
