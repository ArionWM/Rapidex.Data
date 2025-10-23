# Entity Logic; Validation & Calculation & Other Signals

The **Implementer** architecture is designed for extensibility.  
It allows integration with different message hubs and signal (topic) systems.  

The **Topic** structure enables separation between **multi-workspace** and **multi-tenant** environments,  
providing advanced customization capabilities (See: [Signal Hub](SignalHub.md)).

In Rapidex.Data, Entity logic (validation and calculation-like operations and other signals) is managed via signals.

## Concrete Entities - Entity Implementer Classes

Run time metadata manuplation and entity logic is implemented via **Implementer** classes.

Entities can have multiple Implementer classes, each implementing the `IConcreteEntityImplementer<T>` interface, where `T` is the concrete entity type.

Rapidex.Data scans assemblies (with [Library Declaration](LibraryDeclaration.md) or manually registered) for Implementer classes during database mettadata loading.

Implementer classes have `SetupMetadata` method for related modifications or registrations.

```csharp
internal class MyEntityImplementer : IConcreteEntityImplementer<MyEntity>
{
    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        //Manipulate metadata or register to signals here

    }
}
```

## Soft Entities - Definitions

*doc preparing*


## Predefined Signals & Handlers

### Exec Logic (Calculation, etc.)

Custom logic is implemented via `ExecLogic` signal. In this signal handler, you can implement calculation-like logic and return entity with updated values.

### Before Save

`BeforeSave` signal is called before entity is saved to database. You can implement any logic that needs to be executed before saving the entity.

In this signal handler, you can implement any logic and return entity with updated values.

Note: `BeforeSave` signal is not call `ExecLogic` signal. If required, you need to call it manually inside `BeforeSave` handler.

### Validation

Validation logic is implemented via `Validate` signal. This signal is called after 'BeforeSave' signal.

If *validate* handler result contains errors, the operation is aborted with `DataValidationException`.

### After Save

*doc preparing*

### After Commit

*doc preparing*

### After Delete

*doc preparing*

## Call Logic With Manual

In Rapidex.Data, you can direct calls to custom logic methods `entity.Validate()` and `entity.ExecLogic()` extension methods;

## Exceptions

Bulk update methods (Query.Update()) do not trigger any signal include entity logic (validation, calculation, etc.).

## Sample

```csharp

internal class ContactImplementer : IConcreteEntityImplementer<Contact>
{
    protected static void CalculateContactValues(Contact contact)
    {
        if (contact.BirthDate.IsNOTNullOrEmpty())
        {
            DateTimeOffset now = DateTimeOffset.Now;
            int age = now.Year - contact.BirthDate.Value.Year;
            if (now.DayOfYear < contact.BirthDate.Value.DayOfYear)
                age--;
            contact.Age = age;
        }

        if (contact.FullName.IsNullOrEmpty())
        {
            contact.FullName = (contact.FirstName + " " + contact.LastName).Trim();
        }
    }

    protected static ISignalHandlingResult Validate(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();
        IValidationResult validationResult = new ValidationResult();

        // ....

        if (contact.FirstName.IsNullOrEmpty())
            validationResult.Error("FirstName", "First name is required.");

        return args.CreateHandlingValidationResult(contact, validationResult);
    }

    protected static ISignalHandlingResult ExecLogic(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();

        CalculateContactValues(contact);
        return args.CreateHandlingResult(contact);
    }

    protected static ISignalHandlingResult BeforeSave(IEntityReleatedMessageArguments args)
    {
        Contact contact = (Contact)args.Entity.EnsureForActualEntity();
        CalculateContactValues(contact);
        return args.CreateHandlingResult();
    }

    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false)
            .MarkOnlyBaseSchema();

        this.SubscribeBeforeSave(ContactImplementer.BeforeSave);
        this.SubscribeValidate(ContactImplementer.Validate);
        this.SubscribeExecLogic(ContactImplementer.ExecLogic);        
    }
}
```