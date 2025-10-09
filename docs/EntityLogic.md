# Entity Logic; Validation & Calculation & Other Signals

In Rapidex.Data, Entity logic (validation and calculation-like operations and other signals) is managed via signals.

## Concrete Entities - Entity Implementer Classes


## Dynamic Entities - Definitions


## Validation


## Custom Logic


## Call Logic With Manual

In Rapidex.Data, you can direct calls to custom logic methods `entity.Validate()` and `entity.ExecLogic()` extension methods;

## Exceptions

Bulk update methods (Query.Update()) do not trigger any signal include entity logic (validation, calculation, etc.).