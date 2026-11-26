using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rapidex.Data.Linq;

/// <summary>
/// LINQ-style extensions that translate expressions onto IQuery&lt;T&gt;.
/// Each operator clones the query for safe composition.
/// </summary>
public static class RapidexLinqExtensions
{
    /// <summary>
    /// Applies a boolean predicate by translating it to Rapidex criteria.
    /// </summary>
    /// <param name="query">Typed query</param>
    /// <param name="predicate">Filter expression</param>
    public static IQuery<T> Where<T>(this IQuery<T> query, Expression<Func<T, bool>> predicate) where T : IConcreteEntity
    {
        query.NotNull();
        predicate.NotNull();
        IQuery<T> cloned = CloneQuery(query);
        new ExpressionToCriteriaVisitor(cloned).Apply(predicate);
        return cloned;
    }

    /// <summary>
    /// Orders ascending by a scalar field expression.
    /// </summary>
    /// <param name="query">Typed query</param>
    /// <param name="keySelector">Field selector</param>
    public static IQuery<T> OrderBy<T, TKey>(this IQuery<T> query, Expression<Func<T, TKey>> keySelector) where T : IConcreteEntity
    {
        return OrderByInternal(query, keySelector, OrderDirection.Asc);
    }

    /// <summary>
    /// Orders descending by a scalar field expression.
    /// </summary>
    /// <param name="query">Typed query</param>
    /// <param name="keySelector">Field selector</param>
    public static IQuery<T> OrderByDescending<T, TKey>(this IQuery<T> query, Expression<Func<T, TKey>> keySelector) where T : IConcreteEntity
    {
        return OrderByInternal(query, keySelector, OrderDirection.Desc);
    }

    /// <summary>
    /// Appends an ascending then-by field.
    /// </summary>
    /// <param name="query">Typed query</param>
    /// <param name="keySelector">Field selector</param>
    public static IQuery<T> ThenBy<T, TKey>(this IQuery<T> query, Expression<Func<T, TKey>> keySelector) where T : IConcreteEntity
    {
        return OrderByInternal(query, keySelector, OrderDirection.Asc, clone: true);
    }

    /// <summary>
    /// Appends a descending then-by field.
    /// </summary>
    /// <param name="query">Typed query</param>
    /// <param name="keySelector">Field selector</param>
    public static IQuery<T> ThenByDescending<T, TKey>(this IQuery<T> query, Expression<Func<T, TKey>> keySelector) where T : IConcreteEntity
    {
        return OrderByInternal(query, keySelector, OrderDirection.Desc, clone: true);
    }

    /// <summary>
    /// Skips the given number of rows.
    /// </summary>
    /// <param name="query">Typed query</param>
    /// <param name="count">Rows to skip</param>
    public static IQuery<T> Skip<T>(this IQuery<T> query, int count) where T : IConcreteEntity
    {
        query.NotNull();
        IQuery<T> cloned = CloneQuery(query);
        if (cloned.Paging == null)
            cloned.Paging = new PagingInfo();
        cloned.Paging.StartIndex = count;
        cloned.Query.Skip(count);
        return cloned;
    }

    /// <summary>
    /// Takes at most the given number of rows.
    /// </summary>
    /// <param name="query">Typed query</param>
    /// <param name="count">Max rows</param>
    public static IQuery<T> Take<T>(this IQuery<T> query, int count) where T : IConcreteEntity
    {
        query.NotNull();
        IQuery<T> cloned = CloneQuery(query);
        long start = cloned.Paging?.StartIndex ?? 0;
        if (start > 0)
            cloned.Page(count, start, includeTotalCount: false);
        else
            cloned.Limit(count);
        return cloned;
    }

    /// <summary>
    /// Loads all matching entities.
    /// </summary>
    /// <param name="query">Typed query</param>
    public static async Task<List<T>> ToListAsync<T>(this IQuery<T> query) where T : IConcreteEntity
    {
        query.NotNull();
        IEntityLoadResult<T> result = await query.Load();
        return result.ToList();
    }

    /// <summary>
    /// Loads the first matching entity or default.
    /// </summary>
    /// <param name="query">Typed query</param>
    public static async Task<T> FirstOrDefaultAsync<T>(this IQuery<T> query) where T : IConcreteEntity
    {
        query.NotNull();
        return await query.First();
    }

    /// <summary>
    /// Returns the number of matching rows.
    /// </summary>
    /// <param name="query">Typed query</param>
    public static Task<long> CountAsync<T>(this IQuery<T> query) where T : IConcreteEntity
    {
        query.NotNull();
        return query.Count();
    }

    /// <summary>
    /// Returns whether any matching row exists.
    /// </summary>
    /// <param name="query">Typed query</param>
    public static Task<bool> AnyAsync<T>(this IQuery<T> query) where T : IConcreteEntity
    {
        query.NotNull();
        return query.Exist();
    }

    static IQuery<T> OrderByInternal<T, TKey>(IQuery<T> query, Expression<Func<T, TKey>> keySelector, OrderDirection direction, bool clone = true)
        where T : IConcreteEntity
    {
        query.NotNull();
        keySelector.NotNull();
        IQuery<T> target = clone ? CloneQuery(query) : query;
        string field = ResolveSimpleField(target.EntityMetadata, keySelector);
        return target.OrderBy(direction, field);
    }

    static string ResolveSimpleField<T, TKey>(IDbEntityMetadata em, Expression<Func<T, TKey>> keySelector)
        where T : IConcreteEntity
    {
        var resolver = new MemberPathResolver(em);
        ResolvedMemberPath path = resolver.Resolve(keySelector.Body);
        if (!path.IsSimpleScalar)
            throw new NotSupportedException("OrderBy currently supports only simple scalar fields.");
        return path.SimpleFieldName;
    }

    static IQuery<T> CloneQuery<T>(IQuery<T> query) where T : IConcreteEntity
    {
        return (IQuery<T>)query.Clone();
    }
}
