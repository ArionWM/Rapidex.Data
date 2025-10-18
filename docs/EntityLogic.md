# Entity Logic; Validation & Calculation & Other Signals

The **Implementer** architecture is designed for extensibility.  
It allows integration with different message hubs and signal (topic) systems.  

The **Topic** structure enables separation between **multi-workspace** and **multi-tenant** environments,  
providing advanced customization capabilities.

In Rapidex.Data, Entity logic (validation and calculation-like operations and other signals) is managed via signals.

## Concrete Entities - Entity Implementer Classes


## Soft Entities - Definitions


## Validation


## Custom Logic


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

        //See: SignalHub.md
        //Register to signals
        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_BEFORESAVE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.BeforeSave);

        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_VALIDATE,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.Validate);

        Signal.Hub.SubscribeEntityReleated(
            DataReleatedSignalConstants.SIGNAL_EXEC_LOGIC,
            SignalTopic.ANY,
            SignalTopic.ANY,
            SignalTopic.ANY,
            nameof(Contact),
            ContactImplementer.ExecLogic);
    }
}
```