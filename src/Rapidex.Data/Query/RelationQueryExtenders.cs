using Rapidex.Data.Metadata.Relations;
using Rapidex.Data.Query;
using System;
using static Rapidex.Data.Reference;
using static Rapidex.Data.RelationN2N;
using static Rapidex.Data.RelationOne2N;

namespace Rapidex.Data;

/// <summary>
/// Additive query helpers for null checks and EXISTS-based relation filters.
/// Does not alter Nested / Related semantics.
/// </summary>
public static class RelationQueryExtenders
{
    /// <summary>
    /// Adds a WHERE field IS NULL condition.
    /// </summary>
    /// <param name="qc">Query criteria</param>
    /// <param name="field">Field name</param>
    public static T IsNull<T>(this T qc, string field) where T : IQueryCriteria
    {
        field.NotEmpty();
        string column = QualifiedColumn(qc, field);
        qc.Query.WhereNull(column);
        return qc;
    }

    /// <summary>
    /// Adds a WHERE field IS NOT NULL condition.
    /// </summary>
    /// <param name="qc">Query criteria</param>
    /// <param name="field">Field name</param>
    public static T IsNotNull<T>(this T qc, string field) where T : IQueryCriteria
    {
        field.NotEmpty();
        string column = QualifiedColumn(qc, field);
        qc.Query.WhereNotNull(column);
        return qc;
    }

    /// <summary>
    /// Filters master rows that have a matching referenced entity (EXISTS subquery).
    /// Does not use JOIN; Nested remains available for join-based filtering.
    /// </summary>
    /// <param name="qc">Master query criteria</param>
    /// <param name="referenceField">Reference field on the master entity</param>
    /// <param name="nestedCriteria">Optional criteria on the referenced entity</param>
    public static T ExistsReference<T>(this T qc, string referenceField, Action<IQueryCriteria> nestedCriteria = null) where T : IQueryCriteria
    {
        referenceField.NotEmpty();
        IDbFieldMetadata fm = qc.EntityMetadata.Fields.Get(referenceField)
            .NotNull($"Field '{referenceField}' not found in entity '{qc.EntityMetadata.Name}'");

        if (fm is not ReferenceDbFieldMetadata refFm)
            throw new InvalidOperationException($"Field '{referenceField}' is not a reference field in entity '{qc.EntityMetadata.Name}'");

        IDbEntityMetadata refEm = refFm.ReferencedEntityMetadata.EnsureIsNotPremature(qc.Schema.ParentDbScope);
        if (refEm.IsPremature)
            throw new InvalidOperationException($"Field '{referenceField}' referred metadata is premature '{refEm.Name}'");

        DbQueryCriteria sub = CreateSubCriteria(qc, refEm);
        nestedCriteria?.Invoke(sub);

        string refPk = qc.Schema.Structure.CheckObjectName(refEm.PrimaryKey.Name);
        string masterRef = qc.Schema.Structure.CheckObjectName(refFm.Name);
        sub.Query.WhereColumns($"{sub.Alias}.{refPk}", "=", $"{qc.Alias}.{masterRef}");

        qc.Query.WhereExists(sub.Query);
        return qc;
    }

    /// <summary>
    /// Filters master rows that have at least one matching One2N detail (EXISTS subquery).
    /// </summary>
    /// <param name="qc">Master query criteria</param>
    /// <param name="relationField">One2N relation field on the master entity</param>
    /// <param name="nestedCriteria">Optional criteria on the detail entity</param>
    public static T ExistsOne2N<T>(this T qc, string relationField, Action<IQueryCriteria> nestedCriteria = null) where T : IQueryCriteria
    {
        relationField.NotEmpty();
        IDbFieldMetadata fm = qc.EntityMetadata.Fields.Get(relationField)
            .NotNull($"Field '{relationField}' not found in entity '{qc.EntityMetadata.Name}'");

        if (fm is not VirtualRelationOne2NDbFieldMetadata relFm)
            throw new InvalidOperationException($"Field '{relationField}' is not a One2N relation in entity '{qc.EntityMetadata.Name}'");

        IDbEntityMetadata detailEm = qc.Schema.ParentDbScope.Metadata.Get(relFm.DetailEntityName)
            .NotNull($"Metadata for '{relFm.DetailEntityName}' not found")
            .EnsureIsNotPremature(qc.Schema.ParentDbScope);

        DbQueryCriteria sub = CreateSubCriteria(qc, detailEm);
        nestedCriteria?.Invoke(sub);

        string detailParentField = qc.Schema.Structure.CheckObjectName(relFm.DetailParentFieldName);
        string masterPk = qc.Schema.Structure.CheckObjectName(qc.EntityMetadata.PrimaryKey.Name);
        sub.Query.WhereColumns($"{sub.Alias}.{detailParentField}", "=", $"{qc.Alias}.{masterPk}");

        qc.Query.WhereExists(sub.Query);
        return qc;
    }

    /// <summary>
    /// Filters master rows that have at least one matching N2N related entity (EXISTS via junction).
    /// </summary>
    /// <param name="qc">Master query criteria</param>
    /// <param name="relationField">N2N relation field on the master entity</param>
    /// <param name="nestedCriteria">Optional criteria on the target entity</param>
    public static T ExistsN2N<T>(this T qc, string relationField, Action<IQueryCriteria> nestedCriteria = null) where T : IQueryCriteria
    {
        relationField.NotEmpty();
        IDbFieldMetadata fm = qc.EntityMetadata.Fields.Get(relationField)
            .NotNull($"Field '{relationField}' not found in entity '{qc.EntityMetadata.Name}'");

        if (fm is not VirtualRelationN2NDbFieldMetadata relFm)
            throw new InvalidOperationException($"Field '{relationField}' is not an N2N relation in entity '{qc.EntityMetadata.Name}'");

        IDbEntityMetadata jEm = qc.Schema.ParentDbScope.Metadata.Get(relFm.JunctionEntityName)
            .NotNull($"Metadata for '{relFm.JunctionEntityName}' not found");
        IDbEntityMetadata tEm = qc.Schema.ParentDbScope.Metadata.Get(relFm.TargetEntityName)
            .NotNull($"Metadata for '{relFm.TargetEntityName}' not found");

        string sourceFieldName = qc.Schema.Structure.CheckObjectName(JunctionHelper.GetJunctionSourceFieldName(relFm));
        string targetFieldName = qc.Schema.Structure.CheckObjectName(JunctionHelper.GetJunctionTargetFieldName(relFm));
        string masterPk = qc.Schema.Structure.CheckObjectName(qc.EntityMetadata.PrimaryKey.Name);
        string targetPk = qc.Schema.Structure.CheckObjectName(tEm.PrimaryKey.Name);

        DbQueryCriteria jSub = CreateSubCriteria(qc, jEm);
        jSub.Query.WhereColumns($"{jSub.Alias}.{sourceFieldName}", "=", $"{qc.Alias}.{masterPk}");
        jSub.NotEq(targetFieldName, null);

        if (nestedCriteria != null)
            jSub.Nested(targetFieldName, targetPk, tEm, nestedCriteria);

        qc.Query.WhereExists(jSub.Query);
        return qc;
    }

    static DbQueryCriteria CreateSubCriteria(IQueryCriteria parent, IDbEntityMetadata em)
    {
        int aliasNo = 0;
        if (parent is DbQueryBase dbBase)
            aliasNo = dbBase.GetAliasSubIndex();

        return new DbQueryCriteria(parent.Schema, em, aliasNo);
    }

    static string QualifiedColumn(IQueryCriteria qc, string field)
    {
        field = qc.Schema.Structure.CheckObjectName(field);
        if (qc.Mode == QueryMode.BulkUpdate)
            return field;
        return $"{qc.Alias}.{field}";
    }
}
