using Rapidex.SignalHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public static class MessageHubExtensions
{
    //public static async Task<IResult> SendOnNew(this ISignalHub hub, string tenant, IEntity entity)
    //{
    //    var em = entity.GetMetadata();

    //    SignalTopic topic = new SignalTopic();
    //    topic.Tenant = tenant;
    //    topic.Workspace = entity._Scope.SchemaName;
    //    topic.Module = em.ModuleName;
    //    topic.Entity = em.Name;
    //    topic.EntityId = entity.GetId().ToString();
    //    topic.Signal = DataReleatedSignalConstants.Signal_New;

    //    EntityReleatedMessageArguments args = new EntityReleatedMessageArguments(topic.Signal, entity);
    //    return await hub.Publish(topic, args);
    //}

    public static async Task<IResult<int>> SubscribeOnNew(this ISignalHub hub, SignalTopic topic, Func<IEntityReleatedMessageArguments, IEntityReleatedMessageArguments> handler)
    {
        return await hub.Subscribe(null, topic, args =>
        {
            IEntityReleatedMessageArguments eArgs = (IEntityReleatedMessageArguments)args;
            try
            {
                var response = handler(eArgs);
                return response;
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        });
    }

    public static async Task<IResult<int>> SubscribeOnBeforeSave(this ISignalHub hub, SignalTopic topic, Func<IEntityReleatedMessageArguments, IEntityReleatedMessageArguments> handler)
    {
        return await hub.Subscribe(null, topic, args =>
        {
            IEntityReleatedMessageArguments eArgs = (IEntityReleatedMessageArguments)args;
            try
            {
                var response = handler(eArgs);
                return response;
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        });
    }

    public static async Task<IResult<int>> SubscribeOnAfterSave(this ISignalHub hub, SignalTopic topic, Func<IEntityReleatedMessageArguments, IEntityReleatedMessageArguments> handler)
    {
        return await hub.Subscribe(null, topic, args =>
        {
            IEntityReleatedMessageArguments eArgs = (IEntityReleatedMessageArguments)args;
            try
            {
                var response = handler(eArgs);
                return response;
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        });
    }

}
