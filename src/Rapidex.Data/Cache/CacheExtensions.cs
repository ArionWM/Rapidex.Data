using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SqlKata;

namespace Rapidex.Data;

public static class CacheExtensions
{
    /// <summary>
    /// For tests
    /// </summary>
    internal static List<string> TagContext { get; set; } = new List<string>();



    public static string GetEntityCacheKey(IDbSchemaScope dbSchema, IDbEntityMetadata em, object id)
    {
        string dbName = dbSchema.ParentDbScope.Name;
        string schemaName = dbSchema.SchemaName;
        string key = $"{dbName}:{schemaName}:{em.Name}:{id}";
        return key;
    }

    public static string GetEntityCacheKey(IEntity entity)
    {
        entity.NotNull();

        IDbSchemaScope dbSchema = entity._Schema;
        string dbName = entity._DbName ?? entity._Schema?.ParentDbScope?.DatabaseName;
        dbName.NotEmpty();

        string schemaName = entity._SchemaName ?? entity._Schema?.SchemaName;
        schemaName.NotEmpty();

        string typeName = entity._TypeName ?? entity.GetMetadata()?.Name;
        typeName.NotEmpty();

        var id = entity.GetId();

        string key = $"{dbName}:{schemaName}:{typeName}:{id}";
        return key;
    }

    public static string GetCacheKeyHash(this SqlResult result)
    {
        // Use a span-based approach for better performance
        int capacity = result.Sql.Length + 100; // Estimate for bindings

        if (result.NamedBindings.IsNOTNullOrEmpty())
        {
            // Pre-calculate required capacity to avoid resizing
            foreach (var binding in result.NamedBindings)
            {
                capacity += binding.Key.Length + 20; // Key + separator + estimated value length
            }
        }

        StringBuilder sb = new StringBuilder(capacity);

        // Append SQL query
        sb.Append(result.Sql);
        sb.Append('|');

        // Append named bindings in sorted order for stability across machines/processes
        if (result.NamedBindings.IsNOTNullOrEmpty())
        {
            var sortedBindings = result.NamedBindings.OrderBy(kvp => kvp.Key, StringComparer.Ordinal);

            foreach (var binding in sortedBindings)
            {
                sb.Append(binding.Key);
                sb.Append('=');

                switch (binding.Value)
                {
                    case null:
                        sb.Append("N");
                        break;
                    case DateTimeOffset dto:
                        sb.Append(dto.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case DateTime dt:
                        sb.Append(dt.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case decimal dec:
                        sb.Append(dec.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case double dbl:
                        sb.Append(dbl.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case float flt:
                        sb.Append(flt.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    default:
                        sb.Append(binding.Value.ToString());
                        break;
                }

                sb.Append(';');
            }
        }

        // Generate stable SHA256 hash (32 bytes = 64 hex characters)
        string combinedString = sb.ToString();
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combinedString));

        // Convert to hex string (64 characters - well under 200 character limit)
        return Convert.ToHexString(hashBytes);

    }

    public static string GetQueryCacheKey(IDbEntityMetadata em, IDbSchemaScope dbSchema, string hash)
    {
        dbSchema.NotNull();
        em.NotNull();
        hash.NotEmpty();

        string dbName = dbSchema.ParentDbScope.DatabaseName;
        string schemaName = dbSchema.SchemaName;
        string typeName = em.Name;
        string key = $"{dbName}:{schemaName}:{typeName}:QUERY:{hash}";
        return key;
    }

    public static string GetQueryCacheKey(IDbEntityMetadata em, IDbSchemaScope dbSchema, SqlResult result)
    {
        var queryHash = result.GetCacheKeyHash();
        return GetQueryCacheKey(em, dbSchema, queryHash);
    }

    public static async Task<IEntity> Get(this IEntityCache cache, IDbSchemaScope dbSchema, string typeName, long id)
    {
        if (id.IsPrematureOrEmptyId())
            return null;

        var em = dbSchema.ParentDbScope.Metadata.Get(typeName).NotNull();

        var entity = await cache.Get<IEntity>(dbSchema, em, id);
        entity?._loadSource = LoadSource.Cache;
        return entity;
    }

    public static async Task Set(this IEntityCache cache, IEnumerable<IEntity> entities)
    {
        foreach (var entity in entities)
            await cache.Set(entity);
    }


    public static async Task SetQuery(this IEntityCache cache, IDbEntityMetadata em, IDbSchemaScope dbSchema, SqlResult result, IEntityLoadResult loadResult)
    {
        var queryHash = result.GetCacheKeyHash();

        IEntity[] entities = loadResult.ToArray(); //WARN: Wrong
        await cache.Set(dbSchema, em, queryHash, entities);
    }

    public static async Task<IEntityLoadResult> GetQuery(this IEntityCache cache, IDbEntityMetadata em, IDbSchemaScope dbSchema, SqlResult result)
    {
        var queryHash = result.GetCacheKeyHash();
        var items = await cache.GetMultiple<IEntity>(dbSchema, em, queryHash).NotNull();

        if (items.IsNullOrEmpty())
            return null;

        EntityLoadResult lres = new EntityLoadResult(items);
        return lres;
    }

    internal static void SetTagContext(this IEntityCache cache, params string[] tags)
    {
        TagContext.AddRange(tags);
    }

    internal static void ClearTagContext(this IEntityCache cache)
    {
        TagContext.Clear();
    }
}
