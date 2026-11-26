using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rapidex.Data.Linq;

/// <summary>
/// Translates a boolean LINQ expression into IQueryCriteria calls.
/// </summary>
internal sealed class ExpressionToCriteriaVisitor
{
    readonly IQueryCriteria _criteria;
    readonly MemberPathResolver _pathResolver;

    public ExpressionToCriteriaVisitor(IQueryCriteria criteria)
    {
        _criteria = criteria.NotNull();
        _pathResolver = new MemberPathResolver(criteria.EntityMetadata);
    }

    /// <summary>
    /// Applies a predicate expression onto the criteria instance.
    /// </summary>
    /// <param name="predicate">Boolean lambda</param>
    public void Apply(LambdaExpression predicate)
    {
        predicate.NotNull();
        VisitBoolean(predicate.Body);
    }

    void VisitBoolean(Expression expression)
    {
        expression = MemberPathResolver.StripConverts(expression);

        switch (expression.NodeType)
        {
            case ExpressionType.AndAlso:
                VisitAndAlso((BinaryExpression)expression);
                return;
            case ExpressionType.OrElse:
                VisitOrElse((BinaryExpression)expression);
                return;
            case ExpressionType.Not:
                VisitNot((UnaryExpression)expression);
                return;
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
                VisitComparison((BinaryExpression)expression);
                return;
            case ExpressionType.Call:
                VisitMethodCallAsBoolean((MethodCallExpression)expression);
                return;
            case ExpressionType.Constant:
                if (expression is ConstantExpression { Value: true })
                    return;
                if (expression is ConstantExpression { Value: false })
                {
                    _criteria.Eq(DatabaseConstants.FIELD_ID, -1);
                    return;
                }
                break;
        }

        throw new NotSupportedException($"Boolean expression '{expression.NodeType}' is not supported by Rapidex LINQ.");
    }

    void VisitAndAlso(BinaryExpression binary)
    {
        VisitBoolean(binary.Left);
        VisitBoolean(binary.Right);
    }

    void VisitOrElse(BinaryExpression binary)
    {
        _criteria.Or(
            q => new ExpressionToCriteriaVisitor(q).VisitBoolean(binary.Left),
            q => new ExpressionToCriteriaVisitor(q).VisitBoolean(binary.Right));
    }

    void VisitNot(UnaryExpression unary)
    {
        _criteria.Not(q => new ExpressionToCriteriaVisitor(q).VisitBoolean(unary.Operand));
    }

    void VisitComparison(BinaryExpression binary)
    {
        Expression left = MemberPathResolver.StripConverts(binary.Left);
        Expression right = MemberPathResolver.StripConverts(binary.Right);

        if (TryGetMemberPath(left, out ResolvedMemberPath leftPath) && !IsProbablyEntityMember(right))
        {
            ApplyComparison(leftPath, binary.NodeType, ValueEvaluator.Evaluate(right));
            return;
        }

        if (TryGetMemberPath(right, out ResolvedMemberPath rightPath) && !IsProbablyEntityMember(left))
        {
            ApplyComparison(rightPath, MirrorComparison(binary.NodeType), ValueEvaluator.Evaluate(left));
            return;
        }

        throw new NotSupportedException($"Comparison between '{left.NodeType}' and '{right.NodeType}' is not supported.");
    }

    void ApplyComparison(ResolvedMemberPath path, ExpressionType op, object value)
    {
        if (path.Segments.Count == 1 && path.Last.Kind == MemberPathKind.ScalarOrForeignKey)
        {
            ApplyScalarComparison(_criteria, path.SimpleFieldName, op, value);
            return;
        }

        if (path.Last.Kind != MemberPathKind.ScalarOrForeignKey)
            throw new NotSupportedException($"Cannot compare relation field '{path.Last.Field.Name}' directly. Use Any(...).");

        ApplyNestedReferenceComparison(path, op, value);
    }

    void ApplyNestedReferenceComparison(ResolvedMemberPath path, ExpressionType op, object value)
    {
        var navSegments = path.Segments.Take(path.Segments.Count - 1).ToList();
        MemberPathSegment leaf = path.Last;

        Action<IQueryCriteria> leafAction = q => ApplyScalarComparison(q, leaf.Field.Name, op, value);

        for (int i = navSegments.Count - 1; i >= 0; i--)
        {
            MemberPathSegment nav = navSegments[i];
            if (nav.Kind != MemberPathKind.ReferenceNavigation)
                throw new NotSupportedException($"Unexpected segment kind '{nav.Kind}' in navigation path.");

            Action<IQueryCriteria> inner = leafAction;
            string fieldName = nav.Field.Name;
            leafAction = q => q.ExistsReference(fieldName, inner);
        }

        leafAction(_criteria);
    }

    static void ApplyScalarComparison(IQueryCriteria criteria, string field, ExpressionType op, object value)
    {
        switch (op)
        {
            case ExpressionType.Equal:
                criteria.Eq(field, value);
                break;
            case ExpressionType.NotEqual:
                criteria.NotEq(field, value);
                break;
            case ExpressionType.GreaterThan:
                criteria.Gt(field, value);
                break;
            case ExpressionType.GreaterThanOrEqual:
                criteria.GtEq(field, value);
                break;
            case ExpressionType.LessThan:
                criteria.Lt(field, value);
                break;
            case ExpressionType.LessThanOrEqual:
                criteria.LtEq(field, value);
                break;
            default:
                throw new NotSupportedException($"Operator '{op}' is not supported.");
        }
    }

    void VisitMethodCallAsBoolean(MethodCallExpression call)
    {
        if (MemberPathResolver.IsGetContent(call))
            throw new NotSupportedException("GetContent() cannot be translated to SQL.");

        if (TryResolveStringLike(call, out string likePattern, out ResolvedMemberPath stringPath))
        {
            if (stringPath.Segments.Count != 1 || stringPath.Last.Kind != MemberPathKind.ScalarOrForeignKey)
                throw new NotSupportedException("String methods are only supported on simple scalar fields.");
            _criteria.Like(stringPath.SimpleFieldName, likePattern);
            return;
        }

        if (IsEnumerableContains(call, out ResolvedMemberPath inPath, out object[] values))
        {
            if (!inPath.IsSimpleScalar)
                throw new NotSupportedException("In/Contains is only supported on simple scalar fields.");
            _criteria.In(inPath.SimpleFieldName, values);
            return;
        }

        if (IsRelationAny(call, out ResolvedMemberPath relationPath, out LambdaExpression anyPredicate))
        {
            ApplyRelationAny(relationPath, anyPredicate);
            return;
        }

        throw new NotSupportedException($"Method '{call.Method.DeclaringType?.Name}.{call.Method.Name}' is not supported by Rapidex LINQ.");
    }

    void ApplyRelationAny(ResolvedMemberPath relationPath, LambdaExpression anyPredicate)
    {
        if (relationPath.Segments.Count != 1)
            throw new NotSupportedException("Nested relation Any() is not supported.");

        MemberPathSegment seg = relationPath.Last;
        Action<IQueryCriteria> nested = null;
        if (anyPredicate != null)
            nested = q => new ExpressionToCriteriaVisitor(q).Apply(anyPredicate);

        switch (seg.Kind)
        {
            case MemberPathKind.RelationOne2N:
                _criteria.ExistsOne2N(seg.Field.Name, nested);
                break;
            case MemberPathKind.RelationN2N:
                _criteria.ExistsN2N(seg.Field.Name, nested);
                break;
            default:
                throw new NotSupportedException($"Any() is not supported on field kind '{seg.Kind}'.");
        }
    }

    bool TryGetMemberPath(Expression expression, out ResolvedMemberPath path)
    {
        path = null;
        expression = MemberPathResolver.StripConverts(expression);
        if (expression is not MemberExpression)
            return false;

        // Surface GetContent() as a hard failure (do not swallow).
        Expression probe = expression;
        while (probe is MemberExpression member)
            probe = MemberPathResolver.StripConverts(member.Expression);
        if (probe is MethodCallExpression methodCall && MemberPathResolver.IsGetContent(methodCall))
            throw new NotSupportedException("GetContent() cannot be translated to SQL. Use navigation property casts or Exists helpers.");

        try
        {
            path = _pathResolver.Resolve(expression);
            return true;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    static bool IsProbablyEntityMember(Expression expression)
    {
        Expression current = MemberPathResolver.StripConverts(expression);
        while (current is MemberExpression member)
            current = MemberPathResolver.StripConverts(member.Expression);
        return current is ParameterExpression;
    }

    static ExpressionType MirrorComparison(ExpressionType op) => op switch
    {
        ExpressionType.GreaterThan => ExpressionType.LessThan,
        ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
        ExpressionType.LessThan => ExpressionType.GreaterThan,
        ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
        _ => op
    };

    bool TryResolveStringLike(MethodCallExpression call, out string pattern, out ResolvedMemberPath path)
    {
        pattern = null;
        path = null;

        if (call.Object == null)
            return false;

        string methodName = call.Method.Name;
        if (methodName is not (nameof(string.Contains) or nameof(string.StartsWith) or nameof(string.EndsWith)))
            return false;

        if (!TryGetMemberPath(call.Object, out path))
            return false;

        object arg = ValueEvaluator.Evaluate(call.Arguments[0]);
        string text = arg?.ToString() ?? string.Empty;

        pattern = methodName switch
        {
            nameof(string.Contains) => $"*{text}*",
            nameof(string.StartsWith) => $"{text}*",
            nameof(string.EndsWith) => $"*{text}",
            _ => text
        };
        return true;
    }

    bool IsEnumerableContains(MethodCallExpression call, out ResolvedMemberPath path, out object[] values)
    {
        path = null;
        values = null;

        if (call.Method.Name != nameof(Enumerable.Contains) && call.Method.Name != "Contains")
            return false;

        // Enumerable.Contains(source, value) OR MemoryExtensions.Contains(span, value)
        if (call.Method.IsStatic && call.Arguments.Count >= 2)
        {
            Expression valueArg = call.Arguments[1];
            Expression sourceArg = call.Arguments[0];
            // MemoryExtensions.Contains(span, value[, comparer]) — value may be arg[1]
            if (!TryGetMemberPath(valueArg, out path))
                return false;
            values = ValueEvaluator.EvaluateToArray(UnwrapSpanSource(sourceArg));
            return true;
        }

        // list.Contains(value) where value is entity field
        if (!call.Method.IsStatic && call.Object != null && call.Arguments.Count == 1)
        {
            if (!TryGetMemberPath(call.Arguments[0], out path))
                return false;
            values = ValueEvaluator.EvaluateToArray(call.Object);
            return true;
        }

        return false;
    }

    static Expression UnwrapSpanSource(Expression source)
    {
        source = MemberPathResolver.StripConverts(source);
        if (source is MethodCallExpression call
            && call.Method.DeclaringType == typeof(MemoryExtensions)
            && call.Method.Name is nameof(MemoryExtensions.AsSpan) or "AsSpan")
        {
            return call.Arguments[0];
        }
        return source;
    }

    bool IsRelationAny(MethodCallExpression call, out ResolvedMemberPath path, out LambdaExpression predicate)
    {
        path = null;
        predicate = null;

        if (call.Method.Name != "Any")
            return false;

        bool fromStubs = call.Method.DeclaringType == typeof(RelationLinqStubs);
        bool fromEnumerable = call.Method.DeclaringType == typeof(Enumerable);
        if (!fromStubs && !fromEnumerable)
            return false;

        Expression source;
        int predicateArgIndex;

        if (call.Method.IsStatic)
        {
            if (call.Arguments.Count < 1)
                return false;
            source = call.Arguments[0];
            predicateArgIndex = call.Arguments.Count >= 2 ? 1 : -1;
        }
        else
        {
            source = call.Object;
            predicateArgIndex = call.Arguments.Count >= 1 ? 0 : -1;
        }

        if (source == null || !TryGetMemberPath(source, out path))
            return false;

        if (path.Last.Kind is not (MemberPathKind.RelationOne2N or MemberPathKind.RelationN2N))
            return false;

        if (predicateArgIndex >= 0)
        {
            Expression predExpr = MemberPathResolver.StripConverts(call.Arguments[predicateArgIndex]);
            // Func<T,bool> inside Expression becomes a quoted or nested lambda / constant delegate — unwrap Quote
            if (predExpr is UnaryExpression { NodeType: ExpressionType.Quote } quote)
                predExpr = quote.Operand;
            if (predExpr is LambdaExpression lambda)
                predicate = lambda;
            else
                throw new NotSupportedException("Any() predicate must be a lambda expression.");
        }

        return true;
    }
}
