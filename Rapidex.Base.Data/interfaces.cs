using Microsoft.Extensions.Options;
using Rapidex.Base;
using Rapidex.Data.Entities;
using Rapidex.Data.Metadata;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static Rapidex.Data.EntitySerializationDataCreator;

namespace Rapidex.Data;







[Obsolete("//TODO: To Metadata.Data ...")]
public interface IPredefinedValueProcessor
{
    void Register(IDbEntityMetadata em, bool @override, params ObjDictionary[] entityValues);
    void Register(IDbEntityMetadata em, JsonNode jsonMetadataDef);
    void Clear(IDbEntityMetadata em);
    void Remove(IDbEntityMetadata em, long id);

    PredefinedValueItems Get(IDbEntityMetadata em);

    Task Apply(IDbSchemaScope scope);

    Task Apply(IDbSchemaScope scope, IEnumerable<IEntity> unregisteredData, bool @override);

    void Clear();
}

public interface IBlobRepository
{
    Task<IResult<BlobRecord>> Set(Stream content, string name, string contentType, long id = DatabaseConstants.DEFAULT_EMPTY_ID);

    //DbDataModificationManager currentTransaction ve dbChangesScope threadLocal oldukları için async olduğunda farklı scope'a geçiyor (silme işlemi)
    IEntityUpdateResult Delete(long id);
    Task<IResult<StreamContent>> Get(long id);

}

public interface ILazyBlob : IReference, IDataType<long>
{
    new internal bool? SkipDirectLoad => false;

    new StreamContent GetContent();

    BlobRecord SetContent(Stream stream, string name, string contentType);
}


public interface IRapidexMetadataReleatedAssemblyDefinition : IRapidexAssemblyDefinition
{
    void SetupMetadata(IDbScope db);
}

///// <summary>
///// Concrete entity'ler için metadata özelleştirmeleri ve filtreler için çalışır
///// </summary>
//public interface IEntityMetadataImplementer
//{
//    IDbScope Scope { get; }
//    IDbEntityMetadata Entity { get; }
//    IDbEntityMetadataManager Manager { get; }

//    //Predefined values

//    void Apply();
//}
