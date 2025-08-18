using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rapidex.Data;


/// <summary>
/// Add metadata definitions to host
/// </summary>
/// <see cref="ImplementerExtender"/>
/// <see cref="ImplementerExtender.ScanConcreteDefinitions(Rapidex.Data.IDbMetadataContainer)"/>
/// <see cref="ImplementerExtender.ScanDefinitions(Rapidex.Data.IDbMetadataContainer, string)"/>
public interface IMetadataImplementHost
{
    string ModuleName { get; }
    //IDbScope DbScope { get; }
    IDbMetadataContainer Parent { get; }
    IDbEntityMetadataFactory EntityMetadataFactory { get; }
    IFieldMetadataFactory FieldMetadataFactory { get; }

    void SetParent(IDbMetadataContainer parent);

    IUpdateResult AddJson(string json);
    IUpdateResult AddYaml(string yaml);
    IUpdateResult AddConcrete(Type type);
}

//See: JsonImplementerInterfaceConverter
//[JsonConverter(typeof(JsonImplementerInterfaceConverter))]
[JsonDerivedBase]
public interface IImplementer
{
    string[] SupportedTags { get; }
    bool Implemented { get; }
    IUpdateResult Implement(IMetadataImplementHost host, IImplementer parentImplementer, ref object target);
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



