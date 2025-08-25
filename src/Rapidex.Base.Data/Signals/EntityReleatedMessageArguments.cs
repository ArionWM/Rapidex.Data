using Mapster;
using Rapidex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Signals;
public class EntityReleatedMessageArguments : SignalArguments, IEntityReleatedMessageArguments
{
    public IEntity Entity { get; set; }

    public EntityReleatedMessageArguments()
    {

    }

    public EntityReleatedMessageArguments(string signalName)
    {
        SignalName = signalName;
    }


    public EntityReleatedMessageArguments(string signalName, IEntity entity)
    {
        SignalName = signalName;
        Entity = entity;
    }


}

internal class EntityReleatedMessageArgumentsMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
                .NewConfig<EntityReleatedMessageArguments, EntityReleatedMessageArguments>()
                .Ignore(dest => dest.Entity)
                .AfterMapping((src, dest) =>
                {
                    dest.Entity = src.Entity;//Deep clone istemiyoruz
                });
    }
}
