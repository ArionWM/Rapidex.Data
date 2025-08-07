using Mapster;
using System;

namespace Rapidex.Data;
internal class EntityMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<IEntity, IEntity>()
            .MapWith(source => CloneEntity(source));
    }

    private static IEntity CloneEntity(IEntity source)
    {
        if (source == null)
            return null;

        return source.Clone();
    }
}
