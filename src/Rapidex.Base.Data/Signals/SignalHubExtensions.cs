using Rapidex.SignalHub;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;
public static class SignalHubExtensions
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
    //    return  hub.Publish(topic, args);
    //}

    ////+/+/common/beforesave/myEntity/+

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, SignalTopic topic, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {
        return hub.Subscribe(null, topic, args =>
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

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, [NotNull] string signal, string? databaseOrTenantShortName, string? workspace, string? module, [NotNull] string entity, string? entityId, string? field, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {

        SignalTopic topic = SignalTopic.Create(databaseOrTenantShortName ?? SignalTopic.ANY, workspace ?? SignalTopic.ANY, module ?? SignalTopic.ANY, signal, entity, entityId, field);
        return hub.SubscribeEntityReleated(topic, handler);
    }

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, [NotNull] string signal, string? databaseOrTenantShortName, string? workspace, string? module, [NotNull] IDbEntityMetadata em, string? field, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {
        SignalTopic topic = SignalTopic.Create(databaseOrTenantShortName ?? SignalTopic.ANY, workspace ?? SignalTopic.ANY, module ?? SignalTopic.ANY, signal, em.NavigationName, null, field);
        return hub.SubscribeEntityReleated(topic, handler);
    }

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, [NotNull] string signal, [NotNull] IDbEntityMetadata em, string? field, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {
        SignalTopic topic = SignalTopic.Create(SignalTopic.ANY, SignalTopic.ANY, SignalTopic.ANY, signal, em.NavigationName, null, field);
        return hub.SubscribeEntityReleated(topic, handler);
    }

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, [NotNull] string signal, string? databaseOrTenantShortName, string? workspace, string? module, [NotNull] IEntity entity, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {
        var em = entity.GetMetadata();
        object id = entity.GetId();

        SignalTopic topic = SignalTopic.Create(databaseOrTenantShortName ?? SignalTopic.ANY, workspace ?? SignalTopic.ANY, module ?? SignalTopic.ANY, signal, em.NavigationName, id.ToString(), null);
        return hub.SubscribeEntityReleated(topic, handler);
    }

    public static ISignalHandlingResult CreateResult(this IEntityReleatedMessageArguments args)
    {
        var res = new SignalHandlingResult(args.ClientId);
        return res;
    }


}
