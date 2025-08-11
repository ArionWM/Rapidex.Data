using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rapidex.Data;

//TODO: Not possible for this level, must remove and use another approach.


public interface IImplementHost
{
    string ModuleName { get; }
    //IDbScope DbScope { get; }
    IDbMetadataContainer Parent { get; }

    void SetParent(IDbMetadataContainer parent);

    IUpdateResult AddJson(string json);
    IUpdateResult AddYaml(string yaml);
}

//See: JsonImplementerInterfaceConverter
//[JsonConverter(typeof(JsonImplementerInterfaceConverter))]
[JsonDerivedBase]
public interface IImplementer
{
    string[] SupportedTags { get; }
    bool Implemented { get; }
    IUpdateResult Implement(IImplementHost host, IImplementer parentImplementer, ref object target);
}


public interface IImplementTarget
{

}

public interface IImplementer<T> : IImplementer where T : IImplementTarget
{
    void Implement(IImplementTarget parentModule, ref T target)
    {
        this.Implement(parentModule, ref target);
    }
}



