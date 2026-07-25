using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Rapidex;

public class TwoLevelDictionary<K1, K2, TValue> where K1 : notnull where K2 : notnull
{
    private readonly ConcurrentDictionary<K1, ConcurrentDictionary<K2, TValue>> innerDictionary;
    private readonly IEqualityComparer<K2> k2Comparer;

    public TwoLevelDictionary()
    {
        var k1Comparer = typeof(K1).IsSupportTo<string>() ? (IEqualityComparer<K1>)StringComparer.InvariantCultureIgnoreCase : null;
        k2Comparer = typeof(K2).IsSupportTo<string>() ? (IEqualityComparer<K2>)StringComparer.InvariantCultureIgnoreCase : null;
        innerDictionary = new ConcurrentDictionary<K1, ConcurrentDictionary<K2, TValue>>(k1Comparer);
    }

    public TwoLevelDictionary(IEqualityComparer<K1> equalityComparer)
    {
        k2Comparer = typeof(K2).IsSupportTo<string>() ? (IEqualityComparer<K2>)StringComparer.InvariantCultureIgnoreCase : null;
        innerDictionary = new ConcurrentDictionary<K1, ConcurrentDictionary<K2, TValue>>(equalityComparer);
    }

    /// <summary>
    /// key1 için alt sözlüğü döndürür. Mevcutsa hiçbir tahsis yapmaz;
    /// yoksa static factory ile oluşturur (closure tahsisi olmaz).
    /// </summary>
    /// <param name="key1">Birinci seviye anahtar</param>
    protected ConcurrentDictionary<K2, TValue> GetSubDictionary(K1 key1)
    {
        if (innerDictionary.TryGetValue(key1, out var subDict))
            return subDict;

        return innerDictionary.GetOrAdd(key1, static (_, comparer) => new ConcurrentDictionary<K2, TValue>(comparer), k2Comparer);
    }

    protected void SetInternal(K1 key1, K2 key2, TValue value)
    {
        var subDict = this.GetSubDictionary(key1);
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

    /// <summary>
    /// Kayıt yoksa false döndürür. Get(K1,K2) ile farkı; null olarak saklanmış
    /// bir değeri "kayıt yok" durumundan ayırt edebilmesidir.
    /// </summary>
    /// <param name="key1">Birinci seviye anahtar</param>
    /// <param name="key2">İkinci seviye anahtar</param>
    /// <param name="value">Bulunan değer</param>
    public bool TryGetValue(K1 key1, K2 key2, out TValue value)
    {
        if (innerDictionary.TryGetValue(key1, out var dict))
        {
            return dict.TryGetValue(key2, out value);
        }

        value = default;
        return false;
    }

    public TValue GetOr(K1 key1, K2 key2, Func<K1, K2, TValue> nofunc)
    {
        var subDict = this.GetSubDictionary(key1);

        return subDict.GetOrAdd(key2, _ => nofunc(key1, key2));
    }

    public TValue GetOr(K1 key1, K2 key2, Func<TValue> nofunc)
    {
        var subDict = this.GetSubDictionary(key1);

        return subDict.GetOrAdd(key2, _ => nofunc());
    }

    public ConcurrentDictionary<K2, TValue> Get(K1 key1)
    {
        return this.GetSubDictionary(key1);
    }

    public void Clear()
    {
        innerDictionary.Clear();
    }

    public bool ContainsKey(K1 key1)
    {
        return innerDictionary.ContainsKey(key1);
    }

    public bool ContainsKey(K1 key1, K2 key2)
    {
        if (innerDictionary.TryGetValue(key1, out var dict))
        {
            return dict.ContainsKey(key2);
        }
        return false;
    }
}