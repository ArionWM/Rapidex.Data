using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;

public static class Extensions
{
    public static bool IsPagingSet(this IPaging paging)
    {
        return paging != null && paging.PageSize > 0;
    }
}

public static class ConsoleX
{
    public static void WriteLine(string message)
    {
        //Console.WriteLine(message); 
        Task.Run(() => Console.WriteLine(message));
    }

}

public static class UpdateResultX
{
    public static IUpdateResult<T> Added<T>(this IUpdateResult<T> ur, params T[] items)
    {
        foreach (T item in items)
        {
            ur.Added(item);
        }
        return ur;
    }

    public static IUpdateResult<T> Modified<T>(this IUpdateResult<T> ur, params T[] items)
    {
        foreach (T item in items)
        {
            ur.Modified(item);
        }
        return ur;
    }

    public static IUpdateResult<T> Deleted<T>(this IUpdateResult<T> ur, params T[] items)
    {
        foreach (T item in items)
        {
            ur.Deleted(item);
        }
        return ur;
    }

    public static T[] GetModifieds<T>(this IUpdateResult<T> ur)
    {
        return ur.AddedItems.Union(ur.ModifiedItems).ToArray();
    }
}

public static class ObjectX
{
    public static T Clone<T>(this T obj) where T: ICloneable
    {
        return (T)obj.Clone();
    }

    public static T TClone<T>(this T obj) where T : ICloneable
    {
        return (T)obj.Clone();
    }
}
