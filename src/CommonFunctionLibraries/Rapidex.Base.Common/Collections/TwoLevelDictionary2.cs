using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Rapidex.Collections;

public class TwoLevelDictionary2<K1, K2, TValue> where K1 : notnull where K2 : notnull
{
    private readonly ConcurrentDictionary<K1, ConcurrentDictionary<K2, TValue>> innerDictionary;
    private readonly IEqualityComparer<K2> k2Comparer;

    public TwoLevelDictionary2()
    {
        var k1Comparer = typeof(K1).IsSupportTo<string>() ? (IEqualityComparer<K1>)StringComparer.InvariantCultureIgnoreCase : null;
        k2Comparer = typeof(K2).IsSupportTo<string>() ? (IEqualityComparer<K2>)StringComparer.InvariantCultureIgnoreCase : null;
        innerDictionary = new ConcurrentDictionary<K1, ConcurrentDictionary<K2, TValue>>(k1Comparer);
    }

    public TwoLevelDictionary2(IEqualityComparer<K1> equalityComparer)
    {
        k2Comparer = typeof(K2).IsSupportTo<string>() ? (IEqualityComparer<K2>)StringComparer.InvariantCultureIgnoreCase : null;
        innerDictionary = new ConcurrentDictionary<K1, ConcurrentDictionary<K2, TValue>>(equalityComparer);
    }

    protected void SetInternal(K1 key1, K2 key2, TValue value)
    {
        var subDict = innerDictionary.GetOrAdd(key1, _ => new ConcurrentDictionary<K2, TValue>(k2Comparer));
        subDict[key2] = value;
    }

    public void Set(K1 key1, K2 key2, TValue value)
    {
        this.SetInternal(key1, key2, value);
    }

    protected TValue GetInternal(K1 key1, K2 key2)
    {
        if (innerDictionary.TryGetValue(key1, out var dict))
        {
            if (dict.TryGetValue(key2, out var value))
                return value;
        }

        return default;
    }

    public TValue Get(K1 key1, K2 key2)
    {
        return this.GetInternal(key1, key2);
    }

    public TValue GetOr(K1 key1, K2 key2, Func<K1, K2, TValue> nofunc)
    {
        var subDict = innerDictionary.GetOrAdd(key1, _ => new ConcurrentDictionary<K2, TValue>(k2Comparer));

        return subDict.GetOrAdd(key2, _ => nofunc(key1, key2));
    }

    public TValue GetOr(K1 key1, K2 key2, Func<TValue> nofunc)
    {
        var subDict = innerDictionary.GetOrAdd(key1, _ => new ConcurrentDictionary<K2, TValue>(k2Comparer));

        return subDict.GetOrAdd(key2, _ => nofunc());
    }

    public ConcurrentDictionary<K2, TValue> Get(K1 key1)
    {
        var subDict = innerDictionary.GetOrAdd(key1, _ => new ConcurrentDictionary<K2, TValue>(k2Comparer));
        return subDict;
    }
}