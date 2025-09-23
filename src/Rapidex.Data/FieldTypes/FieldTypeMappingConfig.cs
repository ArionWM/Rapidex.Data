using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

internal class FieldTypeMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<IDataType, IDataType>()
            .MapWith(source => (IDataType)source.Clone());
    }
}
