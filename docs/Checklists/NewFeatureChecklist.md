# New Feature Checklist

Before adding a feature, answer these questions:

1. Which project owns this feature?
2. Does it belong in the core model, extensions, observable layer, serializer, EF bridge, or SQL adapter?
3. Does it introduce a new ownership edge?
4. Does it need a detach/cleanup path?
5. Does it affect row state semantics?
6. Does it affect serialization shape?
7. Does it affect merge behavior?
8. Does it affect public website documentation?
9. Which invariant tests must be added?
10. Does it deserve an ADR?
