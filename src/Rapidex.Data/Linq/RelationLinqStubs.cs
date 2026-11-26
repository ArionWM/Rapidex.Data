using System;

namespace Rapidex.Data.Linq;

/// <summary>
/// Stub methods so relation.Any(...) can appear inside Expression trees.
/// Calling these outside Rapidex LINQ translation throws.
/// </summary>
public static class RelationLinqStubs
{
    const string Message = "Relation Any() is only supported inside Rapidex LINQ Where expressions.";

    /// <summary>
    /// Translation stub for One2N Any().
    /// </summary>
    /// <param name="relation">One2N relation field</param>
    public static bool Any<T>(this RelationOne2N<T> relation) where T : IConcreteEntity
        => throw new InvalidOperationException(Message);

    /// <summary>
    /// Translation stub for One2N Any(predicate).
    /// </summary>
    /// <param name="relation">One2N relation field</param>
    /// <param name="predicate">Detail predicate</param>
    public static bool Any<T>(this RelationOne2N<T> relation, Func<T, bool> predicate) where T : IConcreteEntity
        => throw new InvalidOperationException(Message);

    /// <summary>
    /// Translation stub for N2N Any().
    /// </summary>
    /// <param name="relation">N2N relation field</param>
    public static bool Any<T>(this RelationN2N<T> relation) where T : IConcreteEntity
        => throw new InvalidOperationException(Message);

    /// <summary>
    /// Translation stub for N2N Any(predicate).
    /// </summary>
    /// <param name="relation">N2N relation field</param>
    /// <param name="predicate">Target predicate</param>
    public static bool Any<T>(this RelationN2N<T> relation, Func<T, bool> predicate) where T : IConcreteEntity
        => throw new InvalidOperationException(Message);
}
