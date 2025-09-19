using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapidex;

public static class CollectionExtensions //From ProCore
{
    public static void Add<T>(this HashSet<T> hashset, IEnumerable<T> items)
    {
        if (items == null)
            return;

        foreach (T item in items)
        {
            if (!hashset.Contains(item))
            {
                hashset.Add(item);
            }
        }
    }


    /// <summary>
    /// İlgili nesne ve diğerlerinden bir array üretir
    /// 
    /// Örn: int i = 1;
    /// i.CreateArray(); -> {1}
    /// i.CreateArray(2, 3); -> {1, 2, 3}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="first">dizinin ilk elemanı</param>
    /// <param name="otherItems">diğer elemanlar</param>
    /// <returns></returns>
    public static T[] CreateArray<T>(this T first, params T[] otherItems)
    {
        HashSet<T> hashset = new HashSet<T>();
        if (first != null)
            hashset.Add(first);
        hashset.Add(otherItems);
        return hashset.ToArray();
    }

    public static Array CreateArray(this object first, Type elementType, params object[] otherItems)
    {
        IList list = TypeHelper.CreateInstance<IList>(typeof(List<>).MakeGenericType(elementType));

        if (first != null)
            list.Add(first);

        foreach (object oi in otherItems)
            list.Add(Convert.ChangeType(oi, elementType));

        Type arrayType = elementType.MakeArrayType();
        Array array = TypeHelper.CreateInstance<Array>(arrayType, list.Count);
        list.CopyTo(array, 0);
        return array;
    }

    public static List<T> CreateList<T>(this T first, params T[] otherItems)
    {
        HashSet<T> hashset = new HashSet<T>();
        if (first != null)
            hashset.Add(first);
        hashset.Add(otherItems);
        return hashset.ToList();
    }

    public static void MoveItemTo<T>(this List<T> list, T item, int toIndex)
    {
        if (list.Count <= toIndex)
            throw new InvalidOperationException($"List count: {list.Count}, required index invalid: {toIndex} ");

        int index = list.IndexOf(item);
        if (index == -1)
            throw new InvalidOperationException($"item not found in list");

        list.RemoveAt(index);
        list.Insert(toIndex, item);
    }

    public static string Join(this IEnumerable<string> strarr, string seperator)
    {
        return string.Join(seperator, strarr.ToArray());
    }

    public static string Join(this string[] strarr, string seperator)
    {
        return string.Join(seperator, strarr);
    }

    public static string[] JoinWithMaxLenght(this IEnumerable<string> strarr, string seperator, int maxJoinedStringLenght)
    {
        List<string> parts = new List<string>();
        int lastPartLenght = 0;
        List<string> lastPartContent = new List<string>();
        foreach (string str in strarr)
        {
            int expectedLenght = lastPartLenght + str.Length + seperator.Length * (lastPartContent.Count + 1);
            if (expectedLenght > maxJoinedStringLenght)
            {
                string lastPartStr = lastPartContent.Join(seperator);
                parts.Add(lastPartStr);
                lastPartContent.Clear();
                lastPartLenght = 0;
            }

            lastPartLenght += str.Length;
            lastPartContent.Add(str);
        }

        if (lastPartContent.Count > 0)
        {
            string lastPartStr = lastPartContent.Join(seperator);
            parts.Add(lastPartStr);
        }

        return parts.ToArray();
    }


    //https://stackoverflow.com/questions/18986129/how-can-i-split-an-array-into-n-parts
    /// <summary>
    /// Splits an array into several smaller arrays.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The array to split.</param>
    /// <param name="size">The size of the smaller arrays.</param>
    /// <returns>An array containing smaller arrays.</returns>
    public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> items, int size)
    {
        T[] array = items.ToArray();
        for (var i = 0; i < (float)array.Length / size; i++)
        {
            yield return array.Skip(i * size).Take(size);
        }
    }

    /// <summary>
    /// Diziden verilen sayıda elemanı (baştan başlayarak) döndürür
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="elements"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static IEnumerable<T> First<T>(this IEnumerable<T> elements, int count)
    {
        T[] array = elements.ToArray(); ;
        if (array.Length < count)
            count = array.Length;

        T[] dest = new T[count];
        Array.Copy(array, dest, count);
        return dest;
    }


    public static string[] TrimElements(this IEnumerable<string> strenum)
    {
        if (strenum.IsNullOrEmpty())
            return new string[0];

        List<string> _elements = new List<string>();
        foreach (string item in strenum)
        {
            if (item.IsNullOrEmpty())
                continue;

            string itm = item.Trim().Trim(' ');
            if (itm.IsNullOrEmpty())
                continue;

            _elements.Add(itm);
        }
        return _elements.ToArray();
    }

    public static string[] DistinctWithTrimElements(this IEnumerable<string> strenum)
    {
        if (strenum.IsNullOrEmpty())
            return new string[0];

        HashSet<string> _elements = new HashSet<string>();
        foreach (string item in strenum)
        {
            if (item.IsNullOrEmpty())
                continue;

            string itm = item.Trim().Trim(' ');
            if (itm.IsNullOrEmpty())
                continue;

            _elements.Add(itm);
        }
        return _elements.ToArray();
    }


    public static T[] TrimElements<T>(this IEnumerable<T> enumerate)
    {
        if (enumerate == null || !enumerate.Any())
            return new T[0];

        List<T> _elements = new List<T>();
        foreach (T item in enumerate)
        {
            if (item == null)
                continue;

            _elements.Add(item);
        }
        return _elements.ToArray();
    }

    public static T[] DistinctWithTrimElements<T>(this IEnumerable<T> enumerate)
    {
        if (enumerate.IsNullOrEmpty())
            return new T[0];

        HashSet<T> _elements = new HashSet<T>();
        foreach (T item in enumerate)
        {
            if (item.IsNullOrEmpty())
                continue;

            _elements.Add(item);
        }
        return _elements.ToArray();
    }


    public static IDictionary<string, object> ToStringKeys(this IDictionary<object, object> dict, bool searchDeep)
    {
        if (dict == null || dict.Count == 0)
            return new Dictionary<string, object>();

        Dictionary<string, object> strDict = new Dictionary<string, object>();
        foreach (KeyValuePair<object, object> kvp in dict)
        {
            object value = kvp.Value;
            if (searchDeep)
            {
                if (value is IDictionary<object, object> objDict)
                {
                    value = objDict.ToStringKeys(searchDeep);
                }

                if (value is IList list)
                {
                    List<object> newList = new List<object>();
                    foreach (object item in list)
                    {
                        if (item is IDictionary<object, object> objDictItem)
                        {
                            newList.Add(objDictItem.ToStringKeys(searchDeep));
                        }
                        else
                        {
                            newList.Add(item);
                        }
                    }
                    value = newList;
                }
            }

            strDict.Add(kvp.Key.ToString(), value);
        }
        return strDict;
    }

    public static IDictionary<string, object> ToObjectValues(this IDictionary<string, string> dict, bool searchDeep)
    {
        if (dict == null || dict.Count == 0)
            return new Dictionary<string, object>();

        Dictionary<string, object> strDict = new Dictionary<string, object>();
        foreach (KeyValuePair<string, string> kvp in dict)
        {
            object value = kvp.Value;
            if (searchDeep)
            {
                if (value is IDictionary<string, string> objDict)
                {
                    value = objDict.ToObjectValues(searchDeep);
                }

                if (value is IList list)
                {
                    List<object> newList = new List<object>();
                    foreach (object item in list)
                    {
                        if (item is IDictionary<string, string> objDictItem)
                        {
                            newList.Add(objDictItem.ToObjectValues(searchDeep));
                        }
                        else
                        {
                            newList.Add(item);
                        }
                    }
                    value = newList;
                }
            }

            strDict.Add(kvp.Key.ToString(), value);
        }
        return strDict;
    }

    public static void OrderByDeep<TSource, TKey>(ref IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var res = source.OrderBy(keySelector);
        var array = res.ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            TSource item = array[i];
            if (item is IEnumerable<TSource> subItems)
            {
                OrderByDeep(ref subItems, keySelector);
                array[i] = item;
            }
        }

        source = array;
    }

    public static IEnumerable<TSource> OrderByDeep<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        OrderByDeep(ref source, keySelector);
        return source;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source == null || action == null)
            return;

        foreach (T item in source)
        {
            action(item);
        }
    }
}
