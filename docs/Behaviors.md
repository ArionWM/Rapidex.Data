# What is Rapidex *Behaviors* ?

Many entities (definition) share common behaviors that are typically implemented using inheritance or similar design patterns.

In contrast, Rapidex.Data introduces a dynamic behavior model that promotes reusability and enables behaviors to be defined and applied dynamically.

Most common samples;

- "Auditable", etc. features: Automatically track creation and modification timestamps, as well as the user responsible for these actions.
- "Soft Deletable", etc. features: Implement soft deletion by marking records as deleted without physically removing them from the database.
- "Active / Status", etc. features: Manage the active/inactive status of records, allowing for easy filtering and management of active entities.
- "Tag / category", etc. features: Enable similar functionality to any entities

and many others.

Behaviors can contain fields and / or logic that can be reused across multiple entity definitions. 

This approach not only promotes dynamism and reusability, but also enables higher layers — such as the application or UI — to provide dynamic and generic capabilities.
For example, a *CalendarEntity* behavior can be defined and attached to entities that need to be represented within the application’s calendar view.

See: (article preparing ...)

## Structure of a Behavior

Behaviors contain two main parts: *definition* and *instance*. 

- **Behavior Definition**: Defines the structure and metadata of the behavior (metadata and logic injection). This includes fields, validation rules, and any other metadata changes. The definition is applied when the behavior is attached to an entity.

- **Behavior Instance**: Represents the runtime state and logic of the behavior. This includes any data or methods that operate on the entity's data while the application is running.

## Built-in Behaviors

### ArchiveEntity

Ensures that the Entity has IsArchived field. (in future on Rapidex: and apply archive filters and actions for UI)

> See `IsArchived` and `IsNotArchived` extension methods for filtering archived and non-archived records.

### DefinitionEntity

Ensures that the Entity has Name and Description fields.

Auto added behaviors: `ArchiveEntity`

### StartEnd

Ensures that the Entity has `StartTime` and `EndTime` fields and StartTime < EndTime validation rules 

### HasTags 

Ensures that the Entity has Tags field. And manage entity specific `TagRecord` records. These records hold the tags used in an entity.

## Usage of Behaviors

### Attach Behaviors to an Concrete Entity

You can attach behaviors to an entity definition.

In implementer class. See: [Entity Definition](EntityDefinition.md) 

```csharp
class MyEntityImplementer: IConcreteEntityImplementer<MyEntity>
{
    //...

    void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
                .AddBehavior<ArchiveEntity>(true, false)
                .AddBehavior<HasTags>(true, false);
    }
}
```

or

In library definition. See: [Library Declaration](LibraryDeclaration.md)

```csharp
void SetupMetadata(IDbScope db)
{
    db.Metadata.Get("MyEntity")
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false);
}
```

### Attach Behaviors with YAML

```yaml
_tag: entity
name: myAnotherEntity
fields:
- name: Type
# ....

behaviors: [archiveEntity, hasTags]  #<--
```

### Check if an Entity has a Behavior

From metadata

```csharp
var metadata = db.Metadata.Get("MyEntity");
if(metadata.Has<HasTags>())
{
    //...
}
```

Or direct with instance
```csharp
//...

Contact contact = work.Find<Contact>(123);
if(contact.Has<ArchiveEntity>())
{
    //...
}
```

### Using Behavior Provided Methods

```csharp
var db = Database.Dbs.Db();
var work = db.BeginWork();
//...

Contact contact = work.Find<Contact>(123);
if (!contact.Behavior<ArchiveEntity>().IsArchived())
{
    contact.Behavior<ArchiveEntity>().Archive();
    work.CommitChanges();
}
```

## Define Custom Behaviors

Behavior definition (should implement `IEntityBehaviorDefinition` interface) defines the structure and metadata of the behavior. Behavior instance (should implement `IEntityBehaviorInstance` interface) contains the runtime logic.

You can use single class to implement both definition and instance (See: `EntityBehaviorBase<>` class).


```csharp
public class MyStartTimeBehavior : EntityBehaviorBase<MyStartTimeBehavior> //<- IEntityBehaviorDefinition, IEntityBehaviorInstance
{
    public const string FIELD_START_TIME = "StartTime";

    public MyStartTimeBehavior() { }
    public MyStartTimeBehavior(IEntity entity) : base(entity) { }

    //Definition (IEntityBehaviorDefinition)
    public override IUpdateResult SetupMetadata(IDbEntityMetadata em)
    {
        // Define fields or other metadata changes 
        em.AddFieldIfNotExist<DateTimeOffset>(FIELD_START_TIME);

        // Subscribe to signals for calculation, validation, etc.
        Signal.Hub.SubscribeEntityReleated(DataReleatedSignalConstants.SIGNAL_NEW, SignalTopic.ANY, SignalTopic.ANY, SignalTopic.ANY,
            em.Name,
            MyStartTimeBehavior.OnNew);

        return new UpdateResult();
    }

    protected static ISignalHandlingResult OnNew(IEntityReleatedMessageArguments args)
    {
        IEntity myEntity = args.Entity.EnsureForActualEntity();
        myEntity[FIELD_START_TIME] = DateTimeOffset.Now;

        return args.CreateHandlingResult(myEntity);
    }

    //Instance (IEntityBehaviorInstance)
    public bool IsStartTimeExpired(DateTimeOffset time)
    {
        DateTimeOffset startTime = this.Entity.GetValue<DateTimeOffset>(FIELD_START_TIME);
        return startTime < time;
    }

    public bool IsStartTimeExpired()
    {
        return this.IsStartTimeExpired(DateTimeOffset.Now);
    }
}

```




