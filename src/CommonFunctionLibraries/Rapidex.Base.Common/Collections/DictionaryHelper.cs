using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Text;

namespace Rapidex;

public static class DictionaryHelper
{
    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        dict.NotNull();

        lock (dict)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
    }


    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> with)
    {
        if (with == null)
            return;

        dict.NotNull();

        foreach (TKey withKey in with.Keys)
            dict.Set(withKey, with[withKey]);
    }

    /// <summary>
    /// Set dictionary value with function
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dict"></param>
    /// <param name="key"></param>
    /// <param name="action">newValue = Func(currentValueOfKey) </param>
    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue, TValue> action)
    {
        dict.NotNull();

        lock (dict)
        {
            TValue currentValue = default(TValue);
            if (dict.ContainsKey(key))
                currentValue = dict[key];

            TValue newValue = action(currentValue);
            dict.Set(key, newValue);
        }
    }





    /// <summary>
    /// SortedList' te aynı anahtarda bir diğeri mevcut ise anahtarı bir ilerletir ve kaydetmeye zorlar
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="list"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void Set<TValue>(this SortedList<int, TValue> list, int key, TValue value)
    {
        list.NotNull();

        int _key = key;
        while (list.ContainsKey(_key))
        {
            _key++;
        }

        list.Add(_key, value);
    }

    public static bool TrySetForUniqueValue<TValue>(this SortedList<int, TValue> list, int key, TValue value)
    {
        list.NotNull();

        if (list.Values.Contains(value))
            return false;

        int _key = key;
        while (list.ContainsKey(_key))
        {
            _key++;
        }

        list.Add(_key, value);
        return true;
    }

    public static void AddKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys)
    {
        dict.NotNull();

        lock (dict)
        {
            foreach (TKey key in keys)
                if (!dict.ContainsKey(key))
                    dict.Add(key, default(TValue));
        }
    }

    public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, bool _throwIfNotExist = false)
    {
        dict.NotNull();
        key.NotNull();

        //lock (dict)
        //{
        if (dict.TryGetValue(key, out TValue value))
            return value;

        if (_throwIfNotExist)
            throw new InvalidOperationException($"value not found for: {key}");

        return default(TValue);
        //}
    }




    public static TValue GetOr<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> notAvailFunc )
    {
        dict.NotNull();

        if (dict.TryGetValue(key, out TValue value))
            return value;

        TValue val = notAvailFunc();
        dict.Set(key, val);
        return val;
    }

    public static ReqType Value<ReqType>(this IDictionary dict, object key, bool _throwIfNotExist = false)
    {
        dict.NotNull();
        key.NotNull();

        lock (dict)
        {
            if (!dict.Contains(key))
            {
                if (_throwIfNotExist)
                    throw new InvalidOperationException($"value not found for: {key}");

                return default(ReqType);
            }

            object valueOriginalType = dict[key];
            return valueOriginalType.As<ReqType>();
        }
    }


    public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> from)
    {
        foreach (TKey key in from.Keys)
            target.Set(key, from.Get(key));
    }




    public static Dictionary<TKey, TValue> ExportTo<TKey, TValue>(this IDictionary<TKey, TValue> from, params TKey[] keys)
    {
        Dictionary<TKey, TValue> to = new Dictionary<TKey, TValue>();
        foreach (TKey key in keys)
        {
            TValue val = from.Get(key);
            if (!EqualityComparer<TValue>.Default.Equals(val, default(TValue)))
                to.Set(key, val);
        }

        return to;
    }
}
