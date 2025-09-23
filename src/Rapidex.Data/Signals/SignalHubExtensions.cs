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
    ////+/+/common/beforesave/myEntity/#

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, SignalTopic topic, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {
        return hub.Subscribe(topic, args =>
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

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, [NotNull] string @event, string? databaseOrTenantShortName, string? workspace, string? module, [NotNull] string entity, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {

        SignalTopic topic = SignalTopic.Create(databaseOrTenantShortName ?? SignalTopic.ANY, workspace ?? SignalTopic.ANY, module ?? SignalTopic.ANY, @event, entity, SignalTopic.ANY_ALL_SECTIONS);
        return hub.SubscribeEntityReleated(topic, handler);
    }

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, [NotNull] string @event, string? databaseOrTenantShortName, string? workspace, string? module, [NotNull] IDbEntityMetadata em, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {
        SignalTopic topic = SignalTopic.Create(databaseOrTenantShortName ?? SignalTopic.ANY, workspace ?? SignalTopic.ANY, module ?? SignalTopic.ANY, @event, em.NavigationName, SignalTopic.ANY_ALL_SECTIONS);
        return hub.SubscribeEntityReleated(topic, handler);
    }

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, [NotNull] string @event, [NotNull] IDbEntityMetadata em, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {
        SignalTopic topic = SignalTopic.Create(SignalTopic.ANY, SignalTopic.ANY, SignalTopic.ANY, @event, em.NavigationName, SignalTopic.ANY_ALL_SECTIONS);
        return hub.SubscribeEntityReleated(topic, handler);
    }

    public static IResult<int> SubscribeEntityReleated(this ISignalHub hub, [NotNull] string @event, [NotNull] IEntity entity, Func<IEntityReleatedMessageArguments, ISignalHandlingResult> handler)
    {
        var em = entity.GetMetadata();
        object id = entity.GetId();

        SignalTopic topic = SignalTopic.Create(entity._DbName, entity._SchemaName, SignalTopic.ANY, @event, em.NavigationName, id.ToString(), SignalTopic.ANY_ALL_SECTIONS);
        return hub.SubscribeEntityReleated(topic, handler);
    }

    public static ISignalHandlingResult CreateHandlingResult(this IEntityReleatedMessageArguments args)
    {
        var res = new SignalHandlingResult(args.HandlerId);
        return res;
    }


}
