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

    public static string GetEntityCacheKey(IEntity entity)
    {
        IDbSchemaScope dbSchema = entity._Schema;
        string dbName = dbSchema.ParentDbScope.Name;
        string schemaName = dbSchema.SchemaName;
        var typeName = entity._TypeName;
        var id = entity.GetId();

        return $"{dbName}:{schemaName}:{typeName}:{id}";
    }

    public static void SetEntity(this ICache cache, IEntity entity, TimeSpan? expiration = null)
    {
        if (entity.HasPrematureId())
            return;

        string key = GetEntityCacheKey(entity);
        cache.Set(key, entity, expiration, expiration);
    }

    public static void SetEntities(this ICache cache, IEnumerable<IEntity> entities, TimeSpan? expiration = null)
    {
        foreach (var entity in entities)
        {
            cache.SetEntity(entity, expiration);
        }
    }

    public static IEntity GetEntity(this ICache cache, IDbSchemaScope dbSchema, string typeName, long id)
    {
        if (id.IsPrematureId())
            return null;

        string dbName = dbSchema.ParentDbScope.Name;
        string schemaName = dbSchema.SchemaName;
        string key = $"{dbName}:{schemaName}:{typeName}:{id}";
        var entity = cache.GetOrSet<IEntity>(key, () => null);
        entity?._loadSource = LoadSource.Cache;
        return entity;
    }

    public static void RemoveEntity(this ICache cache, IEntity entity)
    {
        if (entity.HasPrematureId())
            return;

        string key = GetEntityCacheKey(entity);
        cache.Remove(key);
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

    public static string GetQueryCacheKey(IDbEntityMetadata em, IDbSchemaScope dbSchema, SqlResult result)
    {
        string dbName = dbSchema.ParentDbScope.Name;
        string schemaName = dbSchema.SchemaName;
        var typeName = em.Name;
        var queryHash = result.GetCacheKeyHash();
        return $"{dbName}:{schemaName}:{typeName}:QUERY:{queryHash}";
    }

    public static void StoreQuery(this ICache cache, IDbEntityMetadata em, IDbSchemaScope dbSchema, SqlResult result, IEntityLoadResult loadResult, TimeSpan? expiration = null)
    {
        string key = GetQueryCacheKey(em, dbSchema, result);
        IEntity[] entities = loadResult.ToArray(); //WARN: Wrong
        cache.Set(key, entities, expiration, expiration);
    }

    public static IEntityLoadResult GetQuery(this ICache cache, IDbEntityMetadata em, IDbSchemaScope dbSchema, SqlResult result)
    {
        string key = GetQueryCacheKey(em, dbSchema, result);
        var items = cache.GetOrSet<IEntity[]>(key, () => null);
        if (items != null)
        {
            foreach (var entity in items)
            {
                entity._loadSource = LoadSource.Cache;
            }
        }

        if (items.IsNullOrEmpty())
            return null;

        EntityLoadResult lres = new EntityLoadResult(items);
        return lres;
    }

    internal static void SetTagContext(this ICache cache, params string[] tags)
    {
        TagContext.AddRange(tags);
    }

    internal static void ClearTagContext(this ICache cache)
    {
        TagContext.Clear();
    }
}
