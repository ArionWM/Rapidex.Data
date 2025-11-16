# Rapidex Data Framework Instructions

## Database Entity (DMO) Pattern

- All concrete entities inherit from `DbConcreteEntityBase`
- Use `Enumeration<T>` for type-safe enum properties (e.g., `Enumeration<StatusType>`)
- Use `Reference<T>` for other entity references (e.g., `Reference<Customer>`)
- Always include `IConcreteEntityImplementer<T>` implementation
- Subscribe to appropriate signals for business logic in implementer class
- Entity properties can't be nullable 
- Use appropriate default values
- All entity and enumeration 'id' value type is `long` (int64)

## Development code generation

When working with C# code, follow these instructions very carefully.

It is **EXTREMELY important that you follow the instructions in the rule files very carefully.**

### Code Style

- Target .NET 8 and C# 13.0
- Use nullable reference types appropriately
- Follow existing naming patterns for consistency
  - For local members, use `this`
  - For static members, use class name
- Develop minimal and readable code
- Create protected methods for repeating contents 

### Clean Architecture

When implementing backend services, follow these Clean Architecture principles to ensure maintainability, scalability, and separation of concerns. This rule is tailored for .NET solutions with a multi-project structure.

### Json

- Use `jsonstring.FromJson<MyClass>` or `JsonHelper.FromJson<MyClass>(jsonstring)` or `obj.ToJson()` rather than 
`JsonSerializer.Deserialize<T>()` and `JsonSerializer.Serialize<T>(obj)`

### If-Else clauses

Use `switch` instead of `if-else` whenever possible

### Null & type checks

- Use `ObjectHelper` extensions (`IsNullOrEmpty` or `IsNOTNullOrEmpty`) for null & empty checks (if not required to throw exception) rather than `abc == null`, `abc is null` or`abc is not null` 
- Use `AssertionHelper` extensions (`NotNull()`) for null checks (if required for throw exception)
- Use `AssertionHelper` extensions (`NotEmpty()`) for array, list, string empty checks (if required for throw exception)
- Use `AssertionHelper` extensions (`ShouldSupportTo<T>()`, `ShouldSupportTo(Type)`) for type checks (if required for throw exception)


## Others
 
1. Use dependency injection to manage dependencies across layers.
2. Avoid circular dependencies between layers.

