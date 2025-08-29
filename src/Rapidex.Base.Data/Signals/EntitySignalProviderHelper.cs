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
     
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.Signal_New, "On New", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.Signal_BeforeSave, "Before Save", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.Signal_AfterSave, "After Save", "Entity", false));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.Signal_AfterCommit, "After Commit", "Entity", false));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.Signal_BeforeDelete, "Before Delete", "Entity", true));
        hub.RegisterSignalDefinition(new SignalDefinition(DataReleatedSignalConstants.Signal_AfterDelete, "After Delete", "Entity", false));
    }

    public static async Task<IEntity> PublishOnNew(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            Tenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Signal = DataReleatedSignalConstants.Signal_New
        };

        EntityReleatedMessageArguments inputArg = new EntityReleatedMessageArguments(topic.Signal, entity);

        IResult<IEnumerable<ISignalArguments>> result = await Rapidex.Common.SignalHub.Publish(topic, inputArg);
        EntityReleatedMessageArguments outputArg = result.Content.FirstOrDefault() as EntityReleatedMessageArguments;
        return outputArg?.Entity;
    }

    public static async Task<IEntity> PublishOnBeforeSave(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            Tenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Signal = DataReleatedSignalConstants.Signal_BeforeSave
        };

        EntityReleatedMessageArguments inputArg = new EntityReleatedMessageArguments(topic.Signal, entity);

        IResult<IEnumerable<ISignalArguments>> result = await Rapidex.Common.SignalHub.Publish(topic, inputArg);
        EntityReleatedMessageArguments outputArg = result.Content.FirstOrDefault() as EntityReleatedMessageArguments;
        return outputArg?.Entity;
    }

    public static void PublishOnAfterSave(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            Tenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Signal = DataReleatedSignalConstants.Signal_AfterSave
        };
        Rapidex.Common.SignalHub.Publish(topic, new EntityReleatedMessageArguments(topic.Signal, entity));
    }

    public static void PublishOnAfterCommit(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            Tenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Signal = DataReleatedSignalConstants.Signal_AfterCommit
        };
        Rapidex.Common.SignalHub.Publish(topic, new EntityReleatedMessageArguments(topic.Signal, entity));
    }

    public static async Task<IEntity> PublishOnBeforeDelete(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            Tenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Signal = DataReleatedSignalConstants.Signal_BeforeDelete
        };

        EntityReleatedMessageArguments inputArg = new EntityReleatedMessageArguments(topic.Signal, entity);

        IResult<IEnumerable<ISignalArguments>> result = await Rapidex.Common.SignalHub.Publish(topic, inputArg);
        EntityReleatedMessageArguments outputArg = result.Content.FirstOrDefault() as EntityReleatedMessageArguments;
        return outputArg?.Entity;
    }


    public static void PublishOnAfterDelete(this IEntity entity)
    {
        var em = entity.GetMetadata();
        SignalTopic topic = new SignalTopic()
        {
            Tenant = entity._Schema.ParentDbScope.Name,
            Workspace = entity._Schema.SchemaName,
            Module = em.ModuleName ?? CommonConstants.MODULE_COMMON,
            Entity = em.NavigationName,
            EntityId = entity.GetId().ToString(),
            Signal = DataReleatedSignalConstants.Signal_AfterDelete
        };
        Rapidex.Common.SignalHub.Publish(topic, new EntityReleatedMessageArguments(topic.Signal, entity));
    }
}
