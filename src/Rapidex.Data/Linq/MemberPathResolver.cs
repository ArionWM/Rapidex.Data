using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using static Rapidex.Data.Reference;
using static Rapidex.Data.RelationN2N;
using static Rapidex.Data.RelationOne2N;

namespace Rapidex.Data.Linq;

/// <summary>
/// Resolves a member-access chain against entity metadata for LINQ translation.
/// </summary>
internal sealed class MemberPathResolver
{
    public MemberPathResolver(IDbEntityMetadata rootMetadata)
    {
        RootMetadata = rootMetadata.NotNull();
    }

    public IDbEntityMetadata RootMetadata { get; }

    /// <summary>
    /// Parses a member expression into field segments starting from the query parameter.
    /// </summary>
    /// <param name="expression">Member access or parameter expression</param>
    public ResolvedMemberPath Resolve(Expression expression)
    {
        expression = StripConverts(expression);
        if (expression is ParameterExpression)
            throw new NotSupportedException("Entity parameter alone cannot be used as a field path.");

        var members = new List<MemberInfo>();
        Expression current = expression;
        while (current is MemberExpression member)
        {
            members.Add(member.Member);
            current = StripConverts(member.Expression);
        }

        if (current is MethodCallExpression methodCall && IsGetContent(methodCall))
            throw new NotSupportedException("GetContent() cannot be translated to SQL. Use navigation property paths or Exists helpers.");

        if (current is not ParameterExpression)
            throw new NotSupportedException($"Unsupported member path root: {current?.NodeType}");

        members.Reverse();
        return BuildPath(members);
    }

    ResolvedMemberPath BuildPath(List<MemberInfo> members)
    {
        var segments = new List<MemberPathSegment>();
        IDbEntityMetadata currentEm = RootMetadata;
        int index = 0;

        while (index < members.Count)
        {
            MemberInfo member = members[index];
            string name = member.Name;
            IDbFieldMetadata fm = currentEm.Fields.Get(name, false);
            if (fm == null)
                throw new NotSupportedException($"Field '{name}' not found on entity '{currentEm.Name}'.");

            if (fm is ReferenceDbFieldMetadata refFm)
            {
                bool isTerminalFkAccess = index == members.Count - 1
                    || IsReferenceRuntimeMember(members[index + 1]);

                if (isTerminalFkAccess)
                {
                    if (index < members.Count - 1)
                        index++; // consume TargetId / Value / EnumValue
                    segments.Add(new MemberPathSegment(fm, MemberPathKind.ScalarOrForeignKey, currentEm));
                    index++;
                    break;
                }

                IDbEntityMetadata refEm = refFm.ReferencedEntityMetadata
                    .NotNull($"Referenced metadata missing for '{name}'");
                segments.Add(new MemberPathSegment(fm, MemberPathKind.ReferenceNavigation, currentEm, refEm));
                currentEm = refEm;
                index++;
                continue;
            }

            if (fm is VirtualRelationOne2NDbFieldMetadata)
            {
                segments.Add(new MemberPathSegment(fm, MemberPathKind.RelationOne2N, currentEm));
                index++;
                break;
            }

            if (fm is VirtualRelationN2NDbFieldMetadata)
            {
                segments.Add(new MemberPathSegment(fm, MemberPathKind.RelationN2N, currentEm));
                index++;
                break;
            }

            segments.Add(new MemberPathSegment(fm, MemberPathKind.ScalarOrForeignKey, currentEm));
            index++;
            break;
        }

        if (index < members.Count)
            throw new NotSupportedException($"Unsupported remaining path after '{segments.Last().Field.Name}'.");

        return new ResolvedMemberPath(segments);
    }

    static bool IsReferenceRuntimeMember(MemberInfo member)
    {
        Type decl = member.DeclaringType;
        if (decl == null)
            return false;
        return typeof(ReferenceBase).IsAssignableFrom(decl)
            || typeof(IDataType).IsAssignableFrom(decl)
            || typeof(IReference).IsAssignableFrom(decl)
            || (decl.IsGenericType && decl.GetGenericTypeDefinition() == typeof(Enumeration<>));
    }

    internal static bool IsGetContent(MethodCallExpression call)
    {
        return call.Method.Name == nameof(ILazy.GetContent)
            || call.Method.Name == "GetContent";
    }

    internal static Expression StripConverts(Expression expression)
    {
        while (expression is UnaryExpression unary
               && (unary.NodeType == ExpressionType.Convert
                   || unary.NodeType == ExpressionType.ConvertChecked
                   || unary.NodeType == ExpressionType.Quote))
        {
            expression = unary.Operand;
        }
        return expression;
    }
}

internal enum MemberPathKind
{
    ScalarOrForeignKey,
    ReferenceNavigation,
    RelationOne2N,
    RelationN2N
}

internal sealed class MemberPathSegment
{
    public MemberPathSegment(IDbFieldMetadata field, MemberPathKind kind, IDbEntityMetadata owner, IDbEntityMetadata navigatesTo = null)
    {
        Field = field;
        Kind = kind;
        Owner = owner;
        NavigatesTo = navigatesTo;
    }

    public IDbFieldMetadata Field { get; }
    public MemberPathKind Kind { get; }
    public IDbEntityMetadata Owner { get; }
    public IDbEntityMetadata NavigatesTo { get; }
}

internal sealed class ResolvedMemberPath
{
    public ResolvedMemberPath(IReadOnlyList<MemberPathSegment> segments)
    {
        Segments = segments;
    }

    public IReadOnlyList<MemberPathSegment> Segments { get; }

    public MemberPathSegment Last => Segments[Segments.Count - 1];

    public bool IsSimpleScalar => Segments.Count == 1 && Last.Kind == MemberPathKind.ScalarOrForeignKey;

    public string SimpleFieldName => Last.Field.Name;
}
