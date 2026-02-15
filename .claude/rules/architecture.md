---
paths:
  - "src/**/*.cs"
---

# Architecture Rules

## Layer Dependencies (strict, no violations)

```
FoundationLayer  (depends on: nothing)
ReasoningLayer   (depends on: FoundationLayer, Shared)
MetacognitiveLayer (depends on: FoundationLayer, ReasoningLayer, Shared)
AgencyLayer      (depends on: all above)
BusinessApplications (depends on: all above)
UILayer          (depends on: all above)
```

Never add a reference from a lower layer to a higher layer.

## Hexagonal Architecture

- **Ports** = interfaces defining what a layer offers (`I{Concept}Port`)
- **Engines** = pure business logic implementing ports (`{Concept}Engine`)
- **Adapters** = infrastructure implementations (`{Implementation}Adapter`)
- Engines must NOT directly reference infrastructure (no `HttpClient`, no `DbContext`, no `IConfiguration`)
- Engines receive adapters through constructor injection

## New Components Checklist

When creating a new component:
1. Define the port interface in the layer's `Ports/` directory
2. Implement the engine in `Engines/`
3. Create adapters in `Adapters/` if infrastructure access is needed
4. Register in DI via `IServiceCollection` extension method
5. Add tests in the corresponding `tests/` directory
