using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Rapidex.Data.SerializationAndMapping.MetadataImplementers;
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
//[JsonDerivedBase]

//[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization, TypeDiscriminatorPropertyName = "type")]
////[JsonDerivedType(typeof(EntityDefinitionImplementer), typeDiscriminator: "!entity")]
////[JsonDerivedType(typeof(EntityDefinitionImplementer), typeDiscriminator: "entity")]
//[JsonDerivedType(typeof(EntityDefinitionImplementer), typeDiscriminator: "EntityDefinition")]
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

public interface IConcreteEntityImplementer
{
    void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata);
}

public interface IConcreteEntityImplementer<T> : IConcreteEntityImplementer where T : IConcreteEntity
{

}

