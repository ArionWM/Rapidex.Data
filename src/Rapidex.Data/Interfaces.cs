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


namespace Rapidex.Data;


public interface IBlobRepository
{
    IResult<BlobRecord> Set(Stream content, string name, string contentType, long id = DatabaseConstants.DEFAULT_EMPTY_ID);

    //DbDataModificationManager currentTransaction ve dbChangesScope threadLocal oldukları için async olduğunda farklı scope'a geçiyor (silme işlemi)
    IEntityUpdateResult Delete(long id);
    IResult<StreamContent> Get(long id);

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


public interface ICache
{
    T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null);
    Task<T> GetOrSetAsync<T>(string key, Func<T> valueFactory, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null);
    void Set<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, TimeSpan? localCacheExpiration = null);
    void Remove(string key);
    
}


