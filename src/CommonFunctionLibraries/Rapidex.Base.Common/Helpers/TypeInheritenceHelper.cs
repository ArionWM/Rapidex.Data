using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;

//Copilot Generated 
public static class TypeInheritenceHelper
{
    /// <summary>
    /// Static cache for type inheritance relationships to improve performance
    /// Key: Type, Value: HashSet of base types within the current type array
    /// </summary>
    private static readonly ConcurrentDictionary<string, HashSet<Type>> InheritanceTypesCache = new();

    /// <summary>
    /// Static cache for inheritance depth calculations
    /// Key: Type full name, Value: inheritance depth
    /// </summary>
    private static readonly ConcurrentDictionary<string, int> InheritanceDepthCache = new();

    private static readonly TwoLevelList<Type, Type> BaseTypeCache = new();

    /// <summary>
    /// Sorts an array of types based on inheritance hierarchy, placing derived types before their base types.
    /// Uses static caching for improved performance on repeated calls.
    /// For example, if type B derives from type A, then B will appear before A in the result.
    /// </summary>
    /// <param name="types">Array of types to sort</param>
    /// <returns>Sorted array with derived types first</returns>
    public static Type[] SortTypesByInheritanceHierarchy(Type[] types)
    {
        if (types == null || types.Length <= 1)
            return types;

        // Create a cache key based on the types array
        string cacheKey = CreateCacheKey(types);

        // Build inheritance relationships map
        var inheritanceMap = BuildInheritanceMap(types, cacheKey);

        var typeList = types.ToList();
        var result = new List<Type>();
        var processed = new HashSet<Type>();

        // Continue until all types are processed
        while (typeList.Count > 0)
        {
            var currentBatch = new List<Type>();

            // Find types that don't have any unprocessed base types in the current list
            for (int i = typeList.Count - 1; i >= 0; i--)
            {
                var currentType = typeList[i];
                bool hasUnprocessedBaseType = false;

                // Check cached inheritance relationships
                if (inheritanceMap.TryGetValue(currentType, out var baseTypes))
                {
                    foreach (var baseType in baseTypes)
                    {
                        if (!processed.Contains(baseType))
                        {
                            hasUnprocessedBaseType = true;
                            break;
                        }
                    }
                }

                // If no unprocessed base types, this type can be added to current batch
                if (!hasUnprocessedBaseType)
                {
                    currentBatch.Add(currentType);
                    typeList.RemoveAt(i);
                }
            }

            // Sort current batch by inheritance depth (most derived first)
            currentBatch.Sort((x, y) => GetCachedInheritanceDepth(y, types).CompareTo(GetCachedInheritanceDepth(x, types)));
            result.AddRange(currentBatch);

            // Mark these types as processed
            foreach (var type in currentBatch)
            {
                processed.Add(type);
            }

            // Safety check to prevent infinite loop
            if (currentBatch.Count == 0 && typeList.Count > 0)
            {
                // If we can't process any more types, add remaining ones
                result.AddRange(typeList);
                break;
            }
        }

        return result.ToArray();
    }

    private static IEnumerable<Type> CreateBaseTypeCache(Type type)
    {
        var baseTypes = new List<Type>();
        var currentType = type.BaseType;
        while (currentType != null && currentType != typeof(object))
        {
            baseTypes.Add(currentType);
            currentType = currentType.BaseType;
        }
        BaseTypeCache.Add(type, baseTypes);

        return baseTypes;
    }

    public static Type[] RemoveBaseTypes(this Type[] types)
    {
        var typeSet = new HashSet<Type>(types);

        foreach (var type in types)
        {
            IEnumerable<Type> baseTypesList = BaseTypeCache.Get(type);
            if (baseTypesList == null)
            {
                baseTypesList = CreateBaseTypeCache(type);
            }

            foreach (var baseType in baseTypesList)
            {
                if (typeSet.Contains(baseType))
                {
                    typeSet.Remove(baseType);
                }
            }
        }

        return typeSet.ToArray();
    }

    /// <summary>
    /// Builds inheritance relationships map with caching support
    /// </summary>
    /// <param name="types">Array of types to analyze</param>
    /// <param name="cacheKey">Cache key for the type array</param>
    /// <returns>Dictionary mapping each type to its base types within the array</returns>
    private static Dictionary<Type, HashSet<Type>> BuildInheritanceMap(Type[] types, string cacheKey)
    {
        var inheritanceMap = new Dictionary<Type, HashSet<Type>>();
        var typeSet = new HashSet<Type>(types);

        foreach (var type in types)
        {
            var baseTypesKey = $"{cacheKey}_{type.FullName}";

            if (!InheritanceTypesCache.TryGetValue(baseTypesKey, out var baseTypes))
            {
                baseTypes = new HashSet<Type>();

                // Find all base types within the current type array
                var currentType = type.BaseType;
                while (currentType != null && currentType != typeof(object))
                {
                    if (typeSet.Contains(currentType))
                    {
                        baseTypes.Add(currentType);
                    }
                    currentType = currentType.BaseType;
                }

                // Cache the result
                InheritanceTypesCache.TryAdd(baseTypesKey, baseTypes);
            }

            inheritanceMap[type] = baseTypes;
        }

        return inheritanceMap;
    }

    /// <summary>
    /// Gets cached inheritance depth of a type within the given type array.
    /// Higher values indicate more derived types.
    /// </summary>
    /// <param name="type">Type to calculate depth for</param>
    /// <param name="typeArray">Array of types to consider for depth calculation</param>
    /// <returns>Inheritance depth</returns>
    private static int GetCachedInheritanceDepth(Type type, Type[] typeArray)
    {
        var cacheKey = $"{CreateCacheKey(typeArray)}_{type.FullName}_depth";

        if (!InheritanceDepthCache.TryGetValue(cacheKey, out var depth))
        {
            depth = 0;
            var typeSet = new HashSet<Type>(typeArray);

            var currentType = type.BaseType;
            while (currentType != null && currentType != typeof(object))
            {
                if (typeSet.Contains(currentType))
                {
                    depth++;
                }
                currentType = currentType.BaseType;
            }

            // Cache the result
            InheritanceDepthCache.TryAdd(cacheKey, depth);
        }

        return depth;
    }

    /// <summary>
    /// Creates a cache key from the types array
    /// </summary>
    /// <param name="types">Array of types</param>
    /// <returns>Cache key string</returns>
    private static string CreateCacheKey(Type[] types)
    {
        if (types == null || types.Length == 0)
            return "empty";

        // Create a deterministic key based on type full names
        var sortedNames = types.Select(t => t.FullName).OrderBy(n => n);
        return string.Join("|", sortedNames);
    }

    /// <summary>
    /// Clears the static inheritance caches. Call this method if type definitions change at runtime.
    /// </summary>
    public static void ClearInheritanceCache()
    {
        InheritanceTypesCache.Clear();
        InheritanceDepthCache.Clear();
    }
}
