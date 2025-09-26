# Rapidex Data Framework Instructions

## Entity Pattern
- All concrete entities inherit from `DbConcreteEntityBase`
- Use `Enumeration<T>` for type-safe enum properties (e.g., `Enumeration<StatusType>`)
- Use `Reference<T>` for other entity references (e.g., `Reference<Customer>`)
- Always include `IConcreteEntityImplementer<T>` implementation
- Subscribe to appropriate signals for business logic in implementer class
- Entity properties can't be nullable 
- use appropriate default values
- All entity and enumeration 'id' value type is `long` (int64)

## Code Style
- Target .NET 8 and C# 13.0
- Use nullable reference types appropriately
- Follow existing naming patterns for consistency
  - For local members, use `this`
- Develop minimal and readable code
- Create protected methods for repeating contents 
 