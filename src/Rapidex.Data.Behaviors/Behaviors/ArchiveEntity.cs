using System;
using System.Collections.Generic;
using System.Text;

using Rapidex.Data;

namespace Rapidex.Data;

public class ArchiveEntity : EntityBehaviorBase<ArchiveEntity>
{
    public const string FIELD_IS_ARCHIVED = "IsArchived";

    public override string Descripton => "Ensures that the Entity has IsArchived field. And apply archive filters and actions";

    //TODO: Yeni kayıtlarda IsArchived null değil 0 olmalı

    public ArchiveEntity()
    {
    }

    public ArchiveEntity(IEntity entity) : base(entity)
    {
    }

    public override IUpdateResult SetupMetadata(IDbEntityMetadata em)
    {
        em.AddFieldIfNotExist<bool>(FIELD_IS_ARCHIVED);

        //em.AddFilter

        PredefinedFilter filter = new Data.PredefinedFilter();
        filter.Name = "IsNotArchived";
        filter.Caption = "In use";
        filter.Filter = $"({FIELD_IS_ARCHIVED} = false) | ({FIELD_IS_ARCHIVED} = null)";
        filter.Hint = "Show not archived content";
        filter.Description = "Show in use content";
        filter.IsDefault = true;
        em.AddFilter(filter);

        return new UpdateResult();

    }


    public bool IsArchived()
    {
        return this.Entity.GetValue<bool>(FIELD_IS_ARCHIVED);
    }

    public void Archive()
    {
        this.Entity.SetValue(FIELD_IS_ARCHIVED, true);
    }

    public void DeArchive()
    {
        this.Entity.SetValue(FIELD_IS_ARCHIVED, false);
    }
}

public static class ArchiveEntityExtensions
{
    public static IQueryCriteria IsArchived(this IQueryCriteria qo)
    {
        qo.EntityMetadata.Has<ArchiveEntity>();
        return qo.Eq(ArchiveEntity.FIELD_IS_ARCHIVED, true);
    }

    public static IQuery IsArchived(this IQuery qo)
    {
        return qo.Eq(ArchiveEntity.FIELD_IS_ARCHIVED, true);
    }

    public static IQuery<T> IsArchived<T>(this IQuery<T> qo) where T : IConcreteEntity
    {
        return qo.Eq(ArchiveEntity.FIELD_IS_ARCHIVED, true);
    }

    public static IQueryCriteria IsNotArchived(this IQueryCriteria qo)
    {
        return qo.Not(
            qo => qo.Eq(ArchiveEntity.FIELD_IS_ARCHIVED, true));
    }

    public static IQuery IsNotArchived(this IQuery qo)
    {
        return qo.Not(
            qo => qo.Eq(ArchiveEntity.FIELD_IS_ARCHIVED, true));
    }

    public static IQuery<T> IsNotArchived<T>(this IQuery<T> qo) where T : IConcreteEntity
    {
        return qo.Not(
            qo => qo.Eq(ArchiveEntity.FIELD_IS_ARCHIVED, true));
    }
}
