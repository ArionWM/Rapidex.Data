# Signal Hub (MessageBus)

SignalHub is a internal / embedded messaging bus with *synchronous* and asynchronous message support.

SignalHub is use topic based publish/subscribe pattern. 

Rapidex is not use MQTT or similar bus system, because synchronous message system requred with return values for some (*awesome* :) ) functions.

Can access Signal Hub (*ISignalHub*) with `Signal.Hub` and / or (with DI) `serviceProvider.GetRequiredService<ISignalHub>()`

*Note1: This functions will be moved to a separate repository in the future.*

*Note2: This document contains all Rapidex message structure. Will be separated into its own document in the future.*

## Initialization

In Rapidex system, SignalHub is initialized automatically. 

But, if you want to use SignalHub independently, you can initialize it manually.
```csharp

services.AddRapidexSignalHub();

//.....

serviceProvider.StartRapidexSignalHub();

//.....

```


## Message Specification

Two types of messages are supported: Synchronous and Asynchronous.

Both types of messages are used to same topic and payload structure.

### Synchronous Messages

Synchronous messages;

- Are used for immediate callback style communication between components. 
- Are uses Rapidex internal structure on MessageHub, only work InProcess
- Can contain responses (abc)

### Asynchronous Messages

Asynchronous messages 

- Are used for decoupled communication between components. Can use from UI and other API.
- Can't contain responses.


## Subscriber Levels

### System Level Subscribers

Built-in functions and modules that are part of the Rapidex system. These clients have access to all databases (and/or tenants), workspaces, and modules.

### User (?) Level Subscribers

*Planned*

User defined functions (automation). Only access own tenant messages.

## Message Topics

Rapidex Navigation System like (not same) structure used. 

### Structure

Topic structure has 4 base sections. Additional sections are added to the end of the topic and used for specific message types.

`<tenantOrDatabaseName>/<workspace>/<module>/<eventName>/<event type specific sections>`

## Wildcards and Relative Topics

### Single Section Wildcards

Each topic section has a wildcard character that can be used to subscribe to multiple topics at once. The single section wildcard character is `+`.

- Wildcards can't used `Event Name` section.
- Wildcards can't used `tenant` section for user level clients.

### Multi Section Wildcards

The multi-section wildcard is `#`. Only use after `Event Name` section. 

This mean, `+/+/+/AfterSave/#` will match all topics that start with `AfterSave`, regardless of the database/tenant, schema/workspace or module.

### Shorter Topics (for registration)

Shorter topics can be used after `Event Name` section. This equal to `#` usage

### Wildcard Examples

- `+/+/+/AfterSave/+/+` Register for all `AfterSave` messages for all databases (and/or tenants), workspaces, modules and entities. 
- `+/+/+/MySignal/+` Register for all `MySignal` messages for all databases (and/or tenants), workspaces, modules and entities. But; if publisher use `tenant/workspace/module/MySignal/contact/123` then this will not match, because `+` wildcard only match single section and not cover to 6th section (id for this sample).
- `+/+/+/MySignal/#` Register for all `MySignal` messages for all databases (and/or tenants), workspaces, modules and entities. This will match all topics that start with `MySignal`, regardless of the tenant, workspace, module, or entity. And this will match `tenant/workspace/module/MySignal/contact/123` too.

### Samples

#### Publish 

```csharp

```

#### Subscribe

```csharp

```

### Persistence

Persistence not supported (yet - only planned for async messages)

### Build In Message / Topic Types

SignalHub can support different / custom event types. This definitions are for built-in message types for Rapidex.

#### Entity Related Messages

`<tenantOrDatabaseName>/<workspace>/<module>/<eventName>/<entityName>/<entityId>` 

or

`<tenantOrDatabaseName>/<workspace>/<module>/<eventName>/<entityName>/<entityId>/<fieldName>`

|Event Name|Sync|Async|Argument Type|Access From UI|Description|Sample|
|---|---|---|---|---|---|---|
|New|X||IEntityReleatedMessageArguments||New entity created|`+/+/common/new/contact/+` -> Any *contact* entity, after created instance. Useful for predefined field values.. |
|BeforeSave|X||IEntityReleatedMessageArguments||Before entity saved|`+/+/common/beforesave/contact/123` -> Contact by 123 id, call before save|
|AfterSave||X|IEntityReleatedMessageArguments||After entity saved|`+/+/common/aftersave/contact/123` -> Contact by 123 id, call after save (not commited to db yet)|
|AfterCommit||X|IEntityReleatedMessageArguments||After entity releated scope commited||
|BeforeDelete|X||IEntityReleatedMessageArguments||Before entity deleted||
|AfterDelete|X||IEntityReleatedMessageArguments||After entity deleted||
|Editing|X||IEntityReleatedMessageArguments||Entity changing in UI||
|FileAttached||X|IEntityReleatedMessageArguments||A file attached to entity||
|NoteAttached||X|IEntityReleatedMessageArguments||Note (or comment) add to entity||


#### Behavior Releated Messages

|Event Name|Sync|Async|Argument Type|Access From UI|Description|
|---|---|---|---|---|---|
|Archived||X|IEntityReleatedMessageArguments||Entity archived|
|Unarchived||X|IEntityReleatedMessageArguments||Entity unarchived|
|TaskAttached||X|IEntityReleatedMessageArguments||New task (activity) attached to entity|

#### Automation Releated Messages

|Event Name|Sync|Async|Argument Type|Access From UI|Description|Sample|
|---|---|---|---|---|---|---|
|TimeArrived||X|IEntityReleatedMessageArguments||Time arrived releated field value|`+/+/common/task/+/PlannedDateEnd` -> It is called before and after the task's due date (in various periods). How long before / after it was called is in the "args" variable.|


#### Authorization Related Messages

|Event Name|Sync|Async|Argument Type|Access From UI|Description|
|---|---|---|---|---|---|
|Login||X|IEntityReleatedMessageArguments||User logged in|
|Logout||X|IEntityReleatedMessageArguments||User logged out|

#### System Related Messages

|Event Name|Sync|Async|Argument Type|Access From UI|Description|
|---|---|---|---|---|---|
|OnError|X||...||Error occurred|
|SystemStarting||X|...|||
|SystemStarted||X|...|||
|SystemStopping|X||...|||
|WorkspaceCreated||X|...|||
|WorkspaceDeleted||X|...|||
|ModuleInstalled||X|...|||
|ModuleActivated||X|...|||
|ModuleDeactivated||X|...|||

#### To User Messages

|Event Name|Sync|Async|Argument Type|Access From UI|Description|
|---|---|---|---|---|---|
|UiEditedEntityContent|X||?|| |
|UiNotify||X|?||gruba ya da kullanıcıya uyarı göndermek|
|UiTrackedEntityChanged||X|?||Takip edilen entity'deki değişiklikler (dosya eklemek, yorum yapmak vb.)|
|EntityListChanged||X|?||Açık olan liste (Kanban, takvim vb.) içerisinde bir entity değişti ise (örn faz değişikiliği, renk değişikliği vs)|
|UiMessageArrived||X|?|||
|UiCallAvailable||X|?|||

