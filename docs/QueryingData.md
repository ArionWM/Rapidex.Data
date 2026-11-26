# Querying Data

Rapidex queries are built through `IQuery` / `IQuery<T>` and executed asynchronously (`Load`, `First`, `Count`, …).

## Fluent (string field) API

```csharp
var rows = await schema.GetQuery<Order>()
    .Eq(nameof(Order.Status), "Open")
    .Gt(nameof(Order.Amount), 100)
    .OrderBy(OrderDirection.Asc, nameof(Order.Name))
    .Page(20, 0)
    .Load();
```

Nested reference (join-based):

```csharp
await schema.GetQuery<OrderLine>()
    .Nested(nameof(OrderLine.ParentOrder), q => q.Eq(nameof(Order.Name), "A"))
    .Load();
```

## LINQ-style API (`Rapidex.Data.Linq`)

Expression predicates are translated onto the same `IQuery` surface. Each operator clones the query (composition-safe). Unsupported nodes throw `NotSupportedException` (no silent client evaluation).

```csharp
using Rapidex.Data.Linq;

var list = await schema.GetQuery<ConcreteEntity01>()
    .Where(e => e.Number > 20 && e.Address == "Even")
    .OrderBy(e => e.Name)
    .Skip(0)
    .Take(20)
    .ToListAsync();
```

### Supported (MVP+)

| Expression | Maps to |
|---|---|
| `== != > >= < <= && \|\| !` | `Eq` / `NotEq` / `Gt`… / `And` / `Or` / `Not` |
| `string.Contains/StartsWith/EndsWith` | `Like` (`*` wildcards) |
| `values.Contains(field)` | `In` |
| `OrderBy` / `ThenBy` / `Skip` / `Take` | `OrderBy` / `Page` / `Limit` |
| `ToListAsync` / `FirstOrDefaultAsync` / `CountAsync` / `AnyAsync` | `Load` / `First` / `Count` / `Exist` |
| Reference navigation via cast | `ExistsReference` |
| `relation.Any()` / `Any(pred)` | `ExistsOne2N` / `ExistsN2N` |
| Enumeration vs enum | FK `Eq` via `EnsureLowerValue` |

### Reference navigation

`Reference<T>` does not expose target properties. Cast inside the expression (translation only; not executed as `GetContent`):

```csharp
.Where(e => ((ConcreteEntity01)e.MyReference).Name == "Ent 01")
.Where(e => e.MyReference.TargetId == id)
```

`GetContent()` inside a LINQ predicate is rejected.

### Relation Any

Use stubs from `Rapidex.Data.Linq` (`RelationLinqStubs`) so `Any` compiles on relation fields:

```csharp
.Where(m => m.Details01.Any(d => d.Name == "Match"))
.Where(e => e.Relation01.Any())
```

These use **EXISTS** (no master-row duplication). Parent-driven `Related` / `ReleatedN2N` APIs are unchanged.

### Additive Exists helpers (non-LINQ)

```csharp
query.IsNull("MyReference");
query.IsNotNull("MyReference");
query.ExistsReference("MyReference", q => q.Eq("Name", "A"));
query.ExistsOne2N("Details01", q => q.Eq("Name", "X"));
query.ExistsN2N("Relation01", q => q.Eq("Name", "Y"));
```

## Partial Entities

Create with only a subset of fields populated (see `LoadPartial`).
