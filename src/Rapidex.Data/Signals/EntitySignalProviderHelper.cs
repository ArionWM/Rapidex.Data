using Rapidex.Data;
using Rapidex.Data.Signals;
using Rapidex.SignalHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
internal static class EntitySignalProviderHelper
{
    public static void CreatePredefinedContent(ISignalHub hub)
    {
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.SIGNAL_NEW, "On New", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.SIGNAL_BEFORESAVE, "Before Save", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.SIGNAL_AFTERSAVE, "After Save", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.SIGNAL_AFTERCOMMIT, "After Commit", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.SIGNAL_BEFOREDELETE, "Before Delete", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.SIGNAL_AFTERDELETE, "After Delete", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.SIGNAL_VALIDATE, "Validate", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.SIGNAL_EXEC_LOGIC, "ExecLogic", "Entity", true));


        //hub.RegisterSignalDefinition(new SignalDefinition(SignalConstants.Signal_WorkspaceCreated, "Workspace Created", "System", false));
        //hub.RegisterSignalDefinition(new SignalDefinition(SignalConstants.Signal_WorkspaceDeleted, "Workspace Deleted", "System", false));

        hub.RegisterSignalDefinition(new SignalDefinition(SignalConstants.SIGNAL_SCHEMAORWORKSPACECREATED, "Schema / Workspace Created", "System", false));
        hub.RegisterSignalDefinition(new SignalDefinition(SignalConstants.SIGNAL_SCHEMAORWORKSPACEDELETED, "Schema / Workspace Deleted", "System", false));


    }

    public static async Task<IEntity> PublishOnNew(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            DatabaseOrTenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Event = DataReleatedSignalConstants.SIGNAL_NEW
        };

        EntityReleatedMessageArguments inputArg = new EntityReleatedMessageArguments(topic.Event, entity);

        ISignalProcessResult result = await Rapidex.Signal.Hub.PublishAsync(topic, inputArg);
        EntityReleatedMessageArguments outputArg = result.Arguments as EntityReleatedMessageArguments;
        return outputArg?.Entity;
    }

    public static async Task<IEntity> PublishOnBeforeSave(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            DatabaseOrTenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Event = DataReleatedSignalConstants.SIGNAL_BEFORESAVE
        };

        EntityReleatedMessageArguments inputArg = new EntityReleatedMessageArguments(topic.Event, entity);

        ISignalProcessResult result = await Rapidex.Signal.Hub.PublishAsync(topic, inputArg);
        EntityReleatedMessageArguments outputArg = result.Arguments as EntityReleatedMessageArguments;
        return outputArg?.Entity;
    }

    public static void PublishOnAfterSave(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            DatabaseOrTenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Event = DataReleatedSignalConstants.SIGNAL_AFTERSAVE
        };
        Rapidex.Signal.Hub.PublishAsync(topic, new EntityReleatedMessageArguments(topic.Event, entity));
    }

    public static void PublishOnAfterCommit(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            DatabaseOrTenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Event = DataReleatedSignalConstants.SIGNAL_AFTERCOMMIT
        };
        Rapidex.Signal.Hub.PublishAsync(topic, new EntityReleatedMessageArguments(topic.Event, entity));
    }

    public static async Task<IEntity> PublishOnBeforeDelete(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            DatabaseOrTenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Event = DataReleatedSignalConstants.SIGNAL_BEFOREDELETE
        };

        EntityReleatedMessageArguments inputArg = new EntityReleatedMessageArguments(topic.Event, entity);

        ISignalProcessResult result = await Rapidex.Signal.Hub.PublishAsync(topic, inputArg);
        EntityReleatedMessageArguments outputArg = result.Arguments as EntityReleatedMessageArguments;
        return outputArg?.Entity;
    }


    public static void PublishOnAfterDelete(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            DatabaseOrTenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Event = DataReleatedSignalConstants.SIGNAL_AFTERDELETE
        };
        Rapidex.Signal.Hub.PublishAsync(topic, new EntityReleatedMessageArguments(topic.Event, entity));
    }

    public static async Task<IValidationResult> PublishForValidate(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            DatabaseOrTenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Event = DataReleatedSignalConstants.SIGNAL_VALIDATE
        };
        EntityReleatedMessageArguments inputArg = new EntityReleatedMessageArguments(topic.Event, entity);

        ISignalProcessResult result = await Rapidex.Signal.Hub.PublishAsync(topic, inputArg);
        return result;
    }

    public static async Task<IEntity> PublishForExecLogic(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            DatabaseOrTenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Event = DataReleatedSignalConstants.SIGNAL_EXEC_LOGIC
        };
        EntityReleatedMessageArguments inputArg = new EntityReleatedMessageArguments(topic.Event, entity);

        ISignalProcessResult result = await Rapidex.Signal.Hub.PublishAsync(topic, inputArg);
        EntityReleatedMessageArguments outputArg = result.Arguments as EntityReleatedMessageArguments;
        return outputArg?.Entity;
    }



   
}
